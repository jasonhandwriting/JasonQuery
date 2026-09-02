namespace JasonQuery.Core.Security.Database
{
    public enum DatabaseSecurityRuntimeMode
    {
        Uninitialized = 0,
        LegacyDefault = 1,
        LegacyCustom = 2,
        WindowsCurrentUser = 3,
        CustomPassword = 4
    }
}
