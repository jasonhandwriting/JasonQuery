using System;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.DdlPreview
{
    //MySQL 8.0.3 introduced definition-free RENAME COLUMN.
    //Unknown, malformed, or MariaDB versions are never authorized by this policy.
    internal enum MySqlColumnRenameSyntax
    {
        Unknown,
        ChangeRequiresVerifiedDefinition,
        RenameColumn
    }

    internal static class MySqlColumnDdlVersionPolicy
    {
        //Only known MySQL VERSION() suffixes are accepted. Do not infer capabilities from a MariaDB compatibility prefix such as 5.5.5-10.11.2-MariaDB.
        private static readonly Regex VersionPattern = new Regex
        (
            @"^(?<major>[0-9]+)\.(?<minor>[0-9]+)\.(?<patch>[0-9]+)(?:-(?:log|commercial|[0-9]+))?$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
        );

        internal static MySqlColumnRenameSyntax SelectRenameSyntax(string serverVersion)
        {
            if (string.IsNullOrWhiteSpace(serverVersion) || serverVersion.IndexOf("MariaDB", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MySqlColumnRenameSyntax.Unknown;
            }

            var match = VersionPattern.Match(serverVersion.Trim());

            if (!match.Success
                || !int.TryParse(match.Groups["major"].Value, out var major)
                || !int.TryParse(match.Groups["minor"].Value, out var minor)
                || !int.TryParse(match.Groups["patch"].Value, out var patch)
                || major < 5)
            {
                return MySqlColumnRenameSyntax.Unknown;
            }

            if (major > 8 || (major == 8 && (minor > 0 || patch >= 3)))
            {
                return MySqlColumnRenameSyntax.RenameColumn;
            }

            return MySqlColumnRenameSyntax.ChangeRequiresVerifiedDefinition;
        }
    }
}
