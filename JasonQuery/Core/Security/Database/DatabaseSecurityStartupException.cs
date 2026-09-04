using System;

namespace JasonQuery.Core.Security.Database
{
    public sealed class DatabaseSecurityStartupException : Exception
    {
        public DatabaseSecurityStartupException(DatabaseSecurityStartupErrorKind errorKind, string message) : base(message)
        {
            ErrorKind = errorKind;
        }

        public DatabaseSecurityStartupException(DatabaseSecurityStartupErrorKind errorKind, string message, Exception innerException) : base(message, innerException)
        {
            ErrorKind = errorKind;
        }

        public DatabaseSecurityStartupErrorKind ErrorKind { get; }
    }
}
