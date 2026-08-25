using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace Updater.Core
{
    internal sealed class UpdateWorkspace
    {
        private UpdateWorkspace(string root, string packageName)
        {
            Root = root;
            PackagePath = Path.Combine(root, packageName);
            ExtractionRoot = Path.Combine(root, "extracted");
            WorkerExecutablePath = Path.Combine(root, "UpdaterWorker.exe");
            WorkerPlanPath = Path.Combine(root, "worker-plan.json");
        }

        public string Root { get; }

        public string PackagePath { get; }

        public string ExtractionRoot { get; }

        public string WorkerExecutablePath { get; }

        public string WorkerPlanPath { get; }

        public static UpdateWorkspace Create(string packageName)
        {
            var updaterRoot = Path.Combine(Path.GetTempPath(), "JasonQuery", "Updater");

            Directory.CreateDirectory(updaterRoot);
            TryDeleteStaleDirectories(updaterRoot);

            var workspaceRoot = Path.Combine
            (
                updaterRoot,
                $"{DateTime.Now:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}"
            );

            Directory.CreateDirectory(workspaceRoot);

            return new UpdateWorkspace(workspaceRoot, packageName);
        }

        public static string CreateBackupRoot(string installedVersion, string targetVersion)
        {
            var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (string.IsNullOrWhiteSpace(localApplicationData))
            {
                localApplicationData = Path.GetTempPath();
            }

            var backupParent = Path.Combine(localApplicationData, "JasonQuery", "UpdateBackups");
            var folderName = $"{DateTime.Now:yyyyMMdd-HHmmss-fff}_{Sanitize(installedVersion)}_to_{Sanitize(targetVersion)}";

            Directory.CreateDirectory(backupParent);

            return Path.Combine(backupParent, folderName);
        }

        private static void TryDeleteStaleDirectories(string updaterRoot)
        {
            foreach (var directory in Directory.GetDirectories(updaterRoot))
            {
                try
                {
                    if (Directory.GetCreationTimeUtc(directory) < DateTime.UtcNow.AddDays(-7))
                    {
                        Directory.Delete(directory, true);
                    }
                }
                catch
                {
                    // A running or diagnostically retained workspace is left untouched.
                }
            }
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "unknown";
            }

            foreach (var character in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(character, '_');
            }

            return value.Trim();
        }
    }

    public sealed class UpdateBackupRetentionResult
    {
        internal UpdateBackupRetentionResult(List<string> deletedDirectories, List<string> failedDirectories)
        {
            DeletedDirectories = deletedDirectories.AsReadOnly();
            FailedDirectories = failedDirectories.AsReadOnly();
        }

        public IReadOnlyList<string> DeletedDirectories { get; }

        public IReadOnlyList<string> FailedDirectories { get; }
    }

    public static class UpdateBackupRetentionPolicy
    {
        public const int DefaultMaximumVerifiedBackups = 3;
        private const int TimestampLength = 19;

        public static UpdateBackupRetentionResult Prune(string backupParent, string currentBackupRoot, int maximumVerifiedBackups = DefaultMaximumVerifiedBackups)
        {
            if (maximumVerifiedBackups < 1)
            {
                throw new ArgumentOutOfRangeException
                (
                    nameof(maximumVerifiedBackups),
                    "At least one verified update backup must be retained."
                );
            }

            var normalizedParent = NormalizeDirectory(backupParent, nameof(backupParent));
            var normalizedCurrent = NormalizeDirectory(currentBackupRoot, nameof(currentBackupRoot));
            var parentName = new DirectoryInfo(normalizedParent).Name;

            if (!string.Equals(parentName, "UpdateBackups", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Update backup retention may run only inside an UpdateBackups directory.");
            }

            if (!string.Equals(Path.GetDirectoryName(normalizedCurrent), normalizedParent, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The current update backup is not a direct child of the backup directory.");
            }

            if (!Directory.Exists(normalizedCurrent) || !IsUpdaterBackupDirectory(normalizedCurrent) || !HasValidManifest(normalizedCurrent))
            {
                throw new InvalidDataException("The current update backup does not contain a valid backup manifest.");
            }

            var verifiedDirectories = Directory.GetDirectories(normalizedParent)
                                               .Where(IsUpdaterBackupDirectory)
                                               .Where(HasValidManifest)
                                               .Select
                                                (
                                                    path => new BackupDirectory
                                                    {
                                                        Path = NormalizeDirectory(path, nameof(path)),
                                                        CreatedAtUtc = Directory.GetCreationTimeUtc(path)
                                                    }
                                                )
                                               .OrderByDescending(directory => directory.CreatedAtUtc)
                                               .ThenByDescending(directory => directory.Path, StringComparer.OrdinalIgnoreCase)
                                               .ToList();

            var retainedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                normalizedCurrent
            };

            foreach (var directory in verifiedDirectories)
            {
                if (retainedDirectories.Count >= maximumVerifiedBackups)
                {
                    break;
                }

                retainedDirectories.Add(directory.Path);
            }

            var deletedDirectories = new List<string>();
            var failedDirectories = new List<string>();

            foreach (var directory in verifiedDirectories)
            {
                if (retainedDirectories.Contains(directory.Path))
                {
                    continue;
                }

                try
                {
                    Directory.Delete(directory.Path, true);
                    deletedDirectories.Add(directory.Path);
                }
                catch
                {
                    failedDirectories.Add(directory.Path);
                }
            }

            return new UpdateBackupRetentionResult(deletedDirectories, failedDirectories);
        }

        private static bool IsUpdaterBackupDirectory(string path)
        {
            try
            {
                if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            var name = Path.GetFileName(path);

            if (string.IsNullOrWhiteSpace(name) || name.Length <= TimestampLength || name[TimestampLength] != '_')
            {
                return false;
            }

            return DateTime.TryParseExact
            (
                name.Substring(0, TimestampLength),
                "yyyyMMdd-HHmmss-fff",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _
            );
        }

        private static bool HasValidManifest(string backupRoot)
        {
            var manifestPath = Path.Combine(backupRoot, FileUpdateTransaction.BackupManifestFileName);

            if (!File.Exists(manifestPath))
            {
                return false;
            }

            try
            {
                var serializer = new DataContractJsonSerializer(typeof(UpdateBackupManifest));

                using (var stream = new FileStream(manifestPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var manifest = serializer.ReadObject(stream) as UpdateBackupManifest;

                    return manifest != null
                           && manifest.SchemaVersion == UpdateBackupManifest.CurrentSchemaVersion
                           && !string.IsNullOrWhiteSpace(manifest.InstallationRoot)
                           && manifest.Files != null;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string NormalizeDirectory(string path, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A directory is required.", parameterName);
            }

            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private sealed class BackupDirectory
        {
            public string Path { get; set; }

            public DateTime CreatedAtUtc { get; set; }
        }
    }
}
