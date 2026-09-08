using JasonQuery.Core.Text;

namespace JasonQuery.Core.Security.Legacy
{
    /// <summary>
    /// Migration-only compatibility boundary for the historical DBInfo.Password
    /// format. V2 runtime save/load paths must not call this class; it remains only
    /// so the one-time legacy credential migrator can recover and validate existing
    /// stored credentials.
    /// </summary>
    public static class LegacyConnectionCredentialSecurity
    {
        public static string Protect(string password, string domainUser)
        {
            return TextEngine.Encode(TextEngine.Encrypt(password, domainUser));
        }

        public static string Unprotect(string protectedPassword, string domainUser)
        {
            return TextEngine.Decrypt(TextEngine.Decode(protectedPassword), domainUser);
        }
    }
}
