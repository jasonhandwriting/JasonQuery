using System;

namespace JasonQuery.Core.Database.Connection
{
    public sealed class SqlServerVersionInfo
    {
        public static SqlServerVersionInfo Unknown { get; } = new SqlServerVersionInfo(string.Empty, null);

        private SqlServerVersionInfo(string productVersion, Version parsedVersion)
        {
            ProductVersion = productVersion ?? string.Empty;
            ParsedVersion = parsedVersion;
        }

        /// <summary>
        /// SERVERPROPERTY('ProductVersion') 回傳的原始版本文字。
        /// 例如：15.0.2000.5。
        /// </summary>
        public string ProductVersion { get; }

        /// <summary>
        /// 成功解析後的 System.Version。
        /// 無法解析時為 null。
        /// </summary>
        public Version ParsedVersion { get; }

        public bool IsKnown
        {
            get
            {
                return ParsedVersion != null;
            }
        }

        public int MajorVersion
        {
            get
            {
                return ParsedVersion?.Major ?? 0;
            }
        }

        public int MinorVersion
        {
            get
            {
                return ParsedVersion?.Minor ?? 0;
            }
        }

        /// <summary>
        /// SQL Server 的產品版本名稱。
        /// 例如：2019、2022、2025、2008 R2。
        /// </summary>
        public string ReleaseName
        {
            get
            {
                return ResolveReleaseName();
            }
        }

        /// <summary>
        /// 適合顯示於狀態列的簡短文字。
        /// 例如：SQL Server 2019。
        /// </summary>
        public string DisplayText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ProductVersion))
                {
                    return string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(ReleaseName))
                {
                    return $"SQL Server {ReleaseName}";
                }

                if (IsKnown)
                {
                    return $"SQL Server {MajorVersion}.{MinorVersion}";
                }

                return $"SQL Server {ProductVersion}";
            }
        }

        /// <summary>
        /// 適合顯示於錯誤訊息標題或執行記錄的完整文字。
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

                if (string.IsNullOrWhiteSpace(ProductVersion))
                {
                    return DisplayText;
                }

                return $"{DisplayText} ({ProductVersion})";
            }
        }

        public bool IsSqlServer2000OrLower
        {
            get
            {
                return IsKnown && MajorVersion <= 8;
            }
        }

        public bool SupportsOptimizeForSequentialKey
        {
            get
            {
                return IsKnown && MajorVersion >= 15;
            }
        }

        public static SqlServerVersionInfo Parse(string productVersion)
        {
            var normalizedVersion = (productVersion ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(normalizedVersion))
            {
                return Unknown;
            }

            if (!Version.TryParse(normalizedVersion, out var parsedVersion))
            {
                return new SqlServerVersionInfo(normalizedVersion, null);
            }

            return new SqlServerVersionInfo(normalizedVersion, parsedVersion);
        }

        private string ResolveReleaseName()
        {
            if (!IsKnown)
            {
                return string.Empty;
            }

            switch (MajorVersion)
            {
                case 17:
                    {
                        return "2025";
                    }
                case 16:
                    {
                        return "2022";
                    }
                case 15:
                    {
                        return "2019";
                    }
                case 14:
                    {
                        return "2017";
                    }
                case 13:
                    {
                        return "2016";
                    }
                case 12:
                    {
                        return "2014";
                    }
                case 11:
                    {
                        return "2012";
                    }
                case 10:
                    {
                        return MinorVersion >= 50 ? "2008 R2" : "2008";
                    }
                case 9:
                    {
                        return "2005";
                    }
                case 8:
                    {
                        return "2000";
                    }
                case 7:
                    {
                        return "7.0";
                    }
                case 6:
                    {
                        return MinorVersion >= 5 ? "6.5" : "6.0";
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }
    }
}
