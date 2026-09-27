using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbKeyGeneratorTests
    {
        [TestMethod]
        public void Generate_ReturnsExpectedKeyLength()
        {
            var databaseKey = JasonQueryDbKeyGenerator.Generate();

            Assert.HasCount(JasonQueryDbSecurityConstants.DatabaseKeySizeBytes, databaseKey);
        }

        [TestMethod]
        public void Generate_ProducesDifferentKeys()
        {
            var first = JasonQueryDbKeyGenerator.Generate();
            var second = JasonQueryDbKeyGenerator.Generate();

            Assert.IsFalse(first.SequenceEqual(second));
        }

        [TestMethod]
        public void ToDatabasePassword_RoundTripsKeyBytes()
        {
            var databaseKey = JasonQueryDbKeyGenerator.Generate();
            var databasePassword = JasonQueryDbKeyGenerator.ToDatabasePassword(databaseKey);

            CollectionAssert.AreEqual(databaseKey, Convert.FromBase64String(databasePassword));
        }
    }
}
