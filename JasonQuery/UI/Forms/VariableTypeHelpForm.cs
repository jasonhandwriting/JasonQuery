using JasonLibrary.Core;
using JasonQuery.Core.Localization;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class VariableTypeHelpForm : Form
    {
        public VariableTypeHelpForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            LocalizationHelper.ApplyLanguageInfo(this, false);

            lblInfoC54.Text = $@"TO_DATE('{DateTime.Now:yyyy/MM/dd}', 'YYYY/MM/DD')";
            lblInfoC57.Text = lblInfoC54.Text;

            var languageText = LocalizationHelper.GetLanguageString("Example", "form", GetType().Name, "msg", "Example", "Text");

            lblExampleS1.Text = $"{languageText} 1:";
            lblExampleS2.Text = $"{languageText} 2:";
            lblExampleN1.Text = $"{languageText} 1:";
            lblExampleN2.Text = $"{languageText} 2:";
            lblExampleC1.Text = $"{languageText} 1:";
            lblExampleC2.Text = $"{languageText} 2:";
            lblExampleC3.Text = $"{languageText} 3:";
            lblExampleC4.Text = $"{languageText} 4:";
            lblExampleC5.Text = $"{languageText} 5:";

            //顏色會跑掉，故此處再重新設定
            lblColorVariables.BackColor = Color.Aqua;
            lblStringInfo1.BackColor = Color.Aqua;
            lblInfoS12.BackColor = Color.Aqua;
            lblInfoS22.BackColor = Color.Aqua;
            lblNumberInfo1.BackColor = Color.Aqua;
            lblInfoN12.BackColor = Color.Aqua;
            lblInfoN22.BackColor = Color.Aqua;
            lblCustomInfo1.BackColor = Color.Aqua;
            lblInfoC12.BackColor = Color.Aqua;
            lblInfoC22.BackColor = Color.Aqua;
            lblInfoC32.BackColor = Color.Aqua;
            lblInfoC42.BackColor = Color.Aqua;
            lblInfoC52.BackColor = Color.Aqua;

            //Begin:20260210
            lblInfoS01.Location = new Point(lblStringInfo1.Left + lblStringInfo1.Width + 2, lblInfoS01.Top);
            lblStringInfo2.Location = new Point(lblInfoS01.Left + lblInfoS01.Width + 2, lblStringInfo2.Top);
            lblInfoS02.Location = new Point(lblStringInfo2.Left + lblStringInfo2.Width + 2, lblInfoS02.Top);
            lblStringInfo3.Location = new Point(lblInfoS02.Left + lblInfoS02.Width + 2, lblStringInfo3.Top);
            lblInfoN01.Location = new Point(lblNumberInfo1.Left + lblNumberInfo1.Width + 2, lblInfoN01.Top);
            lblNumberInfo2.Location = new Point(lblInfoN01.Left + lblInfoN01.Width + 2, lblNumberInfo2.Top);
            lblInfoN02.Location = new Point(lblNumberInfo2.Left + lblNumberInfo2.Width + 2, lblInfoN02.Top);
            lblNumberInfo3.Location = new Point(lblInfoN02.Left + lblInfoN02.Width + 2, lblNumberInfo3.Top);
            lblInfoC01.Location = new Point(lblCustomInfo1.Left + lblCustomInfo1.Width + 2, lblInfoC01.Top);
            lblCustomInfo2.Location = new Point(lblInfoC01.Left + lblInfoC01.Width + 2, lblCustomInfo2.Top);
            lblInfoC02.Location = new Point(lblCustomInfo2.Left + lblCustomInfo2.Width + 2, lblInfoC02.Top);
            lblCustomInfo3.Location = new Point(lblInfoC02.Left + lblInfoC02.Width + 2, lblCustomInfo3.Top);
            //End:20260210

            lblInfoS11.Location = new Point(lblExampleS1.Left + lblExampleS1.Width + 2, lblInfoS11.Top);
            lblInfoS12.Location = new Point(lblInfoS11.Left + lblInfoS11.Width - 3, lblInfoS12.Top);
            lblInfoS13.Location = new Point(lblInfoS12.Left + lblInfoS12.Width - 2, lblInfoS13.Top);
            lblInfoS14.Location = new Point(lblInfoS13.Left + lblInfoS13.Width - 6, lblInfoS14.Top);
            lblInfoS15.Location = new Point(lblInfoS14.Left + lblInfoS14.Width - 2, lblInfoS15.Top);
            lblInfoS16.Location = new Point(lblInfoS15.Left + lblInfoS15.Width - 6, lblInfoS16.Top);
            lblInfoS17.Location = new Point(lblInfoS16.Left + lblInfoS16.Width - 4, lblInfoS17.Top);

            lblInfoS21.Location = new Point(lblExampleS2.Left + lblExampleS2.Width + 2, lblInfoS21.Top);
            lblInfoS22.Location = new Point(lblInfoS21.Left + lblInfoS21.Width - 3, lblInfoS22.Top);
            lblInfoS23.Location = new Point(lblInfoS22.Left + lblInfoS22.Width - 2, lblInfoS23.Top);
            lblInfoS24.Location = new Point(lblInfoS23.Left + lblInfoS23.Width - 6, lblInfoS24.Top);
            lblInfoS25.Location = new Point(lblInfoS24.Left + lblInfoS24.Width - 2, lblInfoS25.Top);
            lblInfoS26.Location = new Point(lblInfoS25.Left + lblInfoS25.Width - 6, lblInfoS26.Top);
            lblInfoS27.Location = new Point(lblInfoS26.Left + lblInfoS26.Width - 4, lblInfoS27.Top);

            lblInfoN11.Location = new Point(lblExampleN1.Left + lblExampleN1.Width + 2, lblInfoN11.Top);
            lblInfoN12.Location = new Point(lblInfoN11.Left + lblInfoN11.Width - 3, lblInfoN12.Top);
            lblInfoN13.Location = new Point(lblInfoN12.Left + lblInfoN12.Width - 2, lblInfoN13.Top);
            lblInfoN14.Location = new Point(lblInfoN13.Left + lblInfoN13.Width - 6, lblInfoN14.Top);
            lblInfoN15.Location = new Point(lblInfoN14.Left + lblInfoN14.Width - 2, lblInfoN15.Top);
            lblInfoN16.Location = new Point(lblInfoN15.Left + lblInfoN15.Width - 6, lblInfoN16.Top);
            lblInfoN17.Location = new Point(lblInfoN16.Left + lblInfoN16.Width - 4, lblInfoN17.Top);

            lblInfoN21.Location = new Point(lblExampleN2.Left + lblExampleN2.Width + 2, lblInfoN21.Top);
            lblInfoN22.Location = new Point(lblInfoN21.Left + lblInfoN21.Width - 3, lblInfoN22.Top);
            lblInfoN23.Location = new Point(lblInfoN22.Left + lblInfoN22.Width - 2, lblInfoN23.Top);
            lblInfoN24.Location = new Point(lblInfoN23.Left + lblInfoN23.Width - 6, lblInfoN24.Top);
            lblInfoN25.Location = new Point(lblInfoN24.Left + lblInfoN24.Width - 2, lblInfoN25.Top);
            lblInfoN26.Location = new Point(lblInfoN25.Left + lblInfoN25.Width - 6, lblInfoN26.Top);
            lblInfoN27.Location = new Point(lblInfoN26.Left + lblInfoN26.Width - 4, lblInfoN27.Top);

            lblInfoC11.Location = new Point(lblExampleC1.Left + lblExampleC1.Width + 2, lblInfoC11.Top);
            lblInfoC12.Location = new Point(lblInfoC11.Left + lblInfoC11.Width - 3, lblInfoC12.Top);
            lblInfoC13.Location = new Point(lblInfoC12.Left + lblInfoC12.Width - 2, lblInfoC13.Top);
            lblInfoC14.Location = new Point(lblInfoC13.Left + lblInfoC13.Width - 6, lblInfoC14.Top);
            lblInfoC15.Location = new Point(lblInfoC14.Left + lblInfoC14.Width - 2, lblInfoC15.Top);
            lblInfoC16.Location = new Point(lblInfoC15.Left + lblInfoC15.Width - 6, lblInfoC16.Top);
            lblInfoC17.Location = new Point(lblInfoC16.Left + lblInfoC16.Width - 4, lblInfoC17.Top);

            lblInfoC21.Location = new Point(lblExampleC2.Left + lblExampleC2.Width + 2, lblInfoC21.Top);
            lblInfoC22.Location = new Point(lblInfoC21.Left + lblInfoC21.Width - 3, lblInfoC22.Top);
            lblInfoC23.Location = new Point(lblInfoC22.Left + lblInfoC22.Width - 2, lblInfoC23.Top);
            lblInfoC24.Location = new Point(lblInfoC23.Left + lblInfoC23.Width - 6, lblInfoC24.Top);
            lblInfoC25.Location = new Point(lblInfoC24.Left + lblInfoC24.Width - 2, lblInfoC25.Top);
            lblInfoC26.Location = new Point(lblInfoC25.Left + lblInfoC25.Width - 6, lblInfoC26.Top);
            lblInfoC27.Location = new Point(lblInfoC26.Left + lblInfoC26.Width - 4, lblInfoC27.Top);

            lblInfoC31.Location = new Point(lblExampleC3.Left + lblExampleC3.Width + 2, lblInfoC31.Top);
            lblInfoC32.Location = new Point(lblInfoC31.Left + lblInfoC31.Width - 3, lblInfoC32.Top);
            lblInfoC33.Location = new Point(lblInfoC32.Left + lblInfoC32.Width - 2, lblInfoC33.Top);
            lblInfoC34.Location = new Point(lblInfoC33.Left + lblInfoC33.Width - 6, lblInfoC34.Top);
            lblInfoC35.Location = new Point(lblInfoC34.Left + lblInfoC34.Width - 2, lblInfoC35.Top);
            lblInfoC36.Location = new Point(lblInfoC35.Left + lblInfoC35.Width - 6, lblInfoC36.Top);
            lblInfoC37.Location = new Point(lblInfoC36.Left + lblInfoC36.Width - 4, lblInfoC37.Top);

            lblInfoC41.Location = new Point(lblExampleC4.Left + lblExampleC4.Width + 2, lblInfoC41.Top);
            lblInfoC42.Location = new Point(lblInfoC41.Left + lblInfoC41.Width - 3, lblInfoC42.Top);
            lblInfoC43.Location = new Point(lblInfoC42.Left + lblInfoC42.Width - 2, lblInfoC43.Top);
            lblInfoC44.Location = new Point(lblInfoC43.Left + lblInfoC43.Width - 6, lblInfoC44.Top);
            lblInfoC45.Location = new Point(lblInfoC44.Left + lblInfoC44.Width - 2, lblInfoC45.Top);
            lblInfoC46.Location = new Point(lblInfoC45.Left + lblInfoC45.Width - 6, lblInfoC46.Top);
            lblInfoC47.Location = new Point(lblInfoC46.Left + lblInfoC46.Width - 4, lblInfoC47.Top);

            lblInfoC51.Location = new Point(lblExampleC5.Left + lblExampleC5.Width + 2, lblInfoC51.Top);
            lblInfoC52.Location = new Point(lblInfoC51.Left + lblInfoC51.Width - 3, lblInfoC52.Top);
            lblInfoC53.Location = new Point(lblInfoC52.Left + lblInfoC52.Width - 2, lblInfoC53.Top);
            lblInfoC54.Location = new Point(lblInfoC53.Left + lblInfoC53.Width - 6, lblInfoC54.Top);
            lblInfoC55.Location = new Point(lblInfoC53.Left, lblInfoC55.Top);
            lblInfoC56.Location = new Point(lblInfoC55.Left + lblInfoC55.Width - 6, lblInfoC56.Top);
            lblInfoC57.Location = new Point(lblInfoC56.Left + lblInfoC56.Width - 4, lblInfoC57.Top);

            lblColorValue.BackColor = Color.Lime;
            lblStringInfo2.BackColor = Color.Lime;
            lblInfoS14.BackColor = Color.Lime;
            lblInfoS24.BackColor = Color.Lime;
            lblNumberInfo2.BackColor = Color.Lime;
            lblInfoN14.BackColor = Color.Lime;
            lblInfoN24.BackColor = Color.Lime;
            lblCustomInfo2.BackColor = Color.Lime;
            lblInfoC14.BackColor = Color.Lime;
            lblInfoC24.BackColor = Color.Lime;
            lblInfoC34.BackColor = Color.Lime;
            lblInfoC44.BackColor = Color.Lime;
            lblInfoC54.BackColor = Color.Lime;

            lblColorReplaced.BackColor = Color.Orange;
            lblStringInfo3.BackColor = Color.Orange;
            lblInfoS17.BackColor = Color.Orange;
            lblInfoS27.BackColor = Color.Orange;
            lblNumberInfo3.BackColor = Color.Orange;
            lblInfoN17.BackColor = Color.Orange;
            lblInfoN27.BackColor = Color.Orange;
            lblCustomInfo3.BackColor = Color.Orange;
            lblInfoC17.BackColor = Color.Orange;
            lblInfoC27.BackColor = Color.Orange;
            lblInfoC37.BackColor = Color.Orange;
            lblInfoC47.BackColor = Color.Orange;
            lblInfoC57.BackColor = Color.Orange;

            if (!MyLibrary.IsDarkMode)
            {
                return;
            }

            lblInfoS12.ForeColor = Color.Black;
            lblInfoS22.ForeColor = Color.Black;
            lblInfoS14.ForeColor = Color.Black;
            lblInfoS24.ForeColor = Color.Black;
            lblInfoS17.ForeColor = Color.Black;
            lblInfoS27.ForeColor = Color.Black;
            lblInfoN12.ForeColor = Color.Black;
            lblInfoN22.ForeColor = Color.Black;
            lblInfoN14.ForeColor = Color.Black;
            lblInfoN24.ForeColor = Color.Black;
            lblInfoN17.ForeColor = Color.Black;
            lblInfoN27.ForeColor = Color.Black;
            lblInfoC12.ForeColor = Color.Black;
            lblInfoC22.ForeColor = Color.Black;
            lblInfoC32.ForeColor = Color.Black;
            lblInfoC42.ForeColor = Color.Black;
            lblInfoC52.ForeColor = Color.Black;
            lblInfoC14.ForeColor = Color.Black;
            lblInfoC24.ForeColor = Color.Black;
            lblInfoC34.ForeColor = Color.Black;
            lblInfoC44.ForeColor = Color.Black;
            lblInfoC54.ForeColor = Color.Black;
            lblInfoC17.ForeColor = Color.Black;
            lblInfoC27.ForeColor = Color.Black;
            lblInfoC37.ForeColor = Color.Black;
            lblInfoC47.ForeColor = Color.Black;
            lblInfoC57.ForeColor = Color.Black;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
