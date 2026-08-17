using System.Data;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private DataTable BuildPeriodAutoCompleteData(string condition0)
        {
            return _autoCompleteDataBuilder.BuildPeriodAutoCompleteData
            (
                _dtPeriodAutoCompleteTable,
                c1GridAutoCompleteForPeriod,
                condition0
            );
        }

        private DataTable BuildSpaceAutoCompleteData(string condition0)
        {
            return _autoCompleteDataBuilder.BuildSpaceAutoCompleteData
            (
                _dtSpaceAutoCompleteTable,
                c1GridAutoCompleteForSpace,
                condition0
            );
        }
    }
}