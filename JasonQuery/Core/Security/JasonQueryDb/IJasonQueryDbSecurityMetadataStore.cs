using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public interface IJasonQueryDbSecurityMetadataStore
    {
        string MetadataFilePath { get; }

        bool Exists { get; }

        JasonQueryDbSecurityMetadata Load();

        void Save(JasonQueryDbSecurityMetadata metadata);

        void Delete();
    }
}
