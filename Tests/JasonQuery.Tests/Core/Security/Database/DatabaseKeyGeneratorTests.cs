using JasonQuery.Core.Security.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class DatabaseKeyGeneratorTests
    {
        [TestMethod]
        public void Generate_ReturnsExpectedKeyLength()
        {
            var databaseKey = DatabaseKeyGenerator.Generate();

            Assert.AreEqual(DatabaseSecurityConstants.DatabaseKeySizeBytes, databaseKey.Length);
        }

        [TestMethod]
        public void Generate_ProducesDifferentKeys()
        {
            var first = DatabaseKeyGenerator.Generate();
            var second = DatabaseKeyGenerator.Generate();

            Assert.IsFalse(first.SequenceEqual(second));
        }

        [TestMethod]
        public void ToDatabasePassword_RoundTripsKeyBytes()
        {
            var databaseKey = DatabaseKeyGenerator.Generate();
            var databasePassword = DatabaseKeyGenerator.ToDatabasePassword(databaseKey);

            CollectionAssert.AreEqual(databaseKey, Convert.FromBase64String(databasePassword));
        }
    }
}
