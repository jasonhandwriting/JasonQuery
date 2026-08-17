using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;
using JasonQuery.UI.Helpers;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Popup
{
    internal sealed class QueryEditorAutoCompletePopupPresenterHostImplementation : IQueryEditorAutoCompletePopupPresenterHost
    {
        internal sealed class PopupBinding
        {
            public QueryEditorAutoCompleteSession Session { get; set; }

            public C1TrueDBGrid Grid { get; set; }

            public Func<DataTable> GetTable { get; set; }

            public Action<DataTable> SetTable { get; set; }

            public Func<QueryEditorAutoCompleteRequest, QueryEditorAutoCompletePopupContext> CreatePopupContext { get; set; }

            public Func<int, bool> TryApplyKeywordFilterFromEditor { get; set; }

            public int[] FetchStyleColumns { get; set; }
        }

        private readonly Func<int> _getEditorTextLength;
        private readonly Action _recordPopupShownMousePosition;
        private readonly PopupBinding _periodBinding;
        private readonly PopupBinding _spaceBinding;

        public QueryEditorAutoCompletePopupPresenterHostImplementation
        (
            Func<int> getEditorTextLength,
            Action recordPopupShownMousePosition,
            PopupBinding periodBinding,
            PopupBinding spaceBinding
        )
        {
            _getEditorTextLength = getEditorTextLength ?? throw new ArgumentNullException(nameof(getEditorTextLength));
            _recordPopupShownMousePosition = recordPopupShownMousePosition ?? throw new ArgumentNullException(nameof(recordPopupShownMousePosition));
            _periodBinding = periodBinding ?? throw new ArgumentNullException(nameof(periodBinding));
            _spaceBinding = spaceBinding ?? throw new ArgumentNullException(nameof(spaceBinding));

            ValidateBinding(_periodBinding, nameof(periodBinding));
            ValidateBinding(_spaceBinding, nameof(spaceBinding));
        }

        public bool TryApplyResolvedPeriodRequest(QueryEditorAutoCompleteRequest request, int initialCaretPosition)
        {
            return TryApplyResolvedRequest(_periodBinding, request, initialCaretPosition);
        }

        public bool TryApplyResolvedSpaceRequest(QueryEditorAutoCompleteRequest request, int initialCaretPosition)
        {
            return TryApplyResolvedRequest(_spaceBinding, request, initialCaretPosition);
        }

        public bool TryFinalizePeriodAutoComplete(int triggerPosition)
        {
            return TryFinalizePopup(_periodBinding, triggerPosition);
        }

        public bool TryFinalizeSpaceAutoComplete(int triggerPosition)
        {
            return TryFinalizePopup(_spaceBinding, triggerPosition);
        }

        public void HidePeriodPopup()
        {
            HidePopup(_periodBinding, QueryEditorAutoCompleteSessionCloseReason.ExternalHide);
        }

        public void HideSpacePopup()
        {
            HidePopup(_spaceBinding, QueryEditorAutoCompleteSessionCloseReason.ExternalHide);
        }

        public bool ShowResolvedPeriodAutoComplete(QueryEditorAutoCompleteRequest request)
        {
            return TryShowResolvedAutoComplete(_periodBinding, request);
        }

        public bool ShowResolvedSpaceAutoComplete(QueryEditorAutoCompleteRequest request)
        {
            return TryShowResolvedAutoComplete(_spaceBinding, request);
        }

        public void ResizePeriodPopup()
        {
            ResizePopup(_periodBinding);
        }

        public void ResizeSpacePopup()
        {
            ResizePopup(_spaceBinding);
        }

        private bool TryApplyResolvedRequest(PopupBinding binding, QueryEditorAutoCompleteRequest request, int initialCaretPosition)
        {
            if (binding == null || request == null)
            {
                return false;
            }

            if (request.Data == null || request.Data.Rows.Count == 0)
            {
                return false;
            }

            var safeCaretPosition = ResolveInitialAutoCompleteCaretPosition
            (
                request.TriggerPosition,
                initialCaretPosition
            );

            binding.Session.SetPositions(request.TriggerPosition, safeCaretPosition);

            var dtResolved = request.Data;
            var targetTable = binding.GetTable();

            DataTableLifecycleHelper.ReplaceDataTable(ref targetTable, ref dtResolved);
            binding.SetTable(targetTable);

            request.Data = targetTable;

            _recordPopupShownMousePosition();

            return TryShowResolvedAutoComplete(binding, request);
        }

        private bool TryShowResolvedAutoComplete(PopupBinding binding, QueryEditorAutoCompleteRequest request)
        {
            if (binding == null || binding.Grid == null)
            {
                return false;
            }

            if (request == null || request.Data == null || request.Data.Rows.Count == 0)
            {
                HidePopup(binding, QueryEditorAutoCompleteSessionCloseReason.NoCandidates);
                return false;
            }

            if (binding.CreatePopupContext == null)
            {
                HidePopup(binding, QueryEditorAutoCompleteSessionCloseReason.NoCandidates);
                return false;
            }

            var context = binding.CreatePopupContext(request);
            return QueryEditorAutoCompletePopupController.Show(binding.Grid, request.Data, context);
        }

        private bool TryFinalizePopup(PopupBinding binding, int triggerPosition)
        {
            if (binding == null || binding.Grid == null || binding.TryApplyKeywordFilterFromEditor == null)
            {
                return false;
            }

            if (!binding.TryApplyKeywordFilterFromEditor(triggerPosition))
            {
                HidePopup(binding, QueryEditorAutoCompleteSessionCloseReason.NoCandidates);
                return false;
            }

            ResizePopup(binding);
            return binding.Grid.Visible;
        }

        private static void HidePopup(PopupBinding binding, QueryEditorAutoCompleteSessionCloseReason closeReason)
        {
            if (binding == null)
            {
                return;
            }

            binding.Session?.Close(closeReason);
            QueryEditorAutoCompletePopupController.Hide(binding.Grid);
        }

        private void ResizePopup(PopupBinding binding)
        {
            if (binding == null || binding.Grid == null)
            {
                return;
            }

            ApplyFetchStyleColumns(binding);

            var table = binding.GetTable();
            var rowCount = table?.Rows.Count ?? 0;
            var width = GridHelper.ResizeGridColumnWidth(binding.Grid);

            ResizeAutoCompleteGrid(binding.Grid, rowCount, width);
        }

        private static void ApplyFetchStyleColumns(PopupBinding binding)
        {
            if (binding?.Grid?.Splits == null || binding.Grid.Splits.Count == 0)
            {
                return;
            }

            var columns = binding.FetchStyleColumns ?? Array.Empty<int>();
            var displayColumns = binding.Grid.Splits[0].DisplayColumns;

            foreach (var columnIndex in columns)
            {
                if (columnIndex < 0 || columnIndex >= displayColumns.Count)
                {
                    continue;
                }

                //20251004 指定哪一個 Column 要套用 FetchCellStyle
                displayColumns[columnIndex].FetchStyle = true;
            }
        }

        private int ResolveInitialAutoCompleteCaretPosition(int triggerPosition, int initialCaretPosition)
        {
            if (initialCaretPosition < triggerPosition)
            {
                return triggerPosition;
            }

            var textLength = _getEditorTextLength();

            if (initialCaretPosition > textLength)
            {
                return textLength;
            }

            return initialCaretPosition;
        }

        private static void ResizeAutoCompleteGrid(C1TrueDBGrid grid, int rowCount, int width)
        {
            if (grid == null)
            {
                return;
            }

            if (rowCount <= 9)
            {
                width = GridHelper.ResizeGridColumnWidth(grid) + 3;
            }
            else
            {
                width = GridHelper.ResizeGridColumnWidth(grid) + grid.VScrollBar.Width + 5;
            }

            const int height = 181;
            grid.Size = new Size(width, height);
        }

        private static void ValidateBinding(PopupBinding binding, string paramName)
        {
            if (binding.Session == null)
            {
                throw new ArgumentException("Popup binding session cannot be null.", paramName);
            }

            if (binding.Grid == null)
            {
                throw new ArgumentException("Popup binding grid cannot be null.", paramName);
            }

            if (binding.GetTable == null)
            {
                throw new ArgumentException("Popup binding GetTable cannot be null.", paramName);
            }

            if (binding.SetTable == null)
            {
                throw new ArgumentException("Popup binding SetTable cannot be null.", paramName);
            }

            if (binding.CreatePopupContext == null)
            {
                throw new ArgumentException("Popup binding CreatePopupContext cannot be null.", paramName);
            }

            if (binding.TryApplyKeywordFilterFromEditor == null)
            {
                throw new ArgumentException("Popup binding TryApplyKeywordFilterFromEditor cannot be null.", paramName);
            }
        }
    }
}