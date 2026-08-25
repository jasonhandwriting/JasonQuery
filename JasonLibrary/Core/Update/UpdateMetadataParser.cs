using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace JasonLibrary.Core.Update
{
    public sealed class UpdateMetadataParser
    {
        private const int SupportedSchemaVersion = 1;

        public UpdateMetadataManifest Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new FormatException("Update metadata is empty.");
            }

            var normalizedJson = json.TrimStart('\uFEFF', ' ', '\t', '\r', '\n');

            try
            {
                if (normalizedJson.StartsWith("[", StringComparison.Ordinal))
                {
                    var releases = Deserialize<List<UpdateReleaseMetadata>>(normalizedJson);

                    return new UpdateMetadataManifest
                    {
                        SchemaVersion = SupportedSchemaVersion,
                        Product = "JasonQuery",
                        Releases = releases ?? new List<UpdateReleaseMetadata>()
                    };
                }

                if (!normalizedJson.StartsWith("{", StringComparison.Ordinal))
                {
                    throw new FormatException("Update metadata must be a JSON object or array.");
                }

                var manifest = Deserialize<UpdateMetadataManifest>(normalizedJson);

                if (manifest == null)
                {
                    throw new FormatException("Update metadata could not be parsed.");
                }

                if (manifest.SchemaVersion != SupportedSchemaVersion)
                {
                    throw new NotSupportedException
                    (
                        $"Update metadata schema version {manifest.SchemaVersion} is not supported."
                    );
                }

                if (!string.Equals(manifest.Product, "JasonQuery", StringComparison.OrdinalIgnoreCase))
                {
                    throw new FormatException("Update metadata is not for JasonQuery.");
                }

                manifest.Releases = manifest.Releases ?? new List<UpdateReleaseMetadata>();

                return manifest;
            }
            catch (FormatException)
            {
                throw;
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException("Update metadata contains invalid JSON.", ex);
            }
        }

        private static T Deserialize<T>(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);

            using (var stream = new MemoryStream(bytes))
            {
                var serializer = new DataContractJsonSerializer(typeof(T));

                return (T)serializer.ReadObject(stream);
            }
        }
    }
}
