using System;

namespace JasonQuery.Core.Security.Database
{
    public interface IDatabaseSecurityMetadataStore
    {
        string MetadataFilePath { get; }

        bool Exists { get; }

        DatabaseSecurityMetadata Load();

        void Save(DatabaseSecurityMetadata metadata);

        void Delete();
    }
}
