using JasonQuery.Core.QueryEngine.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Types
{
    [TestClass]
    public sealed class TextDataTypeTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void ToString_WithAlias_ReturnsAlias()
        {
            var value = new TextDataType
            {
                Alias = "display",
                Data = "full content"
            };

            var actual = value.ToString();

            Assert.AreEqual("display", actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToString_WithNullAlias_ReturnsNull()
        {
            var value = new TextDataType
            {
                Alias = null,
                Data = "full content"
            };

            var actual = value.ToString();

            Assert.IsNull(actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Data_WhenAssigned_PreservesFullContent()
        {
            var value = new TextDataType();

            value.Data = "full content";

            Assert.AreEqual("full content", value.Data);
        }
    }
}
