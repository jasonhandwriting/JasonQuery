using C1.Win.C1TrueDBGrid;
using JasonQuery.UI.QueryEditor.AutoComplete.Support;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void c1GridAutoCompleteForPeriod_KeyDown(object sender, KeyEventArgs e)
        {
            HandleAutoCompleteGridKeyDown(_periodAutoCompleteSession, e);
        }

        private void c1GridAutoCompleteForPeriod_KeyPress(object sender, KeyPressEventArgs e)
        {
            HandleAutoCompleteGridKeyPress(_periodAutoCompleteSession, e);
        }

        private void c1GridAutoCompleteForPeriod_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            HandleAutoCompleteGridMouseDoubleClick(_periodAutoCompleteSession);
        }

        private void c1GridAutoCompleteForSpace_KeyDown(object sender, KeyEventArgs e)
        {
            HandleAutoCompleteGridKeyDown(_spaceAutoCompleteSession, e);
        }

        private void c1GridAutoCompleteForSpace_KeyPress(object sender, KeyPressEventArgs e)
        {
            HandleAutoCompleteGridKeyPress(_spaceAutoCompleteSession, e);
        }

        private void c1GridAutoCompleteForSpace_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            HandleAutoCompleteGridMouseDoubleClick(_spaceAutoCompleteSession);
        }

        private void c1GridAutoCompleteForPeriod_FetchCellStyle(object sender, FetchCellStyleEventArgs e)
        {
            QueryEditorAutoCompleteGridStyleHelper.ApplyFetchCellStyle
            (
                c1GridAutoCompleteForPeriod,
                e,
                highlightDataTypeColumn: true
            );
        }

        private void c1GridAutoCompleteForSpace_FetchCellStyle(object sender, FetchCellStyleEventArgs e)
        {
            QueryEditorAutoCompleteGridStyleHelper.ApplyFetchCellStyle
            (
                c1GridAutoCompleteForSpace,
                e,
                highlightDataTypeColumn: false
            );
        }
    }
}