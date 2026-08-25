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
        private const int DefaultXOffset = -6;

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
            var location = CalculateLocation(context, anchorPosition);

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

            return CalculateLocation(context, anchorPosition);
        }

        private static Point CalculateLocation(QueryEditorAutoCompletePopupContext context, int anchorPosition)
        {
            var screenPoint = CalculateScreenPointFromEditorPosition(context, anchorPosition);

            var iYShift = QueryEditorAutoCompletePopupOffsetResolver.Resolve
            (
                context.QueryEditorFontSize,
                context.Editor.Zoom
            );

            var iLeft = screenPoint.X - context.MainFormLeft + context.SplitterDistance + DefaultXOffset;
            var iTop = screenPoint.Y - context.MainFormTop - iYShift;

            return new Point(iLeft, iTop);
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
            return context.OwnerForm.PointToScreen
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
