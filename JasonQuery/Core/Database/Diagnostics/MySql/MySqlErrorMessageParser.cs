using System;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.Diagnostics.MySql
{
    internal static class MySqlErrorMessageParser
    {
        public static MySqlErrorMessageInfo Parse(string errorMessage)
        {
            var result = new MySqlErrorMessageInfo();
            var text = (errorMessage ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(text))
            {
                return result;
            }

            if (string.Equals(text, "No database selected", StringComparison.OrdinalIgnoreCase))
            {
                result.Kind = MySqlErrorMessageKind.NoDatabaseSelected;
                return result;
            }

            var match = Regex.Match(text, @"^Unknown database '([^']+)'", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                result.Kind = MySqlErrorMessageKind.UnknownDatabase;
                result.TargetText = match.Groups[1].Value;
                return result;
            }

            match = Regex.Match(text, @"^Unknown column '([^']+)'(?: in '([^']+)')?", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                result.Kind = MySqlErrorMessageKind.UnknownColumn;
                result.TargetText = match.Groups[1].Value;
                result.ClauseName = match.Groups[2].Value;
                return result;
            }

            match = Regex.Match(text, @"^Column '([^']+)' in ([^']+?) is ambiguous", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                result.Kind = MySqlErrorMessageKind.AmbiguousColumn;
                result.TargetText = match.Groups[1].Value;
                result.ClauseName = match.Groups[2].Value;
                return result;
            }

            match = Regex.Match(text, @"^Duplicate column name '([^']+)'", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                result.Kind = MySqlErrorMessageKind.DuplicateColumn;
                result.TargetText = match.Groups[1].Value;
                return result;
            }

            match = Regex.Match(text, @"^Table '([^']+)' doesn't exist", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                result.Kind = MySqlErrorMessageKind.TableNotFound;
                result.TargetText = match.Groups[1].Value;
                return result;
            }

            if (text.IndexOf("You have an error in your SQL syntax", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result.Kind = MySqlErrorMessageKind.SyntaxError;

                match = Regex.Match(text, @"\bat line\s+(\d+)", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    int.TryParse(match.Groups[1].Value, out var lineNumber);
                    result.LineNumber = lineNumber;
                }

                match = Regex.Match(text, @"\bnear\s+'(.*?)'\s+at line\s+\d+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (match.Success)
                {
                    result.NearText = match.Groups[1].Value;
                }

                return result;
            }

            return result;
        }
    }
}
