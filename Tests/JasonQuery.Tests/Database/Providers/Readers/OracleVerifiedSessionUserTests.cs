using JasonQuery.Database.Providers.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.Database.Providers.Readers
{
    [TestClass]
    public sealed class OracleVerifiedSessionUserTests
    {
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

        [TestMethod]
        [TestCategory("D1B3F02B1")]
        public void F02B1_G01_ReaderHasVerifiedSessionUserReadOnlyContract()
        {
            var property = typeof(OracleReader).GetProperty("VerifiedSessionUser", PublicInstance);

            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(string), property.PropertyType);
            Assert.IsNotNull(property.GetGetMethod());
            Assert.IsNull(property.GetSetMethod());
        }

        [TestMethod]
        [TestCategory("D1B3F02B1")]
        public void F02B1_G02_UnconnectedReaderReturnsNoTrustedOwner()
        {
            var property = typeof(OracleReader).GetProperty("VerifiedSessionUser", PublicInstance);

            Assert.IsNotNull(property);
            Assert.AreEqual(string.Empty, Convert.ToString(property.GetValue(new OracleReader())));
        }

        [TestMethod]
        [TestCategory("D1B3F02B1")]
        public void F02B1_G03_DisconnectInvalidatesStoredIdentity()
        {
            var field = typeof(OracleReader).GetField("_verifiedSessionUser", PrivateInstance);

            Assert.IsNotNull(field);

            var reader = new OracleReader();
            field.SetValue(reader, "MiXeD");

            Assert.IsTrue(reader.Disconnect());
            Assert.AreEqual(string.Empty, Convert.ToString(field.GetValue(reader)));
        }

        [TestMethod]
        [TestCategory("D1B3F02B1")]
        public void F02B1_G04_IdentityProbeIsSessionUserNotCurrentSchema()
        {
            var field = typeof(OracleReader).GetField("SessionScopeSql", PrivateStatic);

            Assert.IsNotNull(field);
            Assert.IsTrue(field.IsLiteral);

            var sql = Convert.ToString(field.GetRawConstantValue());

            Assert.IsTrue(sql.Contains("SYS_CONTEXT('USERENV','SESSION_USER')"));
            Assert.IsTrue(sql.Contains("SYS_CONTEXT('USERENV','DB_NAME')"));
            Assert.IsTrue(sql.Contains("SYS_CONTEXT('USERENV','CON_NAME')"));
            Assert.IsFalse(sql.Contains("CURRENT_SCHEMA"));
        }

        [TestMethod]
        [TestCategory("D1B3F02B1")]
        public void F02B1_G05_ExistingConnectionStateContractRemainsPresent()
        {
            var method = typeof(OracleReader).GetMethod("GetState", PublicInstance);

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(System.Data.ConnectionState), method.ReturnType);
        }
    }
}
