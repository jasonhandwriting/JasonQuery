using System;
using System.IO;

namespace JasonQuery.UI.QueryEditor
{
    //The display name and saved file path are separate from editor dirty state.
    //Callers publish a new saved path only after a successful open or Save As.
    public sealed class QueryEditorTabState
    {
        private QueryEditorTabState(string displayName, string savedFilePath)
        {
            DisplayName = displayName;
            SavedFilePath = savedFilePath;
        }

        public string DisplayName { get; private set; }
        public string SavedFilePath { get; private set; }
        public bool HasSavedFile => !string.IsNullOrEmpty(SavedFilePath);

        public static QueryEditorTabState CreateNew(string displayName)
        {
            var normalizedName = (displayName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(normalizedName) || normalizedName.IndexOf('*') >= 0)
            {
                throw new ArgumentException("A new editor requires a display name without a dirty marker.", nameof(displayName));
            }

            return new QueryEditorTabState(normalizedName, string.Empty);
        }

        public static QueryEditorTabState CreateOpenedFile(string savedFilePath)
        {
            return new QueryEditorTabState(GetSavedFileName(savedFilePath), savedFilePath);
        }

        public static QueryEditorTabState Restore(string backupTabTitle, string savedFilePath, out bool wasDirty)
        {
            //The caller converts the legacy FileNotSavedYet sentinel to an empty path.
            var storedTitle = backupTabTitle ?? string.Empty;
            var restoredName = storedTitle.TrimStart('*');
            var restoredPath = savedFilePath ?? string.Empty;

            wasDirty = storedTitle.StartsWith("*", StringComparison.Ordinal);

            //Older metadata without a title can still use the actual saved file name.
            if (string.IsNullOrWhiteSpace(restoredName) && !string.IsNullOrEmpty(restoredPath))
            {
                restoredName = GetSavedFileName(restoredPath);
            }

            return new QueryEditorTabState(restoredName, restoredPath);
        }

        public bool TryRename(string displayName)
        {
            var normalizedName = (displayName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(normalizedName) || normalizedName.IndexOf('*') >= 0
                || string.Equals(DisplayName, normalizedName, StringComparison.Ordinal))
            {
                return false;
            }

            DisplayName = normalizedName;
            return true;
        }

        public void ApplySuccessfulSaveAs(string savedFilePath)
        {
            //Resolve the name before changing either value.
            var displayName = GetSavedFileName(savedFilePath);

            DisplayName = displayName;
            SavedFilePath = savedFilePath;
        }

        public string BuildTitle(bool isDirty)
        {
            return ApplyDirtyPrefix(DisplayName, isDirty);
        }

        public string BuildTooltip(bool isDirty)
        {
            //Preserve the existing full-path tooltip for a saved editor.
            return ApplyDirtyPrefix(HasSavedFile ? SavedFilePath : DisplayName, isDirty);
        }

        public bool MatchesSavedFilePath(string filePath)
        {
            //Keep the existing ordinal comparison; do not infer identity from display names.
            return HasSavedFile && string.Equals(SavedFilePath, filePath, StringComparison.Ordinal);
        }

        private static string ApplyDirtyPrefix(string value, bool isDirty)
        {
            return isDirty ? $"*{value}" : value;
        }

        private static string GetSavedFileName(string savedFilePath)
        {
            if (string.IsNullOrWhiteSpace(savedFilePath))
            {
                throw new ArgumentException("A saved editor requires a file path.", nameof(savedFilePath));
            }

            var fileName = Path.GetFileName(savedFilePath);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("A saved editor requires a file name.", nameof(savedFilePath));
            }

            return fileName;
        }
    }
}
