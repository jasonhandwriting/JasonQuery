namespace JasonQuery.Core.Security.JasonQueryDb
{
    public enum JasonQueryDbSecurityStartupState
    {
        DatabaseMissing,
        Legacy,
        V2Ready,
        V2CustomPasswordRequired,
        V2RecoveryRequired
    }
}
