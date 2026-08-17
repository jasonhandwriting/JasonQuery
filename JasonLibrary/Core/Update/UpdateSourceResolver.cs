using System;
using System.IO;

namespace JasonLibrary.Core.Update
{
    public sealed class UpdateContentLocation
    {
        public UpdateContentLocation(string value, bool isLocalFile)
        {
            Value = value;
            IsLocalFile = isLocalFile;
        }

        public string Value { get; }

        public bool IsLocalFile { get; }
    }

    public static class UpdateSourceResolver
    {
        public static UpdateContentLocation ResolveMetadata(UpdateMetadataSourceKind source, string localFolder)
        {
            switch (UpdateMetadataSettingsContract.NormalizeSource(source))
            {
                case UpdateMetadataSourceKind.GitHub:
                    {
                        return new UpdateContentLocation
                        (
                            UpdateMetadataSettingsContract.GitHubReleasesApiUrl,
                            false
                        );
                    }
                case UpdateMetadataSourceKind.LocalFolder:
                    {
                        return new UpdateContentLocation
                        (
                            Path.Combine(RequireLocalFolder(localFolder), UpdateMetadataSettingsContract.MetadataFileName),
                            true
                        );
                    }
                default:
                    {
                        return new UpdateContentLocation
                        (
                            UpdateMetadataSettingsContract.OfficialWebsiteMetadataUrl,
                            false
                        );
                    }
            }
        }

        public static UpdateContentLocation ResolvePackage(UpdateMetadataSourceKind source, string localFolder, UpdateAssetMetadata asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            if (source == UpdateMetadataSourceKind.LocalFolder)
            {
                var fileName = Path.GetFileName(asset.Name);

                if (string.IsNullOrWhiteSpace(fileName) || !string.Equals(fileName, asset.Name, StringComparison.Ordinal))
                {
                    throw new FormatException("The update package name is invalid.");
                }

                return new UpdateContentLocation
                (
                    Path.Combine(RequireLocalFolder(localFolder), fileName),
                    true
                );
            }

            if (!Uri.TryCreate(asset.BrowserDownloadUrl, UriKind.Absolute, out var uri) || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException("The update package URL must use HTTPS.");
            }

            return new UpdateContentLocation(uri.AbsoluteUri, false);
        }

        private static string RequireLocalFolder(string localFolder)
        {
            if (string.IsNullOrWhiteSpace(localFolder))
            {
                throw new ArgumentException("A local update folder is required.", nameof(localFolder));
            }

            return localFolder.Trim();
        }
    }
}