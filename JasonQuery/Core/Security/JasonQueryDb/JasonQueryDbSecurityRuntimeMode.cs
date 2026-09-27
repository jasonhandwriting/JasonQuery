namespace JasonQuery.Core.Security.JasonQueryDb
{
    public enum JasonQueryDbSecurityRuntimeMode
    {
        Uninitialized = 0,
        LegacyDefault = 1,
        LegacyCustom = 2,
        WindowsCurrentUser = 3,
        CustomPassword = 4
    }
}
