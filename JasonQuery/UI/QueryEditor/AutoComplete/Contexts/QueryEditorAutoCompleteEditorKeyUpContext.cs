using JasonQuery.UI.QueryEditor.AutoComplete.State;
using System;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Contexts
{
    internal sealed class QueryEditorAutoCompleteEditorKeyUpContext
    {
        public QueryEditorAutoCompleteSession PeriodSession { get; set; }

        public QueryEditorAutoCompleteSession SpaceSession { get; set; }

        public bool IsCtrlJPressed { get; set; }

        public Action ResetCtrlJPressed { get; set; }

        public Action HideAllAutoCompletePopups { get; set; }

        public Action HidePeriodPopup { get; set; }

        public Action HideSpacePopup { get; set; }

        public Action TriggerPeriodAutoComplete { get; set; }

        public Action TriggerSpaceAutoComplete { get; set; }

        public void Validate()
        {
            if (ResetCtrlJPressed == null)
            {
                throw new ArgumentNullException(nameof(ResetCtrlJPressed));
            }

            if (HideAllAutoCompletePopups == null)
            {
                throw new ArgumentNullException(nameof(HideAllAutoCompletePopups));
            }

            if (HidePeriodPopup == null)
            {
                throw new ArgumentNullException(nameof(HidePeriodPopup));
            }

            if (HideSpacePopup == null)
            {
                throw new ArgumentNullException(nameof(HideSpacePopup));
            }

            if (TriggerPeriodAutoComplete == null)
            {
                throw new ArgumentNullException(nameof(TriggerPeriodAutoComplete));
            }

            if (TriggerSpaceAutoComplete == null)
            {
                throw new ArgumentNullException(nameof(TriggerSpaceAutoComplete));
            }
        }
    }
}