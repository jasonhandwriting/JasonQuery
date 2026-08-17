namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.State
{
    internal enum QueryEditorAutoCompleteSessionCloseReason
    {
        ExternalHide = 0,
        Escape = 1,
        Commit = 2,
        CommitByTab = 3,
        Navigation = 4,
        InvalidInput = 5,
        NoCandidates = 6,
        RefreshFailed = 7
    }
}