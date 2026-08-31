using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonLibrary.UI.Controls.Stylers;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.UI.Helpers;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SqlVariablesForm : Form
    {
        private readonly SqlVariablesRequest _request = new SqlVariablesRequest();
        private List<string> _lstGridHeaderVariables = new List<string>();
        private string _languageText = string.Empty;
        private DataTable _dtVariables = new DataTable();
        private string _columnValue = "Value";

        internal SqlVariablesResolveResult ResolveResult { get; private set; }

        private enum _enumVariables
        {
            NameOriginal = 0,
            Name,
            Value,
            Type,
            Help
        }

        private enum _enumType
        {
            String = 0,
            Number,
            Custom
        }

        public SqlVariablesForm()
        {
            InitializeComponent();
        }

        internal SqlVariablesForm(SqlVariablesRequest request) : this()
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizationHelper.ApplyLanguageInfo(this, false);

                SetSqlText(_request.SqlText); //將 SQL 寫入 editor 中
                editorSql.Tag = _request.SqlText;

                _lstGridHeaderVariables = new List<string>();

                _lstGridHeaderVariables.Add("Name_Original");
                _languageText = LocalizationHelper.GetLanguageString("Name", "form", GetType().Name, "gridheader", "Name", "Text");
                _lstGridHeaderVariables.Add(_languageText);
                _languageText = LocalizationHelper.GetLanguageString("Value", "form", GetType().Name, "gridheader", "Value", "Text");
                _lstGridHeaderVariables.Add(_languageText);
                _columnValue = _languageText;
                _languageText = LocalizationHelper.GetLanguageString("Type", "form", GetType().Name, "gridheader", "Type", "Text");
                _lstGridHeaderVariables.Add(_languageText);
                _lstGridHeaderVariables.Add("?"); //固定顯示 ？

                _dtVariables = new DataTable();

                _dtVariables.Columns.Add(_lstGridHeaderVariables[(int)_enumVariables.NameOriginal]);
                _dtVariables.Columns.Add(_lstGridHeaderVariables[(int)_enumVariables.Name]);
                _dtVariables.Columns.Add(_lstGridHeaderVariables[(int)_enumVariables.Value]);
                _dtVariables.Columns.Add(_lstGridHeaderVariables[(int)_enumVariables.Type]);
                _dtVariables.Columns.Add(_lstGridHeaderVariables[(int)_enumVariables.Help]);

                var variable = _request.Variables.Split(new[] { "`" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var t in variable)
                {
                    var rowVariables = _dtVariables.NewRow();

                    rowVariables[(int)_enumVariables.NameOriginal] = t;
                    rowVariables[(int)_enumVariables.Name] = t;

                    var value = string.Empty;
                    var type = "0";
                    var attributeName = t.Replace("'", "''");
                    var sbSql = new StringBuilder();

                    sbSql.AppendLine("SELECT AttributeValue, AttributeText");
                    sbSql.AppendLine("  FROM SystemConfig");
                    sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                    sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                    sbSql.AppendLine("   AND AttributeKey = 'ParametersList'");
                    sbSql.AppendLine($"   AND AttributeName = '{attributeName}'");
                    sbSql.Append(" ORDER BY AttributeDate DESC");

                    var sql = sbSql.ToString();
                    var dtTemp = JasonQueryRepository.ExecQuery(sql);

                    if (dtTemp?.Rows.Count > 0)
                    {
                        //取得同一個變數，前一次的設定值
                        value = dtTemp.Rows[0].GetSafeString("AttributeValue");
                        type = dtTemp.Rows[0].GetSafeString("AttributeText");

                        if (!new HashSet<string> { "0", "1", "2" }.Contains(type))
                        {
                            type = "0"; //預設為字串
                        }
                    }

                    rowVariables[(int)_enumVariables.Value] = value;
                    rowVariables[(int)_enumVariables.Type] = type;
                    rowVariables[(int)_enumVariables.Help] = string.Empty;
                    _dtVariables.Rows.Add(rowVariables);
                }

                c1GridVariables.DataSource = _dtVariables;
                c1GridVariables.Splits[0].DisplayColumns[(int)_enumVariables.Help].FilterButton = false;
                c1GridVariables.Splits[0].DisplayColumns[(int)_enumVariables.Help].Button = true;
                c1GridVariables.Splits[0].DisplayColumns[(int)_enumVariables.Help].ButtonAlways = true;
                c1GridVariables.ButtonClick += c1GridVariables_ButtonClick;
                c1GridVariables.Columns[(int)_enumVariables.Help].ButtonPicture = picHelp.Image;

                //20240224 下拉清單只能選取，不能編輯！
                c1GridVariables.Splits[0].DisplayColumns[(int)_enumVariables.Type].DropDownList = true;

                SetComboBox();
                c1GridVariables.Refresh();
                GridVisualStyle();
                GridFontAndBackColor();

                foreach (C1DisplayColumn col in c1GridVariables.Splits[0].DisplayColumns)
                {
                    try
                    {
                        col.AutoSize();
                    }
                    catch (Exception)
                    {
                        col.Width = 2000;
                    }

                    if (col.Name == "Name_Original")
                    {
                        col.Visible = false;
                        col.Frozen = true;
                    }
                    else if (col.Name == _columnValue)
                    {
                        col.Width = 250;
                    }
                    else if (col.Name == "?") //說明欄，只顯示問號按鈕
                    {
                        col.Width = 17;
                    }
                    else
                    {
                        col.Width += 10;
                    }
                }

                ApplySqlStyler();

                panel1.Location = new Point(lblPreview.Left + lblPreview.Width, panel1.Top);

                c1GridVariables.Col = (int)_enumVariables.Value;
                c1GridVariables.Row = 0;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            JasonQueryRepository.UpdateSetting("GlobalConfig", "VariablesFormWidth", Size.Width.ToString());
            JasonQueryRepository.UpdateSetting("GlobalConfig", "VariablesFormHeight", Size.Height.ToString());
        }

        private void SetComboBox()
        {
            //Type 欄位以 ComboBox 動態呈現
            var items = c1GridVariables.Columns[_lstGridHeaderVariables[(int)_enumVariables.Type]].ValueItems;

            items.Translate = true;
            items.Presentation = PresentationEnum.ComboBox;
            items.Validate = true;

            items.Values.Clear();
            _languageText = LocalizationHelper.GetLanguageString("String", "form", GetType().Name, "object", "String", "Text");
            items.Values.Add(new ValueItem("0", _languageText));
            _languageText = LocalizationHelper.GetLanguageString("Number", "form", GetType().Name, "object", "Number", "Text");
            items.Values.Add(new ValueItem("1", _languageText));
            _languageText = LocalizationHelper.GetLanguageString("Custom", "form", GetType().Name, "object", "Custom", "Text");
            items.Values.Add(new ValueItem("2", _languageText));

            //指定哪一個 Column 要套用 FetchCellStyle
            c1GridVariables.Splits[0].DisplayColumns[(int)_enumVariables.Name].FetchStyle = true;
            c1GridVariables.Splits[0].DisplayColumns[_lstGridHeaderVariables[(int)_enumVariables.Value]].FetchStyle = true;
            c1GridVariables.Splits[0].DisplayColumns[_lstGridHeaderVariables[(int)_enumVariables.Help]].FetchStyle = true;
        }

        private static void UpdateVariablesList(string sName, string sValue, string sType)
        {
            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'ParametersList'");
            sbSql.Append($"   AND AttributeName = '{sName}'");

            var sql = sbSql.ToString();
            var dtVariables = JasonQueryRepository.ExecQuery(sql);

            if (dtVariables?.Rows.Count > 0)
            {
                sbSql.Clear();
                sbSql.AppendLine("UPDATE SystemConfig");
                sbSql.AppendLine($"   SET AttributeValue = '{sValue}', AttributeText = '{sType}', AttributeDate = '{MyGlobal.DateTimeNow()}'");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'ParametersList'");
                sbSql.Append($"   AND AttributeName = '{sName}'");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }
            else
            {
                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("        (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue, AttributeText, AttributeDate)");
                sbSql.Append($" VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'ParametersList', '{sName}', '{sValue}', '{sType}', '{MyGlobal.DateTimeNow()}')");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }
        }

        private void c1GridVariables_FetchCellStyle(object sender, FetchCellStyleEventArgs e)
        {
            switch (e.Col)
            {
                case (int)_enumVariables.Name:
                    {
                        e.CellStyle.Locked = true; //變數名稱，此欄 Lock！
                        break;
                    }
                case (int)_enumVariables.Help:
                    {
                        e.CellStyle.Locked = true; //Help，此欄 Lock！
                        break;
                    }
                case (int)_enumVariables.Value:
                    {
                        e.CellStyle.BackColor = Color.LightYellow;

                        if (MyLibrary.IsDarkMode)
                        {
                            e.CellStyle.BackColor = ColorTranslator.FromHtml("#0F243E");
                            e.CellStyle.ForeColor = Color.White;
                        }

                        break;
                    }
            }
        }

        private void GridVisualStyle()
        {
            GridHelper.SetGridVisualStyle(c1GridVariables);
            c1GridVariables.Splits[0].ColumnCaptionHeight = 20;
        }

        private void GridFontAndBackColor()
        {
            const float iFontSize = 10;

            //字型 + 字體大小
            c1GridVariables.Font = new Font(MyLibrary.GridFontName, iFontSize, FontStyle.Regular, GraphicsUnit.Point);

            c1GridVariables.OddRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowForeColor);
            c1GridVariables.OddRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowBackColor);
            c1GridVariables.EvenRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowForeColor);
            c1GridVariables.EvenRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);

            //Grid's 選取顏色
            c1GridVariables.SelectedStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
            c1GridVariables.SelectedStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);

            if (!MyLibrary.IsDarkMode)
            {
                return;
            }

            GridHelper.SetGridVisualStyle(c1GridVariables, iFontSize);
            c1GridVariables.BackColor = ColorTranslator.FromHtml("#2D2D30");

            c1GridVariables.BorderColor = Color.White;
            c1GridVariables.HeadingStyle.Borders.Color = Color.White;
            c1GridVariables.HeadingStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridHeadingForeColor);
            c1GridVariables.RowDivider.Color = Color.White;
            c1GridVariables.Font = new Font(MyLibrary.GridFontName, MyLibrary.GridFontSize, FontStyle.Regular, GraphicsUnit.Point);
            c1GridVariables.HeadingStyle.Font = new Font(MyLibrary.GridFontName, MyLibrary.GridFontSize, FontStyle.Regular, GraphicsUnit.Point);
            c1GridVariables.MarqueeStyle = MarqueeEnum.FloatingEditor;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            ResolveResult = BuildResolveResult();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SaveVariablesList();

            ResolveResult = BuildResolveResult();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void SaveVariablesList()
        {
            var dt = c1GridVariables.GetDataTableSourceOrNull();

            if (dt == null)
            {
                return;
            }

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                var dr = dt.Rows[i];
                var name = dr.GetSafeString((int)_enumVariables.Name);
                var value = dr.GetSafeString((int)_enumVariables.Value);
                var type = dr.GetSafeString((int)_enumVariables.Type);

                if (!string.IsNullOrEmpty(value) && name != value)
                {
                    UpdateVariablesList(name.Replace("'", "''"), value.Replace("'", "''"), type);
                }
            }
        }

        private SqlVariablesResolveResult BuildResolveResult()
        {
            var dt = c1GridVariables.GetDataTableSourceOrNull();
            var parts = _request.VariablesWithPosition.Split(new[] { "`" }, StringSplitOptions.RemoveEmptyEntries);
            var mappingLines = new List<string>();
            var positionMapping = _request.VariablesWithPosition;

            editorSql.ReadOnly = false;
            editorSql.Text = TextHelper.GetSafeString(editorSql.Tag);

            //從最後一個變數開始置換指定值 (位置才不會因為「變數的長度 vs 指定值的長度」而改變)
            for (var i = parts.Length - 1; i >= 0; i--)
            {
                var parts2 = parts[i].Split('|');

                if (parts2.Length < 2)
                {
                    continue;
                }

                var name = parts2[0];

                int.TryParse(parts2[1], out var iPosition);

                var nameOriginal = _lstGridHeaderVariables[(int)_enumVariables.NameOriginal].ToString();
                var escapedName = name.Replace("'", "''");
                var rows = dt?.Select($"{nameOriginal} = '{escapedName}'") ?? Array.Empty<DataRow>();

                if (rows.Length <= 0)
                {
                    continue;
                }

                var type = rows[0][(int)_enumVariables.Type].ToString();
                var value = rows[0][(int)_enumVariables.Value].ToString();

                if (string.IsNullOrEmpty(value))
                {
                    value = type == ((int)_enumType.Number).ToString() ? "0" : "''";
                }
                else if (type == ((int)_enumType.String).ToString())
                {
                    value = $"'{value.Replace("'", "''")}'";
                }
                else if (type == ((int)_enumType.Number).ToString())
                {
                    value = MyGlobal.IsNumeric(value) ? value : "0";
                }

                mappingLines.Add($"{name} => {value}");
                positionMapping = positionMapping.Replace($"`{name}|{iPosition}`", $"`{name}|{iPosition}|{value}`");

                editorSql.SelectionStart = iPosition;
                editorSql.SelectionEnd = iPosition + name.Length;

                editorSql.ReplaceSelection(value);
            }

            //重新取得對應的顯示順序
            mappingLines.Reverse();

            editorSql.SelectionStart = 0;
            editorSql.SelectionEnd = 0;
            editorSql.ScrollCaret();
            editorSql.ReadOnly = true;

            return new SqlVariablesResolveResult
            {
                SqlText = editorSql.Text,
                MappingText = string.Join("\r\n", mappingLines),
                PositionMapping = positionMapping
            };
        }

        private void ApplySqlStyler()
        {
            editorSql.SetSelectionBackColor(true, ColorTranslator.FromHtml(MyLibrary.ColorSelectedTextBackground));
            editorSql.CaretLineBackColor = ColorTranslator.FromHtml(MyLibrary.ColorCurrentLineBackground);
            editorSql.SetWhitespaceForeColor(true, ColorTranslator.FromHtml(MyLibrary.ColorWhiteSpace));

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

            editorSql.Styler = new SqlStyler();
        }

        private void SetSqlText(string sText)
        {
            editorSql.ReadOnly = false;
            editorSql.Text = sText;
            editorSql.ReadOnly = true;
        }

        private static void c1GridVariables_ButtonClick(object sender, ColEventArgs e)
        {
            if (e.ColIndex != (int)_enumVariables.Help)
            {
                return;
            }

            using (var form = new VariableTypeHelpForm())
            {
                form.ShowDialog();
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            editorSql.SelectionStart = 0;
            editorSql.SelectionEnd = editorSql.Text.Length;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(string.Empty, false);
            editorSql.Copy(CopyFormat.Text | CopyFormat.Rtf | CopyFormat.Html);
        }

        private void btnWordWrap_Click(object sender, EventArgs e)
        {
            if (editorSql.WrapMode == WrapMode.Word)
            {
                btnWordWrap.Visible = true;
                btnWordWrap2.Visible = false;
                editorSql.WrapMode = WrapMode.None;
            }
            else
            {
                btnWordWrap.Visible = false;
                btnWordWrap2.Visible = true;
                editorSql.WrapMode = WrapMode.Word;
                editorSql.WrapVisualFlags = (WrapVisualFlags.Start) | (WrapVisualFlags.End) | (WrapVisualFlags.Margin);
            }

            //加上以下這個指令，取消 Word Wrap 後，Focus 才不會跑到最底部！
            editorSql.ScrollCaret();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
