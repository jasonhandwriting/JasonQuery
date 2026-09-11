namespace JasonQuery.Core.Security.Database
{
    /// <summary>
    /// SQLite storage classes supported by the versioned binary migration transport.
    /// </summary>
    public enum DatabaseStorageMigrationValueKind : byte
    {
        Null = 0,
        Int64 = 1,
        Double = 2,
        TextUtf8 = 3,
        Blob = 4
    }
}
