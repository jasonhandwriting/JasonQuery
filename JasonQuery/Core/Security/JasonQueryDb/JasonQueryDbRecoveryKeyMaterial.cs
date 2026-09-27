using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbRecoveryKeyMaterial : IDisposable
    {
        private byte[] _secret;

        public JasonQueryDbRecoveryKeyMaterial(string keyId, byte[] secret)
        {
            JasonQueryDbRecoveryKeyCodec.ValidateKeyId(keyId);

            if (secret == null || secret.Length != JasonQueryDbRecoveryConstants.RecoveryKeySizeBytes)
            {
                throw new ArgumentException
                (
                    $"The recovery secret must contain {JasonQueryDbRecoveryConstants.RecoveryKeySizeBytes} bytes.",
                    nameof(secret)
                );
            }

            KeyId = keyId;
            _secret = (byte[])secret.Clone();
        }

        public string KeyId { get; }

        public byte[] CopySecret()
        {
            ThrowIfDisposed();
            return (byte[])_secret.Clone();
        }

        public void Dispose()
        {
            if (_secret == null)
            {
                return;
            }

            Array.Clear(_secret, 0, _secret.Length);
            _secret = null;
        }

        private void ThrowIfDisposed()
        {
            if (_secret == null)
            {
                throw new ObjectDisposedException(nameof(JasonQueryDbRecoveryKeyMaterial));
            }
        }
    }
}
