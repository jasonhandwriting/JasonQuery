using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbRecoveryCryptographicContractsTests
    {
        [TestMethod]
        public void Generate_ReturnsCanonical32ByteRecoveryKey()
        {
            using (var key = JasonQueryDbRecoveryKeyCodec.Generate())
            {
                var secret = key.CopySecret();

                try
                {
                    Assert.AreEqual(JasonQueryDbRecoveryConstants.RecoveryKeyIdHexLength, key.KeyId.Length);
                    Assert.HasCount(JasonQueryDbRecoveryConstants.RecoveryKeySizeBytes, secret);
                    Assert.AreEqual(JasonQueryDbRecoveryConstants.RecoveryKeyEncodedLength, JasonQueryDbRecoveryKeyCodec.Encode(key).Length);
                }
                finally
                {
                    Array.Clear(secret, 0, secret.Length);
                }
            }
        }

        [TestMethod]
        public void Generate_ReturnsDistinctRecoveryKeys()
        {
            using (var first = JasonQueryDbRecoveryKeyCodec.Generate())
            using (var second = JasonQueryDbRecoveryKeyCodec.Generate())
            {
                Assert.AreNotEqual(JasonQueryDbRecoveryKeyCodec.Encode(first), JasonQueryDbRecoveryKeyCodec.Encode(second));
            }
        }

        [TestMethod]
        public void Codec_RoundTripsRecoveryKey()
        {
            using (var original = CreateRecoveryKey("A1B2C3D4", 17))
            {
                var encoded = JasonQueryDbRecoveryKeyCodec.Encode(original);

                using (var decoded = JasonQueryDbRecoveryKeyCodec.Decode(encoded))
                {
                    var originalSecret = original.CopySecret();
                    var decodedSecret = decoded.CopySecret();

                    try
                    {
                        Assert.AreEqual(original.KeyId, decoded.KeyId);
                        CollectionAssert.AreEqual(originalSecret, decodedSecret);
                    }
                    finally
                    {
                        Array.Clear(originalSecret, 0, originalSecret.Length);
                        Array.Clear(decodedSecret, 0, decodedSecret.Length);
                    }
                }
            }
        }

        [TestMethod]
        public void Codec_RejectsModifiedChecksum()
        {
            using (var key = CreateRecoveryKey("A1B2C3D4", 21))
            {
                var encoded = JasonQueryDbRecoveryKeyCodec.Encode(key);
                var replacement = encoded[encoded.Length - 1] == 'A' ? 'B' : 'A';
                var tampered = encoded.Substring(0, encoded.Length - 1) + replacement;

                Assert.ThrowsExactly<FormatException>(() => JasonQueryDbRecoveryKeyCodec.Decode(tampered));
            }
        }

        [TestMethod]
        public void Codec_RejectsInvalidPrefix()
        {
            using (var key = CreateRecoveryKey("A1B2C3D4", 29))
            {
                var encoded = JasonQueryDbRecoveryKeyCodec.Encode(key);
                var tampered = "JQ2" + encoded.Substring(3);

                Assert.ThrowsExactly<FormatException>(() => JasonQueryDbRecoveryKeyCodec.Decode(tampered));
            }
        }

        [TestMethod]
        public void Codec_RejectsInvalidKeyId()
        {
            using (var key = CreateRecoveryKey("A1B2C3D4", 37))
            {
                var encoded = JasonQueryDbRecoveryKeyCodec.Encode(key);
                var tampered = encoded.Substring(0, 4) + "a1b2c3d4" + encoded.Substring(12);

                Assert.ThrowsExactly<FormatException>(() => JasonQueryDbRecoveryKeyCodec.Decode(tampered));
            }
        }

        [TestMethod]
        public void Codec_RejectsMalformedLength()
        {
            Assert.ThrowsExactly<FormatException>(() => JasonQueryDbRecoveryKeyCodec.Decode("JQ1-TOO-SHORT"));
        }

        [TestMethod]
        public void ProtectUnprotect_RoundTripsSameLogicalDatabaseKey()
        {
            var databaseKey = CreateDatabaseKey();

            try
            {
                using (var recoveryKey = CreateRecoveryKey("A1B2C3D4", 41))
                {
                    var envelope = JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, recoveryKey);
                    var recovered = JasonQueryDbRecoveryEnvelopeProtector.Unprotect(envelope, recoveryKey);

                    try
                    {
                        CollectionAssert.AreEqual(databaseKey, recovered);
                    }
                    finally
                    {
                        Array.Clear(recovered, 0, recovered.Length);
                    }
                }
            }
            finally
            {
                Array.Clear(databaseKey, 0, databaseKey.Length);
            }
        }

        [TestMethod]
        public void Protect_UsesFrozenAlgorithmsAndLengths()
        {
            var databaseKey = CreateDatabaseKey();

            try
            {
                using (var recoveryKey = CreateRecoveryKey("A1B2C3D4", 43))
                {
                    var envelope = JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, recoveryKey);
                    var salt = envelope.CopySalt();
                    var iv = envelope.CopyIv();
                    var wrapped = envelope.CopyWrappedDatabaseKey();
                    var tag = envelope.CopyAuthenticationTag();

                    try
                    {
                        Assert.AreEqual(JasonQueryDbRecoveryConstants.RecoveryEnvelopeVersion, envelope.Version);
                        Assert.AreEqual(JasonQueryDbRecoveryConstants.HkdfSha256, envelope.Kdf);
                        Assert.AreEqual(JasonQueryDbRecoveryConstants.Aes256Cbc, envelope.Cipher);
                        Assert.AreEqual(JasonQueryDbRecoveryConstants.HmacSha256, envelope.Mac);
                        Assert.HasCount(JasonQueryDbRecoveryConstants.HkdfSaltSizeBytes, salt);
                        Assert.HasCount(JasonQueryDbRecoveryConstants.AesIvSizeBytes, iv);
                        Assert.HasCount(JasonQueryDbRecoveryConstants.WrappedDatabaseKeySizeBytes, wrapped);
                        Assert.HasCount(JasonQueryDbRecoveryConstants.AuthenticationTagSizeBytes, tag);
                    }
                    finally
                    {
                        Array.Clear(salt, 0, salt.Length);
                        Array.Clear(iv, 0, iv.Length);
                        Array.Clear(wrapped, 0, wrapped.Length);
                        Array.Clear(tag, 0, tag.Length);
                    }
                }
            }
            finally
            {
                Array.Clear(databaseKey, 0, databaseKey.Length);
            }
        }

        [TestMethod]
        public void Protect_RandomizesEnvelopeForSameDatabaseAndRecoveryKey()
        {
            var databaseKey = CreateDatabaseKey();

            try
            {
                using (var recoveryKey = CreateRecoveryKey("A1B2C3D4", 47))
                {
                    var first = JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, recoveryKey);
                    var second = JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, recoveryKey);
                    var firstSalt = first.CopySalt();
                    var secondSalt = second.CopySalt();
                    var firstIv = first.CopyIv();
                    var secondIv = second.CopyIv();

                    try
                    {
                        Assert.IsFalse(BytesEqual(firstSalt, secondSalt));
                        Assert.IsFalse(BytesEqual(firstIv, secondIv));
                    }
                    finally
                    {
                        Array.Clear(firstSalt, 0, firstSalt.Length);
                        Array.Clear(secondSalt, 0, secondSalt.Length);
                        Array.Clear(firstIv, 0, firstIv.Length);
                        Array.Clear(secondIv, 0, secondIv.Length);
                    }
                }
            }
            finally
            {
                Array.Clear(databaseKey, 0, databaseKey.Length);
            }
        }

        [TestMethod]
        public void Unprotect_RejectsWrongRecoverySecret()
        {
            var databaseKey = CreateDatabaseKey();

            try
            {
                using (var correct = CreateRecoveryKey("A1B2C3D4", 53))
                using (var wrong = CreateRecoveryKey("A1B2C3D4", 59))
                {
                    var envelope = JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, correct);

                    Assert.ThrowsExactly<CryptographicException>
                    (
                        () => JasonQueryDbRecoveryEnvelopeProtector.Unprotect(envelope, wrong)
                    );
                }
            }
            finally
            {
                Array.Clear(databaseKey, 0, databaseKey.Length);
            }
        }

        [TestMethod]
        public void Unprotect_RejectsKeyIdMismatch()
        {
            var databaseKey = CreateDatabaseKey();

            try
            {
                using (var correct = CreateRecoveryKey("A1B2C3D4", 61))
                using (var wrongId = CreateRecoveryKey("11223344", 61))
                {
                    var envelope = JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, correct);

                    Assert.ThrowsExactly<CryptographicException>
                    (
                        () => JasonQueryDbRecoveryEnvelopeProtector.Unprotect(envelope, wrongId)
                    );
                }
            }
            finally
            {
                Array.Clear(databaseKey, 0, databaseKey.Length);
            }
        }

        [TestMethod]
        public void Unprotect_RejectsTamperedCiphertextBeforeDecrypt()
        {
            var databaseKey = CreateDatabaseKey();

            try
            {
                using (var recoveryKey = CreateRecoveryKey("A1B2C3D4", 67))
                {
                    var envelope = JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, recoveryKey);
                    var tamperedWrapped = envelope.CopyWrappedDatabaseKey();

                    try
                    {
                        tamperedWrapped[0] ^= 0x01;

                        var tampered = CloneEnvelope(envelope, wrappedDatabaseKey: tamperedWrapped);

                        Assert.ThrowsExactly<CryptographicException>
                        (
                            () => JasonQueryDbRecoveryEnvelopeProtector.Unprotect(tampered, recoveryKey)
                        );
                    }
                    finally
                    {
                        Array.Clear(tamperedWrapped, 0, tamperedWrapped.Length);
                    }
                }
            }
            finally
            {
                Array.Clear(databaseKey, 0, databaseKey.Length);
            }
        }

        [TestMethod]
        public void Unprotect_RejectsTamperedAuthenticationTag()
        {
            var databaseKey = CreateDatabaseKey();

            try
            {
                using (var recoveryKey = CreateRecoveryKey("A1B2C3D4", 71))
                {
                    var envelope = JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, recoveryKey);
                    var tamperedTag = envelope.CopyAuthenticationTag();

                    try
                    {
                        tamperedTag[tamperedTag.Length - 1] ^= 0x01;

                        var tampered = CloneEnvelope(envelope, authenticationTag: tamperedTag);

                        Assert.ThrowsExactly<CryptographicException>
                        (
                            () => JasonQueryDbRecoveryEnvelopeProtector.Unprotect(tampered, recoveryKey)
                        );
                    }
                    finally
                    {
                        Array.Clear(tamperedTag, 0, tamperedTag.Length);
                    }
                }
            }
            finally
            {
                Array.Clear(databaseKey, 0, databaseKey.Length);
            }
        }

        [TestMethod]
        public void Envelope_RejectsUnknownVersion()
        {
            using (var recoveryKey = CreateRecoveryKey("A1B2C3D4", 73))
            {
                var databaseKey = CreateDatabaseKey();

                try
                {
                    var envelope = JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, recoveryKey);

                    Assert.ThrowsExactly<InvalidDataException>
                    (
                        () => CloneEnvelope(envelope, version: 2)
                    );
                }
                finally
                {
                    Array.Clear(databaseKey, 0, databaseKey.Length);
                }
            }
        }

        [TestMethod]
        public void Envelope_RejectsUnknownAlgorithm()
        {
            using (var recoveryKey = CreateRecoveryKey("A1B2C3D4", 79))
            {
                var databaseKey = CreateDatabaseKey();

                try
                {
                    var envelope = JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, recoveryKey);

                    Assert.ThrowsExactly<InvalidDataException>
                    (
                        () => CloneEnvelope(envelope, kdf: "UNKNOWN-KDF")
                    );
                }
                finally
                {
                    Array.Clear(databaseKey, 0, databaseKey.Length);
                }
            }
        }

        [TestMethod]
        public void Protect_RejectsWrongDatabaseKeyLength()
        {
            var invalidKey = new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes - 1];

            Assert.ThrowsExactly<ArgumentException>
            (
                () =>
                {
                    using (var recoveryKey = CreateRecoveryKey("A1B2C3D4", 83))
                    {
                        JasonQueryDbRecoveryEnvelopeProtector.Protect(invalidKey, recoveryKey);
                    }
                }
            );
        }

        [TestMethod]
        public void DisposedRecoveryKeyMaterial_RejectsSecretCopy()
        {
            var secret = CreateSecret(89);
            var material = new JasonQueryDbRecoveryKeyMaterial("A1B2C3D4", secret);

            try
            {
                material.Dispose();

                Assert.ThrowsExactly<ObjectDisposedException>(() => material.CopySecret());
            }
            finally
            {
                material.Dispose();
                Array.Clear(secret, 0, secret.Length);
            }
        }

        private static JasonQueryDbRecoveryKeyMaterial CreateRecoveryKey(string keyId, int seed)
        {
            var secret = CreateSecret(seed);

            try
            {
                return new JasonQueryDbRecoveryKeyMaterial(keyId, secret);
            }
            finally
            {
                Array.Clear(secret, 0, secret.Length);
            }
        }

        private static byte[] CreateSecret(int seed)
        {
            var secret = new byte[JasonQueryDbRecoveryConstants.RecoveryKeySizeBytes];

            for (var index = 0; index < secret.Length; index++)
            {
                secret[index] = (byte)((seed + (index * 17)) & 0xFF);
            }

            return secret;
        }

        private static byte[] CreateDatabaseKey()
        {
            var databaseKey = new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes];

            for (var index = 0; index < databaseKey.Length; index++)
            {
                databaseKey[index] = (byte)(index + 1);
            }

            return databaseKey;
        }

        private static JasonQueryDbRecoveryEnvelope CloneEnvelope(JasonQueryDbRecoveryEnvelope source, int? version = null,
                                                                  string keyId = null, string kdf = null, string cipher = null,
                                                                  string mac = null, byte[] salt = null, byte[] iv = null,
                                                                  byte[] wrappedDatabaseKey = null, byte[] authenticationTag = null)
        {
            var copiedSalt = salt ?? source.CopySalt();
            var copiedIv = iv ?? source.CopyIv();
            var copiedWrapped = wrappedDatabaseKey ?? source.CopyWrappedDatabaseKey();
            var copiedTag = authenticationTag ?? source.CopyAuthenticationTag();

            try
            {
                return new JasonQueryDbRecoveryEnvelope
                (
                    version ?? source.Version,
                    keyId ?? source.KeyId,
                    kdf ?? source.Kdf,
                    cipher ?? source.Cipher,
                    mac ?? source.Mac,
                    copiedSalt,
                    copiedIv,
                    copiedWrapped,
                    copiedTag
                );
            }
            finally
            {
                if (salt == null)
                {
                    Array.Clear(copiedSalt, 0, copiedSalt.Length);
                }

                if (iv == null)
                {
                    Array.Clear(copiedIv, 0, copiedIv.Length);
                }

                if (wrappedDatabaseKey == null)
                {
                    Array.Clear(copiedWrapped, 0, copiedWrapped.Length);
                }

                if (authenticationTag == null)
                {
                    Array.Clear(copiedTag, 0, copiedTag.Length);
                }
            }
        }

        private static bool BytesEqual(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            for (var index = 0; index < left.Length; index++)
            {
                if (left[index] != right[index])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
