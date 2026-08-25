using System;

namespace JasonQuery.Infrastructure.Exceptions
{
    public static class ExceptionHelper
    {
        public static NotSupportedException NotSupportedDatabase(object databaseType)
        {
            return new NotSupportedException($"The database type '{databaseType}' is not supported.");
        }
    }
}
