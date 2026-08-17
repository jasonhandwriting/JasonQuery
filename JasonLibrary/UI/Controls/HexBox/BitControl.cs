using JasonLibrary.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace JasonLibrary.UI.Controls.HexBox
{
    public partial class BitControl : UserControl
    {
        public event EventHandler BitChanged;

        private List<RichTextBox> _txtBits = new List<RichTextBox>();
        private Panel _innerBorderHeaderPanel;
        private Panel _innerBorderPanel;

        protected virtual void OnBitChanged(EventArgs e)
        {
            BitChanged?.Invoke(this, e);
        }

        public BitControl()
        {
            _innerBorderHeaderPanel = new Panel();
            _innerBorderHeaderPanel.Dock = DockStyle.Fill;
            _innerBorderHeaderPanel.Margin = new Padding(3, 1, 3, 1);

            _innerBorderPanel = new Panel();
            _innerBorderPanel.BackColor = Color.White;
            _innerBorderPanel.Dock = DockStyle.Fill;
            _innerBorderPanel.Margin = new Padding(3, 1, 3, 1);

            InitializeComponent();

            pnBitsEditor.BackColor = VisualStyleInformation.TextControlBorder;
            pnBitsHeader.Controls.Add(_innerBorderHeaderPanel);

            lblBit.Text = "Bit:";
            lblValue.Text = "Value:";

            bool first = true;
            Size size = new Size();
            int pos = 2;

            for (int i = 7; i > -1; i--)
            {
                Label lbl = new Label
                {
                    Tag = i,
                    BorderStyle = BorderStyle.None,
                    Font = new Font("Consolas", 10, FontStyle.Regular, GraphicsUnit.Point, 0),
                    Margin = new Padding(0),
                    Name = "lbl" + i,
                    AutoSize = true,
                    Text = i.ToString()
                };

                lbl.Enter += new EventHandler(txt_Enter);
                lbl.KeyDown += new KeyEventHandler(txt_KeyDown);
                _innerBorderHeaderPanel.Controls.Add(lbl);

                if (first)
                {
                    size = lbl.Size;
                    lbl.AutoSize = false;
                    first = false;
                }

                lbl.Size = size;
                lbl.Left = pos;
                lbl.Top = 3;
                pos += size.Width;
            }

            pnBitsEditor.Controls.Add(_innerBorderPanel);
            pos = 5;

            for (int i = 7; i > -1; i--)
            {
                RichTextBox txt = new RichTextBox
                {
                    Tag = i,
                    BorderStyle = BorderStyle.None,
                    Font = new Font("Consolas", 10, FontStyle.Regular, GraphicsUnit.Point, 0),
                    Margin = new Padding(0),

                    MaxLength = 1,
                    Multiline = false,
                    Name = "txt" + i.ToString(),
                    Size = size,
                    Left = pos,
                    Top = 2
                };

                pos += size.Width;
                txt.TabIndex = 10 - i + 7;
                txt.Text = "0";
                txt.Visible = false;
                txt.SelectionChanged += new EventHandler(txt_SelectionChanged);
                txt.Enter += new EventHandler(txt_Enter);
                txt.KeyDown += new KeyEventHandler(txt_KeyDown);
                _innerBorderPanel.Controls.Add(txt);
                _txtBits.Add(txt);
            }

            UpdateView();
        }

        BitInfo _bitInfo;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BitInfo BitInfo
        {
            get
            {
                return _bitInfo;
            }

            set
            {
                _bitInfo = value;
                UpdateView();
            }
        }

        private void UpdateView()
        {
            foreach (var txt in _txtBits)
            {
                txt.TextChanged -= new EventHandler(txt_TextChanged);
            }

            if (_bitInfo == null)
            {
                foreach (var txt in _txtBits)
                {
                    txt.Text = string.Empty;
                }

                pnBitsEditor.Visible = lblValue.Visible = lblBit.Visible = pnBitsHeader.Visible = false;

                return;
            }
            else
            {
                foreach (var txt in _txtBits)
                {
                    txt.Visible = true;
                }

                pnBitsEditor.Visible = lblValue.Visible = lblBit.Visible = pnBitsHeader.Visible = true;
            }

            foreach (var txt in _txtBits)
            {
                int bit = (int)txt.Tag;

                txt.Text = _bitInfo.GetBitAsString(bit);
            }

            foreach (var txt in _txtBits)
            {
                txt.TextChanged += new EventHandler(txt_TextChanged);
            }
        }

        int GetBitSetInt(byte b, int pos)
        {
            if (IsBitSet(b, pos))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        byte SetBit(byte b, int BitNumber)
        {
            if (BitNumber < 8 && BitNumber > -1)
            {
                return (byte)(b | (byte)(0x01 << BitNumber));
            }
            else
            {
                throw new InvalidOperationException("Der Wert für BitNumber " + BitNumber + " war nicht im zulässigen Bereich! (BitNumber = (min)0 - (max)7)");
            }
        }

        void txt_TextChanged(object sender, EventArgs e)
        {
            var txt = (RichTextBox)sender;
            var index = (int)txt.Tag;
            var value = txt.Text != "0";

            BitInfo[index] = value;
            OnBitChanged(EventArgs.Empty);

            NavigateRight((RichTextBox)sender);
        }

        void NavigateLeft(RichTextBox txt)
        {
            var indexOf = _txtBits.IndexOf(txt);

            NavigateTo(indexOf - 1);
        }

        void NavigateRight(RichTextBox txt)
        {
            var indexOf = _txtBits.IndexOf(txt);

            NavigateTo(indexOf + 1);
        }

        void NavigateTo(int indexOf)
        {
            if (indexOf > _txtBits.Count - 1 || indexOf < 0)
            {
                return;
            }

            var txtFocus = false;

            foreach (var txt in _txtBits)
            {
                if (txt.Focused)
                {
                    txtFocus = true;
                    break;
                }
            }

            if (!txtFocus)
            {
                return;
            }

            var selectBox = _txtBits[indexOf];

            selectBox.Focus();
        }

        void txt_KeyDown(object sender, KeyEventArgs e)
        {
            var txt = (RichTextBox)sender;

            List<Keys> bitKeys = new List<Keys>() { Keys.D0, Keys.D1 };

            var txt7 = _txtBits[0];

            if (txt7.SelectionLength > 1)
            {
                txt7.SelectionLength = 1;
            }

            var modifiersNone = e.Modifiers == Keys.None;
            var updateBit = modifiersNone && bitKeys.Contains(e.KeyCode);

            e.Handled = e.SuppressKeyPress = !updateBit;

            if (!updateBit && modifiersNone)
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        {
                            NavigateLeft(txt);
                            break;
                        }
                    case Keys.Right:
                        {
                            NavigateRight(txt);
                            break;
                        }
                    case Keys.Home:
                        {
                            NavigateTo(0);
                            break;
                        }
                    case Keys.End:
                        {
                            NavigateTo(7);
                            break;
                        }
                }
            }

            //20230831
            if (MyLibrary.IsBlobReadOnly)
            {
                if (e.KeyCode == Keys.D0 || e.KeyCode == Keys.D1)
                {
                    e.SuppressKeyPress = true;
                    SendKeys.SendWait("{RIGHT}"); //右移一格
                }
            }
        }

        void txt_SelectionChanged(object sender, EventArgs e)
        {
            var txt = (RichTextBox)sender;

            UpdateSelection(txt);
        }

        void UpdateSelection(RichTextBox txt)
        {
            txt.SelectionStart = 0;

            if (txt.SelectionLength == 0)
            {
                txt.SelectionLength = 1;
            }
        }

        void txt_Enter(object sender, EventArgs e)
        {
            var txt = (RichTextBox)sender;

            UpdateSelection(txt);
        }
    }
}