using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F7L2")]
    public sealed class SharedOracleSessionRecoveryGoldenTests
    {
        private static SharedOracleSessionRecoveryContext ValidRequest()
        {
            return new SharedOracleSessionRecoveryContext
            {
                IsOracleWorkspace = true,
                HasCurrentLogicalSnapshot = true,
                IsDatabaseBackedAction = true,
                IsActiveTabAndMenuCurrent = true,
                IsExplicitUserInvocation = true,
                HasUnsafeTransactionState = false,
                IsLiveSessionVerified = true,
                IsFreshMetadataFromLiveSession = true,
                HasSameDatabaseAndOwnerScope = true,
                IsExactTargetReResolved = true,
                IsFreshSnapshotSessionIdentityCurrent = true
            };
        }

        [TestMethod]
        public void G01_PassiveCopyPasteDoesNotOfferRecovery()
        {
            var request = ValidRequest();

            request.IsDatabaseBackedAction = false;

            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanOfferRecovery(request));
        }

        [TestMethod]
        public void G02_DisconnectedQualifiedActionMayOfferExplicitRecovery()
        {
            var request = ValidRequest();

            request.IsLiveSessionVerified = false;
            request.IsFreshMetadataFromLiveSession = false;
            request.IsFreshSnapshotSessionIdentityCurrent = false;

            Assert.IsTrue(SharedOracleSessionRecoveryPolicy.CanOfferRecovery(request));
        }

        [TestMethod]
        public void G03_NonOracleActionCannotEnterOracleRecovery()
        {
            var request = ValidRequest();

            request.IsOracleWorkspace = false;

            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanOfferRecovery(request));
        }

        [TestMethod]
        public void G04_MenuPlanningDoesNotBeginRecoveryWithoutUserClick()
        {
            var request = ValidRequest();

            request.IsExplicitUserInvocation = false;

            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanStartRecovery(request));
        }

        [TestMethod]
        public void G05_ClickedQualifiedActionMayStartRecovery()
        {
            var request = ValidRequest();

            request.IsLiveSessionVerified = false;

            Assert.IsTrue(SharedOracleSessionRecoveryPolicy.CanStartRecovery(request));
        }

        [TestMethod]
        public void G06_PendingOrUncertainTransactionBlocksRecovery()
        {
            var request = ValidRequest();

            request.HasUnsafeTransactionState = true;

            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanStartRecovery(request));
            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanAuthorizeRecoveredAction(request));
        }

        [TestMethod]
        public void G07_StaleTabOrMenuPreventsRecovery()
        {
            var request = ValidRequest();

            request.IsActiveTabAndMenuCurrent = false;

            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanStartRecovery(request));
            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanAuthorizeRecoveredAction(request));
        }

        [TestMethod]
        public void G08_ReconnectFailureNeverAuthorizesAction()
        {
            var request = ValidRequest();

            request.IsLiveSessionVerified = false;

            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanAuthorizeRecoveredAction(request));
        }

        [TestMethod]
        public void G09_SameUserNewSessionCannotReauthorizeStaleMetadata()
        {
            var request = ValidRequest();

            request.IsFreshMetadataFromLiveSession = false;
            request.IsFreshSnapshotSessionIdentityCurrent = false;

            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanAuthorizeRecoveredAction(request));
        }

        [TestMethod]
        public void G10_FreshVerifiedTableAllowsExactRequalifiedAction()
        {
            var request = ValidRequest();

            Assert.IsTrue(SharedOracleSessionRecoveryPolicy.CanAuthorizeRecoveredAction(request));
        }

        [TestMethod]
        public void G11_WrongPdbOwnerOrMissingTargetFailsClosed()
        {
            var request = ValidRequest();

            request.HasSameDatabaseAndOwnerScope = false;

            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanAuthorizeRecoveredAction(request));

            request.HasSameDatabaseAndOwnerScope = true;
            request.IsExactTargetReResolved = false;

            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanAuthorizeRecoveredAction(request));
        }

        [TestMethod]
        public void G12_FreshColumnRequiresExactRawIdentityAndLiveBinding()
        {
            var request = ValidRequest();

            //The caller must verify raw owner/table/column before setting this flag.
            Assert.IsTrue(SharedOracleSessionRecoveryPolicy.CanAuthorizeRecoveredAction(request));

            request.IsExactTargetReResolved = false;
            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanAuthorizeRecoveredAction(request));

            request.IsExactTargetReResolved = true;
            request.IsFreshSnapshotSessionIdentityCurrent = false;
            Assert.IsFalse(SharedOracleSessionRecoveryPolicy.CanAuthorizeRecoveredAction(request));
        }
    }
}
