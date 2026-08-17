namespace JasonQuery.Core.Database.Transactions.LockingQueries
{
    internal enum DatabaseLockingQueryKind
    {
        None = 0,

        //Oracle, PostgreSQL, MySQL
        ForUpdate = 1,
        ForNoKeyUpdate = 2,
        ForShare = 3,
        ForKeyShare = 4,
        LockInShareMode = 5,

        //SQL Server
        SqlServerUpdateLock = 10,
        SqlServerExclusiveLock = 11,
        SqlServerHoldLock = 12,
        SqlServerTableExclusiveLock = 13,
        SqlServerSerializable = 14
    }
}