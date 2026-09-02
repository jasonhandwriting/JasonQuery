using JasonQuery.Core.Security.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class LegacyDatabaseSecurityTests
    {
        [TestMethod]
        public void IsDefaultDatabasePassword_ReturnsTrueForHistoricalCredential()
        {
            Assert.IsTrue(LegacyDatabaseSecurity.IsDefaultDatabasePassword("ytec1688"));
        }

        [TestMethod]
        public void IsDefaultDatabasePassword_ReturnsFalseForCustomCredential()
        {
            Assert.IsFalse(LegacyDatabaseSecurity.IsDefaultDatabasePassword("custom"));
        }

        [TestMethod]
        public void CreateCustomDatabasePassword_UsesHistoricalFormat()
        {
            var result = LegacyDatabaseSecurity.CreateCustomDatabasePassword("Abc123!");

            Assert.AreEqual("jasonquery1231Abc123!encryptDB!nf0", result);
        }

        [TestMethod]
        public void IsCustomDatabasePasswordMatch_ReturnsTrueForMatchingPassword()
        {
            var databasePassword = LegacyDatabaseSecurity.CreateCustomDatabasePassword("Abc123!");

            Assert.IsTrue(LegacyDatabaseSecurity.IsCustomDatabasePasswordMatch(databasePassword, "Abc123!"));
        }

        [TestMethod]
        public void IsCustomDatabasePasswordMatch_ReturnsFalseForWrongPassword()
        {
            var databasePassword = LegacyDatabaseSecurity.CreateCustomDatabasePassword("Abc123!");

            Assert.IsFalse(LegacyDatabaseSecurity.IsCustomDatabasePasswordMatch(databasePassword, "Wrong123!"));
        }

        [TestMethod]
        public void IsCustomDatabasePasswordMatch_ReturnsFalseForEmptyPassword()
        {
            var databasePassword = LegacyDatabaseSecurity.CreateCustomDatabasePassword("Abc123!");

            Assert.IsFalse(LegacyDatabaseSecurity.IsCustomDatabasePasswordMatch(databasePassword, string.Empty));
        }

        [TestMethod]
        public void CreateCustomDatabasePassword_ThrowsForEmptyPassword()
        {
            Assert.ThrowsException<ArgumentException>(() => LegacyDatabaseSecurity.CreateCustomDatabasePassword(string.Empty));
        }
    }
}
