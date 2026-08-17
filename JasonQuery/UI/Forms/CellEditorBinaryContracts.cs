using System;

namespace JasonQuery.UI.Forms
{
    public sealed class CellEditorBinaryUpdateRequest
    {
        public string FileName { get; set; }

        public long FileLength { get; set; }
    }

    public sealed class CellEditorBinaryUpdateResult
    {
        public bool Success { get; set; }

        public int AffectedRows { get; set; }

        public string Sql { get; set; }

        public string ErrorMessage { get; set; }

        public string DisplayText { get; set; }

        public string BinaryTypeName { get; set; }

        public string LocatorDescription { get; set; }

        public static CellEditorBinaryUpdateResult Failed(string errorMessage)
        {
            return new CellEditorBinaryUpdateResult
            {
                Success = false,
                ErrorMessage = errorMessage ?? string.Empty
            };
        }
    }

    public sealed class CellEditorBinaryValueAppliedEventArgs : EventArgs
    {
        public CellEditorBinaryValueAppliedEventArgs(byte[] value, string sourceFileName, CellEditorBinaryUpdateResult updateResult)
        {
            Value = value ?? Array.Empty<byte>();
            SourceFileName = sourceFileName ?? string.Empty;
            UpdateResult = updateResult;
        }

        public byte[] Value { get; private set; }

        public string SourceFileName { get; private set; }

        public CellEditorBinaryUpdateResult UpdateResult { get; private set; }
    }
}
