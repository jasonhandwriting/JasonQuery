using JasonQuery.Core.Text;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private sealed class EditorRightClickContext
        {
            public string Word { get; set; } = string.Empty;
            public string SchemaNode { get; set; } = string.Empty;
            public string SchemaType { get; set; } = string.Empty;
            public string SchemaName { get; set; } = string.Empty;
            public string SchemaDbo { get; set; } = string.Empty;
            public string ObjectId { get; set; } = string.Empty;
            public bool IsTable { get; set; }
            public bool IsView { get; set; }
            public bool IsSelected { get; set; } //是否有選取文字
            public bool HasSelectedNonWhiteSpaceText { get; set; } //20260609 是否有選取非空白文字
            public bool IsTableOrView => IsTable || IsView;
        }

        private bool TryHandleEditorLeftMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return false;
            }

            if (c1GridAutoCompleteForPeriod.Visible)
            {
                HandleEditorLeftMouseDownForPeriodAutoComplete();
                return true;
            }

            if (c1GridAutoCompleteForSpace.Visible)
            {
                HandleEditorLeftMouseDownForSpaceAutoComplete();
                return true;
            }

            if (c1GridAutoCompleteForAll.Visible)
            {
                HandleEditorLeftMouseDownForAllAutoComplete();
                return true;
            }

            HandleEditorLeftMouseDownWithCtrlIfNeeded(e);
            return true;
        }

        private void HandleEditorLeftMouseDownWithCtrlIfNeeded(MouseEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            //20230903 0.85 待完成功能：檢查左鍵和 Ctrl 鍵是否同時按下了？這個功能主要是為了讓使用者可以 Ctrl + 左鍵點擊 來快速開啟 Schema Browser 的對應物件頁籤
            if (e.Button == MouseButtons.Left && ModifierKeys.HasFlag(Keys.Control))
            {
                //待確認：是否要隱藏？ HideAutoCompleteGrid()
                //MessageBox.Show("使用者同時按下左鍵和 Ctrl 鍵！");
                //var word = editor.GetTextRange(wordStart, wordEnd - wordStart);
            }
        }

        private bool IsEditorPositionOutsideAutoCompleteKeywordRange(int triggerPosition)
        {
            if (editor.CurrentPosition == triggerPosition)
            {
                return false;
            }

            if (editor.CurrentPosition == triggerPosition - 1)
            {
                return true;
            }

            if (editor.CurrentPosition < triggerPosition)
            {
                return true;
            }

            var keywordEndPosition = triggerPosition;

            for (var i = triggerPosition; i < editor.Text.Length; i++)
            {
                char currentChar = editor.Text[i];

                if (!TextHelper.IsEngAlphabetOrNumber(currentChar, '_'))
                {
                    break;
                }

                keywordEndPosition = i + 1;
            }

            return editor.CurrentPosition > keywordEndPosition;
        }

        private void HandleEditorLeftMouseDownForPeriodAutoComplete()
        {
            if (_periodAutoCompleteSession.TriggerPosition == editor.CurrentPosition)
            {
                return;
            }

            if (IsEditorPositionOutsideAutoCompleteKeywordRange(_periodAutoCompleteSession.TriggerPosition))
            {
                HidePeriodAutoCompletePopup(QueryEditorAutoCompleteSessionCloseReason.Navigation);
            }
        }

        private void HandleEditorLeftMouseDownForSpaceAutoComplete()
        {
            if (_spaceAutoCompleteSession.TriggerPosition == editor.CurrentPosition)
            {
                return;
            }

            if (IsEditorPositionOutsideAutoCompleteKeywordRange(_spaceAutoCompleteSession.TriggerPosition))
            {
                HideSpaceAutoCompletePopup(QueryEditorAutoCompleteSessionCloseReason.Navigation);
            }
        }

        private EditorRightClickContext ResolveEditorRightClickContext()
        {
            HideAutoCompleteGrid(false);

            var hasSelection = editor.SelectionStart != editor.SelectionEnd;
            var selectedText = hasSelection ? editor.SelectedText : string.Empty;

            var context = new EditorRightClickContext
            {
                IsSelected = hasSelection,
                HasSelectedNonWhiteSpaceText = !string.IsNullOrWhiteSpace(selectedText)
            };

            ResolveEditorRightClickWord(context);

            if (ShouldResolveEditorObject(context))
            {
                ResolveEditorObject(context);
            }

            return context;
        }

        private void ResolveEditorRightClickWord(EditorRightClickContext context)
        {
            if (context == null)
            {
                return;
            }

            ClearEditorRightClickTarget(context);

            var rawText = GetEditorRightClickRawText(context.IsSelected);

            if (string.IsNullOrWhiteSpace(rawText))
            {
                return;
            }

            ApplyEditorRightClickObjectText(context, rawText);
        }

        private void ClearEditorRightClickTarget(EditorRightClickContext context)
        {
            context.SchemaNode = string.Empty;
            context.SchemaDbo = string.Empty;
            context.Word = string.Empty;
        }

        private string GetEditorRightClickRawText(bool hasSelection)
        {
            if (hasSelection)
            {
                return (editor.SelectedText ?? string.Empty).Trim();
            }

            return GetEditorTextNearCurrentPosition();
        }

        private string GetEditorTextNearCurrentPosition()
        {
            var text = editor.Text ?? string.Empty;

            if (text.Length == 0)
            {
                return string.Empty;
            }

            var position = editor.CurrentPosition;

            if (position < 0)
            {
                return string.Empty;
            }

            if (position > text.Length)
            {
                position = text.Length;
            }

            var anchor = position;

            if (anchor >= text.Length)
            {
                anchor = text.Length - 1;
            }

            //如果游標剛好停在空白上，優先往左找前一個非空白字元
            if (char.IsWhiteSpace(text[anchor]) && anchor > 0)
            {
                anchor--;
            }

            while (anchor >= 0 && char.IsWhiteSpace(text[anchor]))
            {
                anchor--;
            }

            if (anchor < 0)
            {
                return string.Empty;
            }

            var start = anchor;

            while (start > 0 && !char.IsWhiteSpace(text[start - 1]))
            {
                start--;
            }

            var end = anchor + 1;

            while (end < text.Length && !char.IsWhiteSpace(text[end]))
            {
                end++;
            }

            return text.Substring(start, end - start).Trim();
        }

        private void ApplyEditorRightClickObjectText(EditorRightClickContext context, string rawText)
        {
            var objectText = NormalizeEditorRightClickObjectText(rawText);

            if (string.IsNullOrWhiteSpace(objectText))
            {
                ClearEditorRightClickTarget(context);
                return;
            }

            var parts = objectText.Split(new[] { "." }, StringSplitOptions.None);

            if (!TryApplyEditorRightClickObjectParts(context, parts))
            {
                ClearEditorRightClickTarget(context);
                return;
            }

            if (!IsValidEditorRightClickObjectContext(context))
            {
                ClearEditorRightClickTarget(context);
            }
        }

        private string NormalizeEditorRightClickObjectText(string value)
        {
            var text = (value ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            text = TrimEditorRightClickTokenBoundary(text);

            if (IsSqlServer)
            {
                //支援 [dbo].[TableName]、[MyDb].[dbo].[TableName]
                text = text.Replace("[", string.Empty)
                           .Replace("]", string.Empty);
            }
            else if (IsMySql)
            {
                //支援 `sakila`.`film`
                text = text.Replace("`", string.Empty);
            }

            return TrimEditorRightClickTokenBoundary(text);
        }

        private bool TryApplyEditorRightClickObjectParts(EditorRightClickContext context, string[] parts)
        {
            if (parts == null || parts.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < parts.Length; i++)
            {
                parts[i] = (parts[i] ?? string.Empty).Trim();

                if (string.IsNullOrEmpty(parts[i]))
                {
                    return false;
                }
            }

            if (IsSqlServer)
            {
                return TryApplySqlServerRightClickObjectParts(context, parts);
            }

            return TryApplyDefaultRightClickObjectParts(context, parts);
        }

        private bool TryApplyDefaultRightClickObjectParts(EditorRightClickContext context, string[] parts)
        {
            if (parts.Length == 1)
            {
                context.Word = parts[0];
                return true;
            }

            if (parts.Length == 2)
            {
                //Oracle: PROD.TABLENAME
                //PostgreSQL: public.tablename
                //MySQL: sakila.tablename
                context.SchemaNode = parts[0];
                context.Word = parts[1];
                return true;
            }

            return false;
        }

        private bool TryApplySqlServerRightClickObjectParts(EditorRightClickContext context, string[] parts)
        {
            if (parts.Length == 1)
            {
                context.Word = parts[0];
                return true;
            }

            if (parts.Length == 2)
            {
                //dbo.TableName
                context.SchemaDbo = parts[0];
                context.Word = parts[1];
                return true;
            }

            if (parts.Length == 3)
            {
                //MyDb.dbo.TableName
                context.SchemaNode = parts[0];
                context.SchemaDbo = parts[1];
                context.Word = parts[2];
                return true;
            }

            return false;
        }

        private string TrimEditorRightClickTokenBoundary(string value)
        {
            return (value ?? string.Empty).Trim().Trim(',', ';', '(', ')');
        }

        private bool IsValidEditorRightClickObjectContext(EditorRightClickContext context)
        {
            if (context == null)
            {
                return false;
            }

            if (!IsValidEditorObjectNamePart(context.Word))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(context.SchemaNode) && !IsValidEditorObjectNamePart(context.SchemaNode))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(context.SchemaDbo) && !IsValidEditorObjectNamePart(context.SchemaDbo))
            {
                return false;
            }

            return true;
        }

        private bool IsValidEditorObjectNamePart(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (var c in value)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    continue;
                }

                if (c >= 'a' && c <= 'z')
                {
                    continue;
                }

                if (c >= '0' && c <= '9')
                {
                    continue;
                }

                if (c == '_')
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private bool ShouldResolveEditorObject(EditorRightClickContext context)
        {
            if (context == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(context.Word);
        }

        private void ResolveEditorObject(EditorRightClickContext context)
        {
            QueryEditorObjectResolver.Resolve(this, context);
        }

        private void ApplyEditorContextMenuState(EditorRightClickContext context)
        {
            QueryEditorContextMenuStateApplier.Apply(this, context);
        }

        private void ApplyEditorContextMenuDarkModeStyle()
        {
            QueryEditorContextMenuStateApplier.ApplyDarkModeStyle(this);
        }

        private void ShowEditorContextMenu(MouseEventArgs e)
        {
            QueryEditorContextMenuStateApplier.Show(this, e);
        }
    }
}
