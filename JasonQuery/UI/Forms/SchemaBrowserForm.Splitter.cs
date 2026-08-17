using JasonQuery.Core.Config;
using JasonQuery.Database.Internal.Repositories;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class SchemaBrowserForm
    {
        private void splitContainer1_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            _shouldSaveSplitter = true;
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (splitContainer1.Panel1.Width > 700)
            {
                splitContainer1.SplitterDistance = 700;
            }

            txtSchemaFilter.Size = new Size(c1GridSchemaBrowser.Width - lblFilter.Left - lblFilter.Width - 1, 21);

            AutoResizeGridColumnWidth();

            if (MyGlobal.GlobalTemp == "NoSplit") //避免觸發 splitContainer1_SplitterMoved()
            {
                MyGlobal.GlobalTemp = string.Empty;
                return;
            }

            if (!_shouldSaveSplitter)
            {
                return;
            }

            SaveSplitterData("L/R", splitContainer1.SplitterDistance);
            editorSqlPane.Focus();
            _shouldSaveSplitter = false;
        }

        private void splitContainer3_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            _shouldSaveSplitter = true;
        }

        private void splitContainer3_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (splitContainer1.Panel1.Width > 700)
            {
                splitContainer1.SplitterDistance = 700;
            }

            AutoResizeGridColumnWidthForColumns();

            if (MyGlobal.GlobalTemp == "NoSplit") //避免觸發 splitContainer3_SplitterMoved()
            {
                MyGlobal.GlobalTemp = string.Empty;
                return;
            }

            if (!_shouldSaveSplitter)
            {
                return;
            }

            SaveSplitterData("LL/RR", splitContainer3.SplitterDistance);
            c1GridData.Focus();
            _shouldSaveSplitter = false;
        }

        private void LoadSplitterData(string splitterKey)
        {
            try
            {
                var sbSql = new StringBuilder();

                sbSql.AppendLine("SELECT AttributeValue FROM SystemConfig");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'SplitterConfig'");
                sbSql.Append($"   AND AttributeName = 'SchemaBrowser_{splitterKey}'");

                var sql = sbSql.ToString();
                var dtSplitterData = JasonQueryRepository.ExecQuery(sql);

                if (dtSplitterData.Rows.Count <= 0)
                {
                    return;
                }

                int.TryParse(dtSplitterData.Rows[0]["AttributeValue"].ToString(), out var value);

                if (splitterKey == "LL/RR")
                {
                    splitContainer3.SplitterDistance = value;
                }
                else
                {
                    splitContainer1.SplitterDistance = value;
                }
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
            }
        }

        private void SaveSplitterData(string splitterKey, int splitterValue)
        {
            if (splitterKey != "L/R" && splitterKey != "LL/RR")
            {
                return;
            }

            var sbSql = new StringBuilder();

            sbSql.AppendLine("SELECT * FROM SystemConfig");
            sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
            sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
            sbSql.AppendLine("   AND AttributeKey = 'SplitterConfig'");
            sbSql.Append($"   AND AttributeName = 'SchemaBrowser_{splitterKey}'");

            var sql = sbSql.ToString();
            var dtData = JasonQueryRepository.ExecQuery(sql);

            if (dtData?.Rows.Count > 0)
            {
                sbSql.Clear();
                sbSql.AppendLine("UPDATE SystemConfig");
                sbSql.AppendLine($"   SET AttributeValue = '{splitterValue}'");
                sbSql.AppendLine($" WHERE DomainUser = '{MyGlobal.DomainUser}'");
                sbSql.AppendLine($"   AND MPID = {JasonQueryRepository.DbMotherPid}");
                sbSql.AppendLine("   AND AttributeKey = 'SplitterConfig'");
                sbSql.Append($"   AND AttributeName = 'SchemaBrowser_{splitterKey}'");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }
            else
            {
                sbSql.Clear();
                sbSql.AppendLine("INSERT INTO SystemConfig");
                sbSql.AppendLine("       (DomainUser, MPID, AttributeKey, AttributeName, AttributeValue)");
                sbSql.Append($"VALUES ('{MyGlobal.DomainUser}', {JasonQueryRepository.DbMotherPid}, 'SplitterConfig', 'SchemaBrowser_{splitterKey}', '{splitterValue}')");

                sql = sbSql.ToString();
                JasonQueryRepository.ExecNonQuery(sql);
            }
        }
    }
}