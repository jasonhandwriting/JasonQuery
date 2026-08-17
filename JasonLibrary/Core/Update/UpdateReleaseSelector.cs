using System;
using System.Collections.Generic;
using System.Linq;

namespace JasonLibrary.Core.Update
{
    public sealed class UpdateReleaseSelection
    {
        public UpdateReleaseSelection(string version, UpdateChannel channel, UpdateReleaseMetadata release, UpdateAssetMetadata asset)
        {
            Version = version;
            Channel = channel;
            Release = release;
            Asset = asset;
        }

        public string Version { get; }

        public UpdateChannel Channel { get; }

        public UpdateReleaseMetadata Release { get; }

        public UpdateAssetMetadata Asset { get; }
    }

    public sealed class UpdateReleaseSelector
    {
        private readonly UpdateVersionComparer _versionComparer = new UpdateVersionComparer();

        public UpdateReleaseSelection FindUpdate(UpdateMetadataManifest manifest, string localVersion, UpdateChannel installedChannel)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            if (!_versionComparer.TryNormalize(localVersion, out _))
            {
                throw new FormatException($"Invalid local version: {localVersion}");
            }

            UpdateReleaseSelection best = null;

            foreach (var release in manifest.Releases ?? new List<UpdateReleaseMetadata>())
            {
                if (!IsEligibleRelease(release, installedChannel) || !_versionComparer.TryNormalize(release.TagName, out var version))
                {
                    continue;
                }

                if (_versionComparer.Compare(version, localVersion) <= 0)
                {
                    continue;
                }

                var asset = FindPackageAsset(release);

                if (asset == null)
                {
                    continue;
                }

                var channel = release.Prerelease ? UpdateChannel.Test : UpdateChannel.Production;
                var candidate = new UpdateReleaseSelection(version, channel, release, asset);

                if (best == null || IsBetterCandidate(candidate, best))
                {
                    best = candidate;
                }
            }

            return best;
        }

        public UpdateAssetMetadata FindPackageAsset(UpdateReleaseMetadata release)
        {
            if (release == null)
            {
                return null;
            }

            var expectedName = release.Prerelease ? UpdateMetadataSettingsContract.TestPackageFileName : UpdateMetadataSettingsContract.ProductionPackageFileName;

            return (release.Assets ?? new List<UpdateAssetMetadata>()).FirstOrDefault
            (
                asset => asset != null
                         && string.Equals(asset.Name, expectedName, StringComparison.OrdinalIgnoreCase)
                         && (string.IsNullOrWhiteSpace(asset.State) || string.Equals(asset.State, "uploaded", StringComparison.OrdinalIgnoreCase))
                         && asset.Size > 0 && UpdateDigestValidator.TryGetSha256(asset.Digest, out _)
            );
        }

        private static bool IsEligibleRelease(UpdateReleaseMetadata release, UpdateChannel installedChannel)
        {
            if (release == null || release.Draft)
            {
                return false;
            }

            return installedChannel == UpdateChannel.Test || !release.Prerelease;
        }

        private bool IsBetterCandidate(UpdateReleaseSelection candidate, UpdateReleaseSelection current)
        {
            var comparison = _versionComparer.Compare(candidate.Version, current.Version);

            if (comparison != 0)
            {
                return comparison > 0;
            }

            return candidate.Channel == UpdateChannel.Production && current.Channel == UpdateChannel.Test;
        }
    }
}