using JasonLibrary.UI.Controls;
using JasonQuery.Core.Config;
using JasonQuery.Core.Logging;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private void SelectedTabClick(object sender, EventArgs e)
        {
            try
            {
                if (!(sender is Panel pnlSelected))
                {
                    return;
                }

                _panelColorSelectedName = pnlSelected.Name;

                ColorPickerDialogHelper.Show(this, pnlSelected.BackColor, ColorSelectedTab);
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ColorSelectedTab(Color selectedColor)
        {
            var hexColor = ColorPickerDialogHelper.ToHexRgb(selectedColor);

            foreach (var t in _lstPanelTabColor.Where(t => ((Panel)t).Name == _panelColorSelectedName))
            {
                ((Panel)t).BackColor = selectedColor;
                ((Panel)t).Tag = hexColor;
                _toolTip1.SetToolTip((Panel)t, $"{hexColor} " + $"(R:{selectedColor.R}, G:{selectedColor.G}, B:{selectedColor.B})");

                switch (((Panel)t).Name)
                {
                    case "pnlBackColor":
                        {
                            tabExample.BackColor = selectedColor;
                            break;
                        }
                    case "pnlActiveForeColor":
                        {
                            tabExample.ForeColor = selectedColor;
                            break;
                        }
                    case "pnlInactiveForeColor":
                        {
                            tabExample.TextInactiveColor = selectedColor;
                            break;
                        }
                }

                break;
            }
        }

        private void ApplyMainFormIconStyleSelection(int index, bool updateSelectedValue)
        {
            if (_lstRdoIconStyle == null || _lstRdoIconStyle.Count == 0)
            {
                return;
            }

            if (index < 0 || index >= _lstRdoIconStyle.Count)
            {
                index = DefaultMainFormIconStyleIndex;
            }

            if (updateSelectedValue)
            {
                _selectedMainFormIconStyleIndex = index;
            }

            _lstRdoIconStyle[index].Checked = true;
        }

        private void IconStylePicture_Click(object sender, EventArgs e)
        {
            if (!(sender is PictureBox pictureBox))
            {
                return;
            }

            var index = _lstPicIconStyle?.IndexOf(pictureBox) ?? -1;

            if (index < 0)
            {
                return;
            }

            ApplyMainFormIconStyleSelection(index, true);
        }

        private void IconStyle_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is RadioButton radioButton))
            {
                return;
            }

            if (!radioButton.Checked)
            {
                return;
            }

            if (_isConnecting)
            {
                //連線時重新載入 Grid 可能變更 Checked 狀態，不可覆蓋使用者準備儲存的圖示樣式
                return;
            }

            var index = _lstRdoIconStyle?.IndexOf(radioButton) ?? -1;

            if (index < 0)
            {
                return;
            }

            _selectedMainFormIconStyleIndex = index;
        }
    }
}