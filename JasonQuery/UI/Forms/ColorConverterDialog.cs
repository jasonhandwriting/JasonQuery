using JasonLibrary.UI.Controls;
using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ColorConverterDialog : Form
    {
        private ToolTip _toolTip1 = new ToolTip();

        public ColorConverterDialog()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizationHelper.ApplyLanguageInfo(this);
                pnlColor.Location = new Point(rdoPickColor.Left + rdoPickColor.Width, pnlColor.Top);
                lblInvalidColorCode.ForeColor = Color.DarkRed;

                var rng = new Random(Guid.NewGuid().GetHashCode());
                var i = rng.Next(0, 8);

                switch (i)
                {
                    case 0:
                        {
                            pnlColor.BackColor = Color.YellowGreen;
                            break;
                        }
                    case 1:
                        {
                            pnlColor.BackColor = Color.LightBlue;
                            break;
                        }
                    case 2:
                        {
                            pnlColor.BackColor = Color.LightPink;
                            break;
                        }
                    case 3:
                        {
                            pnlColor.BackColor = Color.LightSeaGreen;
                            break;
                        }
                    case 4:
                        {
                            pnlColor.BackColor = Color.LightSalmon;
                            break;
                        }
                    case 5:
                        {
                            pnlColor.BackColor = Color.Plum;
                            break;
                        }
                    case 6:
                        {
                            pnlColor.BackColor = Color.Orange;
                            break;
                        }
                    default:
                        {
                            pnlColor.BackColor = Color.Gold;
                            break;
                        }
                }

                nudR.Value = pnlColor.BackColor.R;
                nudG.Value = pnlColor.BackColor.G;
                nudB.Value = pnlColor.BackColor.B;
                txtRgbColor.Text = $"{nudR.Value}, {nudG.Value}, {nudB.Value}";
                txtHtmlColor.Text = ColorTranslator.ToHtml(Color.FromArgb(pnlColor.BackColor.ToArgb())).Substring(1);
                txtHtmlColor.Tag = txtHtmlColor.Text;
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void SelectedColorClick(object sender, EventArgs e)
        {
            if (!(sender is Panel pnlSelected))
            {
                return;
            }

            rdoPickColor.Checked = true;
            ColorPickerDialogHelper.Show(this, pnlSelected.BackColor, ColorSelected);
        }

        private void ColorSelected(Color selectedColor)
        {
            var hexColor = ColorPickerDialogHelper.ToHexRgb(selectedColor);

            pnlColor.BackColor = selectedColor;
            pnlColor.Tag = hexColor;
            _toolTip1.SetToolTip(pnlColor, $"{hexColor} (R:{selectedColor.R}, G:{selectedColor.G}, B:{selectedColor.B})");
            txtHtmlColor.Text = hexColor.Substring(1);
            txtHtmlColor.Tag = txtHtmlColor.Text;
            txtRgbColor.Text = $"{selectedColor.R}, {selectedColor.G}, {selectedColor.B}";
            nudR.Value = selectedColor.R;
            nudG.Value = selectedColor.G;
            nudB.Value = selectedColor.B;

            rdoPickColor.Checked = true;
            rdoPickColor.Focus();
            lblInvalidColorCode.Visible = false;
        }

        private void nudColor_Enter(object sender, EventArgs e)
        {
            var nud = sender as NumericUpDown;

            UpDownBase text = nud;
            nud.Select(0, text.Text.Length);
            rdoRgbColor.Checked = true;
        }

        private void nudColor_Leave(object sender, EventArgs e)
        {
            var nud = sender as NumericUpDown;

            UpDownBase text = nud;

            if (string.IsNullOrEmpty(text.Text))
            {
                nud.UpButton();
                nud.DownButton();
                nud.Value = 0;
            }

            if (nud.Value < 0 && nud.Value != 0)
            {
                nud.Value = 0;
            }
        }

        private void nudColor_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var nud = sender as NumericUpDown;

                if (nud.Value > 255)
                {
                    nud.Value = 255;
                }

                if (nud.Value < 0)
                {
                    nud.Value = 0;
                }

                ConvertColor();
            }
            catch (Exception ex)
            {
                var message = TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);

                MessageBox.Show(message, AppConfigHelper.JasonQueryVersion, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ConvertColor()
        {
            txtRgbColor.Text = $"{nudR.Value}, {nudG.Value}, {nudB.Value}";
            pnlColor.BackColor = Color.FromArgb((int)nudR.Value, (int)nudG.Value, (int)nudB.Value);
            txtHtmlColor.Text = ColorTranslator.ToHtml(Color.FromArgb(pnlColor.BackColor.ToArgb())).Substring(1);
            txtHtmlColor.Tag = txtHtmlColor.Text;

            rdoRgbColor.Checked = true;
            lblInvalidColorCode.Visible = false;
        }

        private void nudColor_MouseClick(object sender, MouseEventArgs e)
        {
            var nud = sender as NumericUpDown;
            UpDownBase text = nud;

            nud.Select(0, text.Text.Length);
        }

        private void txtHtmlColor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetterOrDigit(e.KeyChar) || char.IsControl(e.KeyChar))
            {
                rdoHtmlColor.Checked = true;
                lblInvalidColorCode.Visible = false;
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtHtmlColor_KeyUp(object sender, KeyEventArgs e)
        {
            if (txtHtmlColor.Text.Length == 6 && CheckColorCode(txtHtmlColor.Text))
            {
                var color = ColorTranslator.FromHtml($"#{txtHtmlColor.Text}");

                nudR.Value = color.R;
                nudG.Value = color.G;
                nudB.Value = color.B;
                txtRgbColor.Text = $"{nudR.Value}, {nudG.Value}, {nudB.Value}";
                pnlColor.BackColor = Color.FromArgb((int)nudR.Value, (int)nudG.Value, (int)nudB.Value);
                rdoHtmlColor.Checked = true;
            }
        }

        private void txtHtmlColor_MouseClick(object sender, MouseEventArgs e)
        {
            rdoHtmlColor.Checked = true;
        }

        private void txtHtmlColor_Enter(object sender, EventArgs e)
        {
            txtHtmlColor.SelectionStart = 0;
            txtHtmlColor.SelectionLength = txtHtmlColor.Text.Length;
            rdoHtmlColor.Checked = true;
        }

        private void txtHtmlColor_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHtmlColor.Text))
            {
                txtHtmlColor.Text = TextHelper.GetSafeString(txtHtmlColor.Tag);
            }

            if (!CheckColorCode(txtHtmlColor.Text))
            {
                //無效的顏色碼
                lblInvalidColorCode.Visible = true;
            }
            else
            {
                lblInvalidColorCode.Visible = false;
            }
        }

        private static bool CheckColorCode(string sColorCode)
        {
            var regexColorCode = new System.Text.RegularExpressions.Regex("^#[a-fA-F0-9]{6}$");

            return regexColorCode.IsMatch($"#{sColorCode.Trim()}");
        }

        private void btnCopy_HTML_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject($"#{txtHtmlColor.Text}", true, 10, 100);
        }

        private void btnCopy_RGB_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(txtRgbColor.Text, true, 10, 100);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void nudColor_ValueChanged(object sender, EventArgs e)
        {
            ConvertColor();
        }
    }
}
