using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbCustomPasswordKeyDeriverTests
    {
        [TestMethod]
        public void DeriveDatabaseKey_SameInputs_ReturnSameKey()
        {
            var salt = JasonQueryDbCustomPasswordKeyDeriver.CreateSalt();
            var first = JasonQueryDbCustomPasswordKeyDeriver.DeriveDatabaseKey("JasonQuery-Test-Password", salt, 1000);
            var second = JasonQueryDbCustomPasswordKeyDeriver.DeriveDatabaseKey("JasonQuery-Test-Password", salt, 1000);

            CollectionAssert.AreEqual(first, second);
        }

        [TestMethod]
        public void DeriveDatabaseKey_DifferentPassword_ReturnsDifferentKey()
        {
            var salt = JasonQueryDbCustomPasswordKeyDeriver.CreateSalt();
            var first = JasonQueryDbCustomPasswordKeyDeriver.DeriveDatabaseKey("Password-A", salt, 1000);
            var second = JasonQueryDbCustomPasswordKeyDeriver.DeriveDatabaseKey("Password-B", salt, 1000);

            Assert.IsFalse(first.SequenceEqual(second));
        }

        [TestMethod]
        public void CreateSalt_ProducesDifferentSalts()
        {
            var first = JasonQueryDbCustomPasswordKeyDeriver.CreateSalt();
            var second = JasonQueryDbCustomPasswordKeyDeriver.CreateSalt();

            Assert.HasCount(JasonQueryDbSecurityConstants.PasswordSaltSizeBytes, first);
            Assert.HasCount(JasonQueryDbSecurityConstants.PasswordSaltSizeBytes, second);
            Assert.IsFalse(first.SequenceEqual(second));
        }
    }
}
