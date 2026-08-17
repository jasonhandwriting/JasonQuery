using System;

namespace JasonLibrary.Core.Update
{
    public static class UpdateMetadataSettingsContract
    {
        public const string SectionName = "GlobalConfig";
        public const string SourceSettingName = "UpdateMetadataSource";
        public const string LocalFolderSettingName = "UpdateMetadataLocalFolder";

        public const string MetadataFileName = "jasonquery-update.json";
        public const string ProductionPackageFileName = "JasonQuery64.zip";
        public const string TestPackageFileName = "JasonQuery64Test.zip";

        public const string OfficialWebsiteMetadataUrl = "https://www.jasonquery.org/JasonQueryUpdate/jasonquery-update.json";

        public const string GitHubReleasesApiUrl = "https://api.github.com/repos/JasonHandwriting/JasonQuery/releases?per_page=20";

        public const string GitHubApiVersion = "2026-03-10";

        public static UpdateMetadataSourceKind DefaultSource => UpdateMetadataSourceKind.OfficialWebsite;

        public static UpdateMetadataSourceKind ParseSource(string value)
        {
            if (Enum.TryParse(value, true, out UpdateMetadataSourceKind source) && Enum.IsDefined(typeof(UpdateMetadataSourceKind), source))
            {
                return source;
            }

            return DefaultSource;
        }

        public static UpdateMetadataSourceKind NormalizeSource(UpdateMetadataSourceKind source)
        {
            return Enum.IsDefined(typeof(UpdateMetadataSourceKind), source) ? source : DefaultSource;
        }
    }
}