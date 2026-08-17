using System;
using System.Data;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using ScintillaNET;

namespace JasonQuery.UI.QueryEditor.AutoComplete.Contexts
{
    internal interface IQueryEditorAutoCompleteResolveContextFactoryHost
    {
        Scintilla Editor { get; }

        DataSourceType CurrentSourceType { get; }

        bool IsDataSourceSqlServer { get; }

        bool IsDataSourceMySql { get; }

        Func<bool, string> SelectCurrentBlock { get; }

        Func<string, bool, bool, string> FormatSql { get; }

        string ConnectionDatabase { get; }

        DataTable TableAndViews { get; }
    }

    internal sealed class QueryEditorAutoCompleteResolveContextFactory
    {
        private readonly IQueryEditorAutoCompleteResolveContextFactoryHost _host;

        public QueryEditorAutoCompleteResolveContextFactory(IQueryEditorAutoCompleteResolveContextFactoryHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public QueryEditorAutoCompletePeriodResolveContext CreatePeriodResolveContext()
        {
            return PopulateCore(new QueryEditorAutoCompletePeriodResolveContext());
        }

        public QueryEditorAutoCompleteSpaceResolveContext CreateSpaceResolveContext()
        {
            var context = PopulateCore(new QueryEditorAutoCompleteSpaceResolveContext());

            context.ConnectionDatabase = _host.ConnectionDatabase;
            context.TableAndViews = _host.TableAndViews;
            return context;
        }

        private T PopulateCore<T>(T context) where T : QueryEditorAutoCompleteResolveContextBase
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Editor = _host.Editor;
            context.CurrentSourceType = _host.CurrentSourceType;
            context.IsDataSourceSqlServer = _host.IsDataSourceSqlServer;
            context.IsDataSourceMySql = _host.IsDataSourceMySql;
            context.SelectCurrentBlock = _host.SelectCurrentBlock;
            context.FormatSql = _host.FormatSql;

            return context;
        }
    }
}