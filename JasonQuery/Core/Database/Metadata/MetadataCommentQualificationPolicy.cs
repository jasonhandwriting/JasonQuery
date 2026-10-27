using System;
using System.Data;

namespace JasonQuery.Core.Database.Metadata
{
    internal sealed class MetadataCommentQualificationResult
    {
        public string Comment { get; set; }
        public bool HasExistingComment { get; set; }
    }

    /// <summary>
    /// A comment may be NULL/empty even when its base object exists.
    /// Only a single, correctly shaped result is qualified; never pick Rows[0]
    /// from a duplicate set or treat null reader output as an empty comment.
    /// D1-B2 must additionally disable Comment UI when qualification fails.
    /// </summary>
    internal static class MetadataCommentQualificationPolicy
    {
        public static bool TryQualify(DataTable metadata, out MetadataCommentQualificationResult result)
        {
            result = null;

            if (metadata == null || metadata.Rows.Count != 1 || !metadata.Columns.Contains("MetadataComment") || !metadata.Columns.Contains("HasExistingComment"))
            {
                return false;
            }

            var row = metadata.Rows[0];

            if (!TryReadFlag(row["HasExistingComment"], out bool hasExistingComment))
            {
                return false;
            }

            var comment = row["MetadataComment"] == DBNull.Value ? string.Empty : Convert.ToString(row["MetadataComment"]);

            if (comment == null)
            {
                return false;
            }

            result = new MetadataCommentQualificationResult
            {
                Comment = comment,
                HasExistingComment = hasExistingComment
            };

            return true;
        }

        private static bool TryReadFlag(object value, out bool result)
        {
            result = false;

            if (value == null || value == DBNull.Value)
            {
                return false;
            }

            if (value is bool boolean)
            {
                result = boolean;
                return true;
            }

            var text = Convert.ToString(value);

            if (string.Equals(text, "1", StringComparison.Ordinal))
            {
                result = true;
                return true;
            }

            if (string.Equals(text, "0", StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }
    }
}
