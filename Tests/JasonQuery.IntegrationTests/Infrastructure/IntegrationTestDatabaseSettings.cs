using JasonQuery.Core.Database.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal sealed class IntegrationTestDatabaseSettings
    {
        public DataSourceType SourceType { get; private set; }
        public bool Enabled { get; private set; }
        public string ConnectionString { get; private set; } = string.Empty;
        public string Server { get; private set; } = string.Empty;
        public int Port { get; private set; }
        public string UserName { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public string DatabaseName { get; private set; } = string.Empty;
        public string SchemaName { get; private set; } = string.Empty;
        public string OracleSid { get; private set; } = string.Empty;
        public string OracleConnectAs { get; private set; } = string.Empty;
        public bool UseDirectMode { get; private set; }
        public bool UseConnectionPooling { get; private set; }
        public bool UseUnicode { get; private set; }
        public int QueryTimeoutSeconds { get; private set; } = 30;

        public static IntegrationTestDatabaseSettings Load(TestContext testContext, DataSourceType sourceType)
        {
            var prefix = GetPrefix(sourceType);

            var settings = new IntegrationTestDatabaseSettings
            {
                SourceType = sourceType,
                Enabled = IntegrationTestParameterReader.GetBoolean(testContext, $"{prefix}.Enabled", false),
                ConnectionString = IntegrationTestParameterReader.GetString(testContext, $"{prefix}.ConnectionString"),
                Server = IntegrationTestParameterReader.GetString(testContext, $"{prefix}.Server"),
                Port = IntegrationTestParameterReader.GetInt32(testContext, $"{prefix}.Port", GetDefaultPort(sourceType)),
                UserName = IntegrationTestParameterReader.GetString(testContext, $"{prefix}.User"),
                Password = IntegrationTestParameterReader.GetString(testContext, $"{prefix}.Password"),
                DatabaseName = IntegrationTestParameterReader.GetString(testContext, $"{prefix}.Database"),
                SchemaName = IntegrationTestParameterReader.GetString(testContext, $"{prefix}.Schema"),
                OracleSid = IntegrationTestParameterReader.GetString(testContext, $"{prefix}.Sid"),
                OracleConnectAs = IntegrationTestParameterReader.GetString(testContext, $"{prefix}.ConnectAs"),
                UseDirectMode = IntegrationTestParameterReader.GetBoolean(testContext, $"{prefix}.DirectMode", true),
                UseConnectionPooling = IntegrationTestParameterReader.GetBoolean(testContext, $"{prefix}.Pooling", false),
                UseUnicode = IntegrationTestParameterReader.GetBoolean(testContext, $"{prefix}.Unicode", true),
                QueryTimeoutSeconds = IntegrationTestParameterReader.GetInt32(testContext, $"{prefix}.QueryTimeoutSeconds", 30)
            };

            settings.ApplyDefaults();

            return settings;
        }

        public void RequireConfigured()
        {
            if (!Enabled)
            {
                Assert.Inconclusive($"{SourceType} integration tests are disabled. Enable them in JasonQuery.IntegrationTests.runsettings.");
            }

            var missing = GetMissingRequiredSettings();

            if (missing.Count > 0)
            {
                Assert.Inconclusive($"{SourceType} integration tests are missing required settings: {string.Join(", ", missing)}.");
            }
        }

        private void ApplyDefaults()
        {
            QueryTimeoutSeconds = QueryTimeoutSeconds <= 0 ? 30 : QueryTimeoutSeconds;

            switch (SourceType)
            {
                case DataSourceType.Oracle:
                    {
                        SchemaName = string.IsNullOrWhiteSpace(SchemaName) ? (UserName ?? string.Empty).ToUpperInvariant() : SchemaName;
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        SchemaName = string.IsNullOrWhiteSpace(SchemaName) ? "public" : SchemaName;
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        SchemaName = string.IsNullOrWhiteSpace(SchemaName) ? "dbo" : SchemaName;
                        break;
                    }
            }
        }

        private List<string> GetMissingRequiredSettings()
        {
            var missing = new List<string>();

            if (SourceType == DataSourceType.Oracle)
            {
                AddIfMissing(missing, Server, "Server");
                AddIfMissing(missing, UserName, "User");
                AddIfMissing(missing, Password, "Password");
                AddIfMissing(missing, OracleSid, "Sid");

                if (Port <= 0)
                {
                    missing.Add("Port");
                }

                return missing;
            }

            AddIfMissing(missing, ConnectionString, "ConnectionString");

            return missing;
        }

        private static void AddIfMissing(ICollection<string> missing, string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                missing.Add(name);
            }
        }

        private static string GetPrefix(DataSourceType sourceType)
        {
            switch (sourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return "Oracle";
                    }
                case DataSourceType.PostgreSql:
                    {
                        return "PostgreSql";
                    }
                case DataSourceType.SqlServer:
                    {
                        return "SqlServer";
                    }
                case DataSourceType.MySql:
                    {
                        return "MySql";
                    }
                default:
                    {
                        throw new NotSupportedException($"Unsupported data source type: {sourceType}");
                    }
            }
        }

        private static int GetDefaultPort(DataSourceType sourceType)
        {
            switch (sourceType)
            {
                case DataSourceType.Oracle:
                    {
                        return 1521;
                    }
                case DataSourceType.PostgreSql:
                    {
                        return 5432;
                    }
                case DataSourceType.SqlServer:
                    {
                        return 1433;
                    }
                case DataSourceType.MySql:
                    {
                        return 3306;
                    }
                default:
                    {
                        return 0;
                    }
            }
        }
    }

    internal static class IntegrationTestParameterReader
    {
        public static string GetString(TestContext testContext, string name)
        {
            var environmentName = $"JQ_IT_{name.Replace(".", "_").ToUpperInvariant()}";
            var environmentValue = Environment.GetEnvironmentVariable(environmentName);

            if (!string.IsNullOrWhiteSpace(environmentValue))
            {
                return environmentValue.Trim();
            }

            if (testContext?.Properties == null || !testContext.Properties.Contains(name))
            {
                return string.Empty;
            }

            return Convert.ToString(testContext.Properties[name], CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
        }

        public static bool GetBoolean(TestContext testContext, string name, bool defaultValue)
        {
            var value = GetString(testContext, name);

            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            if (bool.TryParse(value, out var parsed))
            {
                return parsed;
            }

            return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(value, "y", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(value, "on", StringComparison.OrdinalIgnoreCase);
        }

        public static int GetInt32(TestContext testContext, string name, int defaultValue)
        {
            var value = GetString(testContext, name);

            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : defaultValue;
        }
    }
}