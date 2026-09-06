using JasonQuery.Core.Text;

namespace JasonQuery.Core.Security.Legacy
{
    /// <summary>
    /// Compatibility boundary for the historical DBInfo.Password format.
    /// This class preserves the legacy TextEngine behavior until stored
    /// connection credentials are migrated to the current storage model.
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
