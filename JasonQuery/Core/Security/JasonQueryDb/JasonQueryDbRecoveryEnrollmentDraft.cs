using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbRecoveryEnrollmentDraft : IDisposable
    {
        private string _recoveryKeyText;

        internal JasonQueryDbRecoveryEnrollmentDraft(Guid managerId, JasonQueryDbSecurityMetadata sourceMetadata,
                                                     JasonQueryDbSecurityMetadata targetMetadata, string recoveryKeyText)
        {
            if (sourceMetadata == null)
            {
                throw new ArgumentNullException(nameof(sourceMetadata));
            }

            if (targetMetadata == null)
            {
                throw new ArgumentNullException(nameof(targetMetadata));
            }

            if (string.IsNullOrWhiteSpace(recoveryKeyText))
            {
                throw new ArgumentException("A recovery key is required.", nameof(recoveryKeyText));
            }

            ManagerId = managerId;
            SourceMetadata = sourceMetadata;
            TargetMetadata = targetMetadata;
            _recoveryKeyText = recoveryKeyText;
        }

        public string RecoveryKeyText
        {
            get
            {
                ThrowIfDisposed();
                return _recoveryKeyText;
            }
        }

        public string KeyId
        {
            get
            {
                ThrowIfDisposed();
                return TargetMetadata.Recovery.KeyId;
            }
        }

        internal Guid ManagerId { get; }

        internal JasonQueryDbSecurityMetadata SourceMetadata { get; }

        internal JasonQueryDbSecurityMetadata TargetMetadata { get; }

        internal bool IsDisposed => _recoveryKeyText == null;

        public void Dispose()
        {
            _recoveryKeyText = null;
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(JasonQueryDbRecoveryEnrollmentDraft));
            }
        }
    }
}
