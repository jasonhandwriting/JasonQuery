using JasonLibrary.Core;
using ScintillaNET;
using System.Drawing;

namespace JasonLibrary.UI.Controls.Stylers
{
    public class SqlStyler : ScintillaStyler
    {
        private const int NUMBER_MARGIN = 1;
        private const int BOOKMARK_MARGIN = 2;
        private const int BOOKMARK_MARKER = 2;

        public static string ColorEditorBackground = string.Empty;
        public static string ColorTextIdentifier = string.Empty;
        public static string ColorComments = string.Empty;
        public static string ColorNumber = string.Empty;
        public static string ColorString = string.Empty;
        public static string ColorCharacter = string.Empty;
        public static string ColorOperatorSymbol = string.Empty;
        public static string ColorUserDefinedTablesViews = string.Empty;
        public static string ColorUserDefinedFunctionsTriggers = string.Empty;
        public static string ColorOperatorKeywords = string.Empty;
        public static string ColorBuiltInFunctions = string.Empty;
        public static string ColorBuiltInKeywords = string.Empty;
        public static string ColorUserDefinedKeywords = string.Empty;
        public static bool IsKeywordFontBold = false;

        public static string KeywordsUserDefinedTables = string.Empty;
        public static string KeywordsUserDefinedViews = string.Empty;
        public static string KeywordsUserDefinedFunctions = string.Empty;
        public static string KeywordsUserDefinedTriggers = string.Empty;
        public static string KeywordsOperatorKeywords = string.Empty;
        public static string KeywordsBuiltInFunctions = string.Empty;
        public static string KeywordsBuiltInKeywords = string.Empty;
        public static string KeywordsUserDefinedKeywords = string.Empty;

        public SqlStyler() : base(Lexer.Sql, lineNumbers: true, codeFolding: true, braceMatching: true, autoIndent: true)
        { }

        public override void ApplyStyle(Scintilla scintilla)
        {
            // Set the Styles
            scintilla.Styles[Style.LineNumber].ForeColor = Color.FromArgb(255, 128, 128, 128);
            scintilla.Styles[Style.LineNumber].BackColor = Color.FromArgb(255, 228, 228, 228);

            var nums = scintilla.Margins[NUMBER_MARGIN];

            nums.Width = 2;
            //nums.Mask = 11;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;

            //var marker = scintilla.Markers[BOOKMARK_MARKER];
            //marker.Symbol = MarkerSymbol.Bookmark;
            //marker.SetBackColor(Color.DeepSkyBlue);
            //marker.SetForeColor(Color.Black);

            var marker = scintilla.Markers[BOOKMARK_MARGIN];

            marker.Symbol = MarkerSymbol.Bookmark;
            marker.SetBackColor(Color.DeepSkyBlue);
            marker.SetForeColor(Color.Black);

            if (MyLibrary.IsDarkMode)
            {
                foreach (var style in scintilla.Styles)
                {
                    style.BackColor = ColorTranslator.FromHtml("#2D2D30");
                }
            }

            var margin = scintilla.Margins[BOOKMARK_MARGIN];

            margin.Width = 12;
            //margin.Sensitive = true;
            //margin.Type = MarginType.Symbol;
            //margin.Mask = (1 << BOOKMARK_MARKER);

            scintilla.Styles[Style.Default].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            scintilla.Styles[Style.Sql.Default].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);

            scintilla.Styles[Style.Sql.Identifier].ForeColor = ColorTranslator.FromHtml(ColorTextIdentifier);
            scintilla.Styles[Style.Sql.Identifier].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            scintilla.Styles[Style.Sql.QOperator].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            scintilla.Styles[Style.Sql.QuotedIdentifier].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            scintilla.Styles[Style.Sql.SqlPlus].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            scintilla.Styles[Style.Sql.SqlPlusPrompt].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            scintilla.Styles[Style.Sql.SqlPlusComment].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);

