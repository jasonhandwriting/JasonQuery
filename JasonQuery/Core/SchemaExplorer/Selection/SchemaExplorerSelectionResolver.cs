using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Metadata;
using System;
using System.Data;

namespace JasonQuery.Core.SchemaExplorer.Selection
{
    public static class SchemaExplorerSelectionResolver
    {
        public static SchemaExplorerSelectionInfo Resolve(SchemaExplorerSelectionResolveRequest request, ISchemaExplorerRowReader rowReader)
        {
            if (request == null)
            {
                return CreateInvalidSelection(DataSourceType.Oracle, -1);
            }

            if (request.IsExternalSelection)
            {
                return ResolveExternalSelection(request);
            }

            if (rowReader == null)
            {
                return CreateInvalidSelection(request.SourceType, request.DisplayRowIndex);
            }

            if (request.DisplayRowIndex < 0)
            {
                return CreateInvalidSelection(request.SourceType, request.DisplayRowIndex);
            }

            if (rowReader.IsGroupRow(request.DisplayRowIndex))
            {
                return ResolveGroupRow(request, rowReader);
            }

            return ResolveDataRow(request, rowReader);
        }

        private static SchemaExplorerSelectionInfo ResolveExternalSelection(SchemaExplorerSelectionResolveRequest request)
        {
            var selection = new SchemaExplorerSelectionInfo
            {
                Kind = SchemaExplorerSelectionKind.ExternalObject,
                SourceType = request.SourceType,
                DisplayRowIndex = request.DisplayRowIndex,
                IsGroupRow = false,
                GroupLevel = -1,
                SchemaNode = request.CurrentSchemaNode ?? string.Empty,
                SchemaType = request.CurrentSchemaType ?? string.Empty,
                SchemaName = request.CurrentSchemaName ?? string.Empty,
                SchemaDbo = request.CurrentSchemaDbo ?? string.Empty,
                ObjectId = request.CurrentObjectId ?? string.Empty
            };

            NormalizeSelection(selection);

            if (string.IsNullOrWhiteSpace(selection.SchemaType) || string.IsNullOrWhiteSpace(selection.SchemaName))
            {
                selection.Kind = SchemaExplorerSelectionKind.Invalid;
            }

            return selection;
        }

        private static SchemaExplorerSelectionInfo ResolveGroupRow(SchemaExplorerSelectionResolveRequest request, ISchemaExplorerRowReader rowReader)
        {
            var groupLevel = rowReader.GetGroupLevel(request.DisplayRowIndex);

            var selection = new SchemaExplorerSelectionInfo
            {
                Kind = SchemaExplorerSelectionKind.GroupNode,
                SourceType = request.SourceType,
                DisplayRowIndex = request.DisplayRowIndex,
                IsGroupRow = true,
                GroupLevel = groupLevel
            };

            if (!SchemaExplorerGroupLevelPolicy.IsObjectGroupLevel(request.SourceType, groupLevel))
            {
                return selection;
            }

            var schemaName = rowReader.GetGroupedText(request.DisplayRowIndex);
            var schemaType = rowReader.GetSchemaTypeFromGroupStartIndex(request.DisplayRowIndex);
            var schemaTypeGroupLevel = SchemaExplorerGroupLevelPolicy.GetSchemaTypeGroupLevel(request.SourceType);
            var schemaType2 = rowReader.FindPreviousGroupText(request.DisplayRowIndex, schemaTypeGroupLevel);
            var schemaDataTable = rowReader.GetSchemaDataTable();

            schemaType = ResolveSchemaType(schemaDataTable, schemaType, schemaType2, schemaName);

            selection.Kind = SchemaExplorerSelectionKind.SchemaObject;
            selection.SchemaType = schemaType;
            selection.SchemaName = schemaName;

            FillDatabaseSpecificInfoForGroupRow(selection, schemaDataTable);

            NormalizeSelection(selection);

            if (string.IsNullOrWhiteSpace(selection.SchemaName))
            {
                selection.Kind = SchemaExplorerSelectionKind.Invalid;
                return selection;
            }

            return selection;
        }

        private static SchemaExplorerSelectionInfo ResolveDataRow(SchemaExplorerSelectionResolveRequest request, ISchemaExplorerRowReader rowReader)
        {
            var row = rowReader.GetDataRow(request.DisplayRowIndex);

            var selection = new SchemaExplorerSelectionInfo
            {
                Kind = SchemaExplorerSelectionKind.SchemaObject,
                SourceType = request.SourceType,
                DisplayRowIndex = request.DisplayRowIndex,
                IsGroupRow = false,
                GroupLevel = -1
            };

            if (row == null)
            {
                selection.Kind = SchemaExplorerSelectionKind.Invalid;
                return selection;
            }

            FillSelectionFromDataRow(selection, row);
            NormalizeSelection(selection);

            if (string.IsNullOrWhiteSpace(selection.SchemaName))
            {
                selection.Kind = SchemaExplorerSelectionKind.Invalid;
                return selection;
            }

            if (request.IsShowColumnInfo)
            {
                selection.Kind = SchemaExplorerSelectionKind.ColumnInfo;
                selection.IsColumnInfoRow = true;
                return selection;
            }

            return selection;
        }

