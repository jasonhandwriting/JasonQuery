namespace JasonQuery.UI.Forms
{
    //B1-B: read-only form-local data signal for the future MainForm-owned F9 grid.
    //F6 working DataTable and dictionary ownership remain unchanged in QueryForm.
    public partial class QueryForm
    {
        internal bool HasSqlNavigatorItemsForMainPanel
        {
            get { return _dtSqlNavigatorTable != null && _dtSqlNavigatorTable.Rows.Count > 0; }
        }
    }
}
