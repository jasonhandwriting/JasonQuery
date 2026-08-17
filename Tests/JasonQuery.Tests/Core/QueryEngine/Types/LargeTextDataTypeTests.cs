using JasonQuery.Core.QueryEngine.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Types
{
    [TestClass]
    public sealed class LargeTextDataTypeTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void Constructor_WithNullDisplayAndPreview_NormalizesToEmptyStrings()
        {
            var value = new LargeTextDataType(null, null, 0, false, null);

            Assert.AreEqual(string.Empty, value.DisplayText);
            Assert.AreEqual(string.Empty, value.PreviewText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Constructor_WithValues_PreservesMetadata()
        {
            var value = new LargeTextDataType("(CLOB)(100)", "abc", 100, true, () => "full");

            Assert.AreEqual("(CLOB)(100)", value.DisplayText);
            Assert.AreEqual("abc", value.PreviewText);
            Assert.AreEqual(100, value.Length);
            Assert.IsTrue(value.IsTruncated);
            Assert.IsFalse(value.IsNull);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToString_WithoutPreview_ReturnsDisplayTextOnly()
        {
            var value = new LargeTextDataType("(CLOB)(100)", string.Empty, 100, false, () => "full");
            var actual = value.ToString();

            Assert.AreEqual("(CLOB)(100)", actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToString_WithPreview_ConcatenatesDisplayAndPreview()
        {
            var value = new LargeTextDataType("(CLOB)(100)", "abc...(truncated)", 100, true, () => "full");
            var actual = value.ToString();

            Assert.AreEqual("(CLOB)(100)abc...(truncated)", actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadContent_WithLoader_ReturnsFullContent()
        {
            var value = new LargeTextDataType("display", "preview", 4, false, () => "full");
            var actual = value.LoadContent();

            Assert.AreEqual("full", actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadContent_WhenCalledTwice_InvokesLoaderOnce()
        {
            var invocationCount = 0;

            var value = new LargeTextDataType
            (
                "display",
                "preview",
                4,
                false,
                () =>
                {
                    invocationCount++;
                    return "full";
                }
            );

            var first = value.LoadContent();
            var second = value.LoadContent();

            Assert.AreEqual("full", first);
            Assert.AreEqual("full", second);
            Assert.AreEqual(1, invocationCount);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToString_DoesNotInvokeContentLoader()
        {
            var invocationCount = 0;

            var value = new LargeTextDataType
            (
                "display",
                "preview",
                4,
                false,
                () =>
                {
                    invocationCount++;
                    return "full";
                }
            );

            var actual = value.ToString();

            Assert.AreEqual("displaypreview", actual);
            Assert.AreEqual(0, invocationCount);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadContent_WithNullLoader_ReturnsEmptyString()
        {
            var value = new LargeTextDataType("display", "preview", 0, false, null);
            var actual = value.LoadContent();

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CreateNull_WithText_CreatesNullValue()
        {
            var value = LargeTextDataType.CreateNull("(NULL)");

            Assert.AreEqual("(NULL)", value.DisplayText);
            Assert.AreEqual(string.Empty, value.PreviewText);
            Assert.AreEqual(0, value.Length);
            Assert.IsFalse(value.IsTruncated);
            Assert.IsTrue(value.IsNull);
            Assert.AreEqual(string.Empty, value.LoadContent());
            Assert.AreEqual("(NULL)", value.ToString());
        }
    }
}