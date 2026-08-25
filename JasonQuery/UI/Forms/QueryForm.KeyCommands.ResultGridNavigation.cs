using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private bool TryHandleResultGridProcessCmdKey(Keys keyData)
        {
            if (!c1TrueDBGrid1.Focused)
            {
                return false;
            }

            var rowCount = c1TrueDBGrid1.Splits[_splitsIndex].Rows.Count;

            switch (keyData)
            {
                case Keys.Control | Keys.Home:
                    {
                        if (rowCount <= 0)
                        {
                            return true;
                        }

                        c1TrueDBGrid1.Row = 0;
                        c1TrueDBGrid1.Select();
                        return true;
                    }
                case Keys.Control | Keys.End:
                    {
                        if (rowCount <= 0)
                        {
                            return true;
                        }

                        c1TrueDBGrid1.Row = rowCount - 1;
                        c1TrueDBGrid1.Select();

                        if (btnNextPage.Enabled)
                        {
                            NextPage();
                        }

                        _isCtrlKeyDown = false;
                        return true;
                    }
                case Keys.Down:
                    {
                        if (rowCount <= 0)
                        {
                            return false;
                        }

                        if (btnNextPage.Enabled && c1TrueDBGrid1.Row == rowCount - 1)
                        {
                            NextPage(c1TrueDBGrid1.Col, rowCount - 1);
                            return true;
                        }

                        return false;
                    }
                case Keys.PageDown:
                    {
                        if (rowCount <= 0)
                        {
                            return false;
                        }

                        if (btnNextPage.Enabled && c1TrueDBGrid1.Row == rowCount - 1)
                        {
                            NextPage(c1TrueDBGrid1.Col, rowCount - 1);
                            return true;
                        }

                        return false;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
