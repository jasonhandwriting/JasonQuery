using System.Collections.Generic;

namespace JasonQuery.Core.Sql.Lexing
{
    internal sealed class SqlTokenizationResult
    {
        public SqlTokenizationResult(List<SqlToken> tokens)
        {
            Tokens = tokens ?? new List<SqlToken>();
        }

        public IReadOnlyList<SqlToken> Tokens { get; }

        public bool IsInsideProtectedText(int position)
        {
            var token = FindTokenContaining(position);

            return token != null && token.IsProtectedText;
        }

        public bool IsInsideKeywordSuppressedText(int position)
        {
            var token = FindTokenContaining(position);

            return token != null && token.SuppressesKeywordMatching;
        }

        public SqlToken FindTokenContaining(int position)
        {
            if (position < 0)
            {
                return null;
            }

            for (var index = 0; index < Tokens.Count; index++)
            {
                var token = Tokens[index];

                if (token.ContainsPosition(position))
                {
                    return token;
                }

                if (token.Start > position)
                {
                    break;
                }
            }

            return null;
        }
    }
}
