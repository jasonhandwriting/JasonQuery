namespace JasonQuery.Core.Database.DdlPreview
{
    internal sealed class DdlExecutionCompletionDecision
    {
        public bool Succeeded { get; set; }

        public DdlMainFormNotificationKind NotificationKind { get; set; }
    }
}