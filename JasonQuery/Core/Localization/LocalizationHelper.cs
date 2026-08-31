using C1.Win.C1Command;
using C1.Win.C1Input;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using JasonQuery.UI.Services;
using JasonQuery.UI.Forms;

namespace JasonQuery.Core.Localization
{
    public static class LocalizationHelper
    {
        public static string LocalizationList = string.Empty; //語系清單
        public static string Localization = "English"; //預設語系
        public static string LocalizationCode = "en-US";
        public static string XmlFileName = "english.xml";
        public static Dictionary<string, string> LocalizationMap = new Dictionary<string, string>(); //語系字典

        //20250507 改寫 dtLocalization
        private static readonly object _syncRoot = new object();
        private static Dictionary<(string, string, string, string, string), string> _localizationCache = new Dictionary<(string, string, string, string, string), string>();
        private static DataTable _localizationData;

        public static DataTable dtLocalization
        {
            get => _localizationData;
            set
            {
                lock (_syncRoot)
                {
                    _localizationData = value;
                    RefreshLocalizationCache(); //切換語系時，更新快取
                }
            }
        }

        public static DataTable XmlToDataTable(string fileName)
        {
            var dt = new DataTable();

            try
            {
                var xml = File.ReadAllText(fileName);
                var xmldoc = new XmlDocument();

                xmldoc.LoadXml(xml);

                var xmlreader = XmlReader.Create(new StringReader(xmldoc.OuterXml));
                var ds = new DataSet();

                ds.ReadXml(xmlreader);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {
                var message = ExceptionDialogService.BuildMessage(ex);

                MessageBox.Show(text: $"An error has occurred while loading localization file:\r\n\r\n{fileName}\r\n\r\n{message}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return dt;
        }

        public static string GetLanguageString(string originalText, string category, string className, string typeName, string id, string attribute)
        {
            id = id ?? string.Empty;

            var result = originalText;

            switch (className)
            {
                case nameof(ConnectionForm):
                    {
                        id = Regex.Replace(id, "_(PostgreSQL|Oracle|SQLServer|MySQL|SQLite)$", string.Empty);
                        break;
                    }
            }

            try
            {
                var key = CreateLocalizationKey(category, className, typeName, id, attribute);

                lock (_syncRoot)
                {
                    if (_localizationCache.TryGetValue(key, out var localizedText) && !string.IsNullOrWhiteSpace(localizedText))
                    {
                        return localizedText;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionDialogService.Show(ex);
            }

            return originalText;
        }

        /// <summary>
        /// 遞迴遍歷所有 Control 及 ToolStripItem，並套用多語系
        /// </summary>
        public static void ApplyLanguageInfo(Form form, bool applyColor = true, bool applyDarkMode = true)
        {
            Color normalColor = (MyLibrary.IsDarkMode && applyDarkMode) ? Color.White : Color.Black;
            Color highlightColor = Color.Red;

            //Form 標題
            form.Text = GetLanguageString(form.Text, "form", form.GetType().Name, "object", "this", "Text");

            var visitedControls = new HashSet<Control>();
            var visitedToolStripItems = new HashSet<ToolStripItem>();

            foreach (var node in TraverseElements(form, visitedControls, visitedToolStripItems))
            {
                switch (node)
                {
                    case Control ctl:
                        {
                            HandleControl(ctl, form, normalColor, highlightColor, applyColor);
                            break;
                        }
                    case ToolStripItem item:
                        {
                            HandleToolStripItem(item, form, normalColor);
                            break;
                        }
                }
            }
        }

        //遞迴產生 Control、SplitContainer Panels、C1DockingTab Pages 及 ToolStripItems
        private static IEnumerable<object> TraverseElements(Control root, HashSet<Control> visitedControls, HashSet<ToolStripItem> visitedToolStripItems)
        {
            if (root == null)
            {
                yield break;
            }

            if (!visitedControls.Add(root))
            {
                yield break;
            }

            yield return root;

            //遍歷子控制項
            foreach (Control child in root.Controls)
            {
                foreach (var desc in TraverseElements(child, visitedControls, visitedToolStripItems))
                {
                    yield return desc;
                }
            }

            //特殊處理 SplitContainer 的兩個 Panel
            if (root is SplitContainer sc)
            {
                if (sc.Panel1 != null)
                {
                    foreach (var desc in TraverseElements(sc.Panel1, visitedControls, visitedToolStripItems))
                    {
                        yield return desc;
                    }
                }

                if (sc.Panel2 != null)
                {
                    foreach (var desc in TraverseElements(sc.Panel2, visitedControls, visitedToolStripItems))
                    {
                        yield return desc;
                    }
                }
            }

            //處理 C1DockingTab 的 C1DockingTabPage
            if (root is C1DockingTab dockTab)
            {
                foreach (C1DockingTabPage page in dockTab.TabPages)
                {
                    foreach (var desc in TraverseElements(page, visitedControls, visitedToolStripItems))
                    {
                        yield return desc;
                    }
                }
            }

            //處理 ToolStrip 及其項目
            if (root is ToolStrip ts)
            {
                foreach (ToolStripItem item in ts.Items)
                {
                    foreach (var desc in TraverseToolStripItem(item, visitedToolStripItems))
                    {
                        yield return desc;
                    }
                }
            }
        }

        private static IEnumerable<ToolStripItem> TraverseToolStripItem(ToolStripItem item, HashSet<ToolStripItem> visitedToolStripItems)
        {
            if (item == null)
            {
                yield break;
            }

            if (!visitedToolStripItems.Add(item))
            {
                yield break;
            }

            yield return item;

            if (item is ToolStripDropDownItem dropDownItem)
            {
                foreach (ToolStripItem child in dropDownItem.DropDownItems)
                {
                    foreach (var desc in TraverseToolStripItem(child, visitedToolStripItems))
                    {
                        yield return desc;
                    }
                }
            }
        }

        //支援翻譯的 Control 型別
        private static readonly Type[] TextTypes =
        {
            typeof(Label), typeof(LinkLabel), typeof(Button), typeof(CheckBox), typeof(RadioButton),
            typeof(GroupBox), typeof(SplitContainer),
            typeof(C1Label), typeof(C1Button), typeof(C1CheckBox), typeof(C1DockingTab), typeof(C1DockingTabPage)
        };

        private static void HandleControl(Control ctl, Form rootForm, Color normalColor, Color highlightColor, bool applyColor)
        {
            if (TextTypes.Any(t => t.IsAssignableFrom(ctl.GetType())))
            {
                var oldEnabled = ctl.Enabled;

                ctl.Enabled = true;

                //取得 Text 的語系內容
                TranslateProperty(() => ctl.Text, v => ctl.Text = v, rootForm, GetControlLocalizationId(ctl), "Text");

                if (applyColor)
                {
                    if (ctl is Label label)
                    {
                        if (label.Name.StartsWith("lblStar", StringComparison.Ordinal))
                        {
                            label.ForeColor = highlightColor;
                        }
                        else if (!label.Name.StartsWith("lblWelcome", StringComparison.Ordinal))
                        {
                            label.ForeColor = normalColor;
                        }
                    }
                    else
                    {
                        ctl.ForeColor = normalColor;
                    }
                }

                ctl.Enabled = oldEnabled;
            }

            if (ctl is GroupBox)
            {
                ctl.BackColor = Color.Transparent;
            }
        }

        private static string GetControlLocalizationId(Control ctl)
        {
            if (ctl == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(ctl.Name))
            {
                return ctl.Name;
            }

            return ctl.GetType().Name;
        }

        private static void HandleToolStripItem(ToolStripItem item, Form rootForm, Color normalColor)
        {
            var oldEnabled = item.Enabled;

            item.Enabled = true;

            //取得 Text 與 ToolTipText 的語系內容
            TranslateProperty(() => item.Text, v => item.Text = v, rootForm, item.Name, "Text");
            TranslateProperty(() => item.ToolTipText, v => item.ToolTipText = v, rootForm, item.Name, "ToolTipText");

            item.ForeColor = normalColor;
            item.Enabled = oldEnabled;
        }

        private static void TranslateProperty(Func<string> getter, Action<string> setter, Form rootForm, string elementName, string attribute)
        {
            var orig = getter() ?? string.Empty;
            var className = rootForm?.GetType().Name ?? string.Empty;
            var translated = GetLanguageString(orig, "form", className, "object", elementName, attribute);

            if (!string.IsNullOrWhiteSpace(translated) && translated != orig)
            {
                setter(translated);
            }
        }

        public static void LoadLocalizationXML()
        {
            if (!TryGetLocalizationXmlFileName(LocalizationList, Localization, out var resolvedXmlFileName))
            {
                XmlFileName = string.Empty;
                dtLocalization = new DataTable();

                LoadGlobalLanguageStrings();
                return;
            }

            XmlFileName = resolvedXmlFileName;

            var xmlFullFileName = Path.Combine(Application.StartupPath, "localization", XmlFileName);

            dtLocalization = File.Exists(xmlFullFileName) ? XmlToDataTable(xmlFullFileName) : new DataTable();

            LoadGlobalLanguageStrings();
        }

        private static bool TryGetLocalizationXmlFileName(string localizationList, string localization, out string fileName)
        {
            fileName = string.Empty;

            if (string.IsNullOrWhiteSpace(localizationList) || string.IsNullOrWhiteSpace(localization))
            {
                return false;
            }

            var items = localizationList.Split(new[] { "`" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in items)
            {
                var separatorIndex = item.IndexOf(';');

                if (separatorIndex <= 0 || separatorIndex >= item.Length - 1)
                {
                    continue;
                }

                var localizationName = item.Substring(0, separatorIndex).Trim();
                var xmlFileName = item.Substring(separatorIndex + 1).Trim();

                if (!string.Equals(localization, localizationName, StringComparison.Ordinal))
                {
                    continue;
                }

                fileName = xmlFileName;
                return true;
            }

            return false;
        }

        private static void LoadGlobalLanguageStrings()
        {
            MyGlobal.AnUnexpectedErrorHasOccurred = GetLanguageString
            (
                "An unexpected error has occurred.",
                "Global",
                "Global",
                "msg",
                "AnUnexpectedErrorHasOccurred",
                "Text"
            );

            MyGlobal.StackTrace = GetLanguageString
            (
                "Stack Trace:",
                "Global",
                "Global",
                "msg",
                "StackTrace",
                "Text"
            );

            MyGlobal.PleaseTryAgain = GetLanguageString
            (
                "Please try again!",
                "Global",
                "Global",
                "msg",
                "PleaseTryAgain",
                "Text"
            );

            MyGlobal.AnErrorHasOccurred = GetLanguageString
            (
                "An error has occurred.",
                "Global",
                "Global",
                "msg",
                "AnErrorHasOccurred",
                "Text"
            );

            MyGlobal.LineName = GetLanguageString
            (
                "Line",
                "Global",
                "Global",
                "msg",
                "LineName",
                "Text"
            );
        }

        private static void RefreshLocalizationCache()
        {
            var newCache = new Dictionary<(string, string, string, string, string), string>();

            if (dtLocalization?.Rows.Count > 0 && dtLocalization.Columns.Count == 6)
            {
                foreach (DataRow dr in dtLocalization?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                {
                    var key = CreateLocalizationKey
                    (
                        dr.Field<string>("Category"),
                        dr.Field<string>("Class"),
                        dr.Field<string>("Type"),
                        dr.Field<string>("ID"),
                        dr.Field<string>("Attribute")
                    );

                    var value = dr.GetSafeString("Name").Trim() ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        newCache[key] = value;
                    }
                }
            }

            _localizationCache = newCache;
        }

        private static string NormalizeKeyPart(string value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static (string, string, string, string, string) CreateLocalizationKey(string category, string className, string typeName, string id, string attribute)
        {
            return
            (
                NormalizeKeyPart(category),
                NormalizeKeyPart(className),
                NormalizeKeyPart(typeName),
                NormalizeKeyPart(id),
                NormalizeKeyPart(attribute)
            );
        }
    }
}
