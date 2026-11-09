using C1.Win.C1TrueDBGrid;
using JasonQuery.UI.LeftPanel;
using JasonQuery.UI.SchemaExplorer;
using System;
using System.Collections.Generic;

namespace JasonQuery.UI.Forms
{
    public partial class MainForm
    {
        //One short-lived UI focus, captured immediately before a metadata reload.
        //No DDL kind, execution state, or DataTable row index is stored.
        private sealed class SchemaExplorerFocusState
        {
            internal string DatabaseScope;
            internal int ConnectionRevision;
            internal string[] Groups;
            internal bool[] Expanded;
            internal int GroupOrdinal = -1;
            internal string LeafText;
            internal int LeafOrdinal = -1;
        }

        private SchemaExplorerFocusState _schemaExplorerFocusPendingRestore;

        private SchemaExplorerFocusState CaptureSchemaExplorerFocusBeforeRebind()
        {
            var grid = _sharedSchemaExplorerGrid;

            if (grid == null || grid.IsDisposed || grid.Splits.Count == 0 || grid.DataSource == null || !IsSharedSchemaSnapshotInCurrentScope())
            {
                return null;
            }

            try
            {
                var rows = grid.Splits[0].Rows;
                var currentRow = grid.Row;

                if (currentRow < 0 || currentRow >= rows.Count)
                {
                    return null;
                }

                var names = new List<string>();
                var expanded = new List<bool>();

                //Read the live GroupBy path. Display-row indices are never retained across a rebind.
                for (var i = 0; i <= currentRow; i++)
                {
                    if (!(rows[i] is GroupRow group))
                    {
                        continue;
                    }

                    if (group.Level < 0 || group.Level > names.Count)
                    {
                        return null;
                    }

                    while (names.Count > group.Level)
                    {
                        names.RemoveAt(names.Count - 1);
                        expanded.RemoveAt(expanded.Count - 1);
                    }

                    names.Add(group.GroupedText);
                    expanded.Add(group.RowType == RowTypeEnum.ExpandedGroupRow);
                }

                //A root-only or initial view keeps the established provider default expansion.
                if (names.Count < 2)
                {
                    return null;
                }

                var capture = new SchemaExplorerFocusState
                {
                    DatabaseScope = GetSharedSchemaDatabaseScope(),
                    ConnectionRevision = _sharedSchemaMetadataSnapshot.ConnectionRevision,
                    Groups = names.ToArray(),
                    Expanded = expanded.ToArray()
                };

                if (rows[currentRow] is GroupRow)
                {
                    //If this group disappears, remain near its former sibling position.
                    var level = names.Count - 1;
                    var parent = level == 0 ? -1 : LocateGroup(grid, capture.Groups, level);

                    if (parent >= 0)
                    {
                        var siblings = ReadChildGroupNames(grid, parent, level);

                        capture.GroupOrdinal = Array.IndexOf(siblings, capture.Groups[level]);
                    }
                }
                else
                {
                    //For an existing column, prefer its exact displayed text after reload.
                    var parent = LocateGroup(grid, capture.Groups, capture.Groups.Length);
                    var dataRow = new C1SchemaExplorerRowReader(grid).GetDataRow(currentRow);

                    if (parent >= 0 && dataRow != null && dataRow.Table.Columns.Contains("Schema_Browser"))
                    {
                        capture.LeafText = Convert.ToString(dataRow["Schema_Browser"]);

                        var siblings = ReadChildColumnNames(grid, parent, capture.Groups.Length - 1);

                        capture.LeafOrdinal = Array.IndexOf(siblings, capture.LeafText);
                    }
                }

                return capture;
            }
            catch (Exception)
            {
                //C1 may be reorganizing groups. A failed UI capture must not block DDL or refresh.
                return null;
            }
        }

