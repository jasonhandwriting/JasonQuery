using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Logging;
using System;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private bool IsSqlExecuteErrorPositionMessage()
        {
            return MyGlobal.GlobalTemp.StartsWith($"SQLExecuteErrorPos{MyGlobal.Separator}", StringComparison.Ordinal);
        }

        private void DispatchSqlExecuteErrorPositionMessage()
        {
            var message = MyGlobal.GlobalTemp.Replace($"SQLExecuteErrorPos{MyGlobal.Separator}", string.Empty);
            var targetFormId = message.Split(';')[0]; //AccessibleDescription
            var messageInfo = message.Substring(targetFormId.Length + 1);

            if (targetFormId != AccessibleDescription)
            {
                return;
            }

            //把變數清空，以免重複觸發！
            MyGlobal.GlobalTemp = string.Empty;

            messageInfo = messageInfo.Replace(MyGlobal.Separator5, MyGlobal.Separator);

            DispatchSqlExecuteErrorPositionByDataSource(messageInfo);
        }

        private void DispatchSqlExecuteErrorPositionByDataSource(string messageInfo)
        {
            switch (_currentSourceType)
            {
                case DataSourceType.Oracle:
                    {
                        using (TraceLogger.Time("HandlenSql Execute Error Position for Oracle"))
                        {
                            //定位：從 1 開始，positionOffset = 0
                            HandleSqlExecuteErrorPosition_Oracle(messageInfo, 0);
                        }

                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        using (TraceLogger.Time("HandlenSql Execute Error Position for PostgreSQL"))
                        {
                            //定位：從 0 開始，positionOffset = -1
                            HandleSqlExecuteErrorPosition_PostgreSql(messageInfo, -1);
                        }

                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        using (TraceLogger.Time("HandlenSql Execute Error Position for SQL Server"))
                        {
                            //定位：從 0 開始，positionOffset = -1
                            HandleSqlExecuteErrorPosition_SqlServer(messageInfo, -1);
                        }

                        break;
                    }
                case DataSourceType.MySql:
                    {
                        using (TraceLogger.Time("HandlenSql Execute Error Position for MySQL"))
                        {
                            //定位：從 1 開始，positionOffset = 0
                            HandleSqlExecuteErrorPosition_MySql(messageInfo, 0);
                        }

                        break;
                    }
            }
        }
    }
}
