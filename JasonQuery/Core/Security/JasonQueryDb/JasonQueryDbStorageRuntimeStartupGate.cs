using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Prevents normal startup from entering the legacy SQLite runtime after
    /// durable physical migration has produced Storage V2.
    ///
    /// Step389F-R4E intentionally keeps JasonQueryDbStorageFormatContract.CurrentVersion
    /// on Legacy V1. Storage V2 is recognized, but normal runtime use remains blocked
    /// until the dedicated runtime-routing cutover is implemented.
    /// </summary>
    internal static class JasonQueryDbStorageRuntimeStartupGate
    {
        public static void EnsureNormalRuntimeReady(IJasonQueryDbSecurityMetadataStore metadataStore)
        {
            if (metadataStore == null)
            {
                throw new ArgumentNullException(nameof(metadataStore));
            }

            if (!metadataStore.Exists)
            {
                return;
            }

            var metadata = metadataStore.Load();

            JasonQueryDbStorageFormatContract.EnsureCurrentReady
            (
                metadata.StorageFormatVersion
            );
        }
    }
}
