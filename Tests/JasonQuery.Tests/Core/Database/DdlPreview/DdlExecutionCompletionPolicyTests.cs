using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    [TestClass]
    public sealed class DdlExecutionCompletionPolicyTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Evaluate_WithError_ReturnsFailureAndNoNotification()
        {
            var actual = DdlExecutionCompletionPolicy.Evaluate("DDL failed", true, true);

            Assert.IsFalse(actual.Succeeded);
            Assert.AreEqual(DdlMainFormNotificationKind.None, actual.NotificationKind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Evaluate_WithEmptyErrorAndPending_ReturnsPendingNotification()
        {
            var actual = DdlExecutionCompletionPolicy.Evaluate(string.Empty, true, false);

            Assert.IsTrue(actual.Succeeded);
            Assert.AreEqual(DdlMainFormNotificationKind.PendingTransaction, actual.NotificationKind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Evaluate_WithEmptyErrorAndClosed_ReturnsClosedNotification()
        {
            var actual = DdlExecutionCompletionPolicy.Evaluate(string.Empty, false, true);

            Assert.IsTrue(actual.Succeeded);
            Assert.AreEqual(DdlMainFormNotificationKind.TransactionClosed, actual.NotificationKind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Evaluate_WithEmptyErrorAndUnchanged_ReturnsNoNotification()
        {
            var actual = DdlExecutionCompletionPolicy.Evaluate(string.Empty, false, false);

            Assert.IsTrue(actual.Succeeded);
            Assert.AreEqual(DdlMainFormNotificationKind.None, actual.NotificationKind);
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("DdlPreview")]
        public void Evaluate_WhenPendingAndClosedAreBothTrue_PendingTakesPrecedence()
        {
            var actual = DdlExecutionCompletionPolicy.Evaluate(string.Empty, true, true);

            Assert.IsTrue(actual.Succeeded);
            Assert.AreEqual(DdlMainFormNotificationKind.PendingTransaction, actual.NotificationKind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Evaluate_WithNullError_IsTreatedAsSuccess()
        {
            var actual = DdlExecutionCompletionPolicy.Evaluate(null, false, false);

            Assert.IsTrue(actual.Succeeded);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Evaluate_WithWhitespaceError_IsTreatedAsFailureToMatchExistingFormBehavior()
        {
            var actual = DdlExecutionCompletionPolicy.Evaluate(" ", false,false);

            Assert.IsFalse(actual.Succeeded);
        }
    }
}