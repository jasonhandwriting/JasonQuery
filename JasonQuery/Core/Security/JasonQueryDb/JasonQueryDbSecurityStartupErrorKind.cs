using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public enum JasonQueryDbSecurityStartupErrorKind
    {
        GeneralSecurityFailure = 0,
        MissingSecurityInformationOrLegacyCustomPassword = 1,
        WindowsCurrentUserKeyUnavailable = 2
    }
}
