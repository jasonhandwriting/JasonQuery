using JasonQuery.Core.QueryEngine.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.QueryEngine.Types
{
    [TestClass]
    public sealed class LargeValueLoaderCachingTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void LargeText_LoadContent_WhenLoaderReturnsNull_InvokesLoaderOnceAndReturnsEmpty()
        {
            var count = 0;
            var value = new LargeTextDataType("display", "preview", 0, false, () => { count++; return null; });

            Assert.AreEqual(string.Empty, value.LoadContent());
            Assert.AreEqual(string.Empty, value.LoadContent());
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LargeBinary_LoadContent_WhenLoaderReturnsNull_InvokesLoaderOnceAndReturnsEmpty()
        {
            var count = 0;
            var value = new LargeBinaryDataType("display", "preview", 0, false, () => { count++; return null; });

            Assert.AreEqual(0, value.LoadContent().Length);
            Assert.AreEqual(0, value.LoadContent().Length);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LargeText_LoadContent_WhenLoaderThrows_DoesNotCacheFailure()
        {
            var count = 0;
            var value = new LargeTextDataType("display", "preview", 0, false, () => { count++; throw new InvalidOperationException("load"); });

            Assert.ThrowsException<InvalidOperationException>(() => value.LoadContent());
            Assert.ThrowsException<InvalidOperationException>(() => value.LoadContent());
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LargeBinary_LoadContent_WhenLoaderThrows_DoesNotCacheFailure()
        {
            var count = 0;
            var value = new LargeBinaryDataType("display", "preview", 0, false, () => { count++; throw new InvalidOperationException("load"); });

            Assert.ThrowsException<InvalidOperationException>(() => value.LoadContent());
            Assert.ThrowsException<InvalidOperationException>(() => value.LoadContent());
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LargeText_LoadContent_WhenLoaderReturnsEmpty_InvokesLoaderOnce()
        {
            var count = 0;
            var value = new LargeTextDataType("display", "preview", 0, false, () => { count++; return string.Empty; });

            value.LoadContent();
            value.LoadContent();

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LargeBinary_LoadContent_WhenLoaderReturnsEmpty_InvokesLoaderOnce()
        {
            var count = 0;
            var value = new LargeBinaryDataType("display", "preview", 0, false, () => { count++; return new byte[0]; });

            value.LoadContent();
            value.LoadContent();

            Assert.AreEqual(1, count);
        }
    }
}
