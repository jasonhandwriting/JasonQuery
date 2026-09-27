namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Logical Storage V1 -> V2 stream frame kinds.
    ///
    /// Values 1 and 2 remain reserved by JasonQueryDbStorageMigrationWireProtocol
    /// for the existing request/response control channel.
    /// </summary>
    public enum JasonQueryDbStorageMigrationLogicalFrameKind : byte
    {
        BeginDatabase = 3,
        BeginSchema = 4,
        BeginTable = 5,
        Column = 6,
        EndTable = 7,
        EndSchema = 8,
        BeginRows = 9,
        BeginRow = 10,
        NullValue = 11,
        Int64Value = 12,
        DoubleValue = 13,
        BeginTextUtf8 = 14,
        TextUtf8Chunk = 15,
        EndTextUtf8 = 16,
        BeginBlob = 17,
        BlobChunk = 18,
        EndBlob = 19,
        EndRow = 20,
        EndRows = 21,
        BeginSequenceState = 22,
        SequenceEntry = 23,
        EndSequenceState = 24,
        BeginSecondarySchema = 25,
        SchemaObject = 26,
        EndSecondarySchema = 27,
        EndDatabase = 28,
        EndStream = 29
    }
}
