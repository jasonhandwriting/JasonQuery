using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Invokes the isolated Modern SQLCipher helper to validate an existing Storage V2 candidate read-only.
    /// This type does not mutate migration journal state, replace the production database, or commit metadata.
    /// </summary>
    internal sealed class JasonQueryDbStorageV2ReadOnlyValidationProcessRunner
    {
        public JasonQueryDbStorageV2ReadOnlyValidationResult Validate(string modernHelperFilePath, string candidateDatabaseFilePath,
                                                                      byte[] databasePasswordUtf8)
        {
            var helperPath = NormalizeExistingFile
            (
                modernHelperFilePath,
                nameof(modernHelperFilePath),
                "Modern Storage V2 helper"
            );

            var candidatePath = NormalizeExistingFile
            (
                candidateDatabaseFilePath,
                nameof(candidateDatabaseFilePath),
                "Storage V2 candidate"
            );

            ValidateSecret(databasePasswordUtf8);
            EnsureNoSqliteSidecars(candidatePath);

            var before = FileSnapshot.Capture(candidatePath);
            Process process = null;
            Task<string> errorTask = null;
            var phase = JasonQueryDbStorageV2ReadOnlyValidationPhase.StartingHelper;

            try
            {
                process = StartHelper(helperPath);
                errorTask = process.StandardError.ReadToEndAsync();

                phase = JasonQueryDbStorageV2ReadOnlyValidationPhase.SendingRequest;

                JasonQueryDbStorageV2ReadOnlyValidatorProtocol.WriteRequest
                (
                    process.StandardInput.BaseStream,
                    candidatePath,
                    databasePasswordUtf8
                );

                process.StandardInput.BaseStream.Close();
                phase = JasonQueryDbStorageV2ReadOnlyValidationPhase.AwaitingValidated;

                var output = process.StandardOutput.BaseStream;
                var response = JasonQueryDbStorageV2ReadOnlyValidatorProtocol.ReadResponse(output);

                if (response.Status != JasonQueryDbStorageV2ReadOnlyValidatorResponseStatus.StorageV2Validated)
                {
                    throw new InvalidDataException
                    (
                        "The Modern helper returned an unexpected Storage V2 validation status."
                    );
                }

                phase = JasonQueryDbStorageV2ReadOnlyValidationPhase.WaitingForExit;
                process.WaitForExit();

                var standardError = GetTaskResult(errorTask);
                var standardErrorObserved = !string.IsNullOrEmpty(standardError);

                phase = JasonQueryDbStorageV2ReadOnlyValidationPhase.VerifyingCompletion;

                if (process.ExitCode != 0)
                {
                    throw new JasonQueryDbStorageV2ReadOnlyValidationException
                    (
                        JasonQueryDbStorageV2ReadOnlyValidationFailureKind.UnexpectedProcessExit,
                        phase,
                        process.ExitCode,
                        standardErrorObserved
                    );
                }

                if (standardErrorObserved)
                {
                    throw new JasonQueryDbStorageV2ReadOnlyValidationException
                    (
                        JasonQueryDbStorageV2ReadOnlyValidationFailureKind.StandardErrorOnSuccess,
                        phase,
                        process.ExitCode,
                        true
                    );
                }

                if (output.ReadByte() != -1)
                {
                    throw new InvalidDataException
                    (
                        "The Modern helper wrote trailing bytes after the Storage V2 validation response."
                    );
                }

                phase = JasonQueryDbStorageV2ReadOnlyValidationPhase.VerifyingCandidateImmutability;

                var after = FileSnapshot.Capture(candidatePath);

                if (!before.Equals(after))
                {
                    throw new JasonQueryDbStorageV2ReadOnlyValidationException
                    (
                        JasonQueryDbStorageV2ReadOnlyValidationFailureKind.CandidateChanged,
                        phase,
                        process.ExitCode,
                        false
                    );
                }

                EnsureNoSqliteSidecars(candidatePath);

                return new JasonQueryDbStorageV2ReadOnlyValidationResult
                (
                    before.Sha256
                );
            }
            catch (JasonQueryDbStorageV2ReadOnlyValidationException)
            {
                StopProcessBestEffort(process);
                DrainStandardErrorTask(errorTask);
                throw;
            }
            catch (Exception ex)
            {
                StopProcessBestEffort(process);

                var standardErrorObserved = DrainStandardErrorTask(errorTask);

                throw CreateSafeFailure
                (
                    ex,
                    phase,
                    process,
                    standardErrorObserved
                );
            }
            finally
            {
                if (process != null)
                {
                    process.Dispose();
                }
            }
        }

        private static Process StartHelper(string helperFilePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = helperFilePath,
                Arguments = string.Empty,
                WorkingDirectory = Path.GetDirectoryName(helperFilePath),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            try
            {
                if (!process.Start())
                {
                    process.Dispose();

                    throw new IOException
                    (
                        "The Modern Storage V2 validation helper could not be started."
                    );
                }

                return process;
            }
            catch
            {
                process.Dispose();
                throw;
            }
        }

        private static JasonQueryDbStorageV2ReadOnlyValidationException CreateSafeFailure(Exception exception, JasonQueryDbStorageV2ReadOnlyValidationPhase phase,
                                                                                          Process process, bool standardErrorObserved)
        {
            var failureKind = ResolveFailureKind(exception, phase);
            int? exitCode = null;

            if (process != null && process.HasExited)
            {
                exitCode = process.ExitCode;

                if (process.ExitCode != 0 && failureKind != JasonQueryDbStorageV2ReadOnlyValidationFailureKind.ProcessStartFailure)
                {
                    failureKind = JasonQueryDbStorageV2ReadOnlyValidationFailureKind.UnexpectedProcessExit;
                }
            }

            return new JasonQueryDbStorageV2ReadOnlyValidationException
            (
                failureKind,
                phase,
                exitCode,
                standardErrorObserved
            );
        }

        private static JasonQueryDbStorageV2ReadOnlyValidationFailureKind ResolveFailureKind(Exception exception,
                                                                                             JasonQueryDbStorageV2ReadOnlyValidationPhase phase)
        {
            if (phase == JasonQueryDbStorageV2ReadOnlyValidationPhase.StartingHelper)
            {
                return JasonQueryDbStorageV2ReadOnlyValidationFailureKind.ProcessStartFailure;
            }

            if (exception is EndOfStreamException)
            {
                return JasonQueryDbStorageV2ReadOnlyValidationFailureKind.ProtocolEndOfStream;
            }

            if (exception is InvalidDataException || exception is NotSupportedException)
            {
                return JasonQueryDbStorageV2ReadOnlyValidationFailureKind.ProtocolViolation;
            }

            if (exception is IOException)
            {
                return JasonQueryDbStorageV2ReadOnlyValidationFailureKind.PipeFailure;
            }

            return JasonQueryDbStorageV2ReadOnlyValidationFailureKind.UnexpectedFailure;
        }

        private static string NormalizeExistingFile(string filePath, string parameterName, string description)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(description + " path is required.", parameterName);
            }

            var normalized = Path.GetFullPath(filePath);

            if (!File.Exists(normalized))
            {
                throw new FileNotFoundException
                (
                    description + " was not found.",
                    normalized
                );
            }

            return normalized;
        }

        private static void ValidateSecret(byte[] value)
        {
            if (value == null || value.Length <= 0 || value.Length > JasonQueryDbStorageV2ReadOnlyValidatorProtocol.MaxDatabasePasswordUtf8Bytes)
            {
                throw new ArgumentException
                (
                    "A bounded database password is required.",
                    nameof(value)
                );
            }
        }

        private static void EnsureNoSqliteSidecars(string candidateDatabaseFilePath)
        {
            foreach (var suffix in new[] { "-journal", "-wal", "-shm" })
            {
                if (File.Exists(candidateDatabaseFilePath + suffix) || Directory.Exists(candidateDatabaseFilePath + suffix))
                {
                    throw new InvalidDataException
                    (
                        "The Storage V2 candidate has an active SQLite sidecar."
                    );
                }
            }
        }

        private static string GetTaskResult(Task<string> task)
        {
            return task == null ? string.Empty : task.GetAwaiter().GetResult();
        }

        private static bool DrainStandardErrorTask(Task<string> task)
        {
            if (task == null)
            {
                return false;
            }

            try
            {
                return !string.IsNullOrEmpty(task.GetAwaiter().GetResult());
            }
            catch
            {
                return false;
            }
        }

        private static void StopProcessBestEffort(Process process)
        {
            if (process == null)
            {
                return;
            }

            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
            catch
            {
            }
        }

        private sealed class FileSnapshot
        {
            private FileSnapshot(long length, DateTime lastWriteTimeUtc, string sha256)
            {
                Length = length;
                LastWriteTimeUtc = lastWriteTimeUtc;
                Sha256 = sha256;
            }

            public long Length { get; }

            public DateTime LastWriteTimeUtc { get; }

            public string Sha256 { get; }

            public static FileSnapshot Capture(string filePath)
            {
                var fileInfo = new FileInfo(filePath);

                return new FileSnapshot
                (
                    fileInfo.Length,
                    fileInfo.LastWriteTimeUtc,
                    JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(filePath)
                );
            }

            public bool Equals(FileSnapshot other)
            {
                return other != null
                       && Length == other.Length
                       && LastWriteTimeUtc == other.LastWriteTimeUtc
                       && string.Equals(Sha256, other.Sha256, StringComparison.Ordinal);
            }
        }
    }

    internal sealed class JasonQueryDbStorageV2ReadOnlyValidationResult
    {
        public JasonQueryDbStorageV2ReadOnlyValidationResult(string candidateDatabaseSha256)
        {
            if (string.IsNullOrWhiteSpace(candidateDatabaseSha256))
            {
                throw new ArgumentException
                (
                    "A candidate SHA-256 value is required.",
                    nameof(candidateDatabaseSha256)
                );
            }

            CandidateDatabaseSha256 = candidateDatabaseSha256;
        }

        public string CandidateDatabaseSha256 { get; }
    }

    internal enum JasonQueryDbStorageV2ReadOnlyValidationPhase
    {
        StartingHelper = 1,
        SendingRequest = 2,
        AwaitingValidated = 3,
        WaitingForExit = 4,
        VerifyingCompletion = 5,
        VerifyingCandidateImmutability = 6
    }

    internal enum JasonQueryDbStorageV2ReadOnlyValidationFailureKind
    {
        UnexpectedFailure = 1,
        ProcessStartFailure = 2,
        ProtocolEndOfStream = 3,
        ProtocolViolation = 4,
        PipeFailure = 5,
        UnexpectedProcessExit = 6,
        StandardErrorOnSuccess = 7,
        CandidateChanged = 8
    }

    internal sealed class JasonQueryDbStorageV2ReadOnlyValidationException : Exception
    {
        public JasonQueryDbStorageV2ReadOnlyValidationException(JasonQueryDbStorageV2ReadOnlyValidationFailureKind failureKind,
                                                                JasonQueryDbStorageV2ReadOnlyValidationPhase phase,
                                                                int? exitCode, bool standardErrorObserved) : base (BuildMessage(failureKind, phase, exitCode, standardErrorObserved))
        {
            FailureKind = failureKind;
            Phase = phase;
            ExitCode = exitCode;
            StandardErrorObserved = standardErrorObserved;
        }

        public JasonQueryDbStorageV2ReadOnlyValidationFailureKind FailureKind { get; }

        public JasonQueryDbStorageV2ReadOnlyValidationPhase Phase { get; }

        public int? ExitCode { get; }

        public bool StandardErrorObserved { get; }

        private static string BuildMessage(JasonQueryDbStorageV2ReadOnlyValidationFailureKind failureKind,
                                           JasonQueryDbStorageV2ReadOnlyValidationPhase phase,
                                           int? exitCode, bool standardErrorObserved)
        {
            return "Storage V2 read-only validation helper failure. " +
                   "Phase=" + phase + "; " +
                   "FailureKind=" + failureKind + "; " +
                   "ExitCode=" + (exitCode.HasValue ? exitCode.Value.ToString() : "Unavailable") + "; " +
                   "StandardErrorObserved=" + standardErrorObserved + ".";
        }
    }
}
