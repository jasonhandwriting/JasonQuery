using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private const int GridDataCaptionBaseAdjustment = -6;
        private const int GridDataCaptionAdditionalLineHeight = 17;

        private sealed class GridDataColumnCaptionInfo
        {
            public C1DataColumn Column { get; set; }
            public string ColumnName { get; set; }
            public string FullDataType { get; set; }
            public string ConstraintText { get; set; }
            public string Comment { get; set; }
        }

        /// <summary>
        /// 重新產生資料編輯 Grid 的多行欄位標題，並依實際需要調整標題列高度。
        /// 此方法不重建 Editor，因此可在切換語系或重新套用畫面設定時單獨呼叫。
        /// </summary>
        private void RefreshGridDataColumnCaptionsAndHeight()
        {
            if (c1GridData == null || _columnInfoCollector == null)
            {
                return;
            }

            var captionInfos = new List<GridDataColumnCaptionInfo>();
            var hasConstraintLine = false;
            var hasCommentLine = false;

            foreach (C1DataColumn column in c1GridData.Columns)
            {
                var columnName = column.DataField;

                if (IsInternalGridDataColumn(columnName))
                {
                    continue;
                }

                if (!_columnInfoCollector.TryGet(columnName, out var columnInfo))
                {
                    continue;
                }

                var constraintText = GetGridDataColumnConstraintText(columnInfo.IsPrimaryKey, columnInfo.IsNullable);
                var comment = columnInfo.ColumnComment ?? string.Empty;

                hasConstraintLine |= !string.IsNullOrWhiteSpace(constraintText);
                hasCommentLine |= !string.IsNullOrWhiteSpace(comment);

                captionInfos.Add
                (
                    new GridDataColumnCaptionInfo
                    {
                        Column = column,
                        ColumnName = columnInfo.ColumnName,
                        FullDataType = columnInfo.FullDataType,
                        ConstraintText = constraintText,
                        Comment = comment
                    }
                );
            }

            foreach (var captionInfo in captionInfos)
            {
                captionInfo.Column.Caption = BuildGridDataColumnCaption(captionInfo, hasConstraintLine, hasCommentLine);
            }

            var captionRows = 2 + (hasConstraintLine ? 1 : 0) + (hasCommentLine ? 1 : 0);

            SetGridDataColumnCaptionHeight(captionRows);
        }

        private bool IsInternalGridDataColumn(string columnName)
        {
            return string.Equals(columnName, MyGlobal.Row_Id_PK_JQ, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(columnName, _identifyColumnName, StringComparison.OrdinalIgnoreCase);
        }

        private string GetGridDataColumnConstraintText(bool isPrimaryKey, bool isNullable)
        {
            var primaryKeyText = isPrimaryKey ? (IsPostgreSql ? "p" : "P") : string.Empty;
            var notNullText = IsPostgreSql ? "not null" : "NOT NULL";

            if (isNullable)
            {
                return primaryKeyText;
            }

            return string.IsNullOrWhiteSpace(primaryKeyText) ? notNullText : $"{primaryKeyText}, {notNullText}";
        }

        private static string BuildGridDataColumnCaption(GridDataColumnCaptionInfo captionInfo, bool hasConstraintLine, bool hasCommentLine)
        {
            var sbCaption = new StringBuilder();

            sbCaption.Append(captionInfo.ColumnName ?? string.Empty);
            sbCaption.AppendLine();
            sbCaption.Append(captionInfo.FullDataType ?? string.Empty);

            if (hasConstraintLine)
            {
                sbCaption.AppendLine();
                sbCaption.Append(captionInfo.ConstraintText ?? string.Empty);
            }

            if (hasCommentLine)
            {
                sbCaption.AppendLine();
                sbCaption.Append(captionInfo.Comment ?? string.Empty);
            }

            return sbCaption.ToString();
        }

        private void SetGridDataColumnCaptionHeight(int captionRows)
        {
            var normalizedCaptionRows = Math.Max(2, Math.Min(4, captionRows));
            var rowHeight = Math.Max(1, c1GridData.RowHeight);
            var additionalLineCount = normalizedCaptionRows - 2;
            var captionHeight = rowHeight * 2 + GridDataCaptionBaseAdjustment + additionalLineCount * GridDataCaptionAdditionalLineHeight;

            c1GridData.Splits[0].ColumnCaptionHeight = Math.Max(rowHeight, captionHeight);
        }
    }
}
