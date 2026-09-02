using System;

namespace JasonQuery.Core.Security.Legacy
{
    public static class LegacyConnectionExportSecurity
    {
        //Historical .jqc archive-password format. It is intentionally isolated from
        //JasonQuery.db security and will be replaced by separate .jqc security work.
        private const string PasswordPrefix = "jasonquery1231";
        private const string PasswordSuffix = "exportDB!nf0";

        public static string CreateArchivePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("A connection-export password is required.", nameof(password));
            }

            return $"{PasswordPrefix}{password}{PasswordSuffix}";
        }
    }
}
