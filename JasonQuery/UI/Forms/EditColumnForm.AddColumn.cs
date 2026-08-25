using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;
using JasonQuery.Core.Localization;
using JasonQuery.UI.Helpers;
using System;
using System.Linq;

namespace JasonQuery.UI.Forms
{
    public partial class EditColumnForm
    {
        private bool _isUpdatingAddColumnEditor;

        private void PrepareAddColumnEditor()
        {
            _isUpdatingAddColumnEditor = true;

            try
            {
                txtColumnName_Add.Text = string.Empty;
                txtParameter1_Add.Text = string.Empty;
                txtParameter2_Add.Text = string.Empty;
                txtDefaultValue_Add.Text = string.Empty;
                txtResolvedType_Add.Text = string.Empty;
                lblAddValidationMessage.Text = string.Empty;
                chkNullAllowed_Add.Checked = true;

                BindAddColumnDefaultKindItems();
                BindOracleLengthSemanticsItems();
                BindAddColumnTypeCatalog();
            }
            finally
            {
                _isUpdatingAddColumnEditor = false;
            }

            ApplySelectedAddColumnTypeDefinition(true);
            UpdateAddColumnPreview();
        }

        private void BindAddColumnTypeCatalog()
        {
            cboDataType_Add.BeginUpdate();

            try
            {
                cboDataType_Add.Items.Clear();

                foreach (var definition in ColumnTypeCatalog.GetDefinitions(_currentSourceType))
                {
                    cboDataType_Add.Items.Add(definition);
                }

                var defaultDefinition = ColumnTypeCatalog.GetDefault(_currentSourceType);

                if (defaultDefinition != null)
                {
                    cboDataType_Add.SelectedItem = cboDataType_Add.Items.Cast<object>()
                                                                  .FirstOrDefault
                                                                   (
                                                                       item => ReferenceEquals(item, defaultDefinition)
                                                                   );
                }
            }
            finally
            {
                cboDataType_Add.EndUpdate();
            }
        }

        private void BindAddColumnDefaultKindItems()
        {
            cboDefaultKind_Add.Items.Clear();

            cboDefaultKind_Add.Items.Add
            (
                new AddColumnDefaultKindItem
                (
                    ColumnDefaultValueKind.None,
                    GetAddColumnLanguageText("DefaultNone", "None")
                )
            );

            cboDefaultKind_Add.Items.Add
            (
                new AddColumnDefaultKindItem
                (
                    ColumnDefaultValueKind.Literal,
                    GetAddColumnLanguageText("DefaultLiteral", "Literal")
                )
            );

            cboDefaultKind_Add.Items.Add
            (
                new AddColumnDefaultKindItem
                (
                    ColumnDefaultValueKind.SqlExpression,
                    GetAddColumnLanguageText("DefaultSqlExpression", "SQL Expression")
                )
            );
            cboDefaultKind_Add.SelectedIndex = 0;
        }

        private void BindOracleLengthSemanticsItems()
        {
            cboOracleLengthSemantics_Add.Items.Clear();

            cboOracleLengthSemantics_Add.Items.Add
            (
                new OracleLengthSemanticsItem
                (
                    OracleLengthSemantics.DatabaseDefault,
                    GetAddColumnLanguageText("LengthSemanticsDefault", "Database Default")
                )
            );

            cboOracleLengthSemantics_Add.Items.Add
            (
                new OracleLengthSemanticsItem
                (
                    OracleLengthSemantics.Byte, "BYTE"
                )
            );

            cboOracleLengthSemantics_Add.Items.Add
            (
                new OracleLengthSemanticsItem
                (
                    OracleLengthSemantics.Char, "CHAR"
                )
            );

            cboOracleLengthSemantics_Add.SelectedIndex = 0;
        }

