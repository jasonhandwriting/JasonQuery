using System;

namespace JasonQuery.Core.Security.Database
{
    public enum DatabaseSecurityStartupErrorKind
    {
        GeneralSecurityFailure = 0,
        MissingSecurityInformationOrLegacyCustomPassword = 1,
        WindowsCurrentUserKeyUnavailable = 2
    }
}
