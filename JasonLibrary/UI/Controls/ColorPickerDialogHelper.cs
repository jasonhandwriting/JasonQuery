using Cyotek.Windows.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonLibrary.UI.Controls
{
    /// <summary>
    /// Opens the Cyotek color picker for JasonQuery color settings.
    /// </summary>
    public static class ColorPickerDialogHelper
    {
        /// <summary>
        /// Displays an opaque color picker and invokes <paramref name="colorSelected"/>
        /// only after the user confirms the selection.
        /// </summary>
        public static bool Show(IWin32Window owner, Color initialColor, Action<Color> colorSelected)
        {
            if (colorSelected == null)
            {
                throw new ArgumentNullException(nameof(colorSelected));
            }

            using (var dialog = new ColorPickerDialog())
            {
                dialog.Color = ToOpaqueColor(initialColor);

                if (dialog.ShowDialog(owner) != DialogResult.OK)
                {
                    return false;
                }

                colorSelected(ToOpaqueColor(dialog.Color));
                return true;
            }
        }

        /// <summary>
        /// Formats a color as an invariant six-digit RGB value.
        /// </summary>
        public static string ToHexRgb(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private static Color ToOpaqueColor(Color color)
        {
            if (color.IsEmpty)
            {
                return Color.White;
            }

            return Color.FromArgb(color.R, color.G, color.B);
        }
    }
}
