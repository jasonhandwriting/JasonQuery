using JasonQuery.Core.Security.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class CustomPasswordDatabaseKeyDeriverTests
    {
        [TestMethod]
        public void DeriveDatabaseKey_SameInputs_ReturnSameKey()
        {
            var salt = CustomPasswordDatabaseKeyDeriver.CreateSalt();
            var first = CustomPasswordDatabaseKeyDeriver.DeriveDatabaseKey("JasonQuery-Test-Password", salt, 1000);
            var second = CustomPasswordDatabaseKeyDeriver.DeriveDatabaseKey("JasonQuery-Test-Password", salt, 1000);

            CollectionAssert.AreEqual(first, second);
        }

        [TestMethod]
        public void DeriveDatabaseKey_DifferentPassword_ReturnsDifferentKey()
        {
            var salt = CustomPasswordDatabaseKeyDeriver.CreateSalt();
            var first = CustomPasswordDatabaseKeyDeriver.DeriveDatabaseKey("Password-A", salt, 1000);
            var second = CustomPasswordDatabaseKeyDeriver.DeriveDatabaseKey("Password-B", salt, 1000);

            Assert.IsFalse(first.SequenceEqual(second));
        }

        [TestMethod]
        public void CreateSalt_ProducesDifferentSalts()
        {
            var first = CustomPasswordDatabaseKeyDeriver.CreateSalt();
            var second = CustomPasswordDatabaseKeyDeriver.CreateSalt();

            Assert.AreEqual(DatabaseSecurityConstants.PasswordSaltSizeBytes, first.Length);
            Assert.AreEqual(DatabaseSecurityConstants.PasswordSaltSizeBytes, second.Length);
            Assert.IsFalse(first.SequenceEqual(second));
        }
    }
}
