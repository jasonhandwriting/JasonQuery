using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.Execution
{
    //F2-C2-F1: only whole-transaction commands are routed through the managed transaction.
    //Never treat ROLLBACK TO SAVEPOINT as a full rollback.
    internal enum EditorTransactionAction
    {
        None = 0,
        Commit = 1,
        Rollback = 2
    }

    internal static class EditorTransactionCommandPolicy
    {
        internal static EditorTransactionAction Classify(string statementSql)
        {
            if (string.IsNullOrWhiteSpace(statementSql))
            {
                return EditorTransactionAction.None;
            }

            var value = statementSql.Trim().TrimEnd(';', ' ', '\t', '\r', '\n');

            value = Regex.Replace(value, @"\s+", " ").Trim().ToUpperInvariant();

            switch (value)
            {
                case "COMMIT":
                case "COMMIT WORK":
                case "COMMIT TRANSACTION":
                    {
                        return EditorTransactionAction.Commit;
                    }
                case "ROLLBACK":
                case "ROLLBACK WORK":
                case "ROLLBACK TRANSACTION":
                    {
                        return EditorTransactionAction.Rollback;
                    }
                default:
                    {
                        return EditorTransactionAction.None;
                    }
            }
        }
    }
}
