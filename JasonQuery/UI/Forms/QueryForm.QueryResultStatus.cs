using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using System.Drawing;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void ApplyQueryCancelStatusIfNeeded()
        {
            if (!MyGlobal.IsProgressCancel)
            {
                return;
            }

            var message = LocalizationHelper.GetLanguageString
            (
                "This operation has been cancelled.",
                "Global",
                "Global",
                "msg",
                "CancelByUser",
                "Text"
            );

            SetFormStatusBarInfo(message, Color.Blue);
        }
    }
}