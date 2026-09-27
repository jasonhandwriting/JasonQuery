using System;
using System.Security.Cryptography;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbDpapiKeyProtector : IJasonQueryDbKeyProtector
    {
        public byte[] Protect(byte[] databaseKey)
        {
            if (databaseKey == null || databaseKey.Length == 0)
            {
                throw new ArgumentException("A database key is required.", nameof(databaseKey));
            }

            return ProtectedData.Protect(databaseKey, JasonQueryDbSecurityConstants.DpapiOptionalEntropy, DataProtectionScope.CurrentUser);
        }

        public byte[] Unprotect(byte[] protectedDatabaseKey)
        {
            if (protectedDatabaseKey == null || protectedDatabaseKey.Length == 0)
            {
                throw new ArgumentException("A protected database key is required.", nameof(protectedDatabaseKey));
            }

            return ProtectedData.Unprotect(protectedDatabaseKey, JasonQueryDbSecurityConstants.DpapiOptionalEntropy, DataProtectionScope.CurrentUser);
        }
    }
}
