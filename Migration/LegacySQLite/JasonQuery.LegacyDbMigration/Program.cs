using JasonQuery.Core.Security.Database;
using System;
using System.Data.SQLite;
using System.Reflection;

namespace JasonQuery.LegacyDbMigration
{
    internal static class Program
    {
        private const string RuntimeInfoArgument = "--runtime-info";

        private const string StreamStorageV1Argument = "--stream-storage-v1";

        private static int Main(string[] args)
        {
            if (args == null ||
                args.Length != 1 ||
                (!string.Equals(args[0], RuntimeInfoArgument, StringComparison.Ordinal) &&
                 !string.Equals(args[0], StreamStorageV1Argument, StringComparison.Ordinal)))
            {
                Console.Error.WriteLine
                (
                    "Usage: JasonQuery.LegacyDbMigration.exe " +
                    "(--runtime-info | --stream-storage-v1)"
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

            if (string.Equals(args[0], RuntimeInfoArgument, StringComparison.Ordinal))
            {
                return RunRuntimeInfo();
            }

            return RunStorageV1ControlChannel();
        }

        private static int RunRuntimeInfo()
        {
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

        private static int RunStorageV1ControlChannel()
        {
            try
            {
                using (var input = Console.OpenStandardInput())
                using (var output = Console.OpenStandardOutput())
                using (var request = DatabaseStorageMigrationWireProtocol.ReadRequest(input))
                {
                    var validator = new LegacyStorageV1ReadOnlyValidator();

                    validator.Validate
                    (
                        request.DatabaseFilePath,
                        request.DatabasePasswordUtf8
                    );

                    DatabaseStorageMigrationWireProtocol.WriteStorageV1ValidatedResponse
                    (
                        output
                    );

                    output.Flush();
                }

                return 0;
            }
            catch (Exception ex)
            {
                //STDERR is diagnostics-only. Never print request path, password/key material, row payloads, or SQL/data dumps here.
                Console.Error.WriteLine
                (
                    "Legacy Storage V1 read-only validation failed."
                );

                Console.Error.WriteLine
                (
                    ex.GetType().FullName
                );

                return 4;
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
