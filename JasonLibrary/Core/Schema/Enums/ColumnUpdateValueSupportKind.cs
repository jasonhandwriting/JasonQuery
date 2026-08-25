namespace JasonLibrary.Core.Schema.Enums
{
    public enum ColumnUpdateValueSupportKind
    {
        Supported = 0,

        UnknownDataType = 10,
        ProviderFallback = 20,

        ArrayType = 30,

        BinaryType = 40,
        LargeBinaryType = 41,
        LargeTextType = 42,

        RowVersionOrTimestamp = 60,
        IntervalType = 61,
        IdentityOrGenerated = 62,

        ComputedColumn = 80,

        DatabaseSpecificUnsupported = 90
    }
}
