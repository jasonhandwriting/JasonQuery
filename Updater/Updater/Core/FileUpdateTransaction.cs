using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace Updater.Core
{
    public sealed class UpdateTransactionResult
    {
        public UpdateTransactionResult(string backupManifestPath, int updatedFileCount)
        {
            BackupManifestPath = backupManifestPath;
            UpdatedFileCount = updatedFileCount;
        }

        public string BackupManifestPath { get; }

        public int UpdatedFileCount { get; }
    }

    public enum UpdateTransactionStage
    {
        CreatingVerifiedBackup,
        ApplyingAndVerifyingUpdate,
        RestoringAndVerifyingPreviousVersion
    }

    public sealed class UpdateTransactionException : IOException
    {
        public UpdateTransactionException(string message, Exception updateException, Exception recoveryException, string backupManifestPath) : base(message, updateException)
        {
            UpdateException = updateException;
            RecoveryException = recoveryException;
            BackupManifestPath = backupManifestPath;
        }

        public Exception UpdateException { get; }

        public Exception RecoveryException { get; }

        public string BackupManifestPath { get; }

        public bool RecoverySucceeded => RecoveryException == null;
    }

    public sealed class ProtectedUpdatePathException : IOException
    {
        public ProtectedUpdatePathException(string relativePath)
            : base($"The update package contains a protected application file: {relativePath}")
        {
            RelativePath = relativePath ?? string.Empty;
        }

        public string RelativePath { get; }
    }

    public sealed class FileUpdateTransaction
    {
        public const string BackupManifestFileName = "backup-manifest.json";
        private const string BackupFilesFolderName = "files";

        private static readonly string[] ProtectedRelativePaths =
        {
            "JasonQuery.db"
        };

        private static readonly string[] ProtectedRelativeDirectories =
        {
            "backup",
            "Log"
        };

        private static readonly HashSet<string> WindowsReservedNames = new HashSet<string>
        (
            new[]
            {
                "CON", "PRN", "AUX", "NUL",
                "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            },
            StringComparer.OrdinalIgnoreCase
        );

        public UpdateTransactionResult Execute(string payloadRoot, string installationRoot, string backupRoot,
                                               string installedVersion, string targetVersion, Action<UpdateTransactionStage> reportProgress = null)
        {
            var normalizedPayloadRoot = NormalizeRoot(payloadRoot, nameof(payloadRoot));
            var normalizedInstallationRoot = NormalizeRoot(installationRoot, nameof(installationRoot));
            var normalizedBackupRoot = NormalizeRoot(backupRoot, nameof(backupRoot));

            EnsureRootsAreSeparate(normalizedPayloadRoot, normalizedInstallationRoot, normalizedBackupRoot);

            if (!File.Exists(Path.Combine(normalizedPayloadRoot, "JasonQuery.exe")))
            {
                throw new InvalidDataException("The update payload does not contain JasonQuery.exe.");
            }

            var sourceFiles = EnumerateSourceFiles(normalizedPayloadRoot).ToList();

            if (sourceFiles.Count == 0)
            {
                throw new InvalidDataException("The update payload does not contain any files.");
            }

            Directory.CreateDirectory(normalizedBackupRoot);

            if (Directory.EnumerateFileSystemEntries(normalizedBackupRoot).Any())
            {
                throw new IOException("The update backup directory must be empty.");
            }

            ReportProgress(reportProgress, UpdateTransactionStage.CreatingVerifiedBackup);

            var manifest = CreateVerifiedBackup
            (
                sourceFiles,
                normalizedInstallationRoot,
                normalizedBackupRoot,
                installedVersion,
                targetVersion
            );

            var manifestPath = Path.Combine(normalizedBackupRoot, BackupManifestFileName);
            var appliedPaths = new List<string>();

            try
            {
                ReportProgress(reportProgress, UpdateTransactionStage.ApplyingAndVerifyingUpdate);

                foreach (var sourceFile in sourceFiles)
                {
                    var targetChanged = false;

                    try
                    {
                        ApplyFile(sourceFile, normalizedInstallationRoot, out targetChanged);
                        appliedPaths.Add(sourceFile.RelativePath);
                    }
                    catch
                    {
                        if (targetChanged)
                        {
                            appliedPaths.Add(sourceFile.RelativePath);
                        }

                        throw;
                    }
                }

                return new UpdateTransactionResult(manifestPath, appliedPaths.Count);
            }
            catch (Exception updateException)
            {
                Exception recoveryException = null;

                try
                {
                    ReportProgress(reportProgress, UpdateTransactionStage.RestoringAndVerifyingPreviousVersion);
                    RestoreEntries(manifest, normalizedBackupRoot, appliedPaths);
                }
                catch (Exception ex)
                {
                    recoveryException = ex;
                }

                var message = recoveryException == null
                    ? "The update failed and the previous version was restored automatically."
                    : "The update failed and automatic recovery could not be completed.";

                throw new UpdateTransactionException(message, updateException, recoveryException, manifestPath);
            }
        }

        private static void ReportProgress(Action<UpdateTransactionStage> reportProgress, UpdateTransactionStage stage)
        {
            try
            {
                reportProgress?.Invoke(stage);
            }
            catch
            {
                // Progress reporting must never interrupt an update transaction.
            }
        }

        public void RestoreBackup(string manifestPath)
        {
            var manifest = LoadManifest(manifestPath);
            var backupRoot = NormalizeRoot(Path.GetDirectoryName(manifestPath), nameof(manifestPath));
            var paths = (manifest.Files ?? new List<UpdateBackupFile>()).Select(file => file.RelativePath).ToList();

            RestoreEntries(manifest, backupRoot, paths);
        }

        private static UpdateBackupManifest CreateVerifiedBackup(IEnumerable<UpdateSourceFile> sourceFiles, string installationRoot, string backupRoot, string installedVersion, string targetVersion)
        {
            var backupFilesRoot = Path.Combine(backupRoot, BackupFilesFolderName);

            Directory.CreateDirectory(backupFilesRoot);

            var manifest = new UpdateBackupManifest
            {
                CreatedAt = DateTimeOffset.UtcNow.ToString("O"),
                InstallationRoot = installationRoot.TrimEnd(Path.DirectorySeparatorChar),
                InstalledVersion = installedVersion ?? string.Empty,
                TargetVersion = targetVersion ?? string.Empty
            };

            foreach (var sourceFile in sourceFiles)
            {
                var targetPath = ResolveUnderRoot(installationRoot, sourceFile.RelativePath);

                var entry = new UpdateBackupFile
                {
                    RelativePath = sourceFile.RelativePath,
                    Existed = File.Exists(targetPath)
                };

                if (entry.Existed)
                {
                    entry.Size = new FileInfo(targetPath).Length;
                    entry.Sha256 = Sha256Digest.ComputeFile(targetPath);

                    var backupPath = ResolveUnderRoot(EnsureTrailingSeparator(backupFilesRoot), sourceFile.RelativePath);
                    var backupFolder = Path.GetDirectoryName(backupPath);

                    if (!string.IsNullOrEmpty(backupFolder))
                    {
                        Directory.CreateDirectory(backupFolder);
                    }

                    File.Copy(targetPath, backupPath, false);
                    VerifyFile(backupPath, entry.Size, entry.Sha256, "backup");
                }

                manifest.Files.Add(entry);
            }

            var manifestPath = Path.Combine(backupRoot, BackupManifestFileName);
            var serializer = new DataContractJsonSerializer(typeof(UpdateBackupManifest));

            using (var stream = new FileStream(manifestPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                serializer.WriteObject(stream, manifest);
                stream.Flush(true);
            }

            return manifest;
        }

        private static void ApplyFile(UpdateSourceFile sourceFile, string installationRoot, out bool targetChanged)
        {
            targetChanged = false;

            var targetPath = ResolveUnderRoot(installationRoot, sourceFile.RelativePath);
            var targetFolder = Path.GetDirectoryName(targetPath);

            if (!string.IsNullOrEmpty(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            var temporaryPath = targetPath + $".jq-update-{Guid.NewGuid():N}.tmp";

            try
            {
                File.Copy(sourceFile.FullPath, temporaryPath, false);
                VerifyFile(temporaryPath, sourceFile.Size, sourceFile.Sha256, "staged update");

                if (File.Exists(targetPath))
                {
                    File.Replace(temporaryPath, targetPath, null, true);
                }
                else
                {
                    File.Move(temporaryPath, targetPath);
                }

                targetChanged = true;
                VerifyFile(targetPath, sourceFile.Size, sourceFile.Sha256, "updated target");
            }
            finally
            {
                TryDeleteFile(temporaryPath);
            }
        }

        private static void RestoreEntries(UpdateBackupManifest manifest, string backupRoot, IEnumerable<string> relativePaths)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            if (manifest.SchemaVersion != UpdateBackupManifest.CurrentSchemaVersion)
            {
                throw new NotSupportedException($"Backup manifest schema version {manifest.SchemaVersion} is not supported.");
            }

            var installationRoot = NormalizeRoot(manifest.InstallationRoot, nameof(manifest.InstallationRoot));
            var requestedPaths = new HashSet<string>(relativePaths ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var entries = (manifest.Files ?? new List<UpdateBackupFile>()).Where(entry => entry != null && requestedPaths.Contains(entry.RelativePath)).Reverse().ToList();

            foreach (var entry in entries)
            {
                ValidateRelativePath(entry.RelativePath);

                var targetPath = ResolveUnderRoot(installationRoot, entry.RelativePath);

                if (!entry.Existed)
                {
                    TryDeleteFile(targetPath, true);
                    continue;
                }

                var backupPath = ResolveUnderRoot
                (
                    EnsureTrailingSeparator(Path.Combine(backupRoot, BackupFilesFolderName)),
                    entry.RelativePath
                );

                VerifyFile(backupPath, entry.Size, entry.Sha256, "backup restore source");

                var targetFolder = Path.GetDirectoryName(targetPath);

                if (!string.IsNullOrEmpty(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                var temporaryPath = targetPath + $".jq-restore-{Guid.NewGuid():N}.tmp";

                try
                {
                    File.Copy(backupPath, temporaryPath, false);
                    VerifyFile(temporaryPath, entry.Size, entry.Sha256, "staged restore");

                    if (File.Exists(targetPath))
                    {
                        File.Replace(temporaryPath, targetPath, null, true);
                    }
                    else
                    {
                        File.Move(temporaryPath, targetPath);
                    }

                    VerifyFile(targetPath, entry.Size, entry.Sha256, "restored target");
                }
                finally
                {
                    TryDeleteFile(temporaryPath);
                }
            }
        }

        private static UpdateBackupManifest LoadManifest(string manifestPath)
        {
            if (string.IsNullOrWhiteSpace(manifestPath) || !File.Exists(manifestPath))
            {
                throw new FileNotFoundException("The update backup manifest was not found.", manifestPath);
            }

            var serializer = new DataContractJsonSerializer(typeof(UpdateBackupManifest));

            using (var stream = new FileStream(manifestPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var manifest = (UpdateBackupManifest)serializer.ReadObject(stream);

                if (manifest == null)
                {
                    throw new InvalidDataException("The update backup manifest is invalid.");
                }

                return manifest;
            }
        }

        private static IEnumerable<UpdateSourceFile> EnumerateSourceFiles(string payloadRoot)
        {
            var relativePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var fullPath in Directory.GetFiles(payloadRoot, "*", SearchOption.AllDirectories)
                                              .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var relativePath = fullPath.Substring(payloadRoot.Length)
                                           .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                ValidateRelativePath(relativePath);
                RejectProtectedPath(relativePath);

                if (!relativePaths.Add(relativePath))
                {
                    throw new InvalidDataException($"The update payload contains duplicate paths: {relativePath}");
                }

                yield return new UpdateSourceFile
                {
                    FullPath = fullPath,
                    RelativePath = relativePath,
                    Size = new FileInfo(fullPath).Length,
                    Sha256 = Sha256Digest.ComputeFile(fullPath)
                };
            }
        }

        private static void RejectProtectedPath(string relativePath)
        {
            if (ProtectedRelativePaths.Any(path => string.Equals(path, relativePath, StringComparison.OrdinalIgnoreCase))
                || ProtectedRelativeDirectories.Any
                (
                    directory => relativePath.StartsWith
                    (
                        directory + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase
                    )
                ))
            {
                throw new ProtectedUpdatePathException(relativePath);
            }
        }

        private static void VerifyFile(string filePath, long expectedSize, string expectedSha256, string purpose)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The {purpose} file was not found.", filePath);
            }

            var actualSize = new FileInfo(filePath).Length;

            if (actualSize != expectedSize)
            {
                throw new InvalidDataException($"The {purpose} file size does not match: {filePath}");
            }

            var actualSha256 = Sha256Digest.ComputeFile(filePath);

            if (!string.Equals(actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"The {purpose} file SHA-256 does not match: {filePath}");
            }
        }

        private static string NormalizeRoot(string path, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A directory is required.", parameterName);
            }

            return EnsureTrailingSeparator(Path.GetFullPath(path));
        }

        private static void EnsureRootsAreSeparate(string payloadRoot, string installationRoot, string backupRoot)
        {
            if (PathsOverlap(payloadRoot, installationRoot) || PathsOverlap(payloadRoot, backupRoot) || PathsOverlap(installationRoot, backupRoot))
            {
                throw new InvalidOperationException("The payload, installation, and backup directories must be separate.");
            }
        }

        private static bool PathsOverlap(string first, string second)
        {
            return first.StartsWith(second, StringComparison.OrdinalIgnoreCase) || second.StartsWith(first, StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveUnderRoot(string root, string relativePath)
        {
            ValidateRelativePath(relativePath);

            var normalizedRoot = EnsureTrailingSeparator(Path.GetFullPath(root));
            var fullPath = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath));

            if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"A file path escaped its expected root: {relativePath}");
            }

            return fullPath;
        }

        private static void ValidateRelativePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            {
                throw new InvalidDataException($"The relative file path is invalid: {relativePath}");
            }

            var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var invalidCharacters = Path.GetInvalidFileNameChars();

            if (segments.Any
                (
                    segment => string.IsNullOrWhiteSpace(segment)
                               || segment == "."
                               || segment == ".."
                               || segment.IndexOf(':') >= 0
                               || segment.IndexOfAny(invalidCharacters) >= 0
                               || segment.EndsWith(".", StringComparison.Ordinal)
                               || segment.EndsWith(" ", StringComparison.Ordinal)
                               || WindowsReservedNames.Contains(Path.GetFileNameWithoutExtension(segment))
                ))
            {
                throw new InvalidDataException($"The relative file path is unsafe: {relativePath}");
            }
        }

        private static string EnsureTrailingSeparator(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ? path : path + Path.DirectorySeparatorChar;
        }

        private static void TryDeleteFile(string filePath, bool throwOnFailure = false)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                }
            }
            catch
            {
                if (throwOnFailure)
                {
                    throw;
                }
            }
        }

        private sealed class UpdateSourceFile
        {
            public string FullPath { get; set; }

            public string RelativePath { get; set; }

            public long Size { get; set; }

            public string Sha256 { get; set; }
        }
    }
}
