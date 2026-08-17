using System;

namespace JasonLibrary.Infrastructure.Enums
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class EnumAliasAttribute : Attribute
    {
        public string Alias { get; }

        public EnumAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }
}