using JasonQuery.Core.Security.JasonQueryDb;
using System;
using System.IO;

namespace JasonQuery.ModernDbMigration
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 0)
            {
                Console.Error.WriteLine("Storage V2 migration helper does not accept command-line arguments.");
                return 10;
            }

            if (!Environment.Is64BitProcess)
            {
                Console.Error.WriteLine("Storage V2 migration helper requires x64.");
                return 11;
            }

            try
            {
                ModernSqlCipherRuntime.InitializeAndValidate();

                using (var input = Console.OpenStandardInput())
                using (var output = Console.OpenStandardOutput())
                using (var requestStream = ModernDbMigrationRequestDispatcher.CreateReplayStream(input, out var requestKind))
                {
                    switch (requestKind)
                    {
                        case ModernDbMigrationRequestKind.CandidateWriter:
                            {
                                RunCandidateWriter(requestStream, output);
                                break;
                            }
                        case ModernDbMigrationRequestKind.ReadOnlyValidator:
                            {
                                RunReadOnlyValidator(requestStream, output);
                                break;
                            }
                        default:
                            {
                                throw new NotSupportedException
                                (
                                    "The Modern migration helper request kind is not supported."
                                );
                            }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                //STDERR is diagnostics-only. Never print request path, password/key material, row payloads, or SQL/data dumps here.
                Console.Error.WriteLine
                (
                    "Storage V2 migration helper operation failed."
                );

                Console.Error.WriteLine
                (
                    "ExceptionType=" + ex.GetType().FullName
                );

                return 99;
            }
        }

        private static void RunCandidateWriter(Stream input, Stream output)
        {
            using (var request = JasonQueryDbStorageV2CandidateWriterProtocol.ReadRequest(input))
            {
                var writer = new ModernStorageV2CandidateWriter();

                writer.CreateAndValidate
                (
                    request.CandidateDatabasePath,
                    request.DatabasePasswordUtf8,
                    input
                );

                JasonQueryDbStorageV2CandidateWriterProtocol.WriteCandidateValidatedResponse
                (
                    output
                );

                output.Flush();
            }
        }

        private static void RunReadOnlyValidator(Stream input, Stream output)
        {
            using (var request = JasonQueryDbStorageV2ReadOnlyValidatorProtocol.ReadRequest(input))
            {
                var validator = new ModernStorageV2ReadOnlyValidator();

                validator.Validate
                (
                    request.DatabaseFilePath,
                    request.DatabasePasswordUtf8
                );

                JasonQueryDbStorageV2ReadOnlyValidatorProtocol.WriteStorageV2ValidatedResponse
                (
                    output
                );

                output.Flush();
            }
        }
    }
}
