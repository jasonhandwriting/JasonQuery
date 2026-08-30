using System;
using System.Collections.Generic;

namespace JasonQuery.Core.SchemaExplorer.LazyLoading
{
    internal enum SchemaBrowserLazyTab
    {
        None,
        SqlPane,
        TableStructure,
        ViewData,
        TableData
    }

    internal sealed class SchemaBrowserLazyLoadState
    {
        private readonly HashSet<SchemaBrowserLazyTab> _loadedTabs = new HashSet<SchemaBrowserLazyTab>();
        private readonly HashSet<SchemaBrowserLazyTab> _loadingTabs = new HashSet<SchemaBrowserLazyTab>();

        public string SelectionKey { get; private set; } = string.Empty;

        public void Reset(string selectionKey)
        {
            SelectionKey = selectionKey ?? string.Empty;
            _loadedTabs.Clear();
            _loadingTabs.Clear();
        }

        public bool TryBegin(SchemaBrowserLazyTab tab)
        {
            if (tab == SchemaBrowserLazyTab.None || string.IsNullOrEmpty(SelectionKey) || _loadedTabs.Contains(tab) || _loadingTabs.Contains(tab))
            {
                return false;
            }

            _loadingTabs.Add(tab);
            return true;
        }

        public void Complete(SchemaBrowserLazyTab tab)
        {
            _loadingTabs.Remove(tab);
            _loadedTabs.Add(tab);
        }

        public void Fail(SchemaBrowserLazyTab tab)
        {
            _loadingTabs.Remove(tab);
        }

        public static SchemaBrowserLazyTab ResolveSupportedTab(SchemaBrowserLazyTab requestedTab, bool isTable, bool isView)
        {
            if (requestedTab == SchemaBrowserLazyTab.TableStructure || requestedTab == SchemaBrowserLazyTab.TableData)
            {
                return isTable ? requestedTab : SchemaBrowserLazyTab.SqlPane;
            }

            if (requestedTab == SchemaBrowserLazyTab.ViewData)
            {
                return isView ? requestedTab : SchemaBrowserLazyTab.SqlPane;
            }

            return SchemaBrowserLazyTab.SqlPane;
        }
    }
}