            scintilla.Styles[Style.Sql.Comment].ForeColor = ColorTranslator.FromHtml(ColorComments);
            scintilla.Styles[Style.Sql.Comment].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            scintilla.Styles[Style.Sql.CommentLine].ForeColor = ColorTranslator.FromHtml(ColorComments);
            scintilla.Styles[Style.Sql.CommentLine].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            scintilla.Styles[Style.Sql.CommentDoc].ForeColor = ColorTranslator.FromHtml(ColorComments);
            scintilla.Styles[Style.Sql.CommentDoc].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            scintilla.Styles[Style.Sql.CommentLineDoc].ForeColor = ColorTranslator.FromHtml(ColorComments);
            scintilla.Styles[Style.Sql.CommentLineDoc].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            scintilla.Styles[Style.Sql.Number].ForeColor = ColorTranslator.FromHtml(ColorNumber);
            scintilla.Styles[Style.Sql.Number].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);

            //雙引號中間的字串
            scintilla.Styles[Style.Sql.String].ForeColor = ColorTranslator.FromHtml(ColorString);
            scintilla.Styles[Style.Sql.String].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);

            //單引號中間的字串
            scintilla.Styles[Style.Sql.Character].ForeColor = ColorTranslator.FromHtml(ColorCharacter);
            scintilla.Styles[Style.Sql.Character].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            //Operator Symbol
            scintilla.Styles[Style.Sql.Operator].ForeColor = ColorTranslator.FromHtml(ColorOperatorSymbol);
            scintilla.Styles[Style.Sql.Operator].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);

            scintilla.Styles[Style.Sql.Word].ForeColor = ColorTranslator.FromHtml(ColorUserDefinedTablesViews);
            scintilla.Styles[Style.Sql.Word].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            scintilla.Styles[Style.Sql.Word2].ForeColor = ColorTranslator.FromHtml(ColorUserDefinedFunctionsTriggers);
            scintilla.Styles[Style.Sql.Word2].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);

            //Operator Keywords
            scintilla.Styles[Style.Sql.User1].ForeColor = ColorTranslator.FromHtml(ColorOperatorKeywords);
            scintilla.Styles[Style.Sql.User1].Bold = IsKeywordFontBold;
            scintilla.Styles[Style.Sql.User1].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);

            scintilla.Styles[Style.Sql.User2].ForeColor = ColorTranslator.FromHtml(ColorBuiltInFunctions);
            scintilla.Styles[Style.Sql.User2].Bold = IsKeywordFontBold;
            scintilla.Styles[Style.Sql.User2].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);
            scintilla.Styles[Style.Sql.User3].ForeColor = ColorTranslator.FromHtml(ColorBuiltInKeywords);
            scintilla.Styles[Style.Sql.User3].Bold = IsKeywordFontBold;
            scintilla.Styles[Style.Sql.User3].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);

            //仿 Toad 的 table name color
            scintilla.Styles[Style.Sql.User4].ForeColor = ColorTranslator.FromHtml(ColorUserDefinedKeywords);
            scintilla.Styles[Style.Sql.User4].Bold = IsKeywordFontBold;
            scintilla.Styles[Style.Sql.User4].BackColor = ColorTranslator.FromHtml(ColorEditorBackground);

            if (MyLibrary.IsDarkMode)
            {
                scintilla.SetFoldMarginHighlightColor(true, ColorTranslator.FromHtml("#2D2D30"));
                scintilla.SetFoldMarginColor(true, ColorTranslator.FromHtml("#2D2D30"));

                scintilla.Styles[Style.Default].ForeColor = ColorTranslator.FromHtml(ColorTextIdentifier);
                scintilla.Styles[Style.Sql.Default].ForeColor = ColorTranslator.FromHtml(ColorTextIdentifier);

                scintilla.Styles[Style.Sql.SqlPlus].ForeColor = ColorTranslator.FromHtml(ColorTextIdentifier);
                scintilla.Styles[Style.Sql.SqlPlusPrompt].ForeColor = ColorTranslator.FromHtml(ColorTextIdentifier);
                scintilla.Styles[Style.Sql.SqlPlusComment].ForeColor = ColorTranslator.FromHtml(ColorTextIdentifier);
            }
        }

        public override void RemoveStyle(Scintilla scintilla)
        {

        }

        public override void SetKeywords(Scintilla scintilla)
        {
            //Set keyword lists
            //Word = 0, User-defined Tables & Views
            scintilla.SetKeywords(0, MyLibrary.KeywordsUserDefinedTables.ToLower() + MyLibrary.KeywordsUserDefinedViews.ToLower());

            //Word2 = 1, User-defined Functions & Triggers
            scintilla.SetKeywords(1, MyLibrary.KeywordsUserDefinedFunctions.ToLower() + MyLibrary.KeywordsUserDefinedTriggers.ToLower());

            //User1 = 4, Operator (Keywords)
            //scintilla.SetKeywords(4, "all and any between cross exists in inner is join left like not null or outer pivot right some unpivot ( ) * yyy");
            scintilla.SetKeywords(4, MyLibrary.KeywordsOperatorKeywords.ToLower());

            //User2 = 5, Built-in Functions 內建 Function, 依 Oracle / PostgreSQL 而有不同
            //SetKeywords(5, "sys objects sysobjects "); //可以挪為它用
            scintilla.SetKeywords(5, MyLibrary.KeywordsBuiltInFunctions.ToLower());

            //User3 = 6, Built-in Keywords 內建 Function, 依 Oracle / PostgreSQL 而有不同 (Color 定義在 SqlStyler.cs 的 ApplyStyle() 裡面)
            //scintilla.SetKeywords(6, JasonLibrary.MyLibrary.sKeyWord6);
            scintilla.SetKeywords(6, MyLibrary.KeywordsBuiltInKeywords.ToLower());

            //User4 = 7, User-defined Keywords 使用者自定的關鍵字 (Color 定義在 SqlStyler.cs 的 ApplyStyle() 裡面)，此為最後一組使用者自定義的顏色
            scintilla.SetKeywords(7, MyLibrary.KeywordsUserDefinedKeywords.ToLower());
        }
    }
}