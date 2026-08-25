using JasonLibrary.Core;
using JasonQuery.UI.Helpers;
using System.Drawing;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private void ShowEditorMessageWelcomeIfNeeded()
        {
            ApplyEditorMessageWelcomeLayout();

            Color titleColor = MyLibrary.IsDarkMode ? Color.Yellow : Color.Green;
            Color normalColor = MyLibrary.IsDarkMode ? Color.White : Color.Black;

            pnlEditorMessageWelcome.BackColor = MyLibrary.IsDarkMode ? ColorTranslator.FromHtml("#2D2D30") : Color.White;

            lblWelcome.ForeColor = Color.Blue;

            lblWelcome1.ForeColor = titleColor;
            lblWelcome11.ForeColor = normalColor;
            lblWelcome12.ForeColor = normalColor;
            lblWelcome13.ForeColor = normalColor;

            lblWelcome2.ForeColor = titleColor;
            lblWelcome21.ForeColor = normalColor;
            lblWelcome22.ForeColor = normalColor;
            lblWelcome221.ForeColor = normalColor;
            lblWelcome222.ForeColor = normalColor;
            lblWelcome223.ForeColor = normalColor;
            lblWelcome23.ForeColor = normalColor;

            lblWelcome3.ForeColor = titleColor;
            lblWelcome31.ForeColor = normalColor;
            lblWelcome32.ForeColor = normalColor;
            lblWelcome33.ForeColor = normalColor;

            lblWelcome4.ForeColor = titleColor;
            lblWelcome41.ForeColor = normalColor;
            lblWelcome42.ForeColor = normalColor;
            lblWelcome43.ForeColor = normalColor;
        }

        private void ApplyEditorMessageWelcomeLayout()
        {
            pnlEditorMessageWelcome.SuspendLayout();

            try
            {
                ControlLayoutHelper.PlaceRightOf(lblWelcomeCountdown, lblWelcome, 5);
            }
            finally
            {
                pnlEditorMessageWelcome.ResumeLayout(true);
            }
        }

        private void HideEditorMessageWelcomeIfNeeded()
        {
            //由於 pnlEditorMessageWelcome 位於 tabMessage 內，會導致 if (pnlEditorMessageWelcome.Visible) 失效，故此處永遠設為不可見

            if (string.IsNullOrEmpty(pnlEditorMessageWelcome.AccessibleDescription))
            {
                pnlEditorMessageWelcome.Visible = false;
                pnlEditorMessageWelcome.AccessibleDescription = "Hide";

                if (tmrHideMessageWelcome.Enabled)
                {
                    tmrHideMessageWelcome.Enabled = false;
                }
            }
        }
    }
}
