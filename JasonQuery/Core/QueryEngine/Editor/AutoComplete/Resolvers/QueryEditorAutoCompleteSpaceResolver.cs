using JasonQuery.Core.Data;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Builders;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using System;
using System.Data;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers
{
    internal sealed class QueryEditorAutoCompleteSpaceResolver
    {
        private readonly QueryEditorAutoCompleteSchemaQueryExecutor _schemaQueryExecutor;

        public QueryEditorAutoCompleteSpaceResolver(QueryEditorAutoCompleteSchemaQueryExecutor schemaQueryExecutor)
        {
            _schemaQueryExecutor = schemaQueryExecutor ?? throw new ArgumentNullException(nameof(schemaQueryExecutor));
        }

        public bool TryResolve(QueryEditorAutoCompleteSpaceResolveContext context, out QueryEditorAutoCompleteRequest request)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Validate();
            request = null;

            var currentBlock = context.SelectCurrentBlock(false);

            if (string.IsNullOrWhiteSpace(currentBlock))
            {
                return false;
            }

            var analysisResult = QueryEditorAutoCompleteSpaceAnalyzer.Analyze
            (
                new QueryEditorAutoCompleteSpaceAnalysisRequest
                {
                    Sql = currentBlock,
                    CaretPosition = ResolveCaretPositionInBlock(context, currentBlock),
                    DataSourceType = context.CurrentSourceType
                }
            );

            if (!analysisResult.CanResolve || !TryBuildAutoCompleteTable(context, analysisResult, out var dtSpace))
            {
                return false;
            }

            request = new QueryEditorAutoCompleteRequest
            {
                TriggerPosition = context.Editor.CurrentPosition,
                Data = dtSpace
            };

            return true;
        }

        private bool TryBuildAutoCompleteTable(QueryEditorAutoCompleteSpaceResolveContext context,
                                               QueryEditorAutoCompleteSpaceAnalysisResult analysisResult, out DataTable dtSpace)
        {
            dtSpace = null;

            switch (analysisResult.Intent)
            {
                case QueryEditorAutoCompleteSpaceIntent.ListColumns:
                    {
                        switch (analysisResult.SourceKind)
                        {
                            case QueryEditorAutoCompleteSpaceSourceKind.ObjectName:
                                {
                                    return TryLoadColumnsFromSchema(context, analysisResult.ObjectName, out dtSpace);
                                }
                            case QueryEditorAutoCompleteSpaceSourceKind.SubquerySql:
                            case QueryEditorAutoCompleteSpaceSourceKind.CteSql:
                                {
                                    return TryLoadColumnsFromSql(context, analysisResult.SourceSql, out dtSpace);
                                }
                            case QueryEditorAutoCompleteSpaceSourceKind.None:
                            default:
                                {
                                    return false;
                                }
                        }
                    }
                case QueryEditorAutoCompleteSpaceIntent.ListObjects:
                case QueryEditorAutoCompleteSpaceIntent.ListDatabases:
                    {
                        return TryLoadObjectsFromMetadata(context, analysisResult.LookupMode, out dtSpace);
                    }
                case QueryEditorAutoCompleteSpaceIntent.None:
                default:
                    {
                        return false;
                    }
            }
        }

        private bool TryLoadColumnsFromSql(QueryEditorAutoCompleteSpaceResolveContext context, string sourceSql, out DataTable dtSpace)
        {
            dtSpace = null;

            var normalizedSql = (sourceSql ?? string.Empty).Trim().TrimEnd(';');

            if (string.IsNullOrWhiteSpace(normalizedSql))
            {
                return false;
            }

            var sqlPrompt = SqlTraceHelper.BuildHeaderNewLine("---Get the AutoComplete List (When the SPACE key is pressed)");
            var sql = string.Format("{0}{1}", sqlPrompt, normalizedSql);

            if (!_schemaQueryExecutor.TryExecute(sql, "When the SPACE key is pressed", out var dtSchemaTable))
            {
                return false;
            }

            return TryBuildColumnAutoCompleteTable(context, ref dtSchemaTable, out dtSpace);
        }

        private bool TryLoadColumnsFromSchema(QueryEditorAutoCompleteSpaceResolveContext context, string tableViewName, out DataTable dtSpace)
        {
            dtSpace = null;

            if (string.IsNullOrWhiteSpace(tableViewName))
            {
                return false;
            }

            var sqlPrompt = SqlTraceHelper.BuildHeaderNewLine("---Get the AutoComplete List (When the SPACE key is pressed)");
            var sql = string.Format("{0}SELECT * FROM {1} WHERE 1 = 2", sqlPrompt, tableViewName);

            if (context.IsDataSourceMySql && !string.IsNullOrEmpty(context.ConnectionDatabase) && !IsQualifiedObjectName(tableViewName))
            {
                sql = string.Format("{0}SELECT * FROM {1}.{2} WHERE 1 = 2", sqlPrompt, context.ConnectionDatabase, tableViewName);
            }

            if (!_schemaQueryExecutor.TryExecute(sql, "When the SPACE key is pressed", out var dtSchemaTable))
            {
                return false;
            }

            return TryBuildColumnAutoCompleteTable(context, ref dtSchemaTable, out dtSpace);
        }

        private static bool TryBuildColumnAutoCompleteTable(QueryEditorAutoCompleteSpaceResolveContext context,
                                                            ref DataTable dtSchemaTable, out DataTable dtSpace)
        {
            dtSpace = null;

            if (dtSchemaTable == null || dtSchemaTable.Rows.Count == 0)
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtSchemaTable);
                return false;
            }

            DataTable dtSourceRows = null;
            DataTable dtResult = null;

            try
            {
                dtSourceRows = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForSpace
                (
                    dtSchemaTable,
                    context.CurrentSourceType
                );

                dtResult = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSpaceTable(dtSourceRows);
                dtSpace = dtResult;
                dtResult = null;

                return dtSpace != null && dtSpace.Rows.Count > 0;
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtSourceRows);
                DataTableLifecycleHelper.DisposeDataTable(ref dtSchemaTable);
                DataTableLifecycleHelper.DisposeDataTable(ref dtResult);
            }
        }

        private bool TryLoadObjectsFromMetadata(QueryEditorAutoCompleteSpaceResolveContext context,
                                                AutoCompleteObjectLookupMode lookupMode, out DataTable dtSpace)
        {
            dtSpace = null;

            if (context.TableAndViews == null)
            {
                return false;
            }

            DataTable dtSourceRows = null;
            DataTable dtResult = null;

            try
            {
                dtSourceRows = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForSpace
                (
                    context.TableAndViews,
                    context.CurrentSourceType,
                    lookupMode,
                    context.ConnectionDatabase
                );

                dtResult = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSpaceTable(dtSourceRows);
                dtSpace = dtResult;
                dtResult = null;

                return dtSpace != null && dtSpace.Rows.Count > 0;
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtSourceRows);
                DataTableLifecycleHelper.DisposeDataTable(ref dtResult);
            }
        }

        private static int ResolveCaretPositionInBlock(QueryEditorAutoCompleteSpaceResolveContext context, string currentBlock)
        {
            if (context?.Editor == null || string.IsNullOrEmpty(currentBlock))
            {
                return 0;
            }

            var editorText = context.Editor.Text ?? string.Empty;
            var currentPosition = Math.Max(0, Math.Min(context.Editor.CurrentPosition, editorText.Length));
            var blockStart = LastIndexOfBlockContainingCaret(editorText, currentBlock, currentPosition);

            if (blockStart < 0)
            {
                return Math.Min(currentPosition, currentBlock.Length);
            }

            return Math.Max(0, Math.Min(currentPosition - blockStart, currentBlock.Length));
        }

        private static int LastIndexOfBlockContainingCaret(string editorText, string currentBlock, int caretPosition)
        {
            if (string.IsNullOrEmpty(editorText) || string.IsNullOrEmpty(currentBlock))
            {
                return -1;
            }

            var maxStart = Math.Min(caretPosition, editorText.Length - currentBlock.Length);

            for (var index = maxStart; index >= 0; index--)
            {
                if (string.Compare(editorText, index, currentBlock, 0, currentBlock.Length, StringComparison.Ordinal) != 0)
                {
                    continue;
                }

                if (caretPosition >= index && caretPosition <= index + currentBlock.Length)
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool IsQualifiedObjectName(string objectName)
        {
            return !string.IsNullOrWhiteSpace(objectName) && objectName.IndexOf('.') >= 0;
        }
    }
}