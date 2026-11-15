using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace JasonQuery.UI.LeftPanel
{
    //F7-U4A-B: display-only filter. Never modify the global schema table or DB state.
    public static class SharedSchemaExplorerFilterPolicy
    {
        public static bool TryApplyFilter(SharedSchemaMetadataSnapshot snapshot, string searchText)
        {
            var table = snapshot?.Table;

            if (table == null || !table.Columns.Contains("SchemaName") || !table.Columns.Contains("Schema_Browser")
                || table.Columns["SchemaName"].DataType != typeof(string)
                || table.Columns["Schema_Browser"].DataType != typeof(string))
            {
                return false;
            }

            var text = (searchText ?? string.Empty).Trim().Replace('%', '*');

            while (text.Contains("**"))
            {
                text = text.Replace("**", "*");
            }

            if (text.Length == 0 || text == "*")
            {
                return TrySetRowFilter(table.DefaultView, string.Empty);
            }

            //Match the legacy F7's implicit trailing wildcard (a prefix search by default).
            if (!text.EndsWith("*", StringComparison.Ordinal))
            {
                text += "*";
            }

            var searchColumnInfo = Array.IndexOf(snapshot.GroupColumns, "SchemaName") >= 0;
            var columns = searchColumnInfo ? new[] { "SchemaName", "Schema_Browser" } : new[] { "Schema_Browser" };

            string filter;

            if (HasOnlyEdgeWildcards(text))
            {
                //DataView LIKE supports edge wildcards, but not ordered internal wildcards.
                var pattern = EscapeLikePattern(text);
                var terms = new List<string>();

                foreach (var column in columns)
                {
                    terms.Add("[" + column + "] LIKE '" + pattern + "'");
                }

                filter = "(" + string.Join(" OR ", terms) + ")";
            }
            else
            {
                //Evaluate ordered fragments against each actual metadata string. Using each *column's own* matching values avoids leaking matches between different rows which happen to share a table name in other schemas.
                var terms = new List<string>();

                foreach (var column in columns)
                {
                    var values = new HashSet<string>(StringComparer.Ordinal);

                    foreach (DataRow row in table.Rows)
                    {
                        if (row.RowState == DataRowState.Deleted || row.IsNull(column))
                        {
                            continue;
                        }

                        var value = Convert.ToString(row[column]);

                        if (MatchesOrderedFragments(value, text))
                        {
                            values.Add(value);
                        }
                    }

                    if (values.Count > 0)
                    {
                        var literals = new List<string>();

                        foreach (var value in values)
                        {
                            literals.Add("'" + value.Replace("'", "''") + "'");
                        }

                        terms.Add("[" + column + "] IN (" + string.Join(", ", literals) + ")");
                    }
                }

                filter = terms.Count == 0 ? "1 = 0" : "(" + string.Join(" OR ", terms) + ")";
            }

            return TrySetRowFilter(table.DefaultView, filter);
        }

        private static bool HasOnlyEdgeWildcards(string text)
        {
            for (var i = 1; i < text.Length - 1; i++)
            {
                if (text[i] == '*')
                {
                    return false;
                }
            }

            return true;
        }

        private static string EscapeLikePattern(string text)
        {
            var result = new StringBuilder(text.Length + 8);

            foreach (var character in text)
            {
                switch (character)
                {
                    case '*':
                        {
                            result.Append('%');
                            break;
                        }
                    case '\'':
                        {
                            result.Append("''");
                            break;
                        }
                    case '[':
                        {
                            result.Append("[[]");
                            break;
                        }
                    case ']':
                        {
                            result.Append("[]]");
                            break;
                        }
                    default:
                        {
                            result.Append(character);
                            break;
                        }
                }
            }

            return result.ToString();
        }

        private static bool MatchesOrderedFragments(string value, string pattern)
        {
            if (value == null)
            {
                return false;
            }

            var fragments = pattern.Split('*');
            var nextIndex = 0;

            for (var i = 0; i < fragments.Length; i++)
            {
                var fragment = fragments[i];

                if (fragment.Length == 0)
                {
                    continue;
                }

                var found = value.IndexOf(fragment, nextIndex, StringComparison.OrdinalIgnoreCase);

                if (found < 0 || (i == 0 && pattern[0] != '*' && found != 0))
                {
                    return false;
                }

                nextIndex = found + fragment.Length;
            }

            return true;
        }

        private static bool TrySetRowFilter(DataView view, string filter)
        {
            var previous = view.RowFilter;

            try
            {
                view.RowFilter = filter;
                return true;
            }
            catch (Exception)
            {
                //Invalid user input must never destroy the previous working view.
                try
                {
                    view.RowFilter = previous;
                }
                catch (Exception)
                {
                    //The caller treats this as a failed, non-authorizing filter.
                }

                return false;
            }
        }
    }
}
