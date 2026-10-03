using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class OracleCreateTableWizardForm
    {
        private Timer _timerMother2Child;

        private void InitializeRuntimeLocalizationConsumer()
        {
            _timerMother2Child = new Timer
            {
                Enabled = true
            };

            _timerMother2Child.Tick += tmrMother2Child_Tick;
            Disposed += OracleCreateTableWizardForm_Disposed;
        }

        private void OracleCreateTableWizardForm_Disposed(object sender, EventArgs e)
        {
            if (_timerMother2Child == null)
            {
                return;
            }

            _timerMother2Child.Stop();
            _timerMother2Child.Tick -= tmrMother2Child_Tick;
            _timerMother2Child.Dispose();
            _timerMother2Child = null;
        }

        private void tmrMother2Child_Tick(object sender, EventArgs e)
        {
            if (!_isFormLoadFinished || string.IsNullOrEmpty(MyGlobal.InfoFromReloadLocalization) || !MyGlobal.InfoFromReloadLocalization.StartsWith("ReloadLocalization`", StringComparison.Ordinal))
            {
                return;
            }

            var temp = MyGlobal.InfoFromReloadLocalization.Replace("ReloadLocalization`", string.Empty);

            temp = temp.Split(';')[0];

            if (temp != AccessibleDescription)
            {
                return;
            }

            MyGlobal.InfoFromReloadLocalization = MyGlobal.InfoFromReloadLocalization.Replace($"{AccessibleDescription};", string.Empty);

            if (MyGlobal.InfoFromReloadLocalization == "ReloadLocalization`")
            {
                MyGlobal.InfoFromReloadLocalization = string.Empty;
            }

            ApplyLocalizationSetting();
        }

        private void ApplyLocalizationSetting()
        {
            var previousOnDeleteDisplayValues = _onDeleteDisplayValues;
            var previousDeferrableDisplayValues = _deferrableDisplayValues;
            var previousIndexTypeDisplayValues = _indexTypeDisplayValues;
            var previousNotSpecifiedText = _notSpecifiedText;
            var previousFormLoadFinished = _isFormLoadFinished;

            _isFormLoadFinished = false;

            try
            {
                LocalizationHelper.ApplyLanguageInfo(this);

                ApplyLocalizedControlLayout();
                ReloadSqlPreviewMenuLocalization();
                ReloadLocalizedDisplayValues();

                TranslateLocalizedDataValues
                (
                    previousOnDeleteDisplayValues,
                    previousDeferrableDisplayValues,
                    previousIndexTypeDisplayValues,
                    previousNotSpecifiedText
                );

                ReloadLocalizedGridValueItems();
                ReloadLocalizedGridCaptionsAndWidths();
            }
            finally
            {
                _isFormLoadFinished = previousFormLoadFinished;
            }
        }

        private void ApplyLocalizedControlLayout()
        {
            lblColumns2.Text = lblColumns.Text;
            chkDoubleQuotes.Location = new Point(lblColumns2.Left + lblColumns2.Width + 5, chkDoubleQuotes.Top);
            cboSchema.Location = new Point(lblSchema.Left + lblSchema.Width, cboSchema.Top);
            txtTableName.Location = new Point(lblTableName.Left + lblTableName.Width, txtTableName.Top);
            cboTableType.Location = new Point(lblTableType.Left + lblTableType.Width, cboTableType.Top);
            txtTableComment.Location = new Point(lblTableComment.Left + lblTableComment.Width, txtTableComment.Top);
            cboForeignKeySchema.Location = new Point(lblForeignKeySchema.Left + lblForeignKeySchema.Width, cboForeignKeySchema.Top);
            cboForeignKeyTable.Location = new Point(lblForeignKeyTable.Left + lblForeignKeyTable.Width, cboForeignKeyTable.Top);
        }

        private void ReloadSqlPreviewMenuLocalization()
        {
            if (_sqlPreviewMenu == null || _sqlPreviewMenu.Items.Count <= SqlPreviewColumn.SaveAs)
            {
                return;
            }

            _sqlPreviewMenu.Items[SqlPreviewColumn.Refresh].Text = LocalizationHelper.GetLanguageString("Refresh", "form", GetType().Name, "menu_sqlpreview", "Refresh", "Text");
            _sqlPreviewMenu.Items[SqlPreviewColumn.SelectAll].Text = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menu_sqlpreview", "SelectAll", "Text");
            _sqlPreviewMenu.Items[SqlPreviewColumn.Copy].Text = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menu_sqlpreview", "Copy", "Text");
            _sqlPreviewMenu.Items[SqlPreviewColumn.SaveAs].Text = LocalizationHelper.GetLanguageString("Save As", "form", GetType().Name, "menu_sqlpreview", "SaveAs", "Text");
        }

        private void ReloadLocalizedDisplayValues()
        {
            var languageText = LocalizationHelper.GetLanguageString("No Action", "form", GetType().Name, "dropdownlist", "NoAction", "Text");

            _onDeleteDisplayValues = new Dictionary<string, string>
            {
                { "NoAction", languageText }
            };

            _defaultOnDeleteText = languageText;
            languageText = LocalizationHelper.GetLanguageString("Cascade", "form", GetType().Name, "dropdownlist", "Cascade", "Text");
            _onDeleteDisplayValues.Add("Cascade", languageText);
            languageText = LocalizationHelper.GetLanguageString("Set Null", "form", GetType().Name, "dropdownlist", "SetNull", "Text");
            _onDeleteDisplayValues.Add("SetNull", languageText);
            languageText = LocalizationHelper.GetLanguageString("Not Deferrable", "form", GetType().Name, "dropdownlist", "NotDeferrable", "Text");

            _deferrableDisplayValues = new Dictionary<string, string>
            {
                { "NotDeferrable", languageText }
            };

            _defaultDeferrableText = languageText;
            languageText = LocalizationHelper.GetLanguageString("Initially Immediate", "form", GetType().Name, "dropdownlist", "InitiallyImmediate", "Text");
            _deferrableDisplayValues.Add("InitiallyImmediate", languageText);
            languageText = LocalizationHelper.GetLanguageString("Initially Deferred", "form", GetType().Name, "dropdownlist", "InitiallyDeferred", "Text");
            _deferrableDisplayValues.Add("InitiallyDeferred", languageText);
            languageText = LocalizationHelper.GetLanguageString("Non-Unique", "form", GetType().Name, "dropdownlist", "Non-Unique", "Text");

            _indexTypeDisplayValues = new Dictionary<string, string>
            {
                { "Non-Unique", languageText }
            };

            _defaultIndexTypeText = languageText;
            languageText = LocalizationHelper.GetLanguageString("Unique", "form", GetType().Name, "dropdownlist", "Unique", "Text");
            _indexTypeDisplayValues.Add("Unique", languageText);
            languageText = LocalizationHelper.GetLanguageString("Bitmap", "form", GetType().Name, "dropdownlist", "Bitmap", "Text");
            _indexTypeDisplayValues.Add("Bitmap", languageText);
            _notSpecifiedText = LocalizationHelper.GetLanguageString("<Not Specified>", "form", GetType().Name, "dropdownlist", "NotSpecified", "Text");
        }

        private void TranslateLocalizedDataValues(Dictionary<string, string> previousOnDeleteDisplayValues, Dictionary<string, string> previousDeferrableDisplayValues, Dictionary<string, string> previousIndexTypeDisplayValues, string previousNotSpecifiedText)
        {
            TranslateLocalizedDataColumn
            (
                _dtIndexesData,
                "IndexesType",
                previousIndexTypeDisplayValues,
                _indexTypeDisplayValues
            );

            TranslateLocalizedDataColumn
            (
                _dtPrimaryKeyData,
                "DeferrableState",
                previousDeferrableDisplayValues,
                _deferrableDisplayValues
            );

            TranslateLocalizedDataColumn
            (
                _dtUniqueData,
                "DeferrableState",
                previousDeferrableDisplayValues,
                _deferrableDisplayValues
            );

            TranslateLocalizedDataColumn
            (
                _dtForeignKeyData,
                "DeferrableState",
                previousDeferrableDisplayValues,
                _deferrableDisplayValues
            );

            TranslateLocalizedDataColumn
            (
                _dtForeignKeyData,
                "OnDelete",
                previousOnDeleteDisplayValues,
                _onDeleteDisplayValues
            );

            TranslateLocalizedDataColumn
            (
                _dtCheckData,
                "DeferrableState",
                previousDeferrableDisplayValues,
                _deferrableDisplayValues
            );

            if (_dtIndexExpressionsData == null || !_dtIndexExpressionsData.Columns.Contains("OrderBy") || string.IsNullOrEmpty(previousNotSpecifiedText))
            {
                return;
            }

            foreach (DataRow row in _dtIndexExpressionsData.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                if (string.Equals(row["OrderBy"].ToString(), previousNotSpecifiedText, StringComparison.Ordinal))
                {
                    row["OrderBy"] = _notSpecifiedText;
                }
            }
        }

        private static void TranslateLocalizedDataColumn(DataTable table, string columnName, Dictionary<string, string> previousDisplayValues, Dictionary<string, string> currentDisplayValues)
        {
            if (table == null || !table.Columns.Contains(columnName) || previousDisplayValues == null || currentDisplayValues == null)
            {
                return;
            }

            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                var currentValue = row[columnName].ToString();

                if (!TryGetLocalizedValueKey(previousDisplayValues, currentValue, out var key) || !currentDisplayValues.TryGetValue(key, out var translatedValue))
                {
                    continue;
                }

                row[columnName] = translatedValue;
            }
        }

        private static bool TryGetLocalizedValueKey(Dictionary<string, string> displayValues, string displayValue, out string key)
        {
            key = string.Empty;

            if (displayValues == null)
            {
                return false;
            }

            foreach (var item in displayValues)
            {
                if (!string.Equals(item.Value, displayValue, StringComparison.Ordinal))
                {
                    continue;
                }

                key = item.Key;
                return true;
            }

            return false;
        }

        private void ReloadLocalizedGridValueItems()
        {
            var items = c1GridIndexes.Columns[IndexesColumn.IndexesType].ValueItems;

            items.Values.Clear();

            foreach (var item in _indexTypeDisplayValues)
            {
                items.Values.Add(new ValueItem(item.Value, item.Value));
            }

            var orderByItems = c1GridIndexExpressions.Columns[IndexExpressionsColumn.OrderBy].ValueItems;

            orderByItems.Values.Clear();
            orderByItems.Values.Add(new ValueItem(_notSpecifiedText, _notSpecifiedText));
            orderByItems.Values.Add(new ValueItem("ASC", "ASC"));
            orderByItems.Values.Add(new ValueItem("DESC", "DESC"));

            ReloadDeferrableValueItems
            (
                c1GridPrimaryKey,
                PrimaryKeyColumn.DeferrableState
            );

            ReloadDeferrableValueItems
            (
                c1GridUnique,
                UniqueColumn.DeferrableState
            );

            ReloadDeferrableValueItems
            (
                c1GridForeignKey,
                ForeignKeyColumn.DeferrableState
            );

            ReloadDeferrableValueItems
            (
                c1GridCheck,
                CheckColumn.DeferrableState
            );

            items = c1GridForeignKey.Columns[ForeignKeyColumn.OnDelete].ValueItems;
            items.Values.Clear();

            foreach (var item in _onDeleteDisplayValues)
            {
                items.Values.Add(new ValueItem(item.Value, item.Value));
            }

            items.MaxComboItems = 4;
        }

        private void ReloadDeferrableValueItems(C1TrueDBGrid grid, int columnIndex)
        {
            var items = grid.Columns[columnIndex].ValueItems;

            items.Values.Clear();

            foreach (var item in _deferrableDisplayValues)
            {
                items.Values.Add(new ValueItem(item.Value, item.Value));
            }
        }

        private void ApplyLocalizedGridColumnCaptions(C1TrueDBGrid grid, string gridHeader)
        {
            foreach (C1DataColumn column in grid.Columns)
            {
                var localizationId = column.DataField;

                if (string.IsNullOrEmpty(localizationId))
                {
                    continue;
                }

                column.Caption = LocalizationHelper.GetLanguageString
                (
                    column.Caption,
                    "form",
                    GetType().Name,
                    gridHeader,
                    localizationId,
                    "Text"
                );
            }
        }
        private void ReloadLocalizedGridCaptionsAndWidths()
        {
            ApplyLocalizedGridColumnCaptions(c1GridColumn, "gridheader_column");

            ApplyLocalizedGridColumnCaptions(c1GridIndexes, "gridheader_index");

            ApplyLocalizedGridColumnCaptions(c1GridIndexExpressions, "gridheader_IndexExpression");

            ApplyLocalizedGridColumnCaptions(c1GridPrimaryKey, "gridheader_primarykey");

            ApplyLocalizedGridColumnCaptions(c1GridPrimaryKeyConstraints, "gridheader_primarykeyconstraint");

            ApplyLocalizedGridColumnCaptions(c1GridUnique, "gridheader_unique");

            ApplyLocalizedGridColumnCaptions(c1GridUniqueConstraints, "gridheader_uniqueconstraint");

            ApplyLocalizedGridColumnCaptions(c1GridForeignKey, "gridheader_foreignkey");

            ApplyLocalizedGridColumnCaptions(c1GridForeignKeyThisTable, "gridheader_foreignkeythistable");

            ApplyLocalizedGridColumnCaptions(c1GridForeignKeyReferencedTable, "gridheader_foreignkeyreferencedtable");

            ApplyLocalizedGridColumnCaptions(c1GridCheck, "gridheader_check");

            _primaryKeyColumnWidth = -1;
            _notNullColumnWidth = -1;
            _visibleColumnWidth = -1;
            _checkedColumnWidth = -1;

            ResizeColumnWidth("COLUMN");
            ResizeColumnWidth("INDEX");
            ResizeColumnWidth("INDEXEXPRESSION");
            ResizeColumnWidth("PRIMARYKEY");
            ResizeColumnWidth("PRIMARYKEYCONSTRAINT");
            ResizeColumnWidth("UNIQUE");
            ResizeColumnWidth("UNIQUECONSTRAINT");
            ResizeColumnWidth("FOREIGNKEY");
            ResizeColumnWidth("FOREIGNKEYTHISTABLE");
            ResizeColumnWidth("FOREIGNKEYREFERENCEDTABLE");
            ResizeColumnWidth("CHECK");
            GridHelper.ResizeGridColumnWidth(c1GridColumn);
            GridHelper.ResizeGridColumnWidth(c1GridIndexes);
            GridHelper.ResizeGridColumnWidth(c1GridIndexExpressions);
            GridHelper.ResizeGridColumnWidth(c1GridPrimaryKey);
            GridHelper.ResizeGridColumnWidth(c1GridPrimaryKeyConstraints);
            GridHelper.ResizeGridColumnWidth(c1GridUnique);
            GridHelper.ResizeGridColumnWidth(c1GridUniqueConstraints);
            GridHelper.ResizeGridColumnWidth(c1GridForeignKey);
            GridHelper.ResizeGridColumnWidth(c1GridForeignKeyThisTable);
            GridHelper.ResizeGridColumnWidth(c1GridForeignKeyReferencedTable);
            GridHelper.ResizeGridColumnWidth(c1GridCheck);
        }
    }
}
