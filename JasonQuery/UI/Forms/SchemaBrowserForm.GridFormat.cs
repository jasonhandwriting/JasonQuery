using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.Helpers;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        /// <summary>
        /// 依目前資料來源設定資料編輯 Grid 的欄位標題、Editor 與顯示樣式。
        /// </summary>
        private void SetGridFormat()
        {
            RefreshGridDataColumnCaptionsAndHeight();
            DisposeGridDataColumnEditors();

            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        ConfigureOracleGridDataEditors();
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        ConfigurePostgreSqlGridDataEditors();
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        ConfigureSqlServerGridDataEditors();
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        ConfigureMySqlGridDataEditors();
                        break;
                    }
            }

            ApplyGridDataColumnEditPolicy();

            GridHelper.SetGridHeaderLine(c1GridData);
            GridHelper.ResizeGridColumnWidth(c1GridData);
            GridHelper.ApplyGridHeadingStyle(_columnInfoCollector, c1GridData);
        }
    }
}
