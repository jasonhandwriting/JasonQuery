using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F7L2B1")]
    public sealed class SharedOracleRecoveryEntryGoldenTests
    {
        private static SharedOracleSessionRecoveryContext EligibleDisconnected()
        {
            return new SharedOracleSessionRecoveryContext
            {
                IsOracleWorkspace = true,
                HasCurrentLogicalSnapshot = true,
                IsDatabaseBackedAction = true,
                IsActiveTabAndMenuCurrent = true,
                IsExplicitUserInvocation = true,
                HasUnsafeTransactionState = false,
                IsLiveSessionVerified = false,
                IsFreshMetadataFromLiveSession = false,
                HasSameDatabaseAndOwnerScope = false,
                IsExactTargetReResolved = false,
                IsFreshSnapshotSessionIdentityCurrent = false
            };
        }

        [TestMethod]
        public void B01_CopyPasteHasNoRecoveryEntry()
        {
            var context = EligibleDisconnected();

            context.IsDatabaseBackedAction = false;

            Assert.IsFalse(SharedOracleRecoveryEntryPolicy.CanOfferEntry(context, SharedOracleRecoveryEntryKind.ObjectRead));
        }

        [TestMethod]
        public void B02_DisconnectedQualifiedTableMayOfferRecoveryEntry()
        {
            Assert.IsTrue(SharedOracleRecoveryEntryPolicy.CanOfferEntry(EligibleDisconnected(), SharedOracleRecoveryEntryKind.Table));
        }

        [TestMethod]
        public void B03_DisconnectedQualifiedColumnMayOfferRecoveryEntry()
        {
            Assert.IsTrue(SharedOracleRecoveryEntryPolicy.CanOfferEntry(EligibleDisconnected(), SharedOracleRecoveryEntryKind.Column));
        }

        [TestMethod]
        public void B04_AlreadyVerifiedLiveSessionDoesNotOfferRecovery()
        {
            var context = EligibleDisconnected();

            context.IsLiveSessionVerified = true;

            Assert.IsFalse(SharedOracleRecoveryEntryPolicy.CanOfferEntry(context, SharedOracleRecoveryEntryKind.Table));
        }

        [TestMethod]
        public void B05_NonOracleWorkspaceDoesNotOfferRecovery()
        {
            var context = EligibleDisconnected();

            context.IsOracleWorkspace = false;

            Assert.IsFalse(SharedOracleRecoveryEntryPolicy.CanOfferEntry(context, SharedOracleRecoveryEntryKind.Table));
        }

        [TestMethod]
        public void B06_InvalidLogicalSnapshotDoesNotOfferRecovery()
        {
            var context = EligibleDisconnected();

            context.HasCurrentLogicalSnapshot = false;

            Assert.IsFalse(SharedOracleRecoveryEntryPolicy.CanOfferEntry(context, SharedOracleRecoveryEntryKind.Table));
        }

        [TestMethod]
        public void B07_ExplicitQualifiedTableClickMayRequestRecovery()
        {
            Assert.IsTrue(SharedOracleRecoveryEntryPolicy.CanAcceptExplicitClick(EligibleDisconnected(), SharedOracleRecoveryEntryKind.Table));
        }

        [TestMethod]
        public void B08_ExplicitQualifiedColumnClickMayRequestRecovery()
        {
            Assert.IsTrue(SharedOracleRecoveryEntryPolicy.CanAcceptExplicitClick(EligibleDisconnected(), SharedOracleRecoveryEntryKind.Column));
        }

        [TestMethod]
        public void B09_MenuPlanningWithoutClickCannotRequestRecovery()
        {
            var context = EligibleDisconnected();

            context.IsExplicitUserInvocation = false;

            Assert.IsFalse(SharedOracleRecoveryEntryPolicy.CanAcceptExplicitClick(context, SharedOracleRecoveryEntryKind.Table));
        }

        [TestMethod]
        public void B10_StaleTabOrMenuCannotRequestRecovery()
        {
            var context = EligibleDisconnected();

            context.IsActiveTabAndMenuCurrent = false;

            Assert.IsFalse(SharedOracleRecoveryEntryPolicy.CanAcceptExplicitClick(context, SharedOracleRecoveryEntryKind.Table));
        }

        [TestMethod]
        public void B11_PendingOrUnknownTransactionCannotRequestRecovery()
        {
            var context = EligibleDisconnected();

            context.HasUnsafeTransactionState = true;

            Assert.IsFalse(SharedOracleRecoveryEntryPolicy.CanOfferEntry(context, SharedOracleRecoveryEntryKind.Table));
            Assert.IsFalse(SharedOracleRecoveryEntryPolicy.CanAcceptExplicitClick(context, SharedOracleRecoveryEntryKind.Table));
        }

        [TestMethod]
        public void B12_UnqualifiedTargetCannotRequestRecovery()
        {
            var context = EligibleDisconnected();

            Assert.IsFalse(SharedOracleRecoveryEntryPolicy.CanOfferEntry(context, SharedOracleRecoveryEntryKind.None));
            Assert.IsFalse(SharedOracleRecoveryEntryPolicy.CanAcceptExplicitClick(context, SharedOracleRecoveryEntryKind.None));
        }
    }
}
