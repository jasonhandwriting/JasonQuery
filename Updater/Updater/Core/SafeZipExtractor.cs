using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Updater.Core
{
    public static class SafeZipExtractor
    {
        private const int MaximumEntryCount = 10000;
        private const long MaximumExpandedSize = 2L * 1024L * 1024L * 1024L;
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

        public static string ExtractAndResolvePayloadRoot(string packagePath, string extractionRoot)
        {
            if (string.IsNullOrWhiteSpace(packagePath) || !File.Exists(packagePath))
            {
                throw new FileNotFoundException("The update package was not found.", packagePath);
            }

            if (string.IsNullOrWhiteSpace(extractionRoot))
            {
                throw new ArgumentException("An extraction directory is required.", nameof(extractionRoot));
            }

            Directory.CreateDirectory(extractionRoot);

            if (Directory.EnumerateFileSystemEntries(extractionRoot).Any())
            {
                throw new IOException("The update extraction directory must be empty.");
            }

            var normalizedRoot = EnsureTrailingSeparator(Path.GetFullPath(extractionRoot));
            var extractedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var fileCount = 0;
            long expandedSize = 0;

            using (var archive = ZipFile.OpenRead(packagePath))
            {
                if (archive.Entries.Count > MaximumEntryCount)
                {
                    throw new InvalidDataException("The update package contains too many entries.");
                }

                foreach (var entry in archive.Entries)
                {
                    RejectSymbolicLink(entry);

                    expandedSize = checked(expandedSize + entry.Length);

                    if (expandedSize > MaximumExpandedSize)
                    {
                        throw new InvalidDataException("The expanded update package is too large.");
                    }

                    var entryName = entry.FullName.Replace('/', Path.DirectorySeparatorChar);

                    if (string.IsNullOrWhiteSpace(entryName))
                    {
                        continue;
                    }

                    if (Path.IsPathRooted(entryName))
                    {
                        throw new InvalidDataException($"The update package contains an absolute path: {entry.FullName}");
                    }

                    ValidateEntryPath(entryName, entry.FullName);

                    var destinationPath = Path.GetFullPath(Path.Combine(normalizedRoot, entryName));

                    if (!destinationPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidDataException($"The update package contains an unsafe path: {entry.FullName}");
                    }

                    if (!extractedPaths.Add(destinationPath))
                    {
                        throw new InvalidDataException($"The update package contains duplicate paths: {entry.FullName}");
                    }

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }

                    var destinationFolder = Path.GetDirectoryName(destinationPath);

                    if (!string.IsNullOrEmpty(destinationFolder))
                    {
                        Directory.CreateDirectory(destinationFolder);
                    }

                    using (var source = entry.Open())
                    using (var destination = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        source.CopyTo(destination);
                        destination.Flush(true);
                    }

                    if (entry.LastWriteTime != default(DateTimeOffset))
                    {
                        File.SetLastWriteTimeUtc(destinationPath, entry.LastWriteTime.UtcDateTime);
                    }

                    fileCount++;
                }
            }

            if (fileCount == 0)
            {
                throw new InvalidDataException("The update package does not contain any files.");
            }

            return ResolvePayloadRoot(extractionRoot);
        }

        private static string ResolvePayloadRoot(string extractionRoot)
        {
            var directExecutable = Path.Combine(extractionRoot, "JasonQuery.exe");

            if (File.Exists(directExecutable))
            {
                return Path.GetFullPath(extractionRoot);
            }

            var candidates = Directory.GetFiles(extractionRoot, "JasonQuery.exe", SearchOption.AllDirectories)
                                      .Select(Path.GetDirectoryName)
                                      .Where(path => !string.IsNullOrWhiteSpace(path))
                                      .Distinct(StringComparer.OrdinalIgnoreCase)
                                      .ToArray();

            if (candidates.Length != 1)
            {
                throw new InvalidDataException("The update package must contain exactly one JasonQuery.exe payload root.");
            }

            return Path.GetFullPath(candidates[0]);
        }

        private static void RejectSymbolicLink(ZipArchiveEntry entry)
        {
            const int unixFileTypeMask = 0xF000;
            const int unixSymbolicLink = 0xA000;
            var unixMode = entry.ExternalAttributes >> 16;

            if ((unixMode & unixFileTypeMask) == unixSymbolicLink)
            {
                throw new InvalidDataException($"The update package contains a symbolic link: {entry.FullName}");
            }
        }

        private static void ValidateEntryPath(string entryName, string displayName)
        {
            var invalidCharacters = Path.GetInvalidFileNameChars();
            var segments = entryName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            foreach (var segment in segments)
            {
                if (string.IsNullOrWhiteSpace(segment))
                {
                    continue;
                }

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(segment);

                if (segment == "."
                    || segment == ".."
                    || segment.IndexOf(':') >= 0
                    || segment.IndexOfAny(invalidCharacters) >= 0
                    || segment.EndsWith(".", StringComparison.Ordinal)
                    || segment.EndsWith(" ", StringComparison.Ordinal)
                    || WindowsReservedNames.Contains(fileNameWithoutExtension))
                {
                    throw new InvalidDataException($"The update package contains an unsafe file name: {displayName}");
                }
            }
        }

        private static string EnsureTrailingSeparator(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ? path : path + Path.DirectorySeparatorChar;
        }
    }
}