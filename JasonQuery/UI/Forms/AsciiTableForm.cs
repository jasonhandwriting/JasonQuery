using C1.Win.C1Themes;
using C1.Win.C1TrueDBGrid;
using IconLibrary;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.UI.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class AsciiTableForm : Form
    {
        private DataTable _dtAsciiTable = new DataTable();
        private ContextMenuStrip _gridMenu = new ContextMenuStrip();
        private string _languageText = string.Empty;

        public AsciiTableForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                _dtAsciiTable.Columns.Add("Decimal");
                _dtAsciiTable.Columns.Add("Hexadecimal");
                _dtAsciiTable.Columns.Add("Binary");
                _dtAsciiTable.Columns.Add("Octal");
                _dtAsciiTable.Columns.Add("Value");
                _dtAsciiTable.Columns.Add("Description");

                CreateTableAsciiTable();

                LocalizationHelper.ApplyLanguageInfo(this, false, false);
                ApplyLocalizationSetting();

                c1Grid.DataSource = _dtAsciiTable;
                GridHelper.ReplaceColumnCaptionByLanguageInfo(c1Grid, Name);
                GridHelper.ResizeGridColumnWidth(c1Grid);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ApplyLocalizationSetting()
        {
            if (MyLibrary.IsDarkMode)
            {
                C1ThemeController.ApplicationTheme = "VS2013Dark";
            }

            GridHelper.SetGridVisualStyle(c1Grid);
            GridFontAndBackColor();
            GridZoom();

            GridHelper.SetGridVisualStyle(c1Grid, 10);
            c1Grid.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);
        }

        private void GridFontAndBackColor()
        {
            const int fontSize = 10;

            c1Grid.Font = new Font(MyLibrary.GridFontName, fontSize, FontStyle.Regular, GraphicsUnit.Point);
            c1Grid.OddRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowForeColor);
            c1Grid.OddRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridOddRowBackColor);
            c1Grid.EvenRowStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowForeColor);
            c1Grid.EvenRowStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridEvenRowBackColor);
            c1Grid.SelectedStyle.ForeColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedForeColor);
            c1Grid.SelectedStyle.BackColor = ColorTranslator.FromHtml(MyLibrary.GridSelectedBackColor);
        }

        private void GridZoom()
        {
            const int fontSize = 10;
            const float pcnt = 1.0F;
            var rowHeight = c1Grid.RowHeight;

            c1Grid.RowHeight = (int)(rowHeight * pcnt) + 5;
            c1Grid.Splits[0].ColumnCaptionHeight = (int)(rowHeight * pcnt) + 5;
            c1Grid.Styles["Normal"].Font = new Font(c1Grid.Styles["Normal"].Font.FontFamily, fontSize * pcnt);
        }

        private void SelectAll()
        {
            var currentRow = c1Grid.Row;
            var currentCol = c1Grid.Col;
            var count = c1Grid.Splits[0].Rows.Count;

            c1Grid.SelectedRows.Clear(); //清除所有已選取的 Row

            for (var i = 0; i < count; i++)
            {
                c1Grid.SelectedRows.Add(i);
            }

            c1Grid.Row = currentRow;
            c1Grid.Col = currentCol;
            c1Grid.Select();
        }

        private void CreateTableAsciiTable()
        {
            var row = _dtAsciiTable.NewRow();

            row["Decimal"] = "000";
            row["Hexadecimal"] = "000";
            row["Binary"] = "00000000";
            row["Octal"] = "000";
            row["Value"] = "NUL";
            row["Description"] = "(Null char.)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "001";
            row["Hexadecimal"] = "001";
            row["Binary"] = "00000001";
            row["Octal"] = "001";
            row["Value"] = "SOH";
            row["Description"] = "(Start of Header)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "002";
            row["Hexadecimal"] = "002";
            row["Binary"] = "00000010";
            row["Octal"] = "002";
            row["Value"] = "STX";
            row["Description"] = "(Start of Text)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "003";
            row["Hexadecimal"] = "003";
            row["Binary"] = "00000011";
            row["Octal"] = "003";
            row["Value"] = "ETX";
            row["Description"] = "(End of Text)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "004";
            row["Hexadecimal"] = "004";
            row["Binary"] = "00000100";
            row["Octal"] = "004";
            row["Value"] = "EOT";
            row["Description"] = "(End of Transmission)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "005";
            row["Hexadecimal"] = "005";
            row["Binary"] = "00000101";
            row["Octal"] = "005";
            row["Value"] = "ENQ";
            row["Description"] = "(Enquiry)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "006";
            row["Hexadecimal"] = "006";
            row["Binary"] = "00000110";
            row["Octal"] = "006";
            row["Value"] = "ACK";
            row["Description"] = "(Acknowledgment)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "007";
            row["Hexadecimal"] = "007";
            row["Binary"] = "00000111";
            row["Octal"] = "007";
            row["Value"] = "BEL";
            row["Description"] = "(Bell)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "008";
            row["Hexadecimal"] = "008";
            row["Binary"] = "00001000";
            row["Octal"] = "010";
            row["Value"] = "BS";
            row["Description"] = "(Backspace)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "009";
            row["Hexadecimal"] = "009";
            row["Binary"] = "00001001";
            row["Octal"] = "011";
            row["Value"] = "HT";
            row["Description"] = "(Horizontal Tab)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "010";
            row["Hexadecimal"] = "00A";
            row["Binary"] = "00001010";
            row["Octal"] = "012";
            row["Value"] = "LF";
            row["Description"] = "(Line Feed)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "011";
            row["Hexadecimal"] = "00B";
            row["Binary"] = "00001011";
            row["Octal"] = "013";
            row["Value"] = "VT";
            row["Description"] = "(Vertical Tab)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "012";
            row["Hexadecimal"] = "00C";
            row["Binary"] = "00001100";
            row["Octal"] = "014";
            row["Value"] = "FF";
            row["Description"] = "(Form Feed)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "013";
            row["Hexadecimal"] = "00D";
            row["Binary"] = "00001101";
            row["Octal"] = "015";
            row["Value"] = "CR";
            row["Description"] = "(Carriage Return)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "014";
            row["Hexadecimal"] = "00E";
            row["Binary"] = "00001110";
            row["Octal"] = "016";
            row["Value"] = "SO";
            row["Description"] = "(Shift Out)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "015";
            row["Hexadecimal"] = "00F";
            row["Binary"] = "00001111";
            row["Octal"] = "017";
            row["Value"] = "SI";
            row["Description"] = "(Shift In)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "016";
            row["Hexadecimal"] = "010";
            row["Binary"] = "00010000";
            row["Octal"] = "020";
            row["Value"] = "DLE";
            row["Description"] = "(Data Link Escape)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "017";
            row["Hexadecimal"] = "011";
            row["Binary"] = "00010001";
            row["Octal"] = "021";
            row["Value"] = "DC1";
            row["Description"] = "(XON)(Device Control 1)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "018";
            row["Hexadecimal"] = "012";
            row["Binary"] = "00010010";
            row["Octal"] = "022";
            row["Value"] = "DC2";
            row["Description"] = "(Device Control 2)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "019";
            row["Hexadecimal"] = "013";
            row["Binary"] = "00010011";
            row["Octal"] = "023";
            row["Value"] = "DC3";
            row["Description"] = "(XOFF)(Device Control 3)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "020";
            row["Hexadecimal"] = "014";
            row["Binary"] = "00010100";
            row["Octal"] = "024";
            row["Value"] = "DC4";
            row["Description"] = "(Device Control 4)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "021";
            row["Hexadecimal"] = "015";
            row["Binary"] = "00010101";
            row["Octal"] = "025";
            row["Value"] = "NAK";
            row["Description"] = "(Negative Acknowledgement)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "022";
            row["Hexadecimal"] = "016";
            row["Binary"] = "00010110";
            row["Octal"] = "026";
            row["Value"] = "SYN";
            row["Description"] = "(Synchronous Idle)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "023";
            row["Hexadecimal"] = "017";
            row["Binary"] = "00010111";
            row["Octal"] = "027";
            row["Value"] = "ETB";
            row["Description"] = "(End of Trans. Block)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "024";
            row["Hexadecimal"] = "018";
            row["Binary"] = "00011000";
            row["Octal"] = "030";
            row["Value"] = "CAN";
            row["Description"] = "(Cancel)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "025";
            row["Hexadecimal"] = "019";
            row["Binary"] = "00011001";
            row["Octal"] = "031";
            row["Value"] = "EM";
            row["Description"] = "(End of Medium)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "026";
            row["Hexadecimal"] = "01A";
            row["Binary"] = "00011010";
            row["Octal"] = "032";
            row["Value"] = "SUB";
            row["Description"] = "(Substitute)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "027";
            row["Hexadecimal"] = "01B";
            row["Binary"] = "00011011";
            row["Octal"] = "033";
            row["Value"] = "ESC";
            row["Description"] = "(Escape)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "028";
            row["Hexadecimal"] = "01C";
            row["Binary"] = "00011100";
            row["Octal"] = "034";
            row["Value"] = "FS";
            row["Description"] = "(File Separator)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "029";
            row["Hexadecimal"] = "01D";
            row["Binary"] = "00011101";
            row["Octal"] = "035";
            row["Value"] = "GS";
            row["Description"] = "(Group Separator)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "030";
            row["Hexadecimal"] = "01E";
            row["Binary"] = "00011110";
            row["Octal"] = "036";
            row["Value"] = "RS";
            row["Description"] = "(Request to Send)(Record Separator)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "031";
            row["Hexadecimal"] = "01F";
            row["Binary"] = "00011111";
            row["Octal"] = "037";
            row["Value"] = "US";
            row["Description"] = "(Unit Separator)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "032";
            row["Hexadecimal"] = "020";
            row["Binary"] = "00100000";
            row["Octal"] = "040";
            row["Value"] = "SP";
            row["Description"] = "(Space)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "033";
            row["Hexadecimal"] = "021";
            row["Binary"] = "00100001";
            row["Octal"] = "041";
            row["Value"] = "!";
            row["Description"] = "(exclamation mark)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "034";
            row["Hexadecimal"] = "022";
            row["Binary"] = "00100010";
            row["Octal"] = "042";
            row["Value"] = "\"";
            row["Description"] = "(double quote)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "035";
            row["Hexadecimal"] = "023";
            row["Binary"] = "00100011";
            row["Octal"] = "043";
            row["Value"] = "#";
            row["Description"] = "(number sign)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "036";
            row["Hexadecimal"] = "024";
            row["Binary"] = "00100100";
            row["Octal"] = "044";
            row["Value"] = "$";
            row["Description"] = "(dollar sign)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "037";
            row["Hexadecimal"] = "025";
            row["Binary"] = "00100101";
            row["Octal"] = "045";
            row["Value"] = "%";
            row["Description"] = "(percent)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "038";
            row["Hexadecimal"] = "026";
            row["Binary"] = "00100110";
            row["Octal"] = "046";
            row["Value"] = "&";
            row["Description"] = "(ampersand)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "039";
            row["Hexadecimal"] = "027";
            row["Binary"] = "00100111";
            row["Octal"] = "047";
            row["Value"] = "'";
            row["Description"] = "(single quote)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "040";
            row["Hexadecimal"] = "028";
            row["Binary"] = "00101000";
            row["Octal"] = "050";
            row["Value"] = "(";
            row["Description"] = "(left/opening parenthesis)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "041";
            row["Hexadecimal"] = "029";
            row["Binary"] = "00101001";
            row["Octal"] = "051";
            row["Value"] = ")";
            row["Description"] = "(right/closing parenthesis)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "042";
            row["Hexadecimal"] = "02A";
            row["Binary"] = "00101010";
            row["Octal"] = "052";
            row["Value"] = "*";
            row["Description"] = "(asterisk)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "043";
            row["Hexadecimal"] = "02B";
            row["Binary"] = "00101011";
            row["Octal"] = "053";
            row["Value"] = "+";
            row["Description"] = "(plus)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "044";
            row["Hexadecimal"] = "02C";
            row["Binary"] = "00101100";
            row["Octal"] = "054";
            row["Value"] = ",";
            row["Description"] = "(comma)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "045";
            row["Hexadecimal"] = "02D";
            row["Binary"] = "00101101";
            row["Octal"] = "055";
            row["Value"] = "-";
            row["Description"] = "(minus or dash)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "046";
            row["Hexadecimal"] = "02E";
            row["Binary"] = "00101110";
            row["Octal"] = "056";
            row["Value"] = ".";
            row["Description"] = "(dot)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "047";
            row["Hexadecimal"] = "02F";
            row["Binary"] = "00101111";
            row["Octal"] = "057";
            row["Value"] = "/";
            row["Description"] = "(forward slash)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "048";
            row["Hexadecimal"] = "030";
            row["Binary"] = "00110000";
            row["Octal"] = "060";
            row["Value"] = "0";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "049";
            row["Hexadecimal"] = "031";
            row["Binary"] = "00110001";
            row["Octal"] = "061";
            row["Value"] = "1";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "050";
            row["Hexadecimal"] = "032";
            row["Binary"] = "00110010";
            row["Octal"] = "062";
            row["Value"] = "2";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "051";
            row["Hexadecimal"] = "033";
            row["Binary"] = "00110011";
            row["Octal"] = "063";
            row["Value"] = "3";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "052";
            row["Hexadecimal"] = "034";
            row["Binary"] = "00110100";
            row["Octal"] = "064";
            row["Value"] = "4";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "053";
            row["Hexadecimal"] = "035";
            row["Binary"] = "00110101";
            row["Octal"] = "065";
            row["Value"] = "5";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "054";
            row["Hexadecimal"] = "036";
            row["Binary"] = "00110110";
            row["Octal"] = "066";
            row["Value"] = "6";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "055";
            row["Hexadecimal"] = "037";
            row["Binary"] = "00110111";
            row["Octal"] = "067";
            row["Value"] = "7";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "056";
            row["Hexadecimal"] = "038";
            row["Binary"] = "00111000";
            row["Octal"] = "070";
            row["Value"] = "8";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "057";
            row["Hexadecimal"] = "039";
            row["Binary"] = "00111001";
            row["Octal"] = "071";
            row["Value"] = "9";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "058";
            row["Hexadecimal"] = "03A";
            row["Binary"] = "00111010";
            row["Octal"] = "072";
            row["Value"] = ":";
            row["Description"] = "(colon)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "059";
            row["Hexadecimal"] = "03B";
            row["Binary"] = "00111011";
            row["Octal"] = "073";
            row["Value"] = ";";
            row["Description"] = "(semi-colon)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "060";
            row["Hexadecimal"] = "03C";
            row["Binary"] = "00111100";
            row["Octal"] = "074";
            row["Value"] = "<";
            row["Description"] = "(less than)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "061";
            row["Hexadecimal"] = "03D";
            row["Binary"] = "00111101";
            row["Octal"] = "075";
            row["Value"] = "=";
            row["Description"] = "(equal sign)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "062";
            row["Hexadecimal"] = "03E";
            row["Binary"] = "00111110";
            row["Octal"] = "076";
            row["Value"] = ">";
            row["Description"] = "(greater than)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "063";
            row["Hexadecimal"] = "03F";
            row["Binary"] = "00111111";
            row["Octal"] = "077";
            row["Value"] = "?";
            row["Description"] = "(question mark)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "064";
            row["Hexadecimal"] = "040";
            row["Binary"] = "01000000";
            row["Octal"] = "100";
            row["Value"] = "@";
            row["Description"] = "(AT symbol)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "065";
            row["Hexadecimal"] = "041";
            row["Binary"] = "01000001";
            row["Octal"] = "101";
            row["Value"] = "A";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "066";
            row["Hexadecimal"] = "042";
            row["Binary"] = "01000010";
            row["Octal"] = "102";
            row["Value"] = "B";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "067";
            row["Hexadecimal"] = "043";
            row["Binary"] = "01000011";
            row["Octal"] = "103";
            row["Value"] = "C";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "068";
            row["Hexadecimal"] = "044";
            row["Binary"] = "01000100";
            row["Octal"] = "104";
            row["Value"] = "D";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "069";
            row["Hexadecimal"] = "045";
            row["Binary"] = "01000101";
            row["Octal"] = "105";
            row["Value"] = "E";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "070";
            row["Hexadecimal"] = "046";
            row["Binary"] = "01000110";
            row["Octal"] = "106";
            row["Value"] = "F";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "071";
            row["Hexadecimal"] = "047";
            row["Binary"] = "01000111";
            row["Octal"] = "107";
            row["Value"] = "G";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "072";
            row["Hexadecimal"] = "048";
            row["Binary"] = "01001000";
            row["Octal"] = "110";
            row["Value"] = "H";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "073";
            row["Hexadecimal"] = "049";
            row["Binary"] = "01001001";
            row["Octal"] = "111";
            row["Value"] = "I";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "074";
            row["Hexadecimal"] = "04A";
            row["Binary"] = "01001010";
            row["Octal"] = "112";
            row["Value"] = "J";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "075";
            row["Hexadecimal"] = "04B";
            row["Binary"] = "01001011";
            row["Octal"] = "113";
            row["Value"] = "K";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "076";
            row["Hexadecimal"] = "04C";
            row["Binary"] = "01001100";
            row["Octal"] = "114";
            row["Value"] = "L";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "077";
            row["Hexadecimal"] = "04D";
            row["Binary"] = "01001101";
            row["Octal"] = "115";
            row["Value"] = "M";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "078";
            row["Hexadecimal"] = "04E";
            row["Binary"] = "01001110";
            row["Octal"] = "116";
            row["Value"] = "N";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "079";
            row["Hexadecimal"] = "04F";
            row["Binary"] = "01001111";
            row["Octal"] = "117";
            row["Value"] = "O";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "080";
            row["Hexadecimal"] = "050";
            row["Binary"] = "01010000";
            row["Octal"] = "120";
            row["Value"] = "P";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "081";
            row["Hexadecimal"] = "051";
            row["Binary"] = "01010001";
            row["Octal"] = "121";
            row["Value"] = "Q";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "082";
            row["Hexadecimal"] = "052";
            row["Binary"] = "01010010";
            row["Octal"] = "122";
            row["Value"] = "R";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "083";
            row["Hexadecimal"] = "053";
            row["Binary"] = "01010011";
            row["Octal"] = "123";
            row["Value"] = "S";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "084";
            row["Hexadecimal"] = "054";
            row["Binary"] = "01010100";
            row["Octal"] = "124";
            row["Value"] = "T";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "085";
            row["Hexadecimal"] = "055";
            row["Binary"] = "01010101";
            row["Octal"] = "125";
            row["Value"] = "U";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "086";
            row["Hexadecimal"] = "056";
            row["Binary"] = "01010110";
            row["Octal"] = "126";
            row["Value"] = "V";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "087";
            row["Hexadecimal"] = "057";
            row["Binary"] = "01010111";
            row["Octal"] = "127";
            row["Value"] = "W";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "088";
            row["Hexadecimal"] = "058";
            row["Binary"] = "01011000";
            row["Octal"] = "130";
            row["Value"] = "X";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "089";
            row["Hexadecimal"] = "059";
            row["Binary"] = "01011001";
            row["Octal"] = "131";
            row["Value"] = "Y";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "090";
            row["Hexadecimal"] = "05A";
            row["Binary"] = "01011010";
            row["Octal"] = "132";
            row["Value"] = "Z";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "091";
            row["Hexadecimal"] = "05B";
            row["Binary"] = "01011011";
            row["Octal"] = "133";
            row["Value"] = "[";
            row["Description"] = "(left/opening bracket)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "092";
            row["Hexadecimal"] = "05C";
            row["Binary"] = "01011100";
            row["Octal"] = "134";
            row["Value"] = "\\";
            row["Description"] = "(back slash)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "093";
            row["Hexadecimal"] = "05D";
            row["Binary"] = "01011101";
            row["Octal"] = "135";
            row["Value"] = "]";
            row["Description"] = "(right/closing bracket)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "094";
            row["Hexadecimal"] = "05E";
            row["Binary"] = "01011110";
            row["Octal"] = "136";
            row["Value"] = "^";
            row["Description"] = "(caret/circumflex)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "095";
            row["Hexadecimal"] = "05F";
            row["Binary"] = "01011111";
            row["Octal"] = "137";
            row["Value"] = "_";
            row["Description"] = "(underscore)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "096";
            row["Hexadecimal"] = "060";
            row["Binary"] = "01100000";
            row["Octal"] = "140";
            row["Value"] = "`";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "097";
            row["Hexadecimal"] = "061";
            row["Binary"] = "01100001";
            row["Octal"] = "141";
            row["Value"] = "a";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "098";
            row["Hexadecimal"] = "062";
            row["Binary"] = "01100010";
            row["Octal"] = "142";
            row["Value"] = "b";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "099";
            row["Hexadecimal"] = "063";
            row["Binary"] = "01100011";
            row["Octal"] = "143";
            row["Value"] = "c";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "100";
            row["Hexadecimal"] = "064";
            row["Binary"] = "01100100";
            row["Octal"] = "144";
            row["Value"] = "d";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "101";
            row["Hexadecimal"] = "065";
            row["Binary"] = "01100101";
            row["Octal"] = "145";
            row["Value"] = "e";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "102";
            row["Hexadecimal"] = "066";
            row["Binary"] = "01100110";
            row["Octal"] = "146";
            row["Value"] = "f";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "103";
            row["Hexadecimal"] = "067";
            row["Binary"] = "01100111";
            row["Octal"] = "147";
            row["Value"] = "g";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "104";
            row["Hexadecimal"] = "068";
            row["Binary"] = "01101000";
            row["Octal"] = "150";
            row["Value"] = "h";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "105";
            row["Hexadecimal"] = "069";
            row["Binary"] = "01101001";
            row["Octal"] = "151";
            row["Value"] = "i";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "106";
            row["Hexadecimal"] = "06A";
            row["Binary"] = "01101010";
            row["Octal"] = "152";
            row["Value"] = "j";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "107";
            row["Hexadecimal"] = "06B";
            row["Binary"] = "01101011";
            row["Octal"] = "153";
            row["Value"] = "k";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "108";
            row["Hexadecimal"] = "06C";
            row["Binary"] = "01101100";
            row["Octal"] = "154";
            row["Value"] = "l";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "109";
            row["Hexadecimal"] = "06D";
            row["Binary"] = "01101101";
            row["Octal"] = "155";
            row["Value"] = "m";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "110";
            row["Hexadecimal"] = "06E";
            row["Binary"] = "01101110";
            row["Octal"] = "156";
            row["Value"] = "n";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "111";
            row["Hexadecimal"] = "06F";
            row["Binary"] = "01101111";
            row["Octal"] = "157";
            row["Value"] = "o";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "112";
            row["Hexadecimal"] = "070";
            row["Binary"] = "01110000";
            row["Octal"] = "160";
            row["Value"] = "p";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "113";
            row["Hexadecimal"] = "071";
            row["Binary"] = "01110001";
            row["Octal"] = "161";
            row["Value"] = "q";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "114";
            row["Hexadecimal"] = "072";
            row["Binary"] = "01110010";
            row["Octal"] = "162";
            row["Value"] = "r";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "115";
            row["Hexadecimal"] = "073";
            row["Binary"] = "01110011";
            row["Octal"] = "163";
            row["Value"] = "s";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "116";
            row["Hexadecimal"] = "074";
            row["Binary"] = "01110100";
            row["Octal"] = "164";
            row["Value"] = "t";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "117";
            row["Hexadecimal"] = "075";
            row["Binary"] = "01110101";
            row["Octal"] = "165";
            row["Value"] = "u";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "118";
            row["Hexadecimal"] = "076";
            row["Binary"] = "01110110";
            row["Octal"] = "166";
            row["Value"] = "v";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "119";
            row["Hexadecimal"] = "077";
            row["Binary"] = "01110111";
            row["Octal"] = "167";
            row["Value"] = "w";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "120";
            row["Hexadecimal"] = "078";
            row["Binary"] = "01111000";
            row["Octal"] = "170";
            row["Value"] = "x";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "121";
            row["Hexadecimal"] = "079";
            row["Binary"] = "01111001";
            row["Octal"] = "171";
            row["Value"] = "y";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "122";
            row["Hexadecimal"] = "07A";
            row["Binary"] = "01111010";
            row["Octal"] = "172";
            row["Value"] = "z";
            row["Description"] = string.Empty;
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "123";
            row["Hexadecimal"] = "07B";
            row["Binary"] = "01111011";
            row["Octal"] = "173";
            row["Value"] = "{";
            row["Description"] = "(left/opening brace)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "124";
            row["Hexadecimal"] = "07C";
            row["Binary"] = "01111100";
            row["Octal"] = "174";
            row["Value"] = "|";
            row["Description"] = "(vertical bar)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "125";
            row["Hexadecimal"] = "07D";
            row["Binary"] = "01111101";
            row["Octal"] = "175";
            row["Value"] = "}";
            row["Description"] = "(right/closing brace)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "126";
            row["Hexadecimal"] = "07E";
            row["Binary"] = "01111110";
            row["Octal"] = "176";
            row["Value"] = "~";
            row["Description"] = "(tilde)";
            _dtAsciiTable.Rows.Add(row);

            row = _dtAsciiTable.NewRow();
            row["Decimal"] = "127";
            row["Hexadecimal"] = "07F";
            row["Binary"] = "01111111";
            row["Octal"] = "177";
            row["Value"] = "DEL";
            row["Description"] = "(delete)";
            _dtAsciiTable.Rows.Add(row);
        }

        private void c1Grid_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            _gridMenu = new ContextMenuStrip();

            _languageText = LocalizationHelper.GetLanguageString("Select All", "form", GetType().Name, "menugrid", "SelectAll", "Text");
            _gridMenu.Items.Add(_languageText);

            _gridMenu.Items[0].Click += delegate
            {
                SelectAll();
            };

            _gridMenu.Items[0].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Select All 16x16.ico");

            _languageText = LocalizationHelper.GetLanguageString("Copy", "form", GetType().Name, "menugrid", "Copy", "Text");
            _gridMenu.Items.Add(_languageText);

            _gridMenu.Items[1].Click += delegate
            {
                CopyDataFromDataGrid();
            };

            _gridMenu.Items[1].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Copy2Script 16x16.ico");

            _gridMenu.Items.Add("-");

            _languageText = LocalizationHelper.GetLanguageString("Export to File", "form", GetType().Name, "menugrid", "ExportToFile", "Text");
            _gridMenu.Items.Add(_languageText);

            _gridMenu.Items[3].Click += delegate
            {
                ExportToFile();
            };

            _gridMenu.Items[3].Image = IconManager.GetImage(MyGlobal.IconLibrary, "Export 16x16.ico");

            if (DatabaseSqlExecutor.CurrentDataSource == DataSourceType.None)
            {
                //未連線的情況下如果要匯出，匯出功能就要改寫過，故，此處直接設為「不可匯出」
                _gridMenu.Items[3].Enabled = false;
            }

            if (MyLibrary.IsDarkMode)
            {
                _gridMenu.BackColor = ColorTranslator.FromHtml("#2D2D30");
                _gridMenu.ForeColor = Color.White;
                _gridMenu.RenderMode = ToolStripRenderMode.System;
            }

            c1Grid.ContextMenuStrip = _gridMenu;
            _gridMenu.Show(c1Grid, new Point(e.X, e.Y));
        }

        private void CopyDataFromDataGrid()
        {
            var i = 0;
            var data = string.Empty;
            var columnName = string.Empty;
            var dataType = string.Empty;
            var isActiveCell = true; //是否為「只點選單一個 cell，並沒有『選取範圍』」?
            var selectedCols = c1Grid.SelectedCols.Count;
            var quotingWith = string.Empty;
            var fieldSeparator = ",";
            var isSelectedWholeColumn = c1Grid.SelectedRows.Count == 0 && c1Grid.SelectedCols.Count > 0;

            try
            {
                if (isSelectedWholeColumn) //整欄選取
                {
                    isActiveCell = false;

                    var count = c1Grid.Splits[0].Rows.Count;

                    for (var row = 0; row < count; row++)
                    {
                        foreach (C1DataColumn column in c1Grid.SelectedCols)
                        {
                            var caption = c1Grid.Columns[column.ToString()].Caption;
                            var cellText = c1Grid.Columns[column.ToString()].CellText(row);
                            string[] splitters = { "\r\n", "\r", "\n" };
                            var parts = caption.Split(splitters, 2, StringSplitOptions.None);
                            var temp1 = parts[0];
                            var temp2 = parts.Length > 1 ? parts[1] : string.Empty;

                            if (row == 0)
                            {
                                //收集 Column Name & Data Type
                                columnName += $"{temp1}{fieldSeparator}";
                                dataType += string.IsNullOrEmpty(temp2) ? string.Empty : $"{temp2}{fieldSeparator}";
                            }

                            data += $"{quotingWith}{cellText}{quotingWith}{fieldSeparator}";
                        }

                        if (!string.IsNullOrEmpty(data))
                        {
                            var temp = data.Substring(0, data.Length - fieldSeparator.Length);

                            data = $"{temp}\r\n";
                        }
                    }
                }
                else //非整欄選取
                {
                    foreach (int row in c1Grid.SelectedRows)
                    {
                        var vr = c1Grid.Splits[0].Rows[row];
                        string temp1;
                        string temp2;

                        if (selectedCols == 0) //整列選取
                        {
                            isActiveCell = false;

                            foreach (C1DataColumn column in c1Grid.Columns)
                            {
                                var caption = column.Caption;
                                var cellText = column.CellText(vr.DataRowIndex);
                                string[] splitters = { "\r\n", "\r", "\n" };
                                var parts = caption.Split(splitters, 2, StringSplitOptions.None);

                                temp1 = parts[0];
                                temp2 = parts.Length > 1 ? parts[1] : string.Empty;

                                if (i == 0)
                                {
                                    //收集 Column Name & Data Type
                                    columnName += $"{temp1}{fieldSeparator}";
                                    dataType += string.IsNullOrEmpty(temp2) ? string.Empty : $"{temp2}{fieldSeparator}";
                                }

                                data += $"{cellText}{fieldSeparator}";
                            }

                            i++;
                        }
                        else //非整列選取 (選取區塊)
                        {
                            foreach (C1DataColumn column in c1Grid.SelectedCols)
                            {
                                isActiveCell = false;

                                var cellText = column.CellText(vr.DataRowIndex);
                                var caption = column.Caption;
                                string[] splitters = { "\r\n", "\r", "\n" };
                                var parts = caption.Split(splitters, 2, StringSplitOptions.None);

                                temp1 = parts[0];
                                temp2 = parts.Length > 1 ? parts[1] : string.Empty;

                                if (i == 0)
                                {
                                    //收集 Column Name & Data Type
                                    columnName += $"{temp1}{fieldSeparator}";
                                    dataType += string.IsNullOrEmpty(temp2) ? string.Empty : $"{temp2}{fieldSeparator}";
                                }

                                data += $"{cellText}{fieldSeparator}";
                            }

                            i++;
                        }

                        if (!string.IsNullOrEmpty(data))
                        {
                            var temp = data.Substring(0, data.Length - fieldSeparator.Length);

                            data = $"{temp}\r\n";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(columnName))
                {
                    var temp = columnName.Substring(0, columnName.Length - fieldSeparator.Length);

                    columnName = $"{temp}\r\n";
                }

                if (!string.IsNullOrEmpty(dataType))
                {
                    var temp = dataType.Substring(0, dataType.Length - fieldSeparator.Length);

                    dataType = $"{temp}\r\n";
                }

                if (isActiveCell)
                {
                    data = c1Grid[c1Grid.Splits[0].Rows[c1Grid.Row].DataRowIndex, c1Grid.Col].ToString();
                }

                CopyTextToClipboard($"{columnName}{dataType}{data}");
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void CopyTextToClipboard(string text)
        {
            try
            {
                Clipboard.Clear();
                Clipboard.SetDataObject(text, true, 5, 200);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ExportToFile()
        {
            using (var form = new ExportToFileForm())
            {
                var dt = c1Grid.GetDataTableSourceOrNull();

                dt = GridHelper.ReplaceColumnNameByLanguageInfo(dt, Name);
                GridHelper.ResizeGridColumnWidth(c1Grid);

                form.Title = Text;
                form.dtData = dt;
                form.SheetName = Text;
                form.FontName = c1Grid.Font.Name;
                form.FontSize = c1Grid.Font.Size;
                form.ShowDialog();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                    {
                        Close();
                        return true;
                    }
                case Keys.Control | Keys.A:
                    {
                        SelectAll();
                        return true;
                    }
                case Keys.Control | Keys.C:
                case Keys.Control | Keys.Insert:
                    {
                        CopyDataFromDataGrid();
                        return true;
                    }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}