namespace JasonQuery.Core.Database.DmlPreview
{
    internal enum DmlPreviewWhereConditionFailureKind
    {
        None = 0,
        InvalidRequest = 1,
        OriginalRowNotFound = 2,
        PrimaryKeyMetadataMissing = 3,
        PrimaryKeyColumnNameMissing = 4,
        PrimaryKeyColumnInfoMissing = 5,
        PrimaryKeyValueColumnMissing = 6,
        DuplicatePrimaryKeyColumn = 7,
        PrimaryKeyLiteralFormattingFailed = 8,
        PhysicalRowIdentifierNotSupported = 9,
        PhysicalRowIdentifierMissing = 10
    }
}