        private void ApplySelectedAddColumnTypeDefinition(bool resetParameterValues)
        {
            var definition = GetSelectedAddColumnTypeDefinition();

            _isUpdatingAddColumnEditor = true;

            try
            {
                var argumentKind = definition?.ArgumentKind ?? ColumnTypeArgumentKind.Custom;

                var showParameter1 = argumentKind == ColumnTypeArgumentKind.Length
                                     || argumentKind == ColumnTypeArgumentKind.PrecisionScale
                                     || argumentKind == ColumnTypeArgumentKind.FractionalSecondsPrecision;

                var showParameter2 = argumentKind == ColumnTypeArgumentKind.PrecisionScale;

                lblParameter1_Add.Visible = showParameter1;
                txtParameter1_Add.Visible = showParameter1;
                lblParameter2_Add.Visible = showParameter2;
                txtParameter2_Add.Visible = showParameter2;

                if (definition != null)
                {
                    lblParameter1_Add.Text = GetAddColumnParameterLabel(definition.Parameter1Label, "Size:");
                    lblParameter2_Add.Text = GetAddColumnParameterLabel(definition.Parameter2Label, "Scale:");
                }

                if (resetParameterValues)
                {
                    txtParameter1_Add.Text = definition?.DefaultParameter1 ?? string.Empty;
                    txtParameter2_Add.Text = definition?.DefaultParameter2 ?? string.Empty;
                }

                var showOracleSemantics = _currentSourceType == DataSourceType.Oracle
                                          && definition != null
                                          && definition.SupportsOracleLengthSemantics;

                lblOracleLengthSemantics_Add.Visible = showOracleSemantics;
                cboOracleLengthSemantics_Add.Visible = showOracleSemantics;

                if (!showOracleSemantics)
                {
                    cboOracleLengthSemantics_Add.SelectedIndex = 0;
                }

                var supportsDefault = definition == null || definition.SupportsDefault;

                lblDefaultKind_Add.Enabled = supportsDefault;
                cboDefaultKind_Add.Enabled = supportsDefault;

                if (!supportsDefault)
                {
                    cboDefaultKind_Add.SelectedIndex = 0;
                }

                ApplyAddColumnDefaultControlState();
                ApplyAddColumnControlLayout();
            }
            finally
            {
                _isUpdatingAddColumnEditor = false;
            }
        }

        private void ApplyAddColumnControlLayout()
        {
            const int controlSpacing = 6;
            const int groupSpacing = 16;
            const int defaultValueSpacing = 10;

            ControlLayoutHelper.PlaceRightOf(cboDataType_Add, lblDataType_Add, controlSpacing);
            ControlLayoutHelper.PlaceRightOf(txtParameter1_Add, lblParameter1_Add, controlSpacing);

            if (lblParameter2_Add.Visible && txtParameter2_Add.Visible)
            {
                ControlLayoutHelper.PlaceRightOf(lblParameter2_Add, txtParameter1_Add, groupSpacing);
                ControlLayoutHelper.PlaceRightOf(txtParameter2_Add, lblParameter2_Add, controlSpacing);
            }

            if (lblOracleLengthSemantics_Add.Visible && cboOracleLengthSemantics_Add.Visible)
            {
                var parameterAnchor = txtParameter2_Add.Visible
                                      ? (System.Windows.Forms.Control)txtParameter2_Add
                                      : txtParameter1_Add;

                ControlLayoutHelper.PlaceRightOf(lblOracleLengthSemantics_Add, parameterAnchor, groupSpacing);
                ControlLayoutHelper.PlaceRightOf(cboOracleLengthSemantics_Add, lblOracleLengthSemantics_Add, controlSpacing);
            }

            ControlLayoutHelper.PlaceRightOf(cboDefaultKind_Add, lblDefaultKind_Add, controlSpacing);
            ControlLayoutHelper.PlaceRightOf(txtDefaultValue_Add, cboDefaultKind_Add, defaultValueSpacing);
            ControlLayoutHelper.PlaceRightOf(txtResolvedType_Add, lblResolvedType_Add, controlSpacing);
        }

        private void ApplyAddColumnDefaultControlState()
        {
            var item = cboDefaultKind_Add.SelectedItem as AddColumnDefaultKindItem;
            var enabled = cboDefaultKind_Add.Enabled && item != null && item.Kind != ColumnDefaultValueKind.None;

            txtDefaultValue_Add.Enabled = enabled;

            if (!enabled)
            {
                txtDefaultValue_Add.Text = string.Empty;
            }
        }

        private ColumnTypeDefinition GetSelectedAddColumnTypeDefinition()
        {
            var selected = cboDataType_Add.SelectedItem as ColumnTypeDefinition;

            if (selected != null && string.Equals(cboDataType_Add.Text, selected.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return selected;
            }

            var text = (cboDataType_Add.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var catalogType = ColumnTypeCatalog.GetDefinitions(_currentSourceType)
                                               .FirstOrDefault
                                                (
                                                    item => string.Equals(item.Key, text, StringComparison.OrdinalIgnoreCase)
                                                            || string.Equals(item.SqlTypeName, text, StringComparison.OrdinalIgnoreCase)
                                                            || string.Equals(item.DisplayName, text, StringComparison.OrdinalIgnoreCase)
                                                );

            return catalogType ?? ColumnTypeCatalog.CreateCustom(_currentSourceType, text);
        }

        private ColumnDefinition CreateAddColumnDefinition()
        {
            var typeDefinition = GetSelectedAddColumnTypeDefinition();
            var defaultKindItem = cboDefaultKind_Add.SelectedItem as AddColumnDefaultKindItem;
            var semanticsItem = cboOracleLengthSemantics_Add.SelectedItem as OracleLengthSemanticsItem;

            return new ColumnDefinition
            {
                ColumnName = txtColumnName_Add.Text,
                TypeKey = typeDefinition != null && !typeDefinition.IsCustom ? typeDefinition.Key : string.Empty,
                CustomTypeText = typeDefinition != null && typeDefinition.IsCustom ? cboDataType_Add.Text : string.Empty,
                Parameter1 = txtParameter1_Add.Text,
                Parameter2 = txtParameter2_Add.Text,
                OracleLengthSemantics = semanticsItem?.Value ?? OracleLengthSemantics.DatabaseDefault,
                NullAllowed = chkNullAllowed_Add.Checked,
                DefaultValueKind = defaultKindItem?.Kind ?? ColumnDefaultValueKind.None,
                DefaultValue = txtDefaultValue_Add.Text
            };
        }

        private ColumnAddBuildResult BuildAddColumnResult()
        {
            return ColumnAddSqlBuilder.Build
            (
                new ColumnAddRequest
                {
                    DataSourceType = _currentSourceType,
                    SchemaDatabase = SchemaDatabase,
                    SchemaName = SchemaName,
                    TableName = TableName,
                    Column = CreateAddColumnDefinition()
                }
            );
        }

        private void UpdateAddColumnPreview()
        {
            var result = BuildAddColumnResult();

            txtResolvedType_Add.Text = result.ResolvedDataType ?? string.Empty;

            lblAddValidationMessage.Text = GetAddColumnValidationMessage(result);

            UpdateSqlPreviewText(result.Succeeded ? result.Sql : string.Empty);
        }

        private string GetAddColumnValidationMessage(ColumnAddBuildResult result)
        {
            if (result == null || result.Succeeded || result.FailureKind == ColumnAddBuildFailureKind.MissingColumnName)
            {
                return string.Empty;
            }

            var key = $"Add{result.FailureKind}";

            return LocalizationHelper.GetLanguageString(result.ErrorMessage ?? string.Empty, "form", GetType().Name, "msg", key, "Text");
        }

        private string GetAddColumnLanguageText(string key, string fallback)
        {
            return LocalizationHelper.GetLanguageString(fallback, "form", GetType().Name, "addcolumn", key, "Text");
        }

        private string GetAddColumnParameterLabel(string label, string fallback)
        {
            var normalized = (label ?? string.Empty).Trim().TrimEnd(':').Replace(" ", string.Empty);
            var key = string.IsNullOrEmpty(normalized) ? fallback.Trim().TrimEnd(':').Replace(" ", string.Empty) : normalized;

            return GetAddColumnLanguageText($"Parameter{key}", string.IsNullOrWhiteSpace(label) ? fallback : label);
        }

        private void SetAddColumnEditorEnabled(bool enabled)
        {
            txtColumnName_Add.Enabled = enabled;
            cboDataType_Add.Enabled = enabled;
            chkNullAllowed_Add.Enabled = enabled;
            txtParameter1_Add.Enabled = enabled;
            txtParameter2_Add.Enabled = enabled;
            cboOracleLengthSemantics_Add.Enabled = enabled;
            cboDefaultKind_Add.Enabled = enabled;
            txtDefaultValue_Add.Enabled = enabled && cboDefaultKind_Add.SelectedIndex > 0;
        }

        private string GetAddColumnExecutionMessage(bool succeeded)
        {
            var columnName = txtColumnName_Add.Text;

            if (succeeded)
            {
                return LocalizationHelper.GetLanguageString("The column {0} was added successfully.", "form", GetType().Name, "msg", "AddOK", "Text").Replace("{0}", columnName);
            }

            return LocalizationHelper.GetLanguageString("The column {0} could not be added.", "form", GetType().Name, "msg", "AddNG", "Text").Replace("{0}", columnName);
        }

        private sealed class AddColumnDefaultKindItem
        {
            public AddColumnDefaultKindItem(ColumnDefaultValueKind kind, string displayText)
            {
                Kind = kind;
                DisplayText = displayText ?? string.Empty;
            }

            public ColumnDefaultValueKind Kind { get; }

            private string DisplayText { get; }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        private sealed class OracleLengthSemanticsItem
        {
            public OracleLengthSemanticsItem(OracleLengthSemantics value, string displayText)
            {
                Value = value;
                DisplayText = displayText ?? string.Empty;
            }

            public OracleLengthSemantics Value { get; }

            private string DisplayText { get; }

            public override string ToString()
            {
                return DisplayText;
            }
        }
    }
}
