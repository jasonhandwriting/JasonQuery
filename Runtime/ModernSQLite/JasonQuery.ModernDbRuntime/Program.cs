using JasonQuery.ModernDbMigration;
using System;
using System.IO;

namespace JasonQuery.ModernDbRuntime
{
    internal static class Program
    {
        private static int Main()
        {
            try
            {
                ModernSqlCipherRuntime.InitializeAndValidate();

                using (var session = new ModernDbRuntimeSession())
                {
                    var input = Console.OpenStandardInput();
                    var output = Console.OpenStandardOutput();
                    var dispatcher = new ModernDbRuntimeRequestDispatcher(session);

                    while (dispatcher.TryDispatchNext(input, output))
                    {
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                WriteDiagnostic(ex);
                return 4;
            }
        }

        private static void WriteDiagnostic(Exception ex)
        {
            try
            {
                var typeName = ex == null ? "UnknownError" : ex.GetType().FullName;
                var message = ex == null ? "The isolated Modern DB runtime failed." : ex.Message;

                Console.Error.WriteLine(typeName + ": " + Sanitize(message));
            }
            catch
            {
            }
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace('\r', ' ').Replace('\n', ' ');
        }
    }
}
