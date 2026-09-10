using System;
using System.Data.SQLite;
using System.Reflection;

namespace JasonQuery.LegacyDbMigration
{
    internal static class Program
    {
        private const string RuntimeInfoArgument = "--runtime-info";

        private static int Main(string[] args)
        {
            if (args == null || args.Length != 1 || !string.Equals(args[0], RuntimeInfoArgument, StringComparison.Ordinal))
            {
                Console.Error.WriteLine
                (
                    "Usage: JasonQuery.LegacyDbMigration.exe --runtime-info"
                );

                return 2;
            }

            if (!Environment.Is64BitProcess)
            {
                Console.Error.WriteLine
                (
                    "The legacy JasonQuery.db migration runtime requires an x64 process."
                );

                return 3;
            }

            try
            {
                var assemblyVersion = typeof(SQLiteConnection).Assembly.GetName().Version.ToString();
                var sqliteVersion = GetRequiredStaticStringProperty("SQLiteVersion");
                var interopVersion = GetRequiredStaticStringProperty("InteropVersion");

                Console.WriteLine("RuntimeRole=LegacyDatabaseMigration");
                Console.WriteLine("ProcessArchitecture=x64");
                Console.WriteLine("System.Data.SQLite.AssemblyVersion=" + assemblyVersion);
                Console.WriteLine("SQLite.Version=" + sqliteVersion);
                Console.WriteLine("SQLite.InteropVersion=" + interopVersion);

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine
                (
                    "Legacy SQLite runtime initialization failed."
                );

                Console.Error.WriteLine
                (
                    ex.GetType().FullName + ": " + ex.Message
                );

                return 1;
            }
        }

        private static string GetRequiredStaticStringProperty(string propertyName)
        {
            var property = typeof(SQLiteConnection).GetProperty
            (
                propertyName,
                BindingFlags.Public | BindingFlags.Static
            );

            if (property == null)
            {
                throw new MissingMemberException
                (
                    typeof(SQLiteConnection).FullName,
                    propertyName
                );
            }

            var value = property.GetValue(null, null) as string;

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException
                (
                    "SQLiteConnection." +
                    propertyName +
                    " returned no value."
                );
            }

            return value;
        }
    }
}
