using Microsoft.Win32;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.SystemInfo
{
    internal sealed class WindowsVersionInfo
    {
        private WindowsVersionInfo(string shortName, string fullName)
        {
            ShortName = shortName;
            FullName = fullName;
        }

        public string ShortName { get; }
        public string FullName { get; }

        internal static WindowsVersionInfo Create(string productName, string displayVersion, string releaseId, int majorVersion,
                                                  int minorVersion, int buildNumber, int updateBuildRevision, string fallbackDescription)
        {
            productName = NormalizeWhitespace(productName);
            displayVersion = NormalizeWhitespace(displayVersion);
            releaseId = NormalizeWhitespace(releaseId);
            fallbackDescription = NormalizeWhitespace(fallbackDescription);

            var isServer = productName.IndexOf("Server", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!isServer && buildNumber >= 22000)
            {
                productName = ReplaceFirst(productName, "Windows 10", "Windows 11");
            }

            if (string.IsNullOrWhiteSpace(productName))
            {
                productName = ResolveClientProductName(majorVersion, minorVersion, buildNumber);
            }

            var shortName = ResolveShortName(productName, isServer, majorVersion, minorVersion, buildNumber);
            var fullName = BuildFullName(productName, displayVersion, releaseId, buildNumber, updateBuildRevision, fallbackDescription);

            return new WindowsVersionInfo(shortName, fullName);
        }

        private static string ResolveShortName(string productName, bool isServer, int majorVersion, int minorVersion, int buildNumber)
        {
            if (isServer)
            {
                return ResolveServerShortName(productName, buildNumber);
            }

            if (Contains(productName, "Windows 11") || majorVersion >= 10 && buildNumber >= 22000)
            {
                return "Win11";
            }

            if (Contains(productName, "Windows 10") || majorVersion >= 10 && buildNumber > 0)
            {
                return "Win10";
            }

            if (Contains(productName, "Windows 8.1") || majorVersion == 6 && minorVersion == 3)
            {
                return "Win8.1";
            }

            if (Contains(productName, "Windows 8") || majorVersion == 6 && minorVersion == 2)
            {
                return "Win8";
            }

            if (Contains(productName, "Windows 7") || majorVersion == 6 && minorVersion == 1)
            {
                return "Win7";
            }

            return "Windows";
        }

        private static string ResolveServerShortName(string productName, int buildNumber)
        {
            var match = Regex.Match(productName ?? string.Empty,
                                    @"Windows\s+Server\s+(?<year>\d{4})(?:\s*(?<r2>R2))?",
                                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (match.Success)
            {
                var year = match.Groups["year"].Value;
                var suffix = match.Groups["r2"].Success ? "R2" : string.Empty;

                return $"WinServer{year}{suffix}";
            }

            if (buildNumber >= 26100)
            {
                return "WinServer2025";
            }

            if (buildNumber >= 20348)
            {
                return "WinServer2022";
            }

            if (buildNumber >= 17763)
            {
                return "WinServer2019";
            }

            if (buildNumber >= 14393)
            {
                return "WinServer2016";
            }

            if (buildNumber >= 9600)
            {
                return "WinServer2012R2";
            }

            if (buildNumber >= 9200)
            {
                return "WinServer2012";
            }

            if (buildNumber >= 7601)
            {
                return "WinServer2008R2";
            }

            return "WinServer";
        }

        private static string BuildFullName(string productName, string displayVersion, string releaseId, int buildNumber, int updateBuildRevision, string fallbackDescription)
        {
            var fullName = productName;
            var versionLabel = !string.IsNullOrWhiteSpace(displayVersion) ? displayVersion : releaseId;

            if (!string.IsNullOrWhiteSpace(versionLabel) && !Contains(fullName, versionLabel))
            {
                fullName = JoinWithSpace(fullName, versionLabel);
            }

            if (buildNumber > 0)
            {
                var buildText = updateBuildRevision >= 0
                                ? string.Format(CultureInfo.InvariantCulture, "{0}.{1}", buildNumber, updateBuildRevision)
                                : buildNumber.ToString(CultureInfo.InvariantCulture);

                fullName = JoinWithSpace(fullName, $"(Build {buildText})");
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = !string.IsNullOrWhiteSpace(fallbackDescription) ? fallbackDescription : "Windows";
            }

            return fullName;
        }

        private static string ResolveClientProductName(int majorVersion, int minorVersion, int buildNumber)
        {
            if (majorVersion >= 10 && buildNumber >= 22000)
            {
                return "Windows 11";
            }

            if (majorVersion >= 10 && buildNumber > 0)
            {
                return "Windows 10";
            }

            if (majorVersion == 6 && minorVersion == 3)
            {
                return "Windows 8.1";
            }

            if (majorVersion == 6 && minorVersion == 2)
            {
                return "Windows 8";
            }

            if (majorVersion == 6 && minorVersion == 1)
            {
                return "Windows 7";
            }

            return string.Empty;
        }

        private static string NormalizeWhitespace(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return Regex.Replace(value.Trim(), @"\s+", " ");
        }

        private static string ReplaceFirst(string value, string oldValue, string newValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var index = value.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);

            return index < 0 ? value : value.Substring(0, index) + newValue + value.Substring(index + oldValue.Length);
        }

        private static string JoinWithSpace(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left))
            {
                return right ?? string.Empty;
            }

            return string.IsNullOrWhiteSpace(right) ? left : $"{left} {right}";
        }

        private static bool Contains(string value, string expected)
        {
            return !string.IsNullOrWhiteSpace(value)
                   && !string.IsNullOrWhiteSpace(expected)
                   && value.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    internal static class WindowsVersionInfoProvider
    {
        private const string CurrentVersionRegistryPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        private static readonly Lazy<WindowsVersionInfo> CurrentInfo = new Lazy<WindowsVersionInfo>(ReadCurrent);

        public static WindowsVersionInfo Current => CurrentInfo.Value;

        private static WindowsVersionInfo ReadCurrent()
        {
            var productName = string.Empty;
            var displayVersion = string.Empty;
            var releaseId = string.Empty;
            var registryBuildNumber = 0;
            var updateBuildRevision = -1;

            try
            {
                var registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default;

                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
                using (var versionKey = baseKey.OpenSubKey(CurrentVersionRegistryPath, false))
                {
                    productName = ReadString(versionKey, "ProductName");
                    displayVersion = ReadString(versionKey, "DisplayVersion");
                    releaseId = ReadString(versionKey, "ReleaseId");
                    registryBuildNumber = ReadInteger(versionKey, "CurrentBuildNumber", 0);
                    updateBuildRevision = ReadInteger(versionKey, "UBR", -1);
                }
            }
            catch
            {
                //OS 資訊不得影響 JasonQuery 啟動或錯誤視窗顯示
            }

            var actualVersion = TryGetActualWindowsVersion();

            if (actualVersion?.IsServer == true
                && productName.IndexOf("Server", StringComparison.OrdinalIgnoreCase) < 0)
            {
                productName = ResolveServerProductName(actualVersion.BuildNumber);
            }

            var majorVersion = actualVersion?.Major ?? 0;
            var minorVersion = actualVersion?.Minor ?? 0;
            var buildNumber = actualVersion?.BuildNumber > 0 ? actualVersion.BuildNumber : registryBuildNumber;
            var fallbackDescription = Environment.OSVersion.VersionString;

            return WindowsVersionInfo.Create(productName, displayVersion, releaseId, majorVersion, minorVersion, buildNumber, updateBuildRevision, fallbackDescription);
        }

        private static string ReadString(RegistryKey key, string valueName)
        {
            return Convert.ToString(key?.GetValue(valueName), CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static int ReadInteger(RegistryKey key, string valueName, int fallbackValue)
        {
            var value = key?.GetValue(valueName);

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue && longValue >= int.MinValue && longValue <= int.MaxValue)
            {
                return (int)longValue;
            }

            return int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue)
                   ? parsedValue
                   : fallbackValue;
        }

        private static ActualWindowsVersion TryGetActualWindowsVersion()
        {
            try
            {
                var versionInfo = new RtlOsVersionInfoEx
                {
                    Size = Marshal.SizeOf(typeof(RtlOsVersionInfoEx))
                };

                if (RtlGetVersion(ref versionInfo) == 0 && versionInfo.MajorVersion > 0)
                {
                    return new ActualWindowsVersion
                    {
                        Major = versionInfo.MajorVersion,
                        Minor = versionInfo.MinorVersion,
                        BuildNumber = versionInfo.BuildNumber,
                        IsServer = versionInfo.ProductType != 1
                    };
                }
            }
            catch
            {
                //Registry 與安全 fallback 仍可提供診斷資訊
            }

            return null;
        }

        private static string ResolveServerProductName(int buildNumber)
        {
            if (buildNumber >= 26100)
            {
                return "Windows Server 2025";
            }

            if (buildNumber >= 20348)
            {
                return "Windows Server 2022";
            }

            if (buildNumber >= 17763)
            {
                return "Windows Server 2019";
            }

            if (buildNumber >= 14393)
            {
                return "Windows Server 2016";
            }

            if (buildNumber >= 9600)
            {
                return "Windows Server 2012 R2";
            }

            if (buildNumber >= 9200)
            {
                return "Windows Server 2012";
            }

            if (buildNumber >= 7601)
            {
                return "Windows Server 2008 R2";
            }

            return "Windows Server";
        }

        [DllImport("ntdll.dll", CharSet = CharSet.Unicode)]
        private static extern int RtlGetVersion(ref RtlOsVersionInfoEx versionInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct RtlOsVersionInfoEx
        {
            public int Size;
            public int MajorVersion;
            public int MinorVersion;
            public int BuildNumber;
            public int PlatformId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string ServicePack;

            public ushort ServicePackMajor;
            public ushort ServicePackMinor;
            public ushort SuiteMask;
            public byte ProductType;
            public byte Reserved;
        }

        private sealed class ActualWindowsVersion
        {
            public int Major { get; set; }
            public int Minor { get; set; }
            public int BuildNumber { get; set; }
            public bool IsServer { get; set; }
        }
    }
}
