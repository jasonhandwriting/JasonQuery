using System;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal static class EditorSelectionHelper
    {
        public static EditorSelectionRange GetExpandedSelectionRange(ITextEditor editor, string text,int start, int end,
                                                                     EditorSelectionMode mode = EditorSelectionMode.Normal)
        {
            var result = new EditorSelectionRange
            {
                Start = 0,
                End = 0,
                FirstNonSpaceStart = 0
            };

            if (string.IsNullOrEmpty(text))
            {
                return result;
            }

            var array = text.ToCharArray();
            var startIndex = Math.Max(0, Math.Min(start, array.Length));
            var endIndex = Math.Max(0, Math.Min(end, array.Length));
            char letter;

            bool IsSelectBlockMode(int i, ref int start2)
            {
                if (letter != '\n')
                {
                    return false;
                }

                if (i - 3 <= 0 || array[i - 1] != '\r' || array[i - 2] != '\n' || array[i - 3] != '\r')
                {
                    return false;
                }

                start2 = i + 1;
                return true;
            }

            //往前找開始位置
            for (var i = startIndex - 1; i >= 0; i--)
            {
                letter = array[i];

                var start2 = i;

                if (mode == EditorSelectionMode.SelectBlock)
                {
                    if (IsSelectBlockMode(i, ref start2))
                    {
                        result.Start = start2;
                        break;
                    }

                    result.Start = start2;
                }
                else
                {
                    result.Start = i;

                    if (letter != '\n')
                    {
                        continue;
                    }

                    result.Start = i + 1;
                    break;
                }
            }

            //往後找結束位置
            for (var i = endIndex; i < array.Length; i++)
            {
                letter = array[i];
                result.End = i;

                if (mode == EditorSelectionMode.SelectBlock)
                {
                    if (letter == '\r')
                    {
                        if (i + 3 >= array.Length)
                        {
                            result.End = array.Length - 1;
                            break;
                        }

                        if (array[i + 1] == '\n' && array[i + 2] == '\r' && array[i + 3] == '\n')
                        {
                            result.End = i;
                            break;
                        }
                    }

                    if (i == array.Length - 1)
                    {
                        result.End = i + 1;
                    }
                }
                else
                {
                    if (letter == '\r' || letter == '\n')
                    {
                        result.End -= 1;
                        break;
                    }
                }
            }

            if ((result.End == 0 && endIndex == array.Length) || result.End < endIndex)
            {
                result.End = endIndex - 1;
            }

            if (mode != EditorSelectionMode.AddComment)
            {
                return result;
            }

            editor.SelectionStart = result.Start;
            editor.SelectionEnd = Math.Max(0, Math.Min(result.End + 1, text.Length));

            var selectedText = editor.SelectedText;
            var parts = selectedText.Split(new[] { "\r\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();

            foreach (var line in parts)
            {
                var trimmedLine = line.Trim();

                if (string.IsNullOrEmpty(trimmedLine))
                {
                    sb.Append(line.Length).Append(",");
                }
                else
                {
                    for (var j = 0; j < line.Length; j++)
                    {
                        if (line[j] != ' ')
                        {
                            sb.Append(j).Append(",");
                            break;
                        }
                    }
                }
            }

            if (sb.Length > 0)
            {
                sb.Length--;
            }

            var noneSpace = sb.ToString();
            var aryNoneSpace = noneSpace.Split(new[] { "," }, StringSplitOptions.None);

            SortByNumeric(aryNoneSpace);
            int.TryParse(aryNoneSpace[0], out var noneSpaceStart);

            result.FirstNonSpaceStart = noneSpaceStart;

            return result;
        }

        private static void SortByNumeric(string[] values)
        {
            var rgx = new Regex("([^0-9]*)([0-9]+)");

            Array.Sort(values, (a, b) =>
            {
                var ma = rgx.Matches(a);
                var mb = rgx.Matches(b);

                for (var i = 0; i < ma.Count; ++i)
                {
                    var ret = string.Compare(ma[i].Groups[1].Value, mb[i].Groups[1].Value, StringComparison.Ordinal);

                    if (ret != 0)
                    {
                        return ret;
                    }

                    ret = int.Parse(ma[i].Groups[2].Value) - int.Parse(mb[i].Groups[2].Value);

                    if (ret != 0)
                    {
                        return ret;
                    }
                }

                return 0;
            });
        }

        public static EditorSelectionRange GetCurrentBlockRangeTreatEntireBlankRowAsEmptyRow(ITextEditor editor)
        {
            var result = new EditorSelectionRange
            {
                Start = 0,
                End = 0,
                FirstNonSpaceStart = 0
            };

            var text = editor?.Text ?? string.Empty;

            if (string.IsNullOrEmpty(text))
            {
                return result;
            }

            var parts = text.Split(new[] { "\r\n" }, StringSplitOptions.None);

            if (parts.Length == 0)
            {
                return result;
            }

            var currentLine = editor.CurrentLine;

            if (currentLine < 0)
            {
                currentLine = 0;
            }
            else if (currentLine > parts.Length - 1)
            {
                currentLine = parts.Length - 1;
            }

            var lineStart = 0;
            var lineEnd = 0;
            var pos = 0;
            string lineText;

            for (var i = currentLine; i > 0; i--)
            {
                lineText = parts[i].Trim();

                if (!string.IsNullOrEmpty(lineText) || i == currentLine)
                {
                    continue;
                }

                lineStart = i + 1;
                break;
            }

            if (currentLine == parts.Length - 1 || (string.IsNullOrEmpty(parts[currentLine])
                                                && currentLine + 1 <= parts.Length - 1
                                                && string.IsNullOrEmpty(parts[currentLine + 1])))
            {
                lineEnd = currentLine;
            }
            else
            {
                for (var i = currentLine; i < parts.Length; i++)
                {
                    lineText = parts[i].Trim();

                    if (string.IsNullOrEmpty(lineText) && i != currentLine)
                    {
                        lineEnd = i - 1;
                        break;
                    }

                    if (string.IsNullOrEmpty(lineText) && i == currentLine)
                    {
                        lineEnd = i;
                        break;
                    }

                    lineEnd = i;
                }

                if (lineStart == 0 && lineEnd == 0)
                {
                    lineEnd = 0;
                }
                else if (lineStart >= 0 && lineEnd == 0)
                {
                    lineEnd = parts.Length - 1;
                }
            }

            for (var i = 0; i < parts.Length; i++)
            {
                pos += parts[i].Length + 2;

                if (i == lineStart - 1)
                {
                    result.Start = pos;
                }
                else if (i == lineEnd)
                {
                    result.End = pos - 2;
                }
            }

            return result;
        }
    }
}