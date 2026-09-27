namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// SQLite storage classes supported by the versioned binary migration transport.
    /// </summary>
    public enum JasonQueryDbStorageMigrationValueKind : byte
    {
        Null = 0,
        Int64 = 1,
        Double = 2,
        TextUtf8 = 3,
        Blob = 4
    }
}
