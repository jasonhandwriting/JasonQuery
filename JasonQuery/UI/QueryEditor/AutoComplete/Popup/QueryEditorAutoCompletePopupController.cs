using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using JasonQuery.UI.Helpers;
using System;
using System.Data;
using System.Drawing;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Popup
{
    internal static class QueryEditorAutoCompletePopupController
    {
        private const int DefaultXOffset = -1;
        //Scintilla SCI_TEXTHEIGHT returns the current styled (Zoom-aware) line height.
        private const int ScintillaTextHeightMessage = 2279;

        public static bool Show(C1TrueDBGrid grid, DataTable dtAutoComplete, QueryEditorAutoCompletePopupContext context)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Validate();

            if (dtAutoComplete == null || dtAutoComplete.Rows.Count == 0)
            {
                Hide(grid);
                return false;
            }

            var anchorPosition = ResolveAnchorPosition(context);
            var location = CalculateLocation(grid.Parent ?? context.OwnerForm, context, anchorPosition);

            BindDataSource(grid, dtAutoComplete);
            ApplyPresentation(grid, context);

            if (context.AutoResizePopup)
            {
                Resize(grid, dtAutoComplete.Rows.Count, context);
            }

            grid.Location = location;
            grid.Visible = true;
            grid.BringToFront();

            return true;
        }

        public static void Hide(C1TrueDBGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            QueryEditorAutoCompletePopupCleanupCoordinator.Cleanup
            (
                () => grid.DataSource = null,
                () => grid.Visible = false
            );
        }

        public static Point CalculateLocation(QueryEditorAutoCompletePopupContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Validate();

            var anchorPosition = ResolveAnchorPosition(context);

            return CalculateLocation(context.OwnerForm, context, anchorPosition);
        }

        private static Point CalculateLocation(System.Windows.Forms.Control parent, QueryEditorAutoCompletePopupContext context, int anchorPosition)
        {
            var screenPoint = CalculateScreenPointFromEditorPosition(context, anchorPosition);

            //With Editor.PointToScreen the anchor is already in the editor's actual screen coordinates.
            //The legacy font/zoom Y-shift belonged to the old MainForm-relative formula and moved the popup above the caret.
            var line = context.Editor.LineFromPosition(anchorPosition);
            var lineHeight = context.Editor.DirectMessage(ScintillaTextHeightMessage, new IntPtr(line), IntPtr.Zero).ToInt32();

            return CalculateClientPosition(screenPoint, parent.PointToScreen(Point.Empty), lineHeight);
        }

        //The popup is positioned in its own Parent's client coordinates, not in MainForm coordinates.
        internal static Point CalculateClientPosition(Point caretScreenPoint, Point parentScreenOrigin, int lineHeight)
        {
            //The popup begins below the active Scintilla display line, not above its top edge.
            return new Point(caretScreenPoint.X - parentScreenOrigin.X + DefaultXOffset, caretScreenPoint.Y - parentScreenOrigin.Y + Math.Max(1, lineHeight));
        }

        private static int ResolveAnchorPosition(QueryEditorAutoCompletePopupContext context)
        {
            var position = context.Position >= 0 ? context.Position : context.Editor.CurrentPosition;

            if (position < 0)
            {
                return 0;
            }

            var textLength = context.Editor.TextLength;

            if (position > textLength)
            {
                return textLength;
            }

            return position;
        }

        private static Point CalculateScreenPointFromEditorPosition(QueryEditorAutoCompletePopupContext context, int position)
        {
            return context.Editor.PointToScreen
            (
                new Point
                (
                    context.Editor.PointXFromPosition(position),
                    context.Editor.PointYFromPosition(position)
                )
            );
        }

        private static void BindDataSource(C1TrueDBGrid grid, DataTable dtAutoComplete)
        {
            grid.DataSource = null;
            grid.DataSource = dtAutoComplete;
        }

        private static void ApplyPresentation(C1TrueDBGrid grid, QueryEditorAutoCompletePopupContext context)
        {
            if (grid.Splits.Count == 0)
            {
                return;
            }

            if (context.HideRecordSelectors)
            {
                grid.Splits[0].RecordSelectors = false;
            }

            foreach (C1DisplayColumn col in grid.Splits[0].DisplayColumns)
            {
                col.FetchStyle = false;
            }

            if (context.FetchStyleColumnIndexes == null || context.FetchStyleColumnIndexes.Length == 0)
            {
                return;
            }

            foreach (var columnIndex in context.FetchStyleColumnIndexes)
            {
                if (columnIndex >= 0 && columnIndex < grid.Splits[0].DisplayColumns.Count)
                {
                    grid.Splits[0].DisplayColumns[columnIndex].FetchStyle = true;
                }
            }
        }

        private static void Resize(C1TrueDBGrid grid, int rowCount, QueryEditorAutoCompletePopupContext context)
        {
            var width = GridHelper.ResizeGridColumnWidth(grid, context.GridName);

            if (rowCount <= context.ScrollBarRowThreshold)
            {
                width += context.WidthPaddingWithoutScrollBar;
            }
            else
            {
                width += grid.VScrollBar.Width + context.WidthPaddingWithScrollBar;
            }

            grid.Size = new Size(width, context.PopupHeight);
        }
    }
}
