using C1.Win.C1Input;
using JasonLibrary.Core;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private readonly GridColumnEditorRegistry _gridColumnEditorRegistry = new GridColumnEditorRegistry();

        private void RegisterGridDataColumnMetadata(int columnIndex, string columnName, GridColumnContentKind contentKind,
                                                    bool isNullable, string defaultValue)
        {
            if (TryGetGridDataColumnInfo(columnName, out var columnInfo) && IsReadOnlyLargeTextColumn(columnInfo))
            {
                contentKind = GridColumnContentKind.LargeText;
            }

            _gridColumnEditorRegistry.Register(new GridColumnEditorDefinition
            {
                Metadata = new GridColumnEditorMetadata
                {
                    ColumnIndex = columnIndex,
                    ColumnName = columnName,
                    ContentKind = contentKind,
                    IsNullable = isNullable,
                    DefaultValue = defaultValue ?? string.Empty
                },
                Validator = PassThroughGridEditorValueValidator.Instance,
                Normalizer = new DefaultGridEditorValueNormalizer
                             (
                                 GridEditorDefaultValueBehavior.Ignore,
                                 GridEditorRequiredEmptyBehavior.KeepEmpty,
                                 GridEditorNullableEmptyBehavior.KeepEmpty
                             )
            });
        }

        private void RegisterGridDataColumnEditor(int columnIndex, Control editor, GridColumnEditorMetadata metadata,
                                                  IGridEditorValueValidator validator, IGridEditorValueNormalizer normalizer)
        {
            if (editor == null || metadata == null)
            {
                editor?.Dispose();
                return;
            }

            if (columnIndex < 0 || columnIndex >= c1GridData.Columns.Count)
            {
                editor.Dispose();
                return;
            }

            metadata.ColumnIndex = columnIndex;
            metadata.ColumnName = c1GridData.Columns[columnIndex].DataField;

            if (TryGetGridDataColumnInfo(metadata.ColumnName, out var columnInfo) && IsReadOnlyLargeTextColumn(columnInfo))
            {
                RegisterGridDataColumnMetadata(columnIndex, metadata.ColumnName, GridColumnContentKind.LargeText,
                                               columnInfo.IsNullable, metadata.DefaultValue);

                editor.Dispose();
                return;
            }

            var definition = new GridColumnEditorDefinition
            {
                Editor = editor,
                Metadata = metadata,
                Validator = validator ?? PassThroughGridEditorValueValidator.Instance,
                Normalizer = normalizer ?? new DefaultGridEditorValueNormalizer
                                           (
                                               GridEditorDefaultValueBehavior.Ignore,
                                               GridEditorRequiredEmptyBehavior.KeepEmpty,
                                               GridEditorNullableEmptyBehavior.KeepEmpty
                                           )
            };

            c1GridData.Columns[columnIndex].Editor = editor;
            _gridDataColumnEditors.Add(editor);
            _gridColumnEditorRegistry.Register(definition);

            editor.Enter += GridDataColumnEditor_Enter;
            editor.Leave += GridDataColumnEditor_Leave;
            editor.KeyPress += GridDataColumnEditor_KeyPress;
            editor.MouseDown += GridDataColumnEditor_MouseDown;
        }

        private void UnregisterGridDataColumnEditorEvents(Control editor)
        {
            if (editor == null)
            {
                return;
            }

            editor.Enter -= GridDataColumnEditor_Enter;
            editor.Leave -= GridDataColumnEditor_Leave;
            editor.KeyPress -= GridDataColumnEditor_KeyPress;
            editor.MouseDown -= GridDataColumnEditor_MouseDown;
        }

        private bool TryGetGridColumnEditorDefinition(int columnIndex, out GridColumnEditorDefinition definition)
        {
            definition = null;

            if (columnIndex < 0 || columnIndex >= c1GridData.Columns.Count)
            {
                return false;
            }

            var columnName = c1GridData.Columns[columnIndex].DataField;

            return _gridColumnEditorRegistry.TryGet(columnName, out definition);
        }

        private void GridDataColumnEditor_Enter(object sender, EventArgs e)
        {
            var editor = sender as Control;

            if (!_gridColumnEditorRegistry.TryGet(editor, out var definition))
            {
                return;
            }

            var currentRow = GetDataRowFromGridDisplayRow(c1GridData, c1GridData.Row);
            definition.EditingRow = currentRow;

            if (currentRow == null || !currentRow.Table.Columns.Contains(definition.Metadata.ColumnName))
            {
                definition.OriginalDataValue = null;
                definition.OriginalDisplayText = editor.Text;
                return;
            }

            definition.OriginalDataValue = currentRow[definition.Metadata.ColumnName];
            definition.OriginalDisplayText = definition.OriginalDataValue == DBNull.Value ? MyLibrary.GridNullShowAs
                                             : Convert.ToString(definition.OriginalDataValue, CultureInfo.CurrentCulture) ?? string.Empty;
        }

        private void GridDataColumnEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            var editor = sender as Control;

            if (!_gridColumnEditorRegistry.TryGet(editor, out var definition))
            {
                return;
            }

            e.Handled = !definition.Validator.IsKeyPressAllowed(editor, e.KeyChar, definition.Metadata);
        }

        private void GridDataColumnEditor_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            var editor = sender as Control;

            if (editor != null)
            {
                editor.ContextMenuStrip = _nullContextMenu;
            }
        }

        private void GridDataColumnEditor_Leave(object sender, EventArgs e)
        {
            var editor = sender as Control;

            if (!_gridColumnEditorRegistry.TryGet(editor, out var definition))
            {
                return;
            }

            var metadata = definition.Metadata;
            var inputValue = editor.Text ?? string.Empty;

            if (metadata.UseCellEditorResult && _cellEditorResult != null)
            {
                inputValue = _cellEditorResult;
                _cellEditorResult = null;
            }

            var validationResult = string.Equals(inputValue, MyLibrary.GridNullShowAs, StringComparison.Ordinal)
                                   ? GridEditorValidationResult.Valid(inputValue)
                                   : definition.Validator.Validate(inputValue, metadata);

            if (!validationResult.IsValid)
            {
                RestoreInvalidGridEditorValue(editor, definition);
                return;
            }

            var normalizedResult = definition.Normalizer.Normalize(inputValue, validationResult, metadata, MyLibrary.GridNullShowAs);

            ApplyGridEditorDisplayText(editor, normalizedResult.DisplayText);

            var editingRow = definition.EditingRow ?? GetDataRowFromGridDisplayRow(c1GridData, c1GridData.Row);

            if (editingRow != null && editingRow.Table.Columns.Contains(metadata.ColumnName))
            {
                editingRow[metadata.ColumnName] = normalizedResult.DataValue ?? DBNull.Value;
            }

            ClearGridEditorEditingState(definition);

            RefreshTableEditStateMarkers();
            c1GridData_AfterColUpdate(null, null);
        }

        private void RestoreInvalidGridEditorValue(Control editor, GridColumnEditorDefinition definition)
        {
            var displayText = definition.OriginalDisplayText ?? string.Empty;

            ApplyGridEditorDisplayText(editor, displayText);

            var editingRow = definition.EditingRow;

            if (editingRow != null && editingRow.Table.Columns.Contains(definition.Metadata.ColumnName)
                && definition.OriginalDataValue != null)
            {
                editingRow[definition.Metadata.ColumnName] = definition.OriginalDataValue;
            }

            ClearGridEditorEditingState(definition);

            RefreshTableEditStateMarkers();
            c1GridData_AfterColUpdate(null, null);
        }

        private static void ClearGridEditorEditingState(GridColumnEditorDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            definition.EditingRow = null;
            definition.OriginalDataValue = null;
            definition.OriginalDisplayText = string.Empty;
        }

        private static void ApplyGridEditorDisplayText(Control editor, string displayText)
        {
            var normalizedText = displayText ?? string.Empty;

            if (editor is C1DateEdit dateEditor)
            {
                if (string.IsNullOrEmpty(normalizedText) || string.Equals(normalizedText, MyLibrary.GridNullShowAs, StringComparison.Ordinal))
                {
                    dateEditor.Value = null;
                    dateEditor.Text = normalizedText;
                    return;
                }

                dateEditor.Value = normalizedText;
                dateEditor.Text = normalizedText;
                return;
            }

            editor.Text = normalizedText;
        }
    }
}
