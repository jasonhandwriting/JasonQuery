using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbSecurityStartupExceptionTests
    {
        [TestMethod]
        public void Constructor_PreservesErrorKindAndMessage()
        {
            var exception = new JasonQueryDbSecurityStartupException
            (
                JasonQueryDbSecurityStartupErrorKind.MissingSecurityInformationOrLegacyCustomPassword,
                "Test message"
            );

            Assert.AreEqual(JasonQueryDbSecurityStartupErrorKind.MissingSecurityInformationOrLegacyCustomPassword, exception.ErrorKind);
            Assert.AreEqual("Test message", exception.Message);
        }

        [TestMethod]
        public void Constructor_WithInnerException_PreservesInnerException()
        {
            var innerException = new InvalidOperationException("Inner");

            var exception = new JasonQueryDbSecurityStartupException
            (
                JasonQueryDbSecurityStartupErrorKind.GeneralSecurityFailure,
                "Outer",
                innerException
            );

            Assert.AreEqual(JasonQueryDbSecurityStartupErrorKind.GeneralSecurityFailure, exception.ErrorKind);
            Assert.AreSame(innerException, exception.InnerException);
        }
    }
}
