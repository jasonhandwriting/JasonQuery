using System;
using System.Reflection;

namespace JasonLibrary.Tests.Infrastructure
{
    internal static class RuntimeContractValueReader
    {
        private const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        internal static object GetRawConstant(Type declaringType, string fieldName)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException(nameof(declaringType));
            }

            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException("A field name is required.", nameof(fieldName));
            }

            var field = declaringType.GetField(fieldName, FieldFlags);

            if (field == null)
            {
                throw new InvalidOperationException($"Static field '{declaringType.FullName}.{fieldName}' was not found.");
            }

            if (!field.IsLiteral)
            {
                throw new InvalidOperationException($"Static field '{declaringType.FullName}.{fieldName}' is not a compile-time constant.");
            }

            return field.GetRawConstantValue();
        }
    }
}
