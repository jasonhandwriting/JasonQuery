namespace JasonQuery.Core.Security.Database
{
    /// <summary>
    /// Durable stage of a physical JasonQuery.db Storage V1 -> V2 migration.
    /// Absence of the journal represents no active/recoverable migration.
    /// </summary>
    public enum DatabaseStorageMigrationStage
    {
        PreparingCandidate = 1,
        CandidateReady = 2,
        ReplacePrepared = 3,
        DatabaseReplaced = 4,
        MetadataCommitted = 5
    }
}
