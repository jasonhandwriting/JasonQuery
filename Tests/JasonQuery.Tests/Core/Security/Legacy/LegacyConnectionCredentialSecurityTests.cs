using JasonQuery.Core.Security.Legacy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Security.Legacy
{
    [TestClass]
    public class LegacyConnectionCredentialSecurityTests
    {
        private const string HistoricalDomainUser = @"TESTDOMAIN\JasonQueryLegacyUser";

        [TestMethod]
        public void Protect_MatchesHistoricalGoldenVectors()
        {
            AssertProtectVector("P@ssw0rd!", "EVNrsBQeWpYv3BHJObziNc8dg+ZTCyB3PQiAQNm3FFQ=");
            AssertProtectVector("Oracle#2026-Test!", "acn/I6TL1iNjj8u0POU6Zw5bOOa03dZg9kCNrl+drmabCCMW+WM8hw==");
            AssertProtectVector("Unicode-資料庫-密碼-2026!", "mFI85334y24aV0AMiM206pLBeMHNxmVZfaas8QOXCS2sDgbB3lSD6y/lchTf3uTe");
        }

        [TestMethod]
        public void Unprotect_RecoversHistoricalGoldenVectors()
        {
            AssertUnprotectVector("EVNrsBQeWpYv3BHJObziNc8dg+ZTCyB3PQiAQNm3FFQ=", "P@ssw0rd!");
            AssertUnprotectVector("acn/I6TL1iNjj8u0POU6Zw5bOOa03dZg9kCNrl+drmabCCMW+WM8hw==", "Oracle#2026-Test!");
            AssertUnprotectVector("mFI85334y24aV0AMiM206pLBeMHNxmVZfaas8QOXCS2sDgbB3lSD6y/lchTf3uTe", "Unicode-資料庫-密碼-2026!");
        }

        [TestMethod]
        public void Protect_IsDeterministicForSamePasswordAndDomainUser()
        {
            var first = LegacyConnectionCredentialSecurity.Protect("P@ssw0rd!", HistoricalDomainUser);
            var second = LegacyConnectionCredentialSecurity.Protect("P@ssw0rd!", HistoricalDomainUser);

            Assert.AreEqual(first, second);
        }

        [TestMethod]
        public void Unprotect_WithDifferentDomainUser_DoesNotRecoverPlainText()
        {
            const string protectedPassword = "EVNrsBQeWpYv3BHJObziNc8dg+ZTCyB3PQiAQNm3FFQ="; //gitleaks:allow - historical compatibility test vector

            var result = LegacyConnectionCredentialSecurity.Unprotect(protectedPassword, @"OTHERDOMAIN\OtherUser");

            Assert.AreNotEqual("P@ssw0rd!", result);
        }

        private static void AssertProtectVector(string plainText, string expectedProtectedPassword)
        {
            var result = LegacyConnectionCredentialSecurity.Protect(plainText, HistoricalDomainUser);

            Assert.AreEqual(expectedProtectedPassword, result, $"Legacy protection mismatch for '{plainText}'.");
        }

        private static void AssertUnprotectVector(string protectedPassword, string expectedPlainText)
        {
            var result = LegacyConnectionCredentialSecurity.Unprotect(protectedPassword, HistoricalDomainUser);

            Assert.AreEqual(expectedPlainText, result, $"Legacy unprotection mismatch for '{expectedPlainText}'.");
        }
    }
}
