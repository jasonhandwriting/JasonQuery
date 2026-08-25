using System;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.Connection
{
    public sealed class DatabaseServerVersionInfo
    {
        public static DatabaseServerVersionInfo Unknown { get; } = new DatabaseServerVersionInfo
        (
            DataSourceType.None,
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            0
        );

        private DatabaseServerVersionInfo(DataSourceType dataSourceType, string rawVersion, string productName, string displayVersion, int majorVersion, int minorVersion)
        {
            DataSourceType = dataSourceType;
            RawVersion = rawVersion ?? string.Empty;
            ProductName = productName ?? string.Empty;
            DisplayVersion = displayVersion ?? string.Empty;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
        }

        public DataSourceType DataSourceType { get; }

        /// <summary>
        /// 資料庫實際回傳的完整版本文字。
        /// </summary>
        public string RawVersion { get; }

        /// <summary>
        /// Oracle、PostgreSQL、SQL Server、MySQL 或 MariaDB。
        /// </summary>
        public string ProductName { get; }

        /// <summary>
        /// 適合顯示於狀態列的簡短版本。
        /// </summary>
        public string DisplayVersion { get; }

        public int MajorVersion { get; }

        public int MinorVersion { get; }

        public bool IsKnown
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ProductName) && !string.IsNullOrWhiteSpace(DisplayVersion);
            }
        }

        /// <summary>
        /// 例如：Oracle 18.0、SQL Server 2019。
        /// </summary>
        public string DisplayText
        {
            get
            {
                if (!IsKnown)
                {
                    return string.Empty;
                }

                return $"{ProductName} {DisplayVersion}";
            }
        }

        /// <summary>
        /// 例如：SQL Server 2019 (15.0.2000.5)。
        /// </summary>
        public string DiagnosticText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DisplayText))
                {
                    return string.Empty;
                }

                if (string.IsNullOrWhiteSpace(RawVersion) || string.Equals(DisplayVersion, RawVersion, StringComparison.OrdinalIgnoreCase)
                )
                {
                    return DisplayText;
                }

                return $"{DisplayText} ({RawVersion})";
            }
        }

        public static DatabaseServerVersionInfo Create(DataSourceType dataSourceType, string rawVersion)
        {
            var normalizedRawVersion = (rawVersion ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(normalizedRawVersion))
            {
                return Unknown;
            }

            switch (dataSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return CreateMajorMinorVersion
                        (
                            dataSourceType,
                            normalizedRawVersion,
                            "Oracle"
                        );
                    }
                case DataSourceType.PostgreSql:
                    {
                        return CreateMajorMinorVersion
                        (
                            dataSourceType,
                            normalizedRawVersion,
                            "PostgreSQL"
                        );
                    }
                case DataSourceType.SqlServer:
                    {
                        return CreateSqlServerVersion
                        (
                            normalizedRawVersion
                        );
                    }
                case DataSourceType.MySql:
                    {
                        return CreateMySqlVersion
                        (
                            normalizedRawVersion
                        );
                    }
                default:
                    {
                        return Unknown;
                    }
            }
        }

        private static DatabaseServerVersionInfo CreateSqlServerVersion(string rawVersion)
        {
            var sqlServerVersion = SqlServerVersionInfo.Parse(rawVersion);

            if (!sqlServerVersion.IsKnown)
            {
                return new DatabaseServerVersionInfo
                (
                    DataSourceType.SqlServer,
                    rawVersion,
                    "SQL Server",
                    rawVersion,
                    0,
                    0
                );
            }

            var displayVersion = !string.IsNullOrWhiteSpace(sqlServerVersion.ReleaseName)
                                 ? sqlServerVersion.ReleaseName
                                 : $"{sqlServerVersion.MajorVersion}." + $"{sqlServerVersion.MinorVersion}";

            return new DatabaseServerVersionInfo
            (
                DataSourceType.SqlServer,
                rawVersion,
                "SQL Server",
                displayVersion,
                sqlServerVersion.MajorVersion,
                sqlServerVersion.MinorVersion
            );
        }

        private static DatabaseServerVersionInfo CreateMySqlVersion(string rawVersion)
        {
            var isMariaDb = rawVersion.IndexOf("MariaDB", StringComparison.OrdinalIgnoreCase) >= 0;
            var productName = isMariaDb ? "MariaDB" : "MySQL";
            var versionText = isMariaDb ? ExtractMariaDbMajorMinor(rawVersion) : ExtractMajorMinor(rawVersion);

            return CreateFromParsedVersion
            (
                DataSourceType.MySql,
                rawVersion,
                productName,
                versionText
            );
        }

        private static DatabaseServerVersionInfo CreateMajorMinorVersion(DataSourceType dataSourceType, string rawVersion, string productName)
        {
            var versionText = ExtractMajorMinor(rawVersion);

            return CreateFromParsedVersion
            (
                dataSourceType,
                rawVersion,
                productName,
                versionText
            );
        }

        private static DatabaseServerVersionInfo CreateFromParsedVersion(DataSourceType dataSourceType, string rawVersion, string productName, string displayVersion)
        {
            ParseMajorMinor
            (
                displayVersion,
                out var majorVersion,
                out var minorVersion
            );

            return new DatabaseServerVersionInfo
            (
                dataSourceType,
                rawVersion,
                productName,
                displayVersion,
                majorVersion,
                minorVersion
            );
        }

        private static string ExtractMajorMinor(string rawVersion)
        {
            var match = Regex.Match
            (
                rawVersion ?? string.Empty,
                @"^\s*(\d+)(?:\.(\d+))?",
                RegexOptions.CultureInvariant
            );

            if (!match.Success)
            {
                return (rawVersion ?? string.Empty).Trim();
            }

            if (!match.Groups[2].Success)
            {
                return match.Groups[1].Value;
            }

            return $"{match.Groups[1].Value}." + $"{match.Groups[2].Value}";
        }

        private static string ExtractMariaDbMajorMinor(string rawVersion)
        {
            var match = Regex.Match
            (
                rawVersion ?? string.Empty,
                @"(\d+)\.(\d+)(?:\.\d+)?(?=-MariaDB)",
                RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant
            );

            if (!match.Success)
            {
                return ExtractMajorMinor(rawVersion);
            }

            return $"{match.Groups[1].Value}." + $"{match.Groups[2].Value}";
        }

        private static void ParseMajorMinor(string versionText, out int majorVersion, out int minorVersion)
        {
            majorVersion = 0;
            minorVersion = 0;

            var parts = (versionText ?? string.Empty).Split('.');

            if (parts.Length > 0)
            {
                int.TryParse(parts[0], out majorVersion);
            }

            if (parts.Length > 1)
            {
                int.TryParse(parts[1], out minorVersion);
            }
        }
    }
}
