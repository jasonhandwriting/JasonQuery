using JasonLibrary.Core.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdateChannelResolverTests
    {
        [TestMethod]
        [DataRow("0.94.0")]
        [DataRow("v0.95.0")]
        [DataRow("1.0.0")]
        [DataRow("0.92")]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void Resolve_ThirdPartIsZero_ReturnsProduction(string version)
        {
            Assert.AreEqual(UpdateChannel.Production, UpdateChannelResolver.Resolve(version));
        }

        [TestMethod]
        [DataRow("0.94.1")]
        [DataRow("v0.95.2")]
        [DataRow("1.0.9")]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void Resolve_ThirdPartIsNonZero_ReturnsTest(string version)
        {
            Assert.AreEqual(UpdateChannel.Test, UpdateChannelResolver.Resolve(version));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void Resolve_InvalidVersion_ThrowsFormatException()
        {
            Assert.ThrowsExactly<FormatException>(() => UpdateChannelResolver.Resolve("0.94.test"));
        }
    }
}
