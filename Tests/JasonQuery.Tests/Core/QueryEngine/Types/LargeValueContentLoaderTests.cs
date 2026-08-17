using JasonQuery.Core.QueryEngine.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.QueryEngine.Types
{
    [TestClass]
    public sealed class LargeValueContentLoaderTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void LoadText_WithContent_ReturnsSuccess()
        {
            var result = LargeValueContentLoader.LoadText(() => "full content");

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual("full content", result.Content);
            Assert.IsNull(result.Error);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadText_WithNullContent_NormalizesToEmptyString()
        {
            var result = LargeValueContentLoader.LoadText(() => null);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(string.Empty, result.Content);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadText_WithNullLoader_ReturnsEmptySuccess()
        {
            var result = LargeValueContentLoader.LoadText(null);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(string.Empty, result.Content);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadText_WhenLoaderThrows_ReturnsFailure()
        {
            var result = LargeValueContentLoader.LoadText(() => throw new InvalidOperationException("text load failed"));

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(string.Empty, result.Content);
            Assert.AreEqual("text load failed", result.ErrorMessage);
            Assert.IsInstanceOfType(result.Error, typeof(InvalidOperationException));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadBinary_WithContent_ReturnsSuccess()
        {
            var expected = new byte[] { 1, 2, 3 };
            var result = LargeValueContentLoader.LoadBinary(() => expected);

            Assert.IsTrue(result.Succeeded);
            Assert.AreSame(expected, result.Content);
            Assert.IsNull(result.Error);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadBinary_WithNullContent_NormalizesToEmptyArray()
        {
            var result = LargeValueContentLoader.LoadBinary(() => null);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(0, result.Content.Length);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadBinary_WithNullLoader_ReturnsEmptySuccess()
        {
            var result = LargeValueContentLoader.LoadBinary(null);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(0, result.Content.Length);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void LoadBinary_WhenLoaderThrows_ReturnsFailure()
        {
            var result = LargeValueContentLoader.LoadBinary(() => throw new InvalidOperationException("binary load failed"));

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(0, result.Content.Length);
            Assert.AreEqual("binary load failed", result.ErrorMessage);
            Assert.IsInstanceOfType(result.Error, typeof(InvalidOperationException));
        }
    }
}