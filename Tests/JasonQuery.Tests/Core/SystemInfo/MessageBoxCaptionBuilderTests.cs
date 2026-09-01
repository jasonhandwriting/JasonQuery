using JasonQuery.Core.SystemInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.SystemInfo
{
    [TestClass]
    public class MessageBoxCaptionBuilderTests
    {
        [TestMethod]
        public void Build_WithDatabaseAndOs_JoinsAllNonEmptyParts()
        {
            var actual = MessageBoxCaptionBuilder.Build("JasonQuery 0.95", "SQL Server 2019", "Win10");

            Assert.AreEqual("JasonQuery 0.95, SQL Server 2019, Win10", actual);
        }

        [TestMethod]
        public void Build_WithEmptyDatabase_DoesNotCreateEmptyCommaSegment()
        {
            var actual = MessageBoxCaptionBuilder.Build("JasonQuery 0.95", string.Empty, "Win10");

            Assert.AreEqual("JasonQuery 0.95, Win10", actual);
        }

        [TestMethod]
        public void Build_WithEmptyOs_DoesNotLeaveTrailingComma()
        {
            var actual = MessageBoxCaptionBuilder.Build("JasonQuery 0.95", "SQL Server 2019", "   ");

            Assert.AreEqual("JasonQuery 0.95, SQL Server 2019", actual);
        }

        [TestMethod]
        public void Build_WithAllOptionalPartsEmpty_ReturnsOnlyBaseCaption()
        {
            var actual = MessageBoxCaptionBuilder.Build("JasonQuery 0.95", null, string.Empty);

            Assert.AreEqual("JasonQuery 0.95", actual);
        }

        [TestMethod]
        public void Build_TrimsEveryIncludedPart()
        {
            var actual = MessageBoxCaptionBuilder.Build("  JasonQuery 0.95  ", " SQL Server 2019 ", " Win10 ");

            Assert.AreEqual("JasonQuery 0.95, SQL Server 2019, Win10", actual);
        }
    }
}
