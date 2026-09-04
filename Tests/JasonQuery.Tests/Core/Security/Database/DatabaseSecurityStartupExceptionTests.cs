using JasonQuery.Core.Security.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class DatabaseSecurityStartupExceptionTests
    {
        [TestMethod]
        public void Constructor_PreservesErrorKindAndMessage()
        {
            var exception = new DatabaseSecurityStartupException
            (
                DatabaseSecurityStartupErrorKind.MissingSecurityInformationOrLegacyCustomPassword,
                "Test message"
            );

            Assert.AreEqual(DatabaseSecurityStartupErrorKind.MissingSecurityInformationOrLegacyCustomPassword, exception.ErrorKind);
            Assert.AreEqual("Test message", exception.Message);
        }

        [TestMethod]
        public void Constructor_WithInnerException_PreservesInnerException()
        {
            var innerException = new InvalidOperationException("Inner");

            var exception = new DatabaseSecurityStartupException
            (
                DatabaseSecurityStartupErrorKind.GeneralSecurityFailure,
                "Outer",
                innerException
            );

            Assert.AreEqual(DatabaseSecurityStartupErrorKind.GeneralSecurityFailure, exception.ErrorKind);
            Assert.AreSame(innerException, exception.InnerException);
        }
    }
}
