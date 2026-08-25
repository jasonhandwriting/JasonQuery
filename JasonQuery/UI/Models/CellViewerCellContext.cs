using JasonLibrary.Core.Schema.Enums;
using System;

namespace JasonQuery.UI.Models
{
    public sealed class CellViewerCellContext
    {
        public string ColumnName { get; set; } = string.Empty;

        public string ColumnType { get; set; } = string.Empty;

        public string DisplayText { get; set; } = string.Empty;

        public CategoryDataTypeKind CategoryDataTypeKindValue { get; set; } = CategoryDataTypeKind.String;

        public bool IsBinaryColumn { get; set; }

        public bool HasBinaryContent { get; set; }

        /// <summary>
        /// 已經在記憶體中的 Binary 內容。
        /// 例如 SingleRecordViewerForm 的 DataTable 已經帶 byte[] 時可以使用。
        /// </summary>
        public byte[] BinaryContent { get; set; }

        /// <summary>
        /// 延遲載入 Binary 內容。
        /// LargeBinaryDataType 應優先使用這個，不要在 CellViewer 開啟時立刻 LoadContent。
        /// </summary>
        public Func<byte[]> BinaryContentLoader { get; set; }

        /// <summary>
        /// Binary 長度。若來源不提供長度，則保持 null。
        /// </summary>
        public int? BinaryLength { get; set; }
    }
}
