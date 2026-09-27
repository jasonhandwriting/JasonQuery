using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbLegacySecurityTests
    {
        [TestMethod]
        public void IsDefaultDatabasePassword_ReturnsTrueForHistoricalCredential()
        {
            Assert.IsTrue(JasonQueryDbLegacySecurity.IsDefaultDatabasePassword("ytec1688"));
        }

        [TestMethod]
        public void IsDefaultDatabasePassword_ReturnsFalseForCustomCredential()
        {
            Assert.IsFalse(JasonQueryDbLegacySecurity.IsDefaultDatabasePassword("custom"));
        }

        [TestMethod]
        public void CreateCustomDatabasePassword_UsesHistoricalFormat()
        {
            var result = JasonQueryDbLegacySecurity.CreateCustomDatabasePassword("Abc123!");

            Assert.AreEqual("jasonquery1231Abc123!encryptDB!nf0", result);
        }

        [TestMethod]
        public void IsCustomDatabasePasswordMatch_ReturnsTrueForMatchingPassword()
        {
            var databasePassword = JasonQueryDbLegacySecurity.CreateCustomDatabasePassword("Abc123!");

            Assert.IsTrue(JasonQueryDbLegacySecurity.IsCustomDatabasePasswordMatch(databasePassword, "Abc123!"));
        }

        [TestMethod]
        public void IsCustomDatabasePasswordMatch_ReturnsFalseForWrongPassword()
        {
            var databasePassword = JasonQueryDbLegacySecurity.CreateCustomDatabasePassword("Abc123!");

            Assert.IsFalse(JasonQueryDbLegacySecurity.IsCustomDatabasePasswordMatch(databasePassword, "Wrong123!"));
        }

        [TestMethod]
        public void IsCustomDatabasePasswordMatch_ReturnsFalseForEmptyPassword()
        {
            var databasePassword = JasonQueryDbLegacySecurity.CreateCustomDatabasePassword("Abc123!");

            Assert.IsFalse(JasonQueryDbLegacySecurity.IsCustomDatabasePasswordMatch(databasePassword, string.Empty));
        }

        [TestMethod]
        public void CreateCustomDatabasePassword_ThrowsForEmptyPassword()
        {
            Assert.ThrowsExactly<ArgumentException>(() => JasonQueryDbLegacySecurity.CreateCustomDatabasePassword(string.Empty));
        }
    }
}
