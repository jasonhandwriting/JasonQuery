using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace JasonQuery.UI.Helpers
{
    public static class ControlLayoutHelper
    {
        public static void PlaceRightOf(Control target, Control anchor, int spacing = 4)
        {
            var controls = GetValidControls(anchor, target);

            if (controls.Length < 2)
            {
                return;
            }

            ExecuteWithCommonParentSuspended(controls, () =>
            {
                target.Location = new Point(anchor.Right + spacing, target.Top);
            });
        }

        public static void PlaceRightOf(Control target, Control anchor, int spacing, int top)
        {
            var controls = GetValidControls(anchor, target);

            if (controls.Length < 2)
            {
                return;
            }

            ExecuteWithCommonParentSuspended(controls, () =>
            {
                target.Location = new Point(anchor.Right + spacing, top);
            });
        }

        public static void ArrangeHorizontalKeepTop(int spacing, params Control[] controls)
        {
            var validControls = GetValidControls(controls);

            if (validControls.Length <= 1)
            {
                return;
            }

            ExecuteWithCommonParentSuspended(validControls, () =>
            {
                ArrangeHorizontalKeepTopCore(spacing, validControls);
            });
        }

        private static void ArrangeHorizontalKeepTopCore(int spacing, Control[] controls)
        {
            for (var i = 1; i < controls.Length; i++)
            {
                var previous = controls[i - 1];
                var current = controls[i];

                current.Left = previous.Right + spacing;
            }
        }

        private static void ExecuteWithCommonParentSuspended(Control[] controls, Action action)
        {
            var parent = GetCommonParentOrThrow(controls);

            parent.SuspendLayout();

            try
            {
                action();
            }
            finally
            {
                parent.ResumeLayout(true);
            }
        }

        private static Control GetCommonParentOrThrow(Control[] controls)
        {
            if (controls == null || controls.Length == 0)
            {
                throw new ArgumentException("At least one control is required.", nameof(controls));
            }

            var parent = controls[0].Parent;

            if (parent == null)
            {
                throw new InvalidOperationException($"The control '{controls[0].Name}' has no parent container.");
            }

            foreach (var control in controls)
            {
                if (control.Parent == null)
                {
                    throw new InvalidOperationException($"The control '{control.Name}' has no parent container.");
                }

                if (!ReferenceEquals(control.Parent, parent))
                {
                    throw new InvalidOperationException("All controls must have the same parent container.");
                }
            }

            return parent;
        }

        private static Control[] GetValidControls(params Control[] controls)
        {
            return controls?.Where(control => control != null).ToArray() ?? new Control[0];
        }

        public static void AdjustGroupBoxBorderForCheckbox(GroupBox groupBox, CheckBox checkBox, int offsetX = 8, int offsetY = 0, int extraWidth = 6)
        {
            if (groupBox == null)
            {
                throw new ArgumentNullException(nameof(groupBox));
            }

            if (checkBox == null)
            {
                throw new ArgumentNullException(nameof(checkBox));
            }

            if (!ReferenceEquals(groupBox.Parent, checkBox.Parent))
            {
                return; //throw new InvalidOperationException("The GroupBox and CheckBox must have the same parent container.");
            }

            var parent = groupBox.Parent;

            parent.SuspendLayout();

            try
            {
                checkBox.AutoSize = true;

                var requiredWidth = checkBox.PreferredSize.Width + extraWidth;

                var spaceWidth = TextRenderer.MeasureText
                (
                    " ",
                    groupBox.Font,
                    Size.Empty,
                    TextFormatFlags.NoPadding
                ).Width;

                if (spaceWidth <= 0)
                {
                    spaceWidth = 4;
                }

                var requiredSpaces = (int)Math.Ceiling((double)requiredWidth / spaceWidth);

                groupBox.Text = new string(' ', requiredSpaces);
                checkBox.Location = new Point(groupBox.Left + offsetX, groupBox.Top + offsetY);
                checkBox.BringToFront();
            }
            finally
            {
                parent.ResumeLayout(true);
            }
        }

    }
}
