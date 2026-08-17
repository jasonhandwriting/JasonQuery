using System;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private const string EditorObjectMenuPayloadPrefix = "JQ_EDITOR_OBJECT_PAYLOAD_V1:";

        private sealed class EditorObjectMenuPayload
        {
            public string SchemaNode { get; set; } = string.Empty;
            public string SchemaType { get; set; } = string.Empty;
            public string SchemaName { get; set; } = string.Empty;
            public string AccessibleDescription { get; set; } = string.Empty;
            public string SchemaDbo { get; set; } = string.Empty;
            public string ObjectId { get; set; } = string.Empty;
        }

        private EditorObjectMenuPayload CreateEditorObjectMenuPayload(EditorRightClickContext context)
        {
            if (context == null)
            {
                return new EditorObjectMenuPayload();
            }

            return new EditorObjectMenuPayload
            {
                SchemaNode = context.SchemaNode,
                SchemaType = context.SchemaType,
                SchemaName = context.SchemaName,
                SchemaDbo = context.SchemaDbo,
                ObjectId = context.ObjectId
            };
        }

        private void SetEditorObjectMenuPayload(int menuIndex, EditorObjectMenuPayload payload)
        {
            _queryEditorContextMenu.Items[menuIndex].AccessibleDescription = BuildEditorObjectPayloadText(payload);
        }

        private EditorObjectMenuPayload GetEditorObjectMenuPayload(int menuIndex)
        {
            return ParseEditorObjectPayloadText(_queryEditorContextMenu.Items[menuIndex].AccessibleDescription);
        }

        private string BuildEditorObjectPayloadText(EditorObjectMenuPayload payload)
        {
            if (payload == null)
            {
                payload = new EditorObjectMenuPayload();
            }

            return EditorObjectMenuPayloadPrefix + string.Join
            (
                ";",
                EncodeEditorPayloadValue(payload.SchemaNode),
                EncodeEditorPayloadValue(payload.SchemaType),
                EncodeEditorPayloadValue(payload.SchemaName),
                EncodeEditorPayloadValue(payload.AccessibleDescription),
                EncodeEditorPayloadValue(payload.SchemaDbo),
                EncodeEditorPayloadValue(payload.ObjectId)
            );
        }

        private EditorObjectMenuPayload ParseEditorObjectPayloadText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new EditorObjectMenuPayload();
            }

            if (text.StartsWith(EditorObjectMenuPayloadPrefix, StringComparison.Ordinal))
            {
                var payloadText = text.Substring(EditorObjectMenuPayloadPrefix.Length);
                var parts = payloadText.Split(new[] { ";" }, StringSplitOptions.None);

                return new EditorObjectMenuPayload
                {
                    SchemaNode = GetDecodedEditorPayloadValue(parts, 0),
                    SchemaType = GetDecodedEditorPayloadValue(parts, 1),
                    SchemaName = GetDecodedEditorPayloadValue(parts, 2),
                    AccessibleDescription = GetDecodedEditorPayloadValue(parts, 3),
                    SchemaDbo = GetDecodedEditorPayloadValue(parts, 4),
                    ObjectId = GetDecodedEditorPayloadValue(parts, 5)
                };
            }

            //相容保護：若還有舊格式或未經 helper 建立的字串，避免直接拋例外
            //這裡採用第四階段後的新統一順序：
            //SchemaNode;SchemaType;SchemaName;AccessibleDescription;SchemaDbo;ObjectId
            var legacyParts = text.Split(new[] { ";" }, StringSplitOptions.None);

            return new EditorObjectMenuPayload
            {
                SchemaNode = GetEditorPayloadValue(legacyParts, 0),
                SchemaType = GetEditorPayloadValue(legacyParts, 1),
                SchemaName = GetEditorPayloadValue(legacyParts, 2),
                AccessibleDescription = GetEditorPayloadValue(legacyParts, 3),
                SchemaDbo = GetEditorPayloadValue(legacyParts, 4),
                ObjectId = GetEditorPayloadValue(legacyParts, 5)
            };
        }

        private string EncodeEditorPayloadValue(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);

            return Convert.ToBase64String(bytes);
        }

        private string GetDecodedEditorPayloadValue(string[] parts, int index)
        {
            var value = GetEditorPayloadValue(parts, index);

            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            try
            {
                var bytes = Convert.FromBase64String(value);

                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetEditorPayloadValue(string[] parts, int index)
        {
            if (parts == null || index < 0 || index >= parts.Length)
            {
                return string.Empty;
            }

            return parts[index] ?? string.Empty;
        }
    }
}