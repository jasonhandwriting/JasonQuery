using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace JasonQuery.Core.Database.Diagnostics.PostgreSql
{
    public static class PostgreSqlStringLengthErrorAnalyzer
    {
        public static PostgreSqlStringLengthDiagnostic TryAnalyze(string sql, string errorMessage, IPostgreSqlColumnLengthMetadataProvider metadataProvider)
        {
            if (metadataProvider == null)
            {
                return PostgreSqlStringLengthDiagnostic.Unknown("尚未建立欄位長度 metadata provider。");
            }

            if (PostgreSqlInsertValuesParser.TryParse(sql, out var insertParseResult))
            {
                return AnalyzeInsert(insertParseResult, errorMessage, metadataProvider);
            }

            if (PostgreSqlUpdateSetParser.TryParse(sql, out var updateParseResult))
            {
                return AnalyzeUpdate(updateParseResult, errorMessage, metadataProvider);
            }

            var reason = new StringBuilder();

            reason.AppendLine("目前只支援以下語法：");
            reason.AppendLine("1. INSERT INTO table (columns) VALUES (...)");
            reason.AppendLine("2. UPDATE table SET column = '...'");

            if (!string.IsNullOrWhiteSpace(insertParseResult.FailureReason))
            {
                reason.AppendLine();
                reason.AppendLine($"INSERT 解析結果：{insertParseResult.FailureReason}");
            }

            if (!string.IsNullOrWhiteSpace(updateParseResult.FailureReason))
            {
                reason.AppendLine($"UPDATE 解析結果：{updateParseResult.FailureReason}");
            }

            return PostgreSqlStringLengthDiagnostic.Unknown(reason.ToString().TrimEnd());
        }

        private static List<PostgreSqlStringLengthCandidate> FindExactInsertCandidates(PostgreSqlInsertValuesParseResult parseResult,
                                                                                       IReadOnlyList<PostgreSqlColumnLengthInfo> columnInfos)
        {
            var result = new List<PostgreSqlStringLengthCandidate>();

            foreach (var valueInfo in parseResult.Values)
            {
                if (!valueInfo.IsStringLiteral)
                {
                    continue;
                }

                if (valueInfo.ColumnIndex < 0 || valueInfo.ColumnIndex >= parseResult.ColumnNames.Count)
                {
                    continue;
                }

                var columnName = parseResult.ColumnNames[valueInfo.ColumnIndex];
                var columnInfo = FindColumnInfo(columnInfos, columnName);

                if (columnInfo == null || !columnInfo.CharacterMaximumLength.HasValue)
                {
                    continue;
                }

                if (valueInfo.ActualLength <= columnInfo.CharacterMaximumLength.Value)
                {
                    continue;
                }

                result.Add(CreateCandidate(columnInfo, valueInfo));
            }

            return result;
        }

        private static List<PostgreSqlStringLengthCandidate> FindExactUpdateCandidates(PostgreSqlUpdateSetParseResult parseResult,
                                                                                       IReadOnlyList<PostgreSqlColumnLengthInfo> columnInfos)
        {
            var result = new List<PostgreSqlStringLengthCandidate>();

            foreach (var valueInfo in parseResult.SetValues)
            {
                if (!valueInfo.IsStringLiteral)
                {
                    continue;
                }

                var columnInfo = FindColumnInfo(columnInfos, valueInfo.ColumnName);

                if (columnInfo == null || !columnInfo.CharacterMaximumLength.HasValue)
                {
                    continue;
                }

                if (valueInfo.ActualLength <= columnInfo.CharacterMaximumLength.Value)
                {
                    continue;
                }

                result.Add(CreateCandidate(columnInfo, valueInfo));
            }

            return result;
        }

        private static List<PostgreSqlStringLengthCandidate> FindFallbackInsertCandidates(PostgreSqlInsertValuesParseResult parseResult,
                                                                                          IReadOnlyList<PostgreSqlColumnLengthInfo> columnInfos,
                                                                                          int? lengthInErrorMessage)
        {
            var result = new List<PostgreSqlStringLengthCandidate>();

            if (!lengthInErrorMessage.HasValue)
            {
                return result;
            }

            foreach (var columnName in parseResult.ColumnNames)
            {
                var columnInfo = FindColumnInfo(columnInfos, columnName);

                if (columnInfo == null || !columnInfo.CharacterMaximumLength.HasValue)
                {
                    continue;
                }

                if (columnInfo.CharacterMaximumLength.Value != lengthInErrorMessage.Value)
                {
                    continue;
                }

                result.Add(new PostgreSqlStringLengthCandidate
                {
                    SchemaName = columnInfo.SchemaName,
                    TableName = columnInfo.TableName,
                    ColumnName = columnInfo.ColumnName,
                    DataType = columnInfo.DataType,
                    MaxLength = columnInfo.CharacterMaximumLength
                });
            }

            return result;
        }

        private static List<PostgreSqlStringLengthCandidate> FindFallbackUpdateCandidates(PostgreSqlUpdateSetParseResult parseResult,
                                                                                          IReadOnlyList<PostgreSqlColumnLengthInfo> columnInfos,
                                                                                          int? lengthInErrorMessage)
        {
            var result = new List<PostgreSqlStringLengthCandidate>();

            if (!lengthInErrorMessage.HasValue)
            {
                return result;
            }

            foreach (var valueInfo in parseResult.SetValues)
            {
                var columnInfo = FindColumnInfo(columnInfos, valueInfo.ColumnName);

                if (columnInfo == null || !columnInfo.CharacterMaximumLength.HasValue)
                {
                    continue;
                }

                if (columnInfo.CharacterMaximumLength.Value != lengthInErrorMessage.Value)
                {
                    continue;
                }

                result.Add(new PostgreSqlStringLengthCandidate
                {
                    SchemaName = columnInfo.SchemaName,
                    TableName = columnInfo.TableName,
                    ColumnName = columnInfo.ColumnName,
                    DataType = columnInfo.DataType,
                    MaxLength = columnInfo.CharacterMaximumLength
                });
            }

            return result;
        }

        private static PostgreSqlColumnLengthInfo FindColumnInfo(IReadOnlyList<PostgreSqlColumnLengthInfo> columnInfos, string columnName)
        {
            foreach (var columnInfo in columnInfos)
            {
                if (string.Equals(columnInfo.ColumnName, columnName, StringComparison.Ordinal))
                {
                    return columnInfo;
                }
            }

            return null;
        }

        private static PostgreSqlStringLengthCandidate CreateCandidate(PostgreSqlColumnLengthInfo columnInfo, PostgreSqlInsertValueInfo valueInfo)
        {
            return new PostgreSqlStringLengthCandidate
            {
                SchemaName = columnInfo.SchemaName,
                TableName = columnInfo.TableName,
                ColumnName = columnInfo.ColumnName,
                DataType = columnInfo.DataType,
                MaxLength = columnInfo.CharacterMaximumLength,
                ActualLength = valueInfo.ActualLength,
                ValuesRowIndex = valueInfo.ValuesRowIndex,
                SourceValuePreview = BuildValuePreview(valueInfo.StringValue)
            };
        }

        private static PostgreSqlStringLengthCandidate CreateCandidate(PostgreSqlColumnLengthInfo columnInfo, PostgreSqlUpdateSetValueInfo valueInfo)
        {
            return new PostgreSqlStringLengthCandidate
            {
                SchemaName = columnInfo.SchemaName,
                TableName = columnInfo.TableName,
                ColumnName = columnInfo.ColumnName,
                DataType = columnInfo.DataType,
                MaxLength = columnInfo.CharacterMaximumLength,
                ActualLength = valueInfo.ActualLength,
                SourceValuePreview = BuildValuePreview(valueInfo.StringValue)
            };
        }

        private static PostgreSqlStringLengthDiagnostic BuildExactDiagnostic(PostgreSqlStringLengthCandidate candidate)
        {
            var diagnostic = new PostgreSqlStringLengthDiagnostic
            {
                Confidence = PostgreSqlStringLengthDiagnosticConfidence.Exact
            };

            diagnostic.Candidates.Add(candidate);

            var sb = new StringBuilder();

            sb.AppendLine(PostgreSqlStringLengthDiagnosticText.Title);
            sb.AppendLine(PostgreSqlStringLengthDiagnosticText.ExactDescription);
            sb.AppendLine();
            sb.AppendLine($"{PostgreSqlStringLengthDiagnosticText.EstimatedColumnLabel}: {candidate.FullColumnName}");

            if (candidate.ValuesRowIndex.HasValue && candidate.ValuesRowIndex.Value > 1)
            {
                sb.AppendLine($"{PostgreSqlStringLengthDiagnosticText.ValuesRowLabel}: {PostgreSqlStringLengthDiagnosticText.BuildValuesRowText(candidate.ValuesRowIndex.Value)}");
            }

            sb.AppendLine($"{PostgreSqlStringLengthDiagnosticText.DataTypeLabel}: {candidate.DisplayDataType}");

            if (candidate.ActualLength.HasValue)
            {
                sb.AppendLine($"{PostgreSqlStringLengthDiagnosticText.ActualLengthLabel}: {candidate.ActualLength.Value} {PostgreSqlStringLengthDiagnosticText.CharactersUnit}");
            }

            if (candidate.MaxLength.HasValue)
            {
                sb.AppendLine($"{PostgreSqlStringLengthDiagnosticText.MaxLengthLabel}: {candidate.MaxLength.Value} {PostgreSqlStringLengthDiagnosticText.CharactersUnit}");
            }

            if (!string.IsNullOrEmpty(candidate.SourceValuePreview))
            {
                sb.AppendLine($"{PostgreSqlStringLengthDiagnosticText.SourceValuePreviewLabel}: {candidate.SourceValuePreview}");
            }

            sb.Append(PostgreSqlStringLengthDiagnosticText.ExactConfidenceLine);

            diagnostic.Message = sb.ToString();

            return diagnostic;
        }

        private static PostgreSqlStringLengthDiagnostic BuildMultipleCandidateDiagnostic(List<PostgreSqlStringLengthCandidate> candidates)
        {
            var diagnostic = new PostgreSqlStringLengthDiagnostic
            {
                Confidence = PostgreSqlStringLengthDiagnosticConfidence.Candidate
            };

            foreach (var candidate in candidates)
            {
                diagnostic.Candidates.Add(candidate);
            }

            var sb = new StringBuilder();

            sb.AppendLine(PostgreSqlStringLengthDiagnosticText.Title);
            sb.AppendLine(PostgreSqlStringLengthDiagnosticText.MultipleCandidateDescription);
            sb.AppendLine();

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];

                var valuesRowText = candidate.ValuesRowIndex.HasValue
                    ? PostgreSqlStringLengthDiagnosticText.BuildCandidateValuesRowPrefix(candidate.ValuesRowIndex.Value)
                    : string.Empty;

                sb.Append($"{i + 1}. {valuesRowText}{candidate.FullColumnName}：{candidate.DisplayDataType}");

                if (candidate.ActualLength.HasValue && candidate.MaxLength.HasValue)
                {
                    sb.Append($"，{PostgreSqlStringLengthDiagnosticText.ActualLengthLabel} {candidate.ActualLength.Value}，{PostgreSqlStringLengthDiagnosticText.MaxLengthLabel} {candidate.MaxLength.Value}");
                }

                sb.AppendLine();
            }

            sb.AppendLine();
            sb.Append(PostgreSqlStringLengthDiagnosticText.CandidateConfidenceLine);

            diagnostic.Message = sb.ToString();

            return diagnostic;
        }

        private static PostgreSqlStringLengthDiagnostic BuildFallbackCandidateDiagnostic(List<PostgreSqlStringLengthCandidate> candidates)
        {
            var diagnostic = new PostgreSqlStringLengthDiagnostic
            {
                Confidence = PostgreSqlStringLengthDiagnosticConfidence.Candidate
            };

            foreach (var candidate in candidates)
            {
                diagnostic.Candidates.Add(candidate);
            }

            var sb = new StringBuilder();

            sb.AppendLine(PostgreSqlStringLengthDiagnosticText.Title);
            sb.AppendLine(PostgreSqlStringLengthDiagnosticText.FallbackCandidateDescription);
            sb.AppendLine(PostgreSqlStringLengthDiagnosticText.FallbackCandidateHint);
            sb.AppendLine();

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];

                sb.AppendLine($"{i + 1}. {candidate.FullColumnName}：{candidate.DisplayDataType}");
            }

            sb.AppendLine();
            sb.Append(PostgreSqlStringLengthDiagnosticText.CandidateConfidenceLine);

            diagnostic.Message = sb.ToString();

            return diagnostic;
        }

        private static int? TryExtractLengthFromErrorMessage(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return null;
            }

            var match = Regex.Match(errorMessage, @"character\s+varying\s*\((\d+)\)", RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                match = Regex.Match(errorMessage, @"character\s*\((\d+)\)", RegexOptions.IgnoreCase);
            }

            if (!match.Success)
            {
                return null;
            }

            if (int.TryParse(match.Groups[1].Value, out var length))
            {
                return length;
            }

            return null;
        }

        private static string BuildValuePreview(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            const int maxLength = 4000;

            if (value.Length <= maxLength)
            {
                return value;
            }

            return $"{value.Substring(0, maxLength)}...";
        }

        private static PostgreSqlStringLengthDiagnostic AnalyzeInsert(PostgreSqlInsertValuesParseResult parseResult,
                                                                      string errorMessage, IPostgreSqlColumnLengthMetadataProvider metadataProvider)
        {
            IReadOnlyList<PostgreSqlColumnLengthInfo> columnInfos;

            try
            {
                columnInfos = metadataProvider.GetColumnLengthInfo(parseResult.SchemaName, parseResult.TableName);
            }
            catch (Exception ex)
            {
                return PostgreSqlStringLengthDiagnostic.Unknown($"取得欄位 metadata 失敗：{ex.Message}");
            }

            if (columnInfos == null || columnInfos.Count == 0)
            {
                return PostgreSqlStringLengthDiagnostic.Unknown($"找不到 {parseResult.FullTableName} 的 character varying / character 欄位 metadata。");
            }

            var exactCandidates = FindExactInsertCandidates(parseResult, columnInfos);

            if (exactCandidates.Count == 1)
            {
                return BuildExactDiagnostic(exactCandidates[0]);
            }

            if (exactCandidates.Count > 1)
            {
                return BuildMultipleCandidateDiagnostic(exactCandidates);
            }

            var lengthInErrorMessage = TryExtractLengthFromErrorMessage(errorMessage);
            var fallbackCandidates = FindFallbackInsertCandidates(parseResult, columnInfos, lengthInErrorMessage);

            if (fallbackCandidates.Count > 0)
            {
                return BuildFallbackCandidateDiagnostic(fallbackCandidates);
            }

            return PostgreSqlStringLengthDiagnostic.Unknown("已解析 INSERT 語法，但沒有找到超過欄位長度限制的 string literal。");
        }

        private static PostgreSqlStringLengthDiagnostic AnalyzeUpdate(PostgreSqlUpdateSetParseResult parseResult,
                                                                      string errorMessage, IPostgreSqlColumnLengthMetadataProvider metadataProvider)
        {
            IReadOnlyList<PostgreSqlColumnLengthInfo> columnInfos;

            try
            {
                columnInfos = metadataProvider.GetColumnLengthInfo(parseResult.SchemaName, parseResult.TableName);
            }
            catch (Exception ex)
            {
                return PostgreSqlStringLengthDiagnostic.Unknown($"取得欄位 metadata 失敗：{ex.Message}");
            }

            if (columnInfos == null || columnInfos.Count == 0)
            {
                return PostgreSqlStringLengthDiagnostic.Unknown($"找不到 {parseResult.FullTableName} 的 character varying / character 欄位 metadata。");
            }

            var exactCandidates = FindExactUpdateCandidates(parseResult, columnInfos);

            if (exactCandidates.Count == 1)
            {
                return BuildExactDiagnostic(exactCandidates[0]);
            }

            if (exactCandidates.Count > 1)
            {
                return BuildMultipleCandidateDiagnostic(exactCandidates);
            }

            var lengthInErrorMessage = TryExtractLengthFromErrorMessage(errorMessage);
            var fallbackCandidates = FindFallbackUpdateCandidates(parseResult, columnInfos, lengthInErrorMessage);

            if (fallbackCandidates.Count > 0)
            {
                return BuildFallbackCandidateDiagnostic(fallbackCandidates);
            }

            return PostgreSqlStringLengthDiagnostic.Unknown("已解析 UPDATE 語法，但沒有找到超過欄位長度限制的 string literal。");
        }
    }
}