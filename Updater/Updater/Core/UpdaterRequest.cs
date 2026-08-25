using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Updater.Core
{
    [DataContract]
    public sealed class UpdaterRequest
    {
        public const int CurrentSchemaVersion = 1;

        [DataMember(Name = "schema_version")]
        public int SchemaVersion { get; set; }

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
                throw new NotSupportedException($"Updater request schema version {SchemaVersion} is not supported.");
            }

            if (!IsKnownSource(SourceKind))
            {
                throw new FormatException("The update source kind is invalid.");
            }

            if (string.IsNullOrWhiteSpace(LocalizationCode))
            {
                throw new FormatException("The localization code is required.");
            }

            if (PackageIsLocal != string.Equals(SourceKind, "LocalFolder", StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException("The update package location type does not match the selected source.");
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

            if (string.IsNullOrWhiteSpace(PackageName)
                || !string.Equals(Path.GetFileName(PackageName), PackageName, StringComparison.Ordinal)
                || !string.Equals(Path.GetExtension(PackageName), ".zip", StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException("The update package name is invalid.");
            }

            if (string.IsNullOrWhiteSpace(PackageLocation))
            {
                throw new FormatException("The update package location is required.");
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

            if (ExpectedSize <= 0)
            {
                throw new FormatException("The expected package size must be greater than zero.");
            }

            if (!Sha256Digest.TryNormalize(ExpectedDigest, out _))
            {
                throw new FormatException("The expected package SHA-256 digest is invalid.");
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

        public static UpdaterRequest Load(string requestPath)
        {
            if (string.IsNullOrWhiteSpace(requestPath) || !File.Exists(requestPath))
            {
                throw new FileNotFoundException("The Updater request file was not found.", requestPath);
            }

            var serializer = new DataContractJsonSerializer(typeof(UpdaterRequest));
            UpdaterRequest request;

            using (var stream = new FileStream(requestPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                request = (UpdaterRequest)serializer.ReadObject(stream);
            }

            if (request == null)
            {
                throw new FormatException("The Updater request file is empty or invalid.");
            }

            request.Validate();

            return request;
        }

        private static bool IsKnownSource(string source)
        {
            return string.Equals(source, "OfficialWebsite", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(source, "GitHub", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(source, "LocalFolder", StringComparison.OrdinalIgnoreCase);
        }
    }
}
