using System;
using System.Globalization;

namespace JasonLibrary.Core.Update
{
    public enum UpdateChannel
    {
        Production = 1,
        Test = 2
    }

    public static class UpdateChannelResolver
    {
        public static UpdateChannel Resolve(string version)
        {
            if (!TryResolve(version, out var channel))
            {
                throw new FormatException($"Invalid version: {version}");
            }

            return channel;
        }

        public static bool TryResolve(string version, out UpdateChannel channel)
        {
            channel = UpdateChannel.Production;

            var versionComparer = new UpdateVersionComparer();

            if (!versionComparer.TryNormalize(version, out var normalizedVersion))
            {
                return false;
            }

            var parts = normalizedVersion.Split('.');

            if (parts.Length < 3 || !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var thirdPart))
            {
                return false;
            }

            channel = thirdPart == 0 ? UpdateChannel.Production : UpdateChannel.Test;
            return true;
        }
    }
}
