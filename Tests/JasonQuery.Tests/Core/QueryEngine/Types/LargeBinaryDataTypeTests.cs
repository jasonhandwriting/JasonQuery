using JasonQuery.Core.QueryEngine.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Types
{
    [TestClass]
    public sealed class LargeBinaryDataTypeTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void Constructor_WithNullDisplayAndPreview_NormalizesToEmptyStrings()
        {
            var value = new LargeBinaryDataType(null, null, 0, false, null);

            Assert.AreEqual(string.Empty, value.DisplayText);
            Assert.AreEqual(string.Empty, value.PreviewText);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Constructor_WithValues_PreservesMetadata()
        {
            var content = new byte[] { 0x01, 0x02 };

            var value = new LargeBinaryDataType("(BLOB)(2)", "0102", 2, true, () => content);

            Assert.AreEqual("(BLOB)(2)", value.DisplayText);
            Assert.AreEqual("0102", value.PreviewText);
            Assert.AreEqual(2, value.Length);
            Assert.IsTrue(value.IsTruncated);
            Assert.IsFalse(value.IsNull);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToString_WithoutPreview_ReturnsDisplayTextOnly()
        {
            var value = new LargeBinaryDataType("(BLOB)(2)", string.Empty, 2, false, () => new byte[] { 0x01, 0x02 });
            var actual = value.ToString();

            Assert.AreEqual("(BLOB)(2)", actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToString_WithPreview_ConcatenatesDisplayAndPreview()
        {
            var value = new LargeBinaryDataType("(BLOB)(2)", "0102", 2, false, () => new byte[] { 0x01, 0x02 });
            var actual = value.ToString();

            Assert.AreEqual("(BLOB)(2)0102", actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadContent_WithLoader_ReturnsFullContent()
        {
            var expected = new byte[] { 0x01, 0x02, 0x03 };
            var value = new LargeBinaryDataType("display", "preview", 3, false, () => expected);
            var actual = value.LoadContent();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadContent_WhenCalledTwice_InvokesLoaderOnceAndReturnsSameArray()
        {
            var invocationCount = 0;
            var expected = new byte[] { 0x01, 0x02, 0x03 };

            var value = new LargeBinaryDataType
            (
                "display",
                "preview",
                3,
                false,
                () =>
                {
                    invocationCount++;
                    return expected;
                }
            );

            var first = value.LoadContent();
            var second = value.LoadContent();

            Assert.AreSame(expected, first);
            Assert.AreSame(first, second);
            Assert.AreEqual(1, invocationCount);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ToString_DoesNotInvokeContentLoader()
        {
            var invocationCount = 0;

            var value = new LargeBinaryDataType
            (
                "display",
                "preview",
                1,
                false,
                () =>
                {
                    invocationCount++;
                    return new byte[] { 0x01 };
                }
            );

            var actual = value.ToString();

            Assert.AreEqual("displaypreview", actual);
            Assert.AreEqual(0, invocationCount);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadContent_WithNullLoader_ReturnsEmptyArray()
        {
            var value = new LargeBinaryDataType("display", "preview", 0, false, null);
            var actual = value.LoadContent();

            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CreateNull_WithText_CreatesNullValue()
        {
            var value = LargeBinaryDataType.CreateNull("(NULL)");

            Assert.AreEqual("(NULL)", value.DisplayText);
            Assert.AreEqual(string.Empty, value.PreviewText);
            Assert.AreEqual(0, value.Length);
            Assert.IsFalse(value.IsTruncated);
            Assert.IsTrue(value.IsNull);
            Assert.AreEqual(0, value.LoadContent().Length);
            Assert.AreEqual("(NULL)", value.ToString());
        }
    }
}
