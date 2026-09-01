namespace JasonQuery.Core.Security.Database
{
    public enum DatabaseSecurityStartupState
    {
        DatabaseMissing,
        Legacy,
        V2Ready,
        V2CustomPasswordRequired
    }
}
