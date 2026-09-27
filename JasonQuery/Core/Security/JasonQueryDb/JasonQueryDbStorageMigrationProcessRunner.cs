using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Runs the isolated legacy V1 reader and modern V2 candidate writer.
    ///
    /// This type owns child-process lifecycle and the bounded plaintext stream bridge only.
    /// It does not mutate the migration journal, replace JasonQuery.db, commit security
    /// metadata, or change runtime storage routing.
    /// </summary>
    public sealed class JasonQueryDbStorageMigrationProcessRunner
    {
        private const string LegacyStreamArgument = "--stream-storage-v1";

        public JasonQueryDbStorageMigrationProcessResult CreateCandidate(string legacyHelperFilePath, string modernHelperFilePath, string sourceDatabaseFilePath,
                                                                         string candidateDatabaseFilePath, byte[] sourceDatabasePasswordUtf8, byte[] targetDatabasePasswordUtf8)
        {
            var paths = ValidateAndNormalizePaths
            (
                legacyHelperFilePath,
                modernHelperFilePath,
                sourceDatabaseFilePath,
                candidateDatabaseFilePath
            );

            ValidateSecret
            (
                sourceDatabasePasswordUtf8,
                JasonQueryDbStorageMigrationWireProtocol.MaxDatabasePasswordUtf8Bytes,
                nameof(sourceDatabasePasswordUtf8)
            );

            ValidateSecret
            (
                targetDatabasePasswordUtf8,
                JasonQueryDbStorageV2CandidateWriterProtocol.MaxDatabasePasswordUtf8Bytes,
                nameof(targetDatabasePasswordUtf8)
            );

            Process legacyProcess = null;
            Process modernProcess = null;
            Task<string> legacyErrorTask = null;
            Task<string> modernErrorTask = null;

            var phase = JasonQueryDbStorageMigrationProcessPhase.StartingModernHelper;

            try
            {
                modernProcess = StartHelper(paths.ModernHelperFilePath, string.Empty);
                modernErrorTask = modernProcess.StandardError.ReadToEndAsync();

                var modernInput = modernProcess.StandardInput.BaseStream;

                phase = JasonQueryDbStorageMigrationProcessPhase.SendingModernRequest;

                JasonQueryDbStorageV2CandidateWriterProtocol.WriteRequest
                (
                    modernInput,
                    paths.CandidateDatabaseFilePath,
                    targetDatabasePasswordUtf8
                );

                phase = JasonQueryDbStorageMigrationProcessPhase.StartingLegacyHelper;
                legacyProcess = StartHelper(paths.LegacyHelperFilePath, LegacyStreamArgument);
                legacyErrorTask = legacyProcess.StandardError.ReadToEndAsync();

                var legacyInput = legacyProcess.StandardInput.BaseStream;

                phase = JasonQueryDbStorageMigrationProcessPhase.SendingLegacyRequest;

                JasonQueryDbStorageMigrationWireProtocol.WriteRequest
                (
                    legacyInput,
                    paths.SourceDatabaseFilePath,
                    sourceDatabasePasswordUtf8
                );

                legacyInput.Close();

                var legacyOutput = legacyProcess.StandardOutput.BaseStream;

                phase = JasonQueryDbStorageMigrationProcessPhase.AwaitingStorageV1Validated;

                RequireLegacyStatus
                (
                    JasonQueryDbStorageMigrationWireProtocol.ReadResponse(legacyOutput),
                    JasonQueryDbStorageMigrationWireResponseStatus.StorageV1Validated
                );

                phase = JasonQueryDbStorageMigrationProcessPhase.AwaitingStorageV1LogicalStreamReady;

                RequireLegacyStatus
                (
                    JasonQueryDbStorageMigrationWireProtocol.ReadResponse(legacyOutput),
                    JasonQueryDbStorageMigrationWireResponseStatus.StorageV1LogicalStreamReady
                );

                phase = JasonQueryDbStorageMigrationProcessPhase.BridgingLogicalStream;

                var logicalBytesForwarded = JasonQueryDbStorageMigrationStreamBridge.Forward
                (
                    legacyOutput,
                    modernInput
                );

                modernInput.Close();

                phase = JasonQueryDbStorageMigrationProcessPhase.AwaitingCandidateValidated;

                var modernOutput = modernProcess.StandardOutput.BaseStream;
                var modernResponse = JasonQueryDbStorageV2CandidateWriterProtocol.ReadResponse(modernOutput);

                if (modernResponse.Status != JasonQueryDbStorageV2CandidateWriterResponseStatus.CandidateValidated)
                {
                    throw new InvalidDataException
                    (
                        $"Storage V2 candidate writer returned unexpected status '{modernResponse.Status}'."
                    );
                }

                phase = JasonQueryDbStorageMigrationProcessPhase.WaitingForHelperExit;
                legacyProcess.WaitForExit();
                modernProcess.WaitForExit();

                var legacyError = GetTaskResult(legacyErrorTask);
                var modernError = GetTaskResult(modernErrorTask);

                phase = JasonQueryDbStorageMigrationProcessPhase.VerifyingHelperCompletion;

                if (legacyProcess.ExitCode != 0)
                {
                    throw JasonQueryDbStorageMigrationProcessFailureFactory.CreateKnown
                    (
                        JasonQueryDbStorageMigrationHelperRole.LegacyStorageV1,
                        phase,
                        JasonQueryDbStorageMigrationProcessFailureKind.UnexpectedProcessExit,
                        legacyProcess.ExitCode,
                        !string.IsNullOrEmpty(legacyError)
                    );
                }

                if (modernProcess.ExitCode != 0)
                {
                    throw JasonQueryDbStorageMigrationProcessFailureFactory.CreateKnown
                    (
                        JasonQueryDbStorageMigrationHelperRole.ModernStorageV2CandidateWriter,
                        phase,
                        JasonQueryDbStorageMigrationProcessFailureKind.UnexpectedProcessExit,
                        modernProcess.ExitCode,
                        !string.IsNullOrEmpty(modernError)
                    );
                }

                if (!string.IsNullOrEmpty(legacyError))
                {
                    throw JasonQueryDbStorageMigrationProcessFailureFactory.CreateKnown
                    (
                        JasonQueryDbStorageMigrationHelperRole.LegacyStorageV1,
                        phase,
                        JasonQueryDbStorageMigrationProcessFailureKind.StandardErrorOnSuccess,
                        legacyProcess.ExitCode,
                        true
                    );
                }

                if (!string.IsNullOrEmpty(modernError))
                {
                    throw JasonQueryDbStorageMigrationProcessFailureFactory.CreateKnown
                    (
                        JasonQueryDbStorageMigrationHelperRole.ModernStorageV2CandidateWriter,
                        phase,
                        JasonQueryDbStorageMigrationProcessFailureKind.StandardErrorOnSuccess,
                        modernProcess.ExitCode,
                        true
                    );
                }

                phase = JasonQueryDbStorageMigrationProcessPhase.VerifyingCandidateOutput;

                if (modernOutput.ReadByte() != -1)
                {
                    throw new InvalidDataException
                    (
                        "Storage V2 candidate writer produced trailing STDOUT bytes after CandidateValidated."
                    );
                }

                if (!File.Exists(paths.CandidateDatabaseFilePath))
                {
                    throw new InvalidDataException
                    (
                        "Storage V2 candidate writer reported success but the candidate database is missing."
                    );
                }

                EnsureNoCandidateSidecars(paths.CandidateDatabaseFilePath);

                return new JasonQueryDbStorageMigrationProcessResult(logicalBytesForwarded);
            }
            catch (Exception ex)
            {
                var legacyObservation = ObserveHelper(legacyProcess, legacyErrorTask);
                var modernObservation = ObserveHelper(modernProcess, modernErrorTask);

                var failure = JasonQueryDbStorageMigrationProcessFailureFactory.Create
                (
                    ex,
                    phase,
                    legacyObservation,
                    modernObservation
                );

                KillAndWait(legacyProcess);
                KillAndWait(modernProcess);
                DrainStandardErrorTask(legacyErrorTask);
                DrainStandardErrorTask(modernErrorTask);

                try
                {
                    DeleteCandidateArtifacts(paths.CandidateDatabaseFilePath);
                }
                catch
                {
                    throw JasonQueryDbStorageMigrationProcessFailureFactory.WithCandidateCleanupFailure(failure);
                }

                throw failure;
            }
            finally
            {
                DisposeProcess(legacyProcess);
                DisposeProcess(modernProcess);
            }
        }

        private static NormalizedPaths ValidateAndNormalizePaths(string legacyHelperFilePath, string modernHelperFilePath,
                                                                 string sourceDatabaseFilePath, string candidateDatabaseFilePath)
        {
            var legacyHelper = NormalizeExistingFile
            (
                legacyHelperFilePath,
                nameof(legacyHelperFilePath),
                "Legacy Storage V1 helper"
            );

            var modernHelper = NormalizeExistingFile
            (
                modernHelperFilePath,
                nameof(modernHelperFilePath),
                "Storage V2 candidate writer"
            );

            if (string.Equals(legacyHelper, modernHelper, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Legacy and modern migration helpers must be different executables.");
            }

            var sourceDatabase = NormalizeExistingFile
            (
                sourceDatabaseFilePath,
                nameof(sourceDatabaseFilePath),
                "Source database"
            );

            var candidateDatabase = NormalizeFilePath
            (
                candidateDatabaseFilePath,
                nameof(candidateDatabaseFilePath)
            );

            if (string.Equals(sourceDatabase, candidateDatabase, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The candidate database path must differ from the source database path.");
            }

            if (File.Exists(candidateDatabase))
            {
                throw new IOException("The Storage V2 candidate database path already exists.");
            }

            if (Directory.Exists(candidateDatabase))
            {
                throw new IOException("The Storage V2 candidate database path identifies a directory.");
            }

            var sourceDirectory = Path.GetDirectoryName(sourceDatabase);
            var candidateDirectory = Path.GetDirectoryName(candidateDatabase);

            if (!string.Equals(sourceDirectory, candidateDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException
                (
                    "The source and candidate databases must reside in the same directory and volume."
                );
            }

            if (string.IsNullOrEmpty(candidateDirectory) || !Directory.Exists(candidateDirectory))
            {
                throw new DirectoryNotFoundException
                (
                    "The Storage V2 candidate database directory does not exist."
                );
            }

            return new NormalizedPaths
            (
                legacyHelper,
                modernHelper,
                sourceDatabase,
                candidateDatabase
            );
        }

        private static string NormalizeExistingFile(string filePath, string parameterName, string description)
        {
            var normalized = NormalizeFilePath(filePath, parameterName);

            if (!File.Exists(normalized))
            {
                throw new FileNotFoundException(description + " was not found.", normalized);
            }

            return normalized;
        }

        private static string NormalizeFilePath(string filePath, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A file path is required.", parameterName);
            }

            return Path.GetFullPath(filePath);
        }

        private static void ValidateSecret(byte[] value, int maximumLength, string parameterName)
        {
            if (value == null || value.Length == 0 || value.Length > maximumLength)
            {
                throw new ArgumentException
                (
                    "A bounded database password payload is required.",
                    parameterName
                );
            }
        }

        private static Process StartHelper(string executableFilePath, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executableFilePath,
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(executableFilePath),
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
                    throw new InvalidOperationException("The database storage-migration helper process did not start.");
                }

                return process;
            }
            catch
            {
                process.Dispose();
                throw;
            }
        }

        private static void RequireLegacyStatus(JasonQueryDbStorageMigrationWireResponse response,
                                                JasonQueryDbStorageMigrationWireResponseStatus expectedStatus)
        {
            if (response.Status != expectedStatus)
            {
                throw new InvalidDataException
                (
                    $"Legacy Storage V1 helper returned unexpected status '{response.Status}'."
                );
            }
        }

        private static string GetTaskResult(Task<string> task)
        {
            if (task == null)
            {
                return string.Empty;
            }

            return task.GetAwaiter().GetResult();
        }

        private static JasonQueryDbStorageMigrationHelperObservation ObserveHelper(Process process, Task<string> standardErrorTask)
        {
            if (process == null)
            {
                return JasonQueryDbStorageMigrationHelperObservation.NotStarted;
            }

            var hasExited = false;
            int? exitCode = null;

            try
            {
                hasExited = process.HasExited;

                if (!hasExited)
                {
                    hasExited = process.WaitForExit(250);
                }

                if (hasExited)
                {
                    exitCode = process.ExitCode;
                }
            }
            catch
            {
                hasExited = false;
                exitCode = null;
            }

            var standardErrorObserved = TryGetStandardErrorObserved(standardErrorTask, hasExited);

            return new JasonQueryDbStorageMigrationHelperObservation
            (
                true,
                hasExited,
                exitCode,
                standardErrorObserved
            );
        }

        private static bool TryGetStandardErrorObserved(Task<string> task, bool helperHasExited)
        {
            if (task == null)
            {
                return false;
            }

            try
            {
                if (!task.IsCompleted && helperHasExited)
                {
                    task.Wait(250);
                }

                if (task.Status != TaskStatus.RanToCompletion)
                {
                    return false;
                }

                return !string.IsNullOrEmpty(task.Result);
            }
            catch
            {
                return false;
            }
        }

        private static void DrainStandardErrorTask(Task<string> task)
        {
            if (task == null)
            {
                return;
            }

            try
            {
                if (!task.IsCompleted)
                {
                    task.Wait(1000);
                }

                if (task.Status == TaskStatus.RanToCompletion)
                {
                    var ignored = task.Result;
                }
            }
            catch
            {
                //Failure diagnostics are intentionally best effort and never expose raw STDERR.
            }
        }

        private static void EnsureNoCandidateSidecars(string candidateDatabaseFilePath)
        {
            var sidecars = GetCandidateSidecarPaths(candidateDatabaseFilePath);

            foreach (var sidecar in sidecars)
            {
                if (File.Exists(sidecar))
                {
                    throw new InvalidDataException
                    (
                        "Storage V2 candidate writer left a SQLite sidecar after successful validation."
                    );
                }
            }
        }

        private static void DeleteCandidateArtifacts(string candidateDatabaseFilePath)
        {
            var paths = GetCandidateSidecarPaths(candidateDatabaseFilePath);

            DeleteFileIfExists(candidateDatabaseFilePath);

            foreach (var path in paths)
            {
                DeleteFileIfExists(path);
            }
        }

        private static string[] GetCandidateSidecarPaths(string candidateDatabaseFilePath)
        {
            return new[]
            {
                candidateDatabaseFilePath + "-journal",
                candidateDatabaseFilePath + "-wal",
                candidateDatabaseFilePath + "-shm"
            };
        }

        private static void DeleteFileIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static void KillAndWait(Process process)
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
                    process.WaitForExit(5000);
                }
            }
            catch
            {
                //Cleanup is best effort here. Candidate-artifact cleanup still runs and
                //the original process/protocol failure remains fail-closed.
            }
        }

        private static void DisposeProcess(Process process)
        {
            if (process != null)
            {
                process.Dispose();
            }
        }

        private sealed class NormalizedPaths
        {
            public NormalizedPaths(string legacyHelperFilePath, string modernHelperFilePath, string sourceDatabaseFilePath, string candidateDatabaseFilePath)
            {
                LegacyHelperFilePath = legacyHelperFilePath;
                ModernHelperFilePath = modernHelperFilePath;
                SourceDatabaseFilePath = sourceDatabaseFilePath;
                CandidateDatabaseFilePath = candidateDatabaseFilePath;
            }

            public string LegacyHelperFilePath { get; }

            public string ModernHelperFilePath { get; }

            public string SourceDatabaseFilePath { get; }

            public string CandidateDatabaseFilePath { get; }
        }
    }

    public sealed class JasonQueryDbStorageMigrationProcessResult
    {
        internal JasonQueryDbStorageMigrationProcessResult(long logicalBytesForwarded)
        {
            if (logicalBytesForwarded < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(logicalBytesForwarded));
            }

            LogicalBytesForwarded = logicalBytesForwarded;
        }

        public long LogicalBytesForwarded { get; }
    }

    public enum JasonQueryDbStorageMigrationHelperRole
    {
        Undetermined = 0,
        LegacyStorageV1 = 1,
        ModernStorageV2CandidateWriter = 2
    }

    public enum JasonQueryDbStorageMigrationProcessPhase
    {
        Unknown = 0,
        StartingModernHelper = 1,
        SendingModernRequest = 2,
        StartingLegacyHelper = 3,
        SendingLegacyRequest = 4,
        AwaitingStorageV1Validated = 5,
        AwaitingStorageV1LogicalStreamReady = 6,
        BridgingLogicalStream = 7,
        AwaitingCandidateValidated = 8,
        WaitingForHelperExit = 9,
        VerifyingHelperCompletion = 10,
        VerifyingCandidateOutput = 11
    }

    public enum JasonQueryDbStorageMigrationProcessFailureKind
    {
        UnexpectedFailure = 0,
        ProcessStartFailure = 1,
        ProtocolEndOfStream = 2,
        ProtocolViolation = 3,
        PipeFailure = 4,
        UnexpectedProcessExit = 5,
        StandardErrorOnSuccess = 6
    }

    public sealed class JasonQueryDbStorageMigrationProcessException : InvalidOperationException
    {
        internal JasonQueryDbStorageMigrationProcessException(JasonQueryDbStorageMigrationHelperRole helperRole, JasonQueryDbStorageMigrationProcessPhase phase,
                                                              JasonQueryDbStorageMigrationProcessFailureKind failureKind, int? exitCode,
                                                              bool standardErrorObserved, bool candidateCleanupFailed) : base(BuildMessage(helperRole, phase, failureKind, exitCode, standardErrorObserved, candidateCleanupFailed))
        {
            HelperRole = helperRole;
            Phase = phase;
            FailureKind = failureKind;
            ExitCode = exitCode;
            StandardErrorObserved = standardErrorObserved;
            CandidateCleanupFailed = candidateCleanupFailed;
        }

        public JasonQueryDbStorageMigrationHelperRole HelperRole { get; }

        public JasonQueryDbStorageMigrationProcessPhase Phase { get; }

        public JasonQueryDbStorageMigrationProcessFailureKind FailureKind { get; }

        public int? ExitCode { get; }

        public bool StandardErrorObserved { get; }

        public bool CandidateCleanupFailed { get; }

        private static string BuildMessage(JasonQueryDbStorageMigrationHelperRole helperRole, JasonQueryDbStorageMigrationProcessPhase phase,
                                           JasonQueryDbStorageMigrationProcessFailureKind failureKind, int? exitCode,
                                           bool standardErrorObserved, bool candidateCleanupFailed)
        {
            var exitCodeText = exitCode.HasValue ? exitCode.Value.ToString() : "Unavailable";

            return "Database storage migration helper failure. " +
                   $"HelperRole={helperRole}; " +
                   $"Phase={phase}; " +
                   $"FailureKind={failureKind}; " +
                   $"ExitCode={exitCodeText}; " +
                   $"StandardErrorObserved={standardErrorObserved}; " +
                   $"CandidateCleanupFailed={candidateCleanupFailed}.";
        }
    }

    internal sealed class JasonQueryDbStorageMigrationHelperObservation
    {
        public static readonly JasonQueryDbStorageMigrationHelperObservation NotStarted = new JasonQueryDbStorageMigrationHelperObservation(false, false, null, false);

        public JasonQueryDbStorageMigrationHelperObservation(bool wasStarted, bool hasExited, int? exitCode, bool standardErrorObserved)
        {
            if (!wasStarted && (hasExited || exitCode.HasValue || standardErrorObserved))
            {
                throw new ArgumentException("A helper that was not started cannot contain process observations.");
            }

            if (hasExited != exitCode.HasValue)
            {
                throw new ArgumentException("Exited helper observations must contain exactly one exit code.");
            }

            WasStarted = wasStarted;
            HasExited = hasExited;
            ExitCode = exitCode;
            StandardErrorObserved = standardErrorObserved;
        }

        public bool WasStarted { get; }

        public bool HasExited { get; }

        public int? ExitCode { get; }

        public bool StandardErrorObserved { get; }
    }

    internal static class JasonQueryDbStorageMigrationProcessFailureFactory
    {
        public static JasonQueryDbStorageMigrationProcessException Create(Exception failure, JasonQueryDbStorageMigrationProcessPhase phase,
                                                                          JasonQueryDbStorageMigrationHelperObservation legacyObservation,
                                                                          JasonQueryDbStorageMigrationHelperObservation modernObservation)
        {
            if (failure == null)
            {
                throw new ArgumentNullException(nameof(failure));
            }

            if (legacyObservation == null)
            {
                throw new ArgumentNullException(nameof(legacyObservation));
            }

            if (modernObservation == null)
            {
                throw new ArgumentNullException(nameof(modernObservation));
            }

            if (failure is JasonQueryDbStorageMigrationProcessException processFailure)
            {
                return processFailure;
            }

            var helperRole = ResolveFailureRole(phase, legacyObservation, modernObservation);
            var observation = SelectObservation(helperRole, legacyObservation, modernObservation);
            var failureKind = ResolveFailureKind(failure, phase);
            var standardErrorObserved = observation == null ? legacyObservation.StandardErrorObserved || modernObservation.StandardErrorObserved : observation.StandardErrorObserved;

            return new JasonQueryDbStorageMigrationProcessException
            (
                helperRole,
                phase,
                failureKind,
                observation?.ExitCode,
                standardErrorObserved,
                false
            );
        }

        public static JasonQueryDbStorageMigrationProcessException CreateKnown(JasonQueryDbStorageMigrationHelperRole helperRole,
                                                                               JasonQueryDbStorageMigrationProcessPhase phase,
                                                                               JasonQueryDbStorageMigrationProcessFailureKind failureKind,
                                                                               int? exitCode, bool standardErrorObserved)
        {
            return new JasonQueryDbStorageMigrationProcessException
            (
                helperRole,
                phase,
                failureKind,
                exitCode,
                standardErrorObserved,
                false
            );
        }

        public static JasonQueryDbStorageMigrationProcessException WithCandidateCleanupFailure(JasonQueryDbStorageMigrationProcessException failure)
        {
            if (failure == null)
            {
                throw new ArgumentNullException(nameof(failure));
            }

            if (failure.CandidateCleanupFailed)
            {
                return failure;
            }

            return new JasonQueryDbStorageMigrationProcessException
            (
                failure.HelperRole,
                failure.Phase,
                failure.FailureKind,
                failure.ExitCode,
                failure.StandardErrorObserved,
                true
            );
        }

        internal static JasonQueryDbStorageMigrationHelperRole ResolveFailureRole(JasonQueryDbStorageMigrationProcessPhase phase,
                                                                                  JasonQueryDbStorageMigrationHelperObservation legacyObservation,
                                                                                  JasonQueryDbStorageMigrationHelperObservation modernObservation)
        {
            switch (phase)
            {
                case JasonQueryDbStorageMigrationProcessPhase.StartingLegacyHelper:
                case JasonQueryDbStorageMigrationProcessPhase.SendingLegacyRequest:
                case JasonQueryDbStorageMigrationProcessPhase.AwaitingStorageV1Validated:
                case JasonQueryDbStorageMigrationProcessPhase.AwaitingStorageV1LogicalStreamReady:
                    {
                        return JasonQueryDbStorageMigrationHelperRole.LegacyStorageV1;
                    }
                case JasonQueryDbStorageMigrationProcessPhase.StartingModernHelper:
                case JasonQueryDbStorageMigrationProcessPhase.SendingModernRequest:
                case JasonQueryDbStorageMigrationProcessPhase.AwaitingCandidateValidated:
                case JasonQueryDbStorageMigrationProcessPhase.VerifyingCandidateOutput:
                    {
                        return JasonQueryDbStorageMigrationHelperRole.ModernStorageV2CandidateWriter;
                    }
            }

            if (legacyObservation.HasExited && legacyObservation.ExitCode.GetValueOrDefault() != 0)
            {
                return JasonQueryDbStorageMigrationHelperRole.LegacyStorageV1;
            }

            if (modernObservation.HasExited && modernObservation.ExitCode.GetValueOrDefault() != 0)
            {
                return JasonQueryDbStorageMigrationHelperRole.ModernStorageV2CandidateWriter;
            }

            if (legacyObservation.HasExited && !modernObservation.HasExited)
            {
                return JasonQueryDbStorageMigrationHelperRole.LegacyStorageV1;
            }

            if (modernObservation.HasExited && !legacyObservation.HasExited)
            {
                return JasonQueryDbStorageMigrationHelperRole.ModernStorageV2CandidateWriter;
            }

            return JasonQueryDbStorageMigrationHelperRole.Undetermined;
        }

        internal static JasonQueryDbStorageMigrationProcessFailureKind ResolveFailureKind(Exception failure, JasonQueryDbStorageMigrationProcessPhase phase)
        {
            if (phase == JasonQueryDbStorageMigrationProcessPhase.StartingLegacyHelper || phase == JasonQueryDbStorageMigrationProcessPhase.StartingModernHelper)
            {
                return JasonQueryDbStorageMigrationProcessFailureKind.ProcessStartFailure;
            }

            if (failure is EndOfStreamException)
            {
                return JasonQueryDbStorageMigrationProcessFailureKind.ProtocolEndOfStream;
            }

            if (failure is InvalidDataException)
            {
                return JasonQueryDbStorageMigrationProcessFailureKind.ProtocolViolation;
            }

            if (failure is IOException)
            {
                return JasonQueryDbStorageMigrationProcessFailureKind.PipeFailure;
            }

            return JasonQueryDbStorageMigrationProcessFailureKind.UnexpectedFailure;
        }

        private static JasonQueryDbStorageMigrationHelperObservation SelectObservation(JasonQueryDbStorageMigrationHelperRole helperRole,
                                                                                       JasonQueryDbStorageMigrationHelperObservation legacyObservation,
                                                                                       JasonQueryDbStorageMigrationHelperObservation modernObservation)
        {
            if (helperRole == JasonQueryDbStorageMigrationHelperRole.LegacyStorageV1)
            {
                return legacyObservation;
            }

            if (helperRole == JasonQueryDbStorageMigrationHelperRole.ModernStorageV2CandidateWriter)
            {
                return modernObservation;
            }

            return null;
        }
    }
}
