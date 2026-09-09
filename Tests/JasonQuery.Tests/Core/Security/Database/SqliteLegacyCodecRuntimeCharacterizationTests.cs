using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SQLite;
using System.Reflection;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class SqliteLegacyCodecRuntimeCharacterizationTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Safety")]
        public void CurrentRuntime_MatchesCharacterizedLegacyBaseline()
        {
            var assemblyVersion = typeof(SQLiteConnection).Assembly.GetName().Version.ToString();
            var sqliteVersion = GetRequiredStaticStringProperty("SQLiteVersion");
            var interopVersion = GetRequiredStaticStringProperty("InteropVersion");

            Assert.AreEqual("1.0.112.0", assemblyVersion);
            Assert.AreEqual("3.30.1", sqliteVersion);
            Assert.AreEqual("1.0.112.0", interopVersion);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Safety")]
        public void CurrentRuntime_ExposesLegacyPasswordApis()
        {
            var connectionType = typeof(SQLiteConnection);

            var setPassword = connectionType.GetMethod
            (
                "SetPassword",
                new[]
                {
                    typeof(string)
                }
            );

            var changePassword = connectionType.GetMethod
            (
                "ChangePassword",
                new[]
                {
                    typeof(string)
                }
            );

            Assert.IsNotNull
            (
                setPassword,
                "The characterized legacy runtime must expose SetPassword(string)."
            );

            Assert.IsNotNull
            (
                changePassword,
                "The characterized legacy runtime must expose ChangePassword(string)."
            );
        }

        private static string GetRequiredStaticStringProperty(string propertyName)
        {
            var property = typeof(SQLiteConnection).GetProperty
            (
                propertyName,
                BindingFlags.Public | BindingFlags.Static
            );

            Assert.IsNotNull(property, $"SQLiteConnection.{propertyName} was not found.");

            var value = property.GetValue(null, null) as string;

            Assert.IsFalse
            (
                string.IsNullOrWhiteSpace(value),
                $"SQLiteConnection.{propertyName} returned no value."
            );

            return value;
        }
    }
}
