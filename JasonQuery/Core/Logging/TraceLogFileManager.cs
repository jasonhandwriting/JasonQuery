using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace JasonQuery.Core.Logging
{
    internal static class TraceLogFileManager
    {
        public const int DefaultRetentionDays = 7;

        private static readonly string[] SearchPatterns =
        {
            "JasonQuery_*.csv",
            "JasonQuery.log",
            "JasonQuery_*.log"
        };

        public static string ResolveLogDirectory(string applicationDirectory)
        {
            var preferredDirectory = string.IsNullOrWhiteSpace(applicationDirectory) ? string.Empty : Path.Combine(applicationDirectory, "log");

            if (CanWriteToDirectory(preferredDirectory))
            {
                return preferredDirectory;
            }

            var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var fallbackDirectory = string.IsNullOrWhiteSpace(localApplicationData) ? string.Empty : Path.Combine(localApplicationData, "JasonQuery", "Logs");

            return CanWriteToDirectory(fallbackDirectory) ? fallbackDirectory : string.Empty;
        }

        public static string CreateSessionId(DateTimeOffset now, int processId)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:yyyyMMdd-HHmmss-fff}-P{1}", now, processId);
        }

        public static string CreateSessionLogFilePath(string directory, DateTimeOffset now, int processId)
        {
            var baseName = string.Format(CultureInfo.InvariantCulture, "JasonQuery_{0:yyyyMMdd_HHmmss_fff}_P{1}", now, processId);
            var filePath = Path.Combine(directory, $"{baseName}.csv");
            var suffix = 2;

            while (File.Exists(filePath))
            {
                filePath = Path.Combine(directory, $"{baseName}_{suffix.ToString("D2", CultureInfo.InvariantCulture)}.csv");
                suffix++;
            }

            return filePath;
        }

        public static int CleanupOldLogFiles(string directory, DateTimeOffset nowUtc, int retentionDays, string activeFilePath = null)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory) || retentionDays < 1)
            {
                return 0;
            }

            var cutoffUtc = nowUtc.UtcDateTime.AddDays(-retentionDays);
            var deletedCount = 0;
            var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pattern in SearchPatterns)
            {
                try
                {
                    foreach (var filePath in Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly))
                    {
                        candidates.Add(filePath);
                    }
                }
                catch
                {
                    //清理失敗不得影響 JasonQuery 啟動
                }
            }

            foreach (var filePath in candidates)
            {
                try
                {
                    if (PathsEqual(filePath, activeFilePath) || File.GetLastWriteTimeUtc(filePath) >= cutoffUtc)
                    {
                        continue;
                    }

                    File.Delete(filePath);
                    deletedCount++;
                }
                catch
                {
                    //檔案可能被其他 JasonQuery 執行個體或 Excel 使用，略過即可
                }
            }

            return deletedCount;
        }

        private static bool CanWriteToDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                return false;
            }

            string testFilePath = null;

            try
            {
                Directory.CreateDirectory(directory);
                testFilePath = Path.Combine(directory, $".jasonquery-log-{Guid.NewGuid():N}.tmp");

                using (File.Create(testFilePath))
                {
                }

                File.Delete(testFilePath);
                return true;
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(testFilePath))
                {
                    try
                    {
                        File.Delete(testFilePath);
                    }
                    catch
                    {
                    }
                }

                return false;
            }
        }

        private static bool PathsEqual(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            return string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);
        }
    }
}
