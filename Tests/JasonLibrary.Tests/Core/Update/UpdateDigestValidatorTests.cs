using JasonLibrary.Core.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdateDigestValidatorTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        public void TryGetSha256_ValidDigest_ReturnsNormalizedHex()
        {
            var digest = "SHA256:" + new string('A', 64);
            var result = UpdateDigestValidator.TryGetSha256(digest, out var sha256);

            Assert.IsTrue(result);
            Assert.AreEqual(new string('a', 64), sha256);
        }

        [DataTestMethod]
        [TestCategory("Unit")]
        [TestCategory("Update")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("md5:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [DataRow("sha256:abc")]
        [DataRow("sha256:gggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggg")]
        public void TryGetSha256_InvalidDigest_ReturnsFalse(string digest)
        {
            Assert.IsFalse(UpdateDigestValidator.TryGetSha256(digest, out _));
        }
    }
}
