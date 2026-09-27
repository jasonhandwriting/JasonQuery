using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace JasonQuery.Database.Internal.Runtime.Modern
{
    internal sealed class ModernDatabaseRuntimeProcessClient : IDisposable
    {
        private const int ExitWaitMilliseconds = 2000;
        private const int MaxDiagnosticCharacters = 8192;

        private readonly string _executablePath;
        private readonly object _syncRoot = new object();
        private readonly StringBuilder _diagnostics = new StringBuilder();

        private Process _process;
        private Stream _input;
        private Stream _output;
        private long _nextRequestId;
        private bool _disposed;
        private bool _identityValidated;

        internal ModernDatabaseRuntimeProcessClient(string executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                throw new ArgumentException("A Modern DB runtime executable path is required.", nameof(executablePath));
            }

            _executablePath = Path.GetFullPath(executablePath);
        }

        internal ModernDatabaseRuntimeIdentity ProbeQualifiedRuntime()
        {
            lock (_syncRoot)
            {
                ThrowIfDisposed();
                EnsureStarted();
                return ReadIdentity(SendRequest(ModernDatabaseRuntimeOperation.Ping, null));
            }
        }

        internal void Open(string databasePath, byte[] databasePasswordUtf8)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("A Modern DB runtime database path is required.", nameof(databasePath));
            }

            if (databasePasswordUtf8 == null || databasePasswordUtf8.Length == 0)
            {
                throw new ArgumentException("A Modern DB runtime database credential is required.", nameof(databasePasswordUtf8));
            }

            lock (_syncRoot)
            {
                ThrowIfDisposed();
                EnsureStarted();

                var frame = SendRequest
                (
                    ModernDatabaseRuntimeOperation.Open,
                    writer =>
                    {
                        ModernDatabaseRuntimeProtocol.WriteString(writer, Path.GetFullPath(databasePath));
                        ModernDatabaseRuntimeProtocol.WriteByteArray(writer, databasePasswordUtf8);
                    }
                );

                EnsureEmptyPayload(frame);
            }
        }

        internal ModernDatabaseRuntimeQueryResult ExecuteQuery(string sql, IReadOnlyList<ModernDatabaseRuntimeParameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("Modern DB runtime query SQL is required.", nameof(sql));
            }

            lock (_syncRoot)
            {
                ThrowIfDisposed();
                EnsureStarted();

                var frame = SendRequest
                (
                    ModernDatabaseRuntimeOperation.ExecuteQuery,
                    writer =>
                    {
                        ModernDatabaseRuntimeProtocol.WriteString(writer, sql);
                        ModernDatabaseRuntimeProtocol.WriteParameters(writer, parameters);
                    }
                );

                using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                {
                    var result = ModernDatabaseRuntimeProtocol.ReadQueryResult(reader);

                    ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);
                    return result;
                }
            }
        }

        internal void ExecuteNonQuery(string sql, IReadOnlyList<ModernDatabaseRuntimeParameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("Modern DB runtime non-query SQL is required.", nameof(sql));
            }

            lock (_syncRoot)
            {
                ThrowIfDisposed();
                EnsureStarted();

                var frame = SendRequest
                (
                    ModernDatabaseRuntimeOperation.ExecuteNonQuery,
                    writer =>
                    {
                        ModernDatabaseRuntimeProtocol.WriteString(writer, sql);
                        ModernDatabaseRuntimeProtocol.WriteParameters(writer, parameters);
                    }
                );

                EnsureEmptyPayload(frame);
            }
        }

        internal void ExecuteBatchNonQuery(IReadOnlyList<string> sqlStatements)
        {
            if (sqlStatements == null)
            {
                throw new ArgumentNullException(nameof(sqlStatements));
            }

            lock (_syncRoot)
            {
                ThrowIfDisposed();
                EnsureStarted();

                var frame = SendRequest
                (
                    ModernDatabaseRuntimeOperation.ExecuteBatchNonQuery,
                    writer => ModernDatabaseRuntimeProtocol.WriteStringList(writer, sqlStatements)
                );

                EnsureEmptyPayload(frame);
            }
        }

        internal void Rekey(byte[] newDatabasePasswordUtf8)
        {
            lock (_syncRoot)
            {
                ThrowIfDisposed();

                if (newDatabasePasswordUtf8 == null || newDatabasePasswordUtf8.Length == 0)
                {
                    throw new ArgumentException("A new Storage V2 database credential is required.", nameof(newDatabasePasswordUtf8));
                }

                EnsureEmptyPayload
                (
                    SendRequest
                    (
                        ModernDatabaseRuntimeOperation.Rekey,
                        writer => ModernDatabaseRuntimeProtocol.WriteByteArray(writer, newDatabasePasswordUtf8)
                    )
                );
            }
        }

        internal void CreateFreshDatabase(string databasePath, byte[] databasePasswordUtf8, byte[] plaintextTemplateBytes)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("A fresh Storage V2 database path is required.", nameof(databasePath));
            }

            if (databasePasswordUtf8 == null || databasePasswordUtf8.Length == 0)
            {
                throw new ArgumentException("A fresh Storage V2 database credential is required.", nameof(databasePasswordUtf8));
            }

            if (plaintextTemplateBytes == null || plaintextTemplateBytes.Length == 0 || plaintextTemplateBytes.Length > ModernDatabaseRuntimeProtocol.MaxFreshTemplateBytes)
            {
                throw new ArgumentException("The fresh Storage V2 template payload is invalid.", nameof(plaintextTemplateBytes));
            }

            lock (_syncRoot)
            {
                ThrowIfDisposed();
                EnsureStarted();

                EnsureEmptyPayload
                (
                    SendRequest
                    (
                        ModernDatabaseRuntimeOperation.CreateFreshDatabase,
                        writer =>
                        {
                            ModernDatabaseRuntimeProtocol.WriteString(writer, Path.GetFullPath(databasePath));
                            ModernDatabaseRuntimeProtocol.WriteByteArray(writer, databasePasswordUtf8);
                            ModernDatabaseRuntimeProtocol.WriteByteArray(writer, plaintextTemplateBytes);
                        }
                    )
                );
            }
        }

        internal bool CanOpenWithoutKey(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("A Storage V2 database path is required.", nameof(databasePath));
            }

            lock (_syncRoot)
            {
                ThrowIfDisposed();
                EnsureStarted();

                var frame = SendRequest
                (
                    ModernDatabaseRuntimeOperation.CanOpenWithoutKey,
                    writer => ModernDatabaseRuntimeProtocol.WriteString(writer, Path.GetFullPath(databasePath))
                );

                using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                {
                    var result = reader.ReadBoolean();

                    ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);
                    return result;
                }
            }
        }

        internal void CloseDatabase()
        {
            lock (_syncRoot)
            {
                ThrowIfDisposed();

                if (_process == null || _process.HasExited)
                {
                    return;
                }

                EnsureEmptyPayload(SendRequest(ModernDatabaseRuntimeOperation.Close, null));
            }
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                try
                {
                    if (_process != null && !_process.HasExited)
                    {
                        try
                        {
                            EnsureEmptyPayload(SendRequest(ModernDatabaseRuntimeOperation.Shutdown, null));
                        }
                        catch
                        {
                        }

                        try
                        {
                            if (!_process.WaitForExit(ExitWaitMilliseconds))
                            {
                                _process.Kill();
                                _process.WaitForExit(ExitWaitMilliseconds);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                finally
                {
                    DisposeProcessResources();
                }
            }
        }

        private void EnsureStarted()
        {
            if (_process != null && !_process.HasExited)
            {
                if (!_identityValidated)
                {
                    ValidateIdentity(ReadIdentity(SendRequestCore(ModernDatabaseRuntimeOperation.Ping, null)));
                    _identityValidated = true;
                }

                return;
            }

            DisposeProcessResources();

            if (!File.Exists(_executablePath))
            {
                throw new FileNotFoundException("The isolated Modern DB runtime executable was not found.", _executablePath);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = _executablePath,
                WorkingDirectory = Path.GetDirectoryName(_executablePath),
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            _process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            _process.ErrorDataReceived += Process_ErrorDataReceived;

            if (!_process.Start())
            {
                DisposeProcessResources();
                throw new InvalidOperationException("The isolated Modern DB runtime process could not be started.");
            }

            _process.BeginErrorReadLine();
            _input = _process.StandardInput.BaseStream;
            _output = _process.StandardOutput.BaseStream;

            ValidateIdentity(ReadIdentity(SendRequestCore(ModernDatabaseRuntimeOperation.Ping, null)));
            _identityValidated = true;
        }

        private ModernDatabaseRuntimeFrame SendRequest(ModernDatabaseRuntimeOperation operation, Action<BinaryWriter> payloadWriter)
        {
            EnsureStarted();
            return SendRequestCore(operation, payloadWriter);
        }

        private ModernDatabaseRuntimeFrame SendRequestCore(ModernDatabaseRuntimeOperation operation, Action<BinaryWriter> payloadWriter)
        {
            if (_process == null || _process.HasExited || _input == null || _output == null)
            {
                throw CreateProcessUnavailableException();
            }

            var requestId = checked(++_nextRequestId);

            try
            {
                ModernDatabaseRuntimeProtocol.WriteRequest(_input, requestId, operation, payloadWriter);
                _input.Flush();

                if (!ModernDatabaseRuntimeProtocol.TryReadFrame(_output, out var frame))
                {
                    AbortProcess();
                    throw CreateProcessUnavailableException();
                }

                if (frame.Kind != ModernDatabaseRuntimeFrameKind.Response || frame.RequestId != requestId || frame.Operation != operation)
                {
                    AbortProcess();
                    throw new InvalidDataException("The isolated Modern DB runtime returned a mismatched protocol frame.");
                }

                if (frame.ResponseStatus == ModernDatabaseRuntimeResponseStatus.Failure)
                {
                    using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                    {
                        var errorType = ModernDatabaseRuntimeProtocol.ReadString(reader) ?? "RemoteError";
                        var errorMessage = ModernDatabaseRuntimeProtocol.ReadString(reader) ?? "The isolated Modern DB runtime operation failed.";

                        ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);
                        throw new InvalidOperationException(errorType + ": " + errorMessage);
                    }
                }

                if (frame.ResponseStatus != ModernDatabaseRuntimeResponseStatus.Success)
                {
                    AbortProcess();
                    throw new InvalidDataException("The isolated Modern DB runtime returned an invalid response status.");
                }

                return frame;
            }
            catch (EndOfStreamException ex)
            {
                AbortProcess();
                throw new InvalidOperationException(CreateProcessUnavailableMessage(), ex);
            }
            catch (IOException ex)
            {
                AbortProcess();
                throw new InvalidOperationException(CreateProcessUnavailableMessage(), ex);
            }
            catch (InvalidDataException)
            {
                AbortProcess();
                throw;
            }
        }

        private static ModernDatabaseRuntimeIdentity ReadIdentity(ModernDatabaseRuntimeFrame frame)
        {
            using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
            {
                var identity = ModernDatabaseRuntimeProtocol.ReadIdentity(reader);

                ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);
                return identity;
            }
        }

        private static void ValidateIdentity(ModernDatabaseRuntimeIdentity identity)
        {
            if (identity == null || !string.Equals(identity.RuntimeName, ModernDatabaseRuntimeProtocol.RuntimeName, StringComparison.Ordinal)
                || identity.ProtocolVersion != ModernDatabaseRuntimeProtocol.ProtocolVersion
                || !string.Equals(identity.SqliteVersion, ModernDatabaseRuntimeProtocol.QualifiedSqliteVersion, StringComparison.Ordinal)
                || !string.Equals(identity.SqlCipherVersionPrefix, ModernDatabaseRuntimeProtocol.QualifiedSqlCipherVersionPrefix, StringComparison.Ordinal)
                || !string.Equals(identity.NativeSha256, ModernDatabaseRuntimeProtocol.QualifiedNativeSha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("The isolated Modern DB runtime identity is invalid.");
            }
        }

        private static void EnsureEmptyPayload(ModernDatabaseRuntimeFrame frame)
        {
            if (frame.Payload.Length != 0)
            {
                throw new InvalidDataException("The isolated Modern DB runtime returned an unexpected response payload.");
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            lock (_diagnostics)
            {
                if (_diagnostics.Length >= MaxDiagnosticCharacters)
                {
                    return;
                }

                var remaining = MaxDiagnosticCharacters - _diagnostics.Length;
                var value = e.Data.Length <= remaining ? e.Data : e.Data.Substring(0, remaining);

                if (_diagnostics.Length > 0)
                {
                    _diagnostics.Append(" | ");
                }

                _diagnostics.Append(value);
            }
        }

        private InvalidOperationException CreateProcessUnavailableException()
        {
            return new InvalidOperationException(CreateProcessUnavailableMessage());
        }

        private string CreateProcessUnavailableMessage()
        {
            string diagnostic;

            lock (_diagnostics)
            {
                diagnostic = _diagnostics.ToString();
            }

            if (string.IsNullOrWhiteSpace(diagnostic))
            {
                return "The isolated Modern DB runtime process is unavailable.";
            }

            return "The isolated Modern DB runtime process is unavailable. Diagnostic: " + diagnostic;
        }

        private void AbortProcess()
        {
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                }
            }
            catch
            {
            }

            DisposeProcessResources();
        }

        private void DisposeProcessResources()
        {
            _identityValidated = false;

            try
            {
                _input?.Dispose();
            }
            catch
            {
            }

            try
            {
                _output?.Dispose();
            }
            catch
            {
            }

            if (_process != null)
            {
                try
                {
                    _process.CancelErrorRead();
                }
                catch
                {
                }

                _process.ErrorDataReceived -= Process_ErrorDataReceived;
                _process.Dispose();
            }

            _input = null;
            _output = null;
            _process = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ModernDatabaseRuntimeProcessClient));
            }
        }
    }
}
