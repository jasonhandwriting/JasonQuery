using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Sql.Lexing;
using System;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        //SQL text is prepared before dispatch, but is NOT treated as a successful DDL receipt.
        private bool _f7PendingEditorStructuralDdl;
        private string _f7EditorTransactionCloseAction = string.Empty;

        private void CaptureF7EditorStructuralDdlIntent(string sqlText, bool isQuery)
        {
            _f7PendingEditorStructuralDdl = false;
            _f7EditorTransactionCloseAction = GetF7TrailingTransactionAction(sqlText);

            if (isQuery || string.IsNullOrWhiteSpace(sqlText))
            {
                return;
            }

            //Use the same provider script splitters as the existing QueryForm execution path.
            //If parsing is uncertain, do not invent a Schema Dirty notification.
            try
            {
                if (IsSqlServer)
                {
                    using (var script = new Devart.Data.SqlServer.SqlScript(sqlText))
                    {
                        for (var i = 0; i < script.Statements.Count; i++)
                        {
                            if (SchemaChangeDdlClassifier.IsStructuralDdl(script.Statements[i].Text))
                            {
                                _f7PendingEditorStructuralDdl = true;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    using (var script = new Devart.Data.Oracle.OracleScript(sqlText))
                    {
                        for (var i = 0; i < script.Statements.Count; i++)
                        {
                            if (SchemaChangeDdlClassifier.IsStructuralDdl(script.Statements[i].Text))
                            {
                                _f7PendingEditorStructuralDdl = true;
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //This is only an optional refresh hint; never block execution or claim success.
                _f7PendingEditorStructuralDdl = false;
            }
        }

        private void NotifyF7CompletedEditorStructuralDdl()
        {
            if (!_f7PendingEditorStructuralDdl)
            {
                return;
            }

            //Called ONLY after F2-C1 acknowledges the entire nonquery script as Complete.
            TransferValueToMainForm("F7EditorStructuralDdl`");
        }

        //An intent hint alone is NOT a receipt. The QueryForm caller uses this only after F2-C1 verifies TX_CLOSED from a successful Reader result.
        private string GetF7ConfirmedEditorTransactionAction()
        {
            return _f7EditorTransactionCloseAction;
        }

        private string GetF7TrailingTransactionAction(string sqlText)
        {
            if ((_currentSourceType != DataSourceType.PostgreSql && _currentSourceType != DataSourceType.SqlServer) || string.IsNullOrWhiteSpace(sqlText))
            {
                return string.Empty;
            }

            try
            {
                var tokens = SqlTokenizer.Tokenize(sqlText, _currentSourceType).Tokens;
                var last = tokens.Count - 1;

                while (last >= 0 && (tokens[last].IsComment || tokens[last].IsSymbol(";")))
                {
                    last--;
                }

                if (last < 0 || tokens[last].Depth != 0)
                {
                    return string.Empty;
                }

                var action = tokens[last].IsWord("ROLLBACK") ? "ROLLBACK" : tokens[last].IsWord("COMMIT") ? "COMMIT" : string.Empty;

                if (string.IsNullOrEmpty(action))
                {
                    return string.Empty;
                }

                var previous = last - 1;

                while (previous >= 0 && tokens[previous].IsComment)
                {
                    previous--;
                }

                //Only a standalone last statement; never a keyword inside SQL text.
                if (previous >= 0 && (!tokens[previous].IsSymbol(";") || tokens[previous].Depth != 0))
                {
                    return string.Empty;
                }

                return action;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
