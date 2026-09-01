using System;
using System.Security.Cryptography;

namespace JasonQuery.Core.Security.Database
{
    public sealed class DpapiDatabaseKeyProtector : IDatabaseKeyProtector
    {
        public byte[] Protect(byte[] databaseKey)
        {
            if (databaseKey == null || databaseKey.Length == 0)
            {
                throw new ArgumentException("A database key is required.", nameof(databaseKey));
            }

            return ProtectedData.Protect(databaseKey, DatabaseSecurityConstants.DpapiOptionalEntropy, DataProtectionScope.CurrentUser);
        }

        public byte[] Unprotect(byte[] protectedDatabaseKey)
        {
            if (protectedDatabaseKey == null || protectedDatabaseKey.Length == 0)
            {
                throw new ArgumentException("A protected database key is required.", nameof(protectedDatabaseKey));
            }

            return ProtectedData.Unprotect(protectedDatabaseKey, DatabaseSecurityConstants.DpapiOptionalEntropy, DataProtectionScope.CurrentUser);
        }
    }
}
