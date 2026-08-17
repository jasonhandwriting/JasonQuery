using System;

namespace JasonQuery.Core.Database.Metadata
{
    internal static class SchemaObjectTypeHelper
    {
        public static bool Is(string schemaType, string expectedType)
        {
            if (string.IsNullOrWhiteSpace(expectedType))
            {
                return false;
            }

            return string.Equals
            (
                (schemaType ?? string.Empty).Trim(),
                expectedType,
                StringComparison.OrdinalIgnoreCase
            );
        }

        public static bool IsAny(string schemaType, params string[] expectedTypes)
        {
            if (expectedTypes == null || expectedTypes.Length == 0)
            {
                return false;
            }

            var normalizedSchemaType = (schemaType ?? string.Empty).Trim();

            foreach (var expectedType in expectedTypes)
            {
                if (string.IsNullOrWhiteSpace(expectedType))
                {
                    continue;
                }

                if (string.Equals(normalizedSchemaType, expectedType, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool StartsWith(string schemaType, string expectedPrefix)
        {
            if (string.IsNullOrWhiteSpace(expectedPrefix))
            {
                return false;
            }

            return (schemaType ?? string.Empty).Trim().StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public static bool StartsWithAny(string schemaType, params string[] expectedPrefixes)
        {
            if (expectedPrefixes == null || expectedPrefixes.Length == 0)
            {
                return false;
            }

            var normalizedSchemaType = (schemaType ?? string.Empty).Trim();

            foreach (var expectedPrefix in expectedPrefixes)
            {
                if (string.IsNullOrWhiteSpace(expectedPrefix))
                {
                    continue;
                }

                if (normalizedSchemaType.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}