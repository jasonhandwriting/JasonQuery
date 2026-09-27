using System;
using System.Security.Cryptography;
using System.Text;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    internal static class JasonQueryDbRecoveryHkdf
    {
        public static byte[] DeriveSha256(byte[] inputKeyMaterial, byte[] salt, string info)
        {
            if (inputKeyMaterial == null || inputKeyMaterial.Length == 0)
            {
                throw new ArgumentException("Input key material is required.", nameof(inputKeyMaterial));
            }

            if (salt == null || salt.Length == 0)
            {
                throw new ArgumentException("HKDF salt is required.", nameof(salt));
            }

            if (string.IsNullOrEmpty(info))
            {
                throw new ArgumentException("HKDF info is required.", nameof(info));
            }

            byte[] pseudoRandomKey = null;
            byte[] infoBytes = null;
            byte[] expandInput = null;

            try
            {
                using (var extract = new HMACSHA256(salt))
                {
                    pseudoRandomKey = extract.ComputeHash(inputKeyMaterial);
                }

                infoBytes = Encoding.ASCII.GetBytes(info);
                expandInput = new byte[infoBytes.Length + 1];
                Buffer.BlockCopy(infoBytes, 0, expandInput, 0, infoBytes.Length);
                expandInput[expandInput.Length - 1] = 0x01;

                using (var expand = new HMACSHA256(pseudoRandomKey))
                {
                    return expand.ComputeHash(expandInput);
                }
            }
            finally
            {
                if (pseudoRandomKey != null)
                {
                    Array.Clear(pseudoRandomKey, 0, pseudoRandomKey.Length);
                }

                if (infoBytes != null)
                {
                    Array.Clear(infoBytes, 0, infoBytes.Length);
                }

                if (expandInput != null)
                {
                    Array.Clear(expandInput, 0, expandInput.Length);
                }
            }
        }
    }
}
