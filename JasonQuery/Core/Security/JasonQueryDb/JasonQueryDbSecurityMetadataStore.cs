using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbSecurityMetadataStore : IJasonQueryDbSecurityMetadataStore
    {
        public JasonQueryDbSecurityMetadataStore(string metadataFilePath)
        {
            if (string.IsNullOrWhiteSpace(metadataFilePath))
            {
                throw new ArgumentException("A metadata file path is required.", nameof(metadataFilePath));
            }

            MetadataFilePath = Path.GetFullPath(metadataFilePath);
        }

        public string MetadataFilePath { get; }

        public bool Exists => File.Exists(MetadataFilePath);

        public JasonQueryDbSecurityMetadata Load()
        {
            if (!Exists)
            {
                throw new FileNotFoundException("The database security metadata file was not found.", MetadataFilePath);
            }

            var json = File.ReadAllText(MetadataFilePath, Encoding.UTF8);

            JasonQueryDbSecurityMetadata metadata;

            try
            {
                metadata = JsonConvert.DeserializeObject<JasonQueryDbSecurityMetadata>(json);
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("The database security metadata is invalid.", ex);
            }

            if (metadata == null)
            {
                throw new InvalidDataException("The database security metadata is empty.");
            }

            metadata.Validate();
            return metadata;
        }

        public void Save(JasonQueryDbSecurityMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            metadata.Validate();

            var directory = Path.GetDirectoryName(MetadataFilePath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            var temporaryPath = MetadataFilePath + $".{Guid.NewGuid():N}.tmp";

            try
            {
                WriteAllTextDurably(temporaryPath, json);

                if (File.Exists(MetadataFilePath))
                {
                    File.Replace(temporaryPath, MetadataFilePath, null, true);
                }
                else
                {
                    File.Move(temporaryPath, MetadataFilePath);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        public void Delete()
        {
            if (File.Exists(MetadataFilePath))
            {
                File.Delete(MetadataFilePath);
            }
        }

        private static void WriteAllTextDurably(string filePath, string content)
        {
            using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(content);
                writer.Flush();
                stream.Flush(true);
            }
        }
    }
}
