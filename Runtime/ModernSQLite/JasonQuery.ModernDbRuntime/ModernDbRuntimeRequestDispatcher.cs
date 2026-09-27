using JasonQuery.Database.Internal.Runtime.Modern;
using JasonQuery.ModernDbMigration;
using System;
using System.IO;

namespace JasonQuery.ModernDbRuntime
{
    internal sealed class ModernDbRuntimeRequestDispatcher
    {
        private readonly ModernDbRuntimeSession _session;

        internal ModernDbRuntimeRequestDispatcher(ModernDbRuntimeSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        internal bool TryDispatchNext(Stream input, Stream output)
        {
            if (!ModernDatabaseRuntimeProtocol.TryReadFrame(input, out var frame))
            {
                return false;
            }

            if (frame.Kind != ModernDatabaseRuntimeFrameKind.Request)
            {
                throw new InvalidDataException("The isolated Modern DB runtime received a non-request frame.");
            }

            try
            {
                var keepRunning = Dispatch(frame, output);

                output.Flush();
                return keepRunning;
            }
            catch (Exception ex)
            {
                ModernDatabaseRuntimeProtocol.WriteFailureResponse
                (
                    output,
                    frame.RequestId,
                    frame.Operation,
                    ex.GetType().FullName,
                    Sanitize(ex.Message)
                );

                output.Flush();
                return true;
            }
        }

        private bool Dispatch(ModernDatabaseRuntimeFrame frame, Stream output)
        {
            switch (frame.Operation)
            {
                case ModernDatabaseRuntimeOperation.Ping:
                    {
                        EnsureEmptyRequest(frame);

                        ModernDatabaseRuntimeProtocol.WriteSuccessResponse
                        (
                            output,
                            frame.RequestId,
                            frame.Operation,
                            writer => ModernDatabaseRuntimeProtocol.WriteIdentity
                            (
                                writer,
                                new ModernDatabaseRuntimeIdentity
                                (
                                    "JasonQuery.ModernDbRuntime",
                                    ModernDatabaseRuntimeProtocol.ProtocolVersion,
                                    ModernSqlCipherRuntime.QualifiedSqliteVersion,
                                    ModernSqlCipherRuntime.QualifiedSqlCipherVersionPrefix,
                                    ModernSqlCipherRuntime.QualifiedNativeSha256
                                )
                            )
                        );

                        return true;
                    }
                case ModernDatabaseRuntimeOperation.Open:
                    {
                        using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                        {
                            var databasePath = ModernDatabaseRuntimeProtocol.ReadString(reader);
                            var credential = ModernDatabaseRuntimeProtocol.ReadByteArray(reader);

                            try
                            {
                                ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);
                                _session.Open(databasePath, credential);
                            }
                            finally
                            {
                                if (credential != null)
                                {
                                    Array.Clear(credential, 0, credential.Length);
                                }
                            }
                        }

                        WriteEmptySuccess(output, frame);
                        return true;
                    }
                case ModernDatabaseRuntimeOperation.Rekey:
                    {
                        using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                        {
                            var newCredential = ModernDatabaseRuntimeProtocol.ReadByteArray(reader);

                            try
                            {
                                ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);
                                _session.Rekey(newCredential);
                            }
                            finally
                            {
                                if (newCredential != null)
                                {
                                    Array.Clear(newCredential, 0, newCredential.Length);
                                }
                            }
                        }

                        WriteEmptySuccess(output, frame);
                        return true;
                    }
                case ModernDatabaseRuntimeOperation.CreateFreshDatabase:
                    {
                        using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                        {
                            var databasePath = ModernDatabaseRuntimeProtocol.ReadString(reader);
                            var credential = ModernDatabaseRuntimeProtocol.ReadByteArray(reader);
                            var templateBytes = ModernDatabaseRuntimeProtocol.ReadByteArray(reader);

                            try
                            {
                                ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);

                                if (templateBytes == null || templateBytes.Length == 0 || templateBytes.Length > ModernDatabaseRuntimeProtocol.MaxFreshTemplateBytes)
                                {
                                    throw new InvalidDataException("The fresh Storage V2 template payload is invalid.");
                                }

                                _session.CreateFreshDatabase(databasePath, credential, templateBytes);
                            }
                            finally
                            {
                                if (credential != null)
                                {
                                    Array.Clear(credential, 0, credential.Length);
                                }

                                if (templateBytes != null)
                                {
                                    Array.Clear(templateBytes, 0, templateBytes.Length);
                                }
                            }
                        }

                        WriteEmptySuccess(output, frame);
                        return true;
                    }
                case ModernDatabaseRuntimeOperation.CanOpenWithoutKey:
                    {
                        using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                        {
                            var databasePath = ModernDatabaseRuntimeProtocol.ReadString(reader);

                            ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);

                            ModernDatabaseRuntimeProtocol.WriteSuccessResponse
                            (
                                output,
                                frame.RequestId,
                                frame.Operation,
                                writer => writer.Write(_session.CanOpenWithoutKey(databasePath))
                            );
                        }

                        return true;
                    }
                case ModernDatabaseRuntimeOperation.ExecuteQuery:
                    {
                        using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                        {
                            var sql = ModernDatabaseRuntimeProtocol.ReadString(reader);
                            var parameters = ModernDatabaseRuntimeProtocol.ReadParameters(reader);

                            ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);

                            var result = _session.ExecuteQuery(sql, parameters);

                            ModernDatabaseRuntimeProtocol.WriteSuccessResponse
                            (
                                output,
                                frame.RequestId,
                                frame.Operation,
                                writer => ModernDatabaseRuntimeProtocol.WriteQueryResult(writer, result)
                            );
                        }

                        return true;
                    }
                case ModernDatabaseRuntimeOperation.ExecuteNonQuery:
                    {
                        using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                        {
                            var sql = ModernDatabaseRuntimeProtocol.ReadString(reader);
                            var parameters = ModernDatabaseRuntimeProtocol.ReadParameters(reader);

                            ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);
                            _session.ExecuteNonQuery(sql, parameters);
                        }

                        WriteEmptySuccess(output, frame);
                        return true;
                    }
                case ModernDatabaseRuntimeOperation.ExecuteBatchNonQuery:
                    {
                        using (var reader = ModernDatabaseRuntimeProtocol.OpenPayloadReader(frame))
                        {
                            var statements = ModernDatabaseRuntimeProtocol.ReadStringList(reader);

                            ModernDatabaseRuntimeProtocol.EnsurePayloadConsumed(reader);
                            _session.ExecuteBatchNonQuery(statements);
                        }

                        WriteEmptySuccess(output, frame);
                        return true;
                    }
                case ModernDatabaseRuntimeOperation.Close:
                    {
                        EnsureEmptyRequest(frame);
                        _session.Close();
                        WriteEmptySuccess(output, frame);
                        return true;
                    }
                case ModernDatabaseRuntimeOperation.Shutdown:
                    {
                        EnsureEmptyRequest(frame);
                        _session.Close();
                        WriteEmptySuccess(output, frame);
                        return false;
                    }
                default:
                    {
                        throw new InvalidDataException("The isolated Modern DB runtime operation is not supported.");
                    }
            }
        }

        private static void WriteEmptySuccess(Stream output, ModernDatabaseRuntimeFrame frame)
        {
            ModernDatabaseRuntimeProtocol.WriteSuccessResponse(output, frame.RequestId, frame.Operation, null);
        }

        private static void EnsureEmptyRequest(ModernDatabaseRuntimeFrame frame)
        {
            if (frame.Payload.Length != 0)
            {
                throw new InvalidDataException("The isolated Modern DB runtime request payload must be empty.");
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