        private void RestoreSchemaExplorerFocusAfterSuccessfulRebind()
        {
            var capture = _schemaExplorerFocusPendingRestore;

            _schemaExplorerFocusPendingRestore = null;

            if (capture == null || !SchemaExplorerTargetFocusPolicy.ShouldApplyAfterBinding
                (
                    true,
                    string.Equals(capture.DatabaseScope, GetSharedSchemaDatabaseScope(), StringComparison.Ordinal) && _sharedSchemaMetadataSnapshot != null && capture.ConnectionRevision == _sharedSchemaMetadataSnapshot.ConnectionRevision,
                    true,
                    IsSharedSchemaSnapshotInCurrentScope()
                ))
            {
                return;
            }

            var grid = _sharedSchemaExplorerGrid;

            if (grid == null || grid.IsDisposed || grid.Splits.Count == 0 || grid.DataSource == null)
            {
                return;
            }

            try
            {
                var path = SchemaExplorerTargetFocusPolicy.BuildRetainedGroupPath(capture.Groups, true, true);
                var parent = -1;
                var focusRow = -1;
                var matched = 0;

                for (var level = 0; level < path.Length; level++)
                {
                    var groupRow = FindGroupRow(grid, parent, level, path[level]);

                    if (groupRow < 0)
                    {
                        //A renamed/dropped group: fall back to its closest surviving sibling.
                        if (parent >= 0 && level == path.Length - 1 && capture.GroupOrdinal >= 0)
                        {
                            var siblings = ReadChildGroupNames(grid, parent, level);
                            var siblingIndex = SchemaExplorerTargetFocusPolicy.FindNearestFocusIndex(path[level], capture.GroupOrdinal, siblings);

                            if (siblingIndex >= 0)
                            {
                                var siblingRow = FindGroupRow(grid, parent, level, siblings[siblingIndex]);

                                if (siblingRow >= 0)
                                {
                                    focusRow = siblingRow;
                                }
                            }
                        }

                        break;
                    }

                    focusRow = groupRow;
                    matched++;

                    //Open only the chosen path, not unrelated schemas or tables.
                    if ((level < path.Length - 1 || capture.Expanded[level]) && grid.Splits[0].Rows[groupRow].RowType == RowTypeEnum.CollapsedGroupRow)
                    {
                        grid.ExpandGroupRow(groupRow);
                    }

                    parent = groupRow;
                }

                if (focusRow < 0)
                {
                    return;
                }

                if (matched == path.Length && parent == focusRow && path.Length > 0 && !string.IsNullOrWhiteSpace(capture.LeafText))
                {
                    var columns = ReadChildColumnNames(grid, focusRow, path.Length - 1);
                    var index = SchemaExplorerTargetFocusPolicy.FindNearestFocusIndex(capture.LeafText, capture.LeafOrdinal, columns);

                    if (index >= 0)
                    {
                        var leafRow = FindChildColumnRow(grid, focusRow, path.Length - 1, columns[index]);

                        if (leafRow >= 0)
                        {
                            focusRow = leafRow;
                        }
                    }
                }

                grid.Row = focusRow;
                grid.Select(focusRow, 0, 1, 1, true);
            }
            catch (Exception)
            {
                //Best-effort visual restoration; never invalidate newly loaded metadata.
            }
        }

        private static int LocateGroup(C1TrueDBGrid grid, string[] path, int count)
        {
            var parent = -1;

            for (var i = 0; i < count; i++)
            {
                parent = FindGroupRow(grid, parent, i, path[i]);

                if (parent < 0)
                {
                    return -1;
                }
            }

            return parent;
        }

        private static int FindGroupRow(C1TrueDBGrid grid, int parentIndex, int level, string text)
        {
            var rows = grid.Splits[0].Rows;

            for (var i = parentIndex + 1; i < rows.Count; i++)
            {
                if (!(rows[i] is GroupRow group))
                {
                    continue;
                }

                if (parentIndex >= 0 && group.Level < level)
                {
                    return -1;
                }

                if (group.Level == level && string.Equals(group.GroupedText, text, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string[] ReadChildGroupNames(C1TrueDBGrid grid, int parent, int level)
        {
            var names = new List<string>();
            var rows = grid.Splits[0].Rows;

            for (var i = parent + 1; i < rows.Count; i++)
            {
                if (!(rows[i] is GroupRow group))
                {
                    continue;
                }

                if (group.Level < level)
                {
                    break;
                }

                if (group.Level == level)
                {
                    names.Add(group.GroupedText);
                }
            }

            return names.ToArray();
        }

        private static string[] ReadChildColumnNames(C1TrueDBGrid grid, int parent, int parentLevel)
        {
            var names = new List<string>();
            var rows = grid.Splits[0].Rows;
            var reader = new C1SchemaExplorerRowReader(grid);

            for (var i = parent + 1; i < rows.Count; i++)
            {
                if (rows[i] is GroupRow group)
                {
                    if (group.Level <= parentLevel)
                    {
                        break;
                    }

                    continue;
                }

                var row = reader.GetDataRow(i);

                if (row != null && row.Table.Columns.Contains("Schema_Browser"))
                {
                    names.Add(Convert.ToString(row["Schema_Browser"]));
                }
            }

            return names.ToArray();
        }

        private static int FindChildColumnRow(C1TrueDBGrid grid, int parent, int parentLevel, string text)
        {
            var rows = grid.Splits[0].Rows;
            var reader = new C1SchemaExplorerRowReader(grid);

            for (var i = parent + 1; i < rows.Count; i++)
            {
                if (rows[i] is GroupRow group)
                {
                    if (group.Level <= parentLevel)
                    {
                        return -1;
                    }

                    continue;
                }

                var row = reader.GetDataRow(i);

                if (row != null && row.Table.Columns.Contains("Schema_Browser") && string.Equals(Convert.ToString(row["Schema_Browser"]), text, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
