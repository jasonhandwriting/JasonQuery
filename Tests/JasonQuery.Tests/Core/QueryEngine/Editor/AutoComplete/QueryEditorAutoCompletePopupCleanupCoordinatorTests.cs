using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompletePopupCleanupCoordinatorTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void Cleanup_ClearsDataSourceBeforeHidingPopup()
        {
            var calls = new List<string>();

            QueryEditorAutoCompletePopupCleanupCoordinator.Cleanup(() => calls.Add("Clear"), () => calls.Add("Hide"));

            CollectionAssert.AreEqual(new[] { "Clear", "Hide" }, calls);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void Cleanup_WithNullActions_DoesNotThrow()
        {
            QueryEditorAutoCompletePopupCleanupCoordinator.Cleanup(null, null);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void Cleanup_WithNullClear_StillHidesPopup()
        {
            var hidden = false;

            QueryEditorAutoCompletePopupCleanupCoordinator.Cleanup(null, () => hidden = true);

            Assert.IsTrue(hidden);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void Cleanup_WithNullHide_StillClearsDataSource()
        {
            var cleared = false;

            QueryEditorAutoCompletePopupCleanupCoordinator.Cleanup(() => cleared = true, null);

            Assert.IsTrue(cleared);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void Cleanup_WhenClearThrows_StillHidesPopup()
        {
            var hidden = false;

            Assert.ThrowsException<InvalidOperationException>
            (
                () => QueryEditorAutoCompletePopupCleanupCoordinator.Cleanup
                (
                    () => throw new InvalidOperationException("clear"),
                    () => hidden = true
                )
            );

            Assert.IsTrue(hidden);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void Cleanup_WhenHideThrows_PropagatesException()
        {
            Assert.ThrowsException<InvalidOperationException>
            (
                () => QueryEditorAutoCompletePopupCleanupCoordinator.Cleanup
                (
                    () => { },
                    () => throw new InvalidOperationException("hide")
                )
            );
        }
    }
}