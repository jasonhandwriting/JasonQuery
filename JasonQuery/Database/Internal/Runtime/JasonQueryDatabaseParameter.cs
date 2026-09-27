using System;

namespace JasonQuery.Database.Internal.Runtime
{
    public sealed class JasonQueryDatabaseParameter
    {
        public JasonQueryDatabaseParameter(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("A database parameter name is required.", nameof(name));
            }

            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object Value { get; }
    }
}
