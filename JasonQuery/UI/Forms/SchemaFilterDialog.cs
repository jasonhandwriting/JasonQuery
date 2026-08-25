using C1.Win.C1Input;
using C1.Win.C1Themes;
using JasonLibrary.Core;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.UI.Helpers;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaFilterDialog : Form
    {
        public string SqlCondition { get; set; }
        public bool IsOKButton { get; set; } = false;

        public Dictionary<string, string> SchemaColumnTypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        //20250602 簡化資料庫判斷方式
        private readonly DataSourceType _currentSourceType;
        private bool IsOracle => _currentSourceType == DataSourceType.Oracle;
        private bool IsPostgreSql => _currentSourceType == DataSourceType.PostgreSql;
        private bool IsSqlServer => _currentSourceType == DataSourceType.SqlServer;
        private bool IsMySql => _currentSourceType == DataSourceType.MySql;

        public SchemaFilterDialog()
        {
            InitializeComponent();

            _currentSourceType = DatabaseSqlExecutor.CurrentDataSource;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                switch (_currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            cboOperator.CharacterCasing = CharacterCasing.Upper;
                            cboOperator.Items.Add("LIKE");
                            cboOperator.Items.Add("BETWEEN");
                            cboOperator.Items.Add("IS NOT NULL");
                            cboOperator.Items.Add("IS NULL");
                            cboOperator.Items.Add("IN");
                            cboOperator.Items.Add("NOT IN");
                            break;
                        }
                    case DataSourceType.PostgreSql:
                    case DataSourceType.SqlServer:
                    case DataSourceType.MySql:
                        {
                            cboOperator.CharacterCasing = CharacterCasing.Lower;
                            cboOperator.Items.Add("like");
                            cboOperator.Items.Add("between");
                            cboOperator.Items.Add("is not null");
                            cboOperator.Items.Add("is null");
                            cboOperator.Items.Add("in");
                            cboOperator.Items.Add("not in");
                            break;
                        }
                }

                LocalizationHelper.ApplyLanguageInfo(this);
                UIHelper.SetC1ComboBoxItemsFromDictionary(cboColumn, SchemaColumnTypes, true);

                if (MyLibrary.IsDarkMode)
                {
                    C1ThemeController.ApplicationTheme = "VS2013Dark";
                    lblLimit500.ForeColor = Color.Yellow;
                    //lblColumnType.ForeColor = Color.White;
                    //lblColumnTypeValue.ForeColor = Color.White;
                }
                else
                {
                    lblLimit500.ForeColor = Color.DarkRed;
                }

                editorSqlCondition.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
                editorSqlCondition.WrapVisualFlags = (MyLibrary.WordWrapVisualFlags_Start ? WrapVisualFlags.Start : WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_End ? WrapVisualFlags.End : WrapVisualFlags.None) | (MyLibrary.WordWrapVisualFlags_Margin ? WrapVisualFlags.Margin : WrapVisualFlags.None);

                if (MyLibrary.WordWrap)
                {
                    editorSqlCondition.WrapMode = WrapMode.Word;
                }
                else
                {
                    editorSqlCondition.WrapMode = WrapMode.None;
                }

                editorSqlCondition.ViewWhitespace = WhitespaceMode.Invisible;

                cboOperator.SelectedIndex = 0;
                ApplySqlStyler();

                editorSqlCondition.Text = SqlCondition;

                if (MyLibrary.SqlFormatterConvertCaseForKeywordsCase == 2) //關鍵字小寫
                {
                    btnAnd.Text = btnAnd.Text.ToLower();
                    btnAnd.Tag = "and";
                    btnOr.Text = btnOr.Text.ToLower();
                    btnOr.Tag = "or";
                }
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private string GetDataType(string sDataType)
        {
            var sDataType2 = string.Empty;
            var iTemp = sDataType.IndexOf('(');

            if (iTemp > 0)
            {
                sDataType = sDataType.Substring(0, iTemp);
            }

            sDataType = sDataType.Trim().ToUpper();

            switch (sDataType)
            {
                case "INT":
                case "INTEGER":
                case "LONG":
                case "NUMBER":
                case "BIGINT":
                case "SMALLINT":
                case "REAL":
                case "DOUBLE":
                case "DOUBLE PRECISION":
                case "NUMERIC":
                case "DECIMAL":
                case "BIT":
                case "TINYINT":
                case "UNSIGNED TINYINT":
                case "MEDIUMINT":
                case "UNSIGNED SMALLINT":
                case "FLOAT":
                case "BOOLEAN":
                case "MONEY":
                case "INT4RANGE":
                case "INT8RANGE":
                case "NUMRANGE":
                case "INT32":
                case "OID":
                    {
                        sDataType2 = "NUMBER";
                        break;
                    }
                case "TIMESTAMP":
                case "DATE":
                case "DATETIME":
                    {
                        sDataType2 = IsOracle ? "DATETIME" : "STRING";
                        break;
                    }
                default: //其餘一律視為文字，前後加上單引號
                    {
                        sDataType2 = "STRING";
                        break;
                    }
            }

            return sDataType2;
        }

        private void ApplySqlStyler()
        {
            SqlStyler.ColorEditorBackground = MyLibrary.ColorEditorBackground;
            SqlStyler.ColorTextIdentifier = MyLibrary.ColorTextIdentifier;
            SqlStyler.ColorComments = MyLibrary.ColorComments;
            SqlStyler.ColorNumber = MyLibrary.ColorNumber;
            SqlStyler.ColorString = MyLibrary.ColorString;
            SqlStyler.ColorCharacter = MyLibrary.ColorCharacter;
            SqlStyler.ColorOperatorSymbol = MyLibrary.ColorOperatorSymbol;
            SqlStyler.ColorUserDefinedTablesViews = MyLibrary.ColorUserDefinedTablesViews;
            SqlStyler.ColorUserDefinedFunctionsTriggers = MyLibrary.ColorUserDefinedFunctionsTriggers;
            SqlStyler.ColorOperatorKeywords = MyLibrary.ColorOperatorKeywords;
            SqlStyler.ColorBuiltInFunctions = MyLibrary.ColorBuiltInFunctions;
            SqlStyler.ColorBuiltInKeywords = MyLibrary.ColorBuiltInKeywords;
            SqlStyler.ColorUserDefinedKeywords = MyLibrary.ColorUserDefinedKeywords;
            SqlStyler.IsKeywordFontBold = MyLibrary.KeywordFontBold;

            SqlStyler.KeywordsUserDefinedTables = MyLibrary.KeywordsUserDefinedTables;
            SqlStyler.KeywordsUserDefinedViews = MyLibrary.KeywordsUserDefinedViews;
            SqlStyler.KeywordsUserDefinedFunctions = MyLibrary.KeywordsUserDefinedFunctions;
            SqlStyler.KeywordsUserDefinedTriggers = MyLibrary.KeywordsUserDefinedTriggers;
            SqlStyler.KeywordsOperatorKeywords = MyLibrary.KeywordsOperatorKeywords;
            SqlStyler.KeywordsBuiltInFunctions = MyLibrary.KeywordsBuiltInFunctions;
            SqlStyler.KeywordsBuiltInKeywords = MyLibrary.KeywordsBuiltInKeywords;
            SqlStyler.KeywordsUserDefinedKeywords = MyLibrary.KeywordsUserDefinedKeywords;

            editorSqlCondition.Styler = new SqlStyler();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            IsOKButton = true;
            SqlCondition = editorSqlCondition.Text.Trim();
            Close();
        }

        private void btnAndOr_Click(object sender, EventArgs e)
        {
            var btn = sender as C1Button;
            var sValue = txtCondition.Text.Trim();

            switch (lblColumnTypeOK.Text)
            {
                case "NUMBER":
                    {
                        sValue = sValue.Replace("'", string.Empty);
                        sValue = string.IsNullOrEmpty(sValue) ? "0" : sValue;
                        break;
                    }
                case "DATETIME": //只有 Oracle 會是 DateTime
                    {
                        if (sValue.StartsWith("'", StringComparison.Ordinal) && sValue.EndsWith("'", StringComparison.Ordinal))
                        {
                            if (sValue.Length <= 12)
                            {
                                sValue = $"TO_DATE({sValue}, '{MyLibrary.DateFormat}')";
                            }
                            else
                            {
                                sValue = $"TO_DATE({sValue}, '{MyLibrary.DateFormat} HH24:MI:SS')";
                            }
                        }
                        else
                        {
                            var sValueNew = sValue.Replace("'", string.Empty);

                            if (sValue.Length <= 10)
                            {
                                sValue = $"TO_DATE('{sValueNew}', '{MyLibrary.DateFormat}')";
                            }
                            else
                            {
                                sValue = $"TO_DATE('{sValueNew}', '{MyLibrary.DateFormat} HH24:MI:SS')";
                            }
                        }

                        break;
                    }
                default:
                    {
                        if (sValue.StartsWith("'", StringComparison.Ordinal) && sValue.EndsWith("'", StringComparison.Ordinal))
                        {
                            //不用特別處理
                        }
                        else
                        {
                            sValue = $"'{sValue.Replace("'", "''")}'";
                        }

                        break;
                    }
            }

            if (lblColumnTypeOK.Text == "STRING" && string.IsNullOrEmpty(sValue))
            {
                sValue = "''";
            }

            if (string.Equals(cboOperator.Text, "LIKE", StringComparison.OrdinalIgnoreCase) && sValue.Length - sValue.Replace("%", string.Empty).Length == 0)
            {
                if (sValue.StartsWith("'", StringComparison.Ordinal) & sValue.Length > 2)
                {
                    if (!sValue.StartsWith("'%", StringComparison.Ordinal))
                    {
                        sValue = $"'%{sValue.Substring(1)}";
                    }
                }

                if (sValue.EndsWith("'", StringComparison.Ordinal) & sValue.Length > 2)
                {
                    if (!sValue.EndsWith("%'", StringComparison.Ordinal))
                    {
                        sValue = $"{sValue.Substring(0, sValue.Length - 1)}%'";
                    }
                }
            }

            //20240714 針對 IS NOT NULL / IS NULL，忽略不處理
            if (string.Equals(cboOperator.Text, "IS NOT NULL", StringComparison.OrdinalIgnoreCase) || string.Equals(cboOperator.Text, "IS NULL", StringComparison.OrdinalIgnoreCase))
            {
                sValue = string.Empty;
            }

            var sTemp = string.IsNullOrWhiteSpace(editorSqlCondition.Text) ? string.Empty : $"\r\n{btn.Tag} ";

            editorSqlCondition.Text += $"{sTemp}{cboColumn.Text} {cboOperator.Text} {sValue}";
            editorSqlCondition.Focus();
            editorSqlCondition.SelectionStart = editorSqlCondition.Text.Length;
        }

        private void btnClearFilter_Click(object sender, EventArgs e)
        {
            editorSqlCondition.Text = string.Empty;
            editorSqlCondition.Focus();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cboColumn_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cboColumn.Text))
            {
                return;
            }

            btnAnd.Enabled = !string.IsNullOrWhiteSpace(cboColumn.Text);
            btnOr.Enabled = !string.IsNullOrWhiteSpace(cboColumn.Text);
            lblColumnTypeValue.Text = TextHelper.GetValueFromDictionary(SchemaColumnTypes, cboColumn.Text);

            var sColumnTypeOK = GetDataType(lblColumnTypeValue.Text);

            if (!string.IsNullOrEmpty(lblColumnTypeValue.Text))
            {
                lblColumnType.Visible = true;
                lblColumnTypeValue.Visible = true;
                lblColumnTypeOK.Text = sColumnTypeOK;
            }
            else
            {
                lblColumnType.Visible = false;
                lblColumnTypeValue.Visible = false;
            }
        }
    }
}
