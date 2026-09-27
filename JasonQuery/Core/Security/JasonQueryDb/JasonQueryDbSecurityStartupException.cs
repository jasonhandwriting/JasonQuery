using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public sealed class JasonQueryDbSecurityStartupException : Exception
    {
        public JasonQueryDbSecurityStartupException(JasonQueryDbSecurityStartupErrorKind errorKind, string message) : base(message)
        {
            ErrorKind = errorKind;
        }

        public JasonQueryDbSecurityStartupException(JasonQueryDbSecurityStartupErrorKind errorKind, string message, Exception innerException) : base(message, innerException)
        {
            ErrorKind = errorKind;
        }

        public JasonQueryDbSecurityStartupErrorKind ErrorKind { get; }
    }
}
