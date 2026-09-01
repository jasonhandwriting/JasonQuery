using System.Collections.Generic;

namespace JasonQuery.Core.SystemInfo
{
    internal static class MessageBoxCaptionBuilder
    {
        public static string Build(string baseCaption, params string[] details)
        {
            var parts = new List<string>();

            AddPart(parts, baseCaption);

            if (details != null)
            {
                foreach (var detail in details)
                {
                    AddPart(parts, detail);
                }
            }

            return string.Join(", ", parts);
        }

        private static void AddPart(ICollection<string> parts, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                parts.Add(value.Trim());
            }
        }
    }
}
