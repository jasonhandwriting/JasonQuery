using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace JasonLibrary.Core.Update
{
    [DataContract]
    public sealed class UpdateExecutionRequest
    {
        public const int CurrentSchemaVersion = 1;

        [DataMember(Name = "schema_version")]
        public int SchemaVersion { get; set; } = CurrentSchemaVersion;

        [DataMember(Name = "localization_code")]
        public string LocalizationCode { get; set; }

        [DataMember(Name = "localization_file")]
        public string LocalizationFile { get; set; }

        [DataMember(Name = "environment")]
        public string Environment { get; set; }

        [DataMember(Name = "source_kind")]
        public string SourceKind { get; set; }

        [DataMember(Name = "package_location")]
        public string PackageLocation { get; set; }

        [DataMember(Name = "package_is_local")]
        public bool PackageIsLocal { get; set; }

        [DataMember(Name = "package_name")]
        public string PackageName { get; set; }

        [DataMember(Name = "expected_size")]
        public long ExpectedSize { get; set; }

        [DataMember(Name = "expected_digest")]
        public string ExpectedDigest { get; set; }

        [DataMember(Name = "installed_version")]
        public string InstalledVersion { get; set; }

        [DataMember(Name = "target_version")]
        public string TargetVersion { get; set; }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
            {
                throw new NotSupportedException($"Update execution request schema version {SchemaVersion} is not supported.");
            }

            if (!Enum.TryParse(SourceKind, true, out UpdateMetadataSourceKind source) || !Enum.IsDefined(typeof(UpdateMetadataSourceKind), source))
            {
                throw new FormatException("The update source kind is invalid.");
            }

            if (string.IsNullOrWhiteSpace(LocalizationCode))
            {
                throw new FormatException("The localization code is required.");
            }

            if (string.IsNullOrWhiteSpace(LocalizationFile)
                || !string.Equals(Path.GetFileName(LocalizationFile), LocalizationFile, StringComparison.Ordinal))
            {
                throw new FormatException("The localization file name is invalid.");
            }

            if (!string.Equals(Environment, "PROD", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(Environment, "TEST", StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException("The update environment is invalid.");
            }

            if (string.IsNullOrWhiteSpace(PackageLocation))
            {
                throw new FormatException("The update package location is required.");
            }

            if (PackageIsLocal != (source == UpdateMetadataSourceKind.LocalFolder))
            {
                throw new FormatException("The update package location type does not match the selected source.");
            }

            if (string.IsNullOrWhiteSpace(PackageName)
                || !string.Equals(Path.GetFileName(PackageName), PackageName, StringComparison.Ordinal)
                || !string.Equals(Path.GetExtension(PackageName), ".zip", StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException("The update package name is invalid.");
            }

            if (ExpectedSize <= 0)
            {
                throw new FormatException("The expected update package size must be greater than zero.");
            }

            if (!UpdateDigestValidator.TryGetSha256(ExpectedDigest, out _))
            {
                throw new FormatException("The expected update package SHA-256 digest is invalid.");
            }

            if (string.IsNullOrWhiteSpace(InstalledVersion) || string.IsNullOrWhiteSpace(TargetVersion))
            {
                throw new FormatException("The installed and target versions are required.");
            }

            if (!TryNormalizeVersion(InstalledVersion, out var installedVersion)
                || !TryNormalizeVersion(TargetVersion, out var targetVersion)
                || targetVersion <= installedVersion)
            {
                throw new FormatException("The target update version must be newer than the installed version.");
            }

            if (PackageIsLocal)
            {
                if (!Path.IsPathRooted(PackageLocation))
                {
                    throw new FormatException("The local update package path must be absolute.");
                }
            }
            else if (!Uri.TryCreate(PackageLocation, UriKind.Absolute, out var uri)
                     || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException("The remote update package URL must use HTTPS.");
            }
        }

        private static bool TryNormalizeVersion(string value, out Version version)
        {
            version = null;

            if (!Version.TryParse(value, out var parsed))
            {
                return false;
            }

            version = new Version
            (
                parsed.Major,
                parsed.Minor,
                Math.Max(0, parsed.Build),
                Math.Max(0, parsed.Revision)
            );

            return true;
        }
    }

    public static class UpdateExecutionRequestFile
    {
        public static string Write(UpdateExecutionRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            request.Validate();

            var requestFolder = Path.Combine(Path.GetTempPath(), "JasonQuery", "UpdaterRequests");

            Directory.CreateDirectory(requestFolder);

            var requestPath = Path.Combine(requestFolder, $"{Guid.NewGuid():N}.json");
            var serializer = new DataContractJsonSerializer(typeof(UpdateExecutionRequest));

            using (var stream = new FileStream(requestPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                serializer.WriteObject(stream, request);
                stream.Flush(true);
            }

            return requestPath;
        }

        public static void TryDelete(string requestPath)
        {
            if (string.IsNullOrWhiteSpace(requestPath))
            {
                return;
            }

            try
            {
                if (File.Exists(requestPath))
                {
                    File.Delete(requestPath);
                }
            }
            catch
            {
                //The Updater will delete a successfully consumed request file.
            }
        }
    }
}