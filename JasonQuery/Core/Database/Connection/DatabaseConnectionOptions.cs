using System;
namespace JasonQuery.Core.Database.Connection
{
    public sealed class DatabaseConnectionOptions
    {
        public bool UseAutoRollback { get; set; } = false;

        public bool UseDirectMode { get; set; } = false;

        public bool UseConnectionPooling { get; set; } = false;

        public bool ExcludeNativeDatabase { get; set; } = true;

        public bool UseUnicode { get; set; } = false;

        public int QueryTimeoutSeconds { get; set; } = 30;

        public void Reset()
        {
            UseAutoRollback = false;
            UseDirectMode = false;
            UseConnectionPooling = false;
            ExcludeNativeDatabase = true;
            UseUnicode = false;
            QueryTimeoutSeconds = 30;
        }
    }
}