        private static string ResolveSchemaType(DataTable schemaDataTable, string schemaType, string schemaType2, string schemaName)
        {
            schemaType = schemaType ?? string.Empty;
            schemaType2 = schemaType2 ?? string.Empty;
            schemaName = schemaName ?? string.Empty;

            if (string.Equals(schemaType, schemaType2, StringComparison.Ordinal) || string.IsNullOrEmpty(schemaType2))
            {
                return schemaType;
            }

            if (schemaDataTable == null || schemaDataTable.Rows.Count == 0)
            {
                return schemaType;
            }

            var tempSchemaObject1 = DataTableSearchHelper.FindValueFromDataTable
            (
                schemaDataTable,
                "SchemaObject",
                ("SchemaType", schemaType),
                ("SchemaName", schemaName)
            );

            var tempSchemaObject2 = DataTableSearchHelper.FindValueFromDataTable
            (
                schemaDataTable,
                "SchemaObject",
                ("SchemaType", schemaType2),
                ("SchemaName", schemaName)
            );

            if (string.IsNullOrEmpty(tempSchemaObject1) && !string.IsNullOrEmpty(tempSchemaObject2))
            {
                return schemaType2;
            }

            return schemaType;
        }

        private static void FillDatabaseSpecificInfoForGroupRow(SchemaExplorerSelectionInfo selection, DataTable schemaDataTable)
        {
            if (schemaDataTable == null || schemaDataTable.Rows.Count == 0)
            {
                return;
            }

            switch (selection.SourceType)
            {
                case DataSourceType.PostgreSql:
                case DataSourceType.MySql:
                    {
                        selection.SchemaNode = DataTableSearchHelper.FindValueFromDataTable
                        (
                            schemaDataTable,
                            "SchemaNode",
                            ("SchemaType", selection.SchemaType),
                            ("SchemaName", selection.SchemaName)
                        );

                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        var row = DataTableSearchHelper.FindDataTableFirstRow
                        (
                            schemaDataTable,
                            ("SchemaType", selection.SchemaType),
                            ("SchemaName", selection.SchemaName)
                        );

                        if (row == null)
                        {
                            return;
                        }

                        selection.SchemaNode = GetSafeString(row, "SchemaNode");
                        selection.SchemaDbo = GetSafeString(row, "SchemaDbo");
                        selection.ObjectId = GetSafeString(row, "ObjectID");
                        selection.CreateDate = GetSafeString(row, "CreateDate");
                        selection.ModifyDate = GetSafeString(row, "ModifyDate");

                        break;
                    }
            }
        }

        private static void FillSelectionFromDataRow(SchemaExplorerSelectionInfo selection, DataRow row)
        {
            switch (selection.SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        selection.SchemaType = GetSafeString(row, "SchemaType");
                        selection.SchemaName = GetSafeString(row, "Schema_Browser");
                        break;
                    }
                case DataSourceType.PostgreSql:
                case DataSourceType.MySql:
                    {
                        selection.SchemaNode = GetSafeString(row, "SchemaNode");
                        selection.SchemaType = GetSafeString(row, "SchemaType");
                        selection.SchemaName = GetSafeString(row, "Schema_Browser");
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        selection.SchemaNode = GetSafeString(row, "SchemaNode");
                        selection.SchemaType = GetSafeString(row, "SchemaType");
                        selection.SchemaName = GetSafeString(row, "Schema_Browser");
                        selection.SchemaDbo = GetSafeString(row, "SchemaDbo");
                        selection.ObjectId = GetSafeString(row, "ObjectID");
                        selection.CreateDate = GetSafeString(row, "CreateDate");
                        selection.ModifyDate = GetSafeString(row, "ModifyDate");
                        break;
                    }
            }
        }

        private static void NormalizeSelection(SchemaExplorerSelectionInfo selection)
        {
            if (selection == null)
            {
                return;
            }

            selection.SchemaType = NormalizeBeforeSeparator(selection.SchemaType);

            var schemaName = selection.SchemaName ?? string.Empty;
            var separatorIndex = schemaName.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);

            if (separatorIndex >= 0)
            {
                if (selection.SourceType == DataSourceType.Oracle && SchemaObjectTypeHelper.Is(selection.SchemaType, SchemaObjectNames.Packages))
                {
                    selection.PackageSpecBody = schemaName.EndsWith("(Spec)", StringComparison.Ordinal) ? "Spec" : "Body";
                }

                schemaName = schemaName.Substring(0, separatorIndex);
            }

            selection.SchemaName = schemaName;
        }

        private static string NormalizeBeforeSeparator(string value)
        {
            value = value ?? string.Empty;

            var separatorIndex = value.IndexOf(MyGlobal.Separator, StringComparison.Ordinal);

            return separatorIndex >= 0 ? value.Substring(0, separatorIndex) : value;
        }

        private static string GetSafeString(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || string.IsNullOrWhiteSpace(columnName))
            {
                return string.Empty;
            }

            if (!row.Table.Columns.Contains(columnName))
            {
                return string.Empty;
            }

            return row[columnName] == DBNull.Value ? string.Empty : Convert.ToString(row[columnName]);
        }

        private static SchemaExplorerSelectionInfo CreateInvalidSelection(DataSourceType sourceType, int displayRowIndex)
        {
            return new SchemaExplorerSelectionInfo
            {
                Kind = SchemaExplorerSelectionKind.Invalid,
                SourceType = sourceType,
                DisplayRowIndex = displayRowIndex
            };
        }
    }
}
