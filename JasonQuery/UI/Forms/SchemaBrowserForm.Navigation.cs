using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private void btnTop_Click(object sender, EventArgs e)
        {
            c1GridData.Row = 0;
            c1GridData.Select();

            CheckButtonsStatus();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            c1GridData.Row = c1GridData.Row - 1;
            c1GridData.Select();

            CheckButtonsStatus();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            c1GridData.Row = c1GridData.Row + 1;
            c1GridData.Select();

            CheckButtonsStatus();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            c1GridData.MoveToLastDataTableRow();
            c1GridData.Select();

            CheckButtonsStatus();
        }

        private void tabSchemaBrowser_TabClick(object sender, EventArgs e)
        {
            var focusMap = new Dictionary<string, Action>
            {
                { "tabSqlPane", () => editorSqlPane.Focus() },
                { "tabTableStructure", () => c1GridStructure.Focus() },
                { "tabView100RowsTop", () => c1Grid100RowsTop.Focus() },
                // "tabData" //20240601 此處需要經由使用者點擊 Grid 觸發「檢查按鈕狀態函數 - CheckButtonEnabled」，Focus 方法並不會觸發！
                { "tabSettings", () => txtTemp.Focus() },
                { "tabSqlPreview", () => editorSqlPreview.Focus() }
            };

            if (focusMap.TryGetValue(tabSchemaBrowser.SelectedTab.Name, out var focusAction))
            {
                focusAction.Invoke();
            }
        }
    }
}