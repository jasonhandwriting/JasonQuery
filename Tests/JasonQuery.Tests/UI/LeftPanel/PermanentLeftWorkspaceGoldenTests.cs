using JasonQuery.Core.Database.Connection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F7L2B2CWORKSPACE")]
    public sealed class PermanentLeftWorkspaceGoldenTests
    {
        private static SharedLeftPanelHostState Connected()
        {
            return SharedLeftPanelHostStatePolicy.Refresh
            (
                SharedLeftPanelHostStatePolicy.Empty,
                true,
                true,
                "SQL Editor 1",
                LeftPanelTargetKind.QueryForm
            );
        }

        private static bool KeepPinned(SharedLeftPanelHostState state, bool mainWorkspaceVisible, bool userRequestedHide)
        {
            //Golden Red: the production policy method does not exist yet.
            //Its purpose is to keep the connected F6/F7/F8/F9 workspace visible even when a legacy Preview toggle asks to hide it.
            var method = typeof(SharedSchemaExplorerPreviewPolicy).GetMethod
            (
                "ShouldKeepPermanentWorkspaceVisible",
                BindingFlags.Public | BindingFlags.Static
            );

            Assert.IsNotNull(method, "Connected left workspace must have a dedicated non-hideable visibility policy.");
            Assert.AreEqual(typeof(bool), method.ReturnType);

            return (bool)method.Invoke(null, new object[] { state, mainWorkspaceVisible, userRequestedHide });
        }

        [TestMethod]
        public void W01_ConnectedMainWorkspaceIsVisible()
        {
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldShowWorkspace(Connected(), true));
        }

        [TestMethod]
        public void W02_DisconnectedWorkspaceIsNotShown()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.ShouldShowWorkspace(SharedLeftPanelHostStatePolicy.Empty, true));
        }

        [TestMethod]
        public void W03_HiddenMainWindowDoesNotForceAnOrphanedLeftPanel()
        {
            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.ShouldShowWorkspace(Connected(), false));
        }

        [TestMethod]
        public void W04_SqlEditorTabSwitchDoesNotHideConnectedLeftPanel()
        {
            var first = Connected();

            var second = SharedLeftPanelHostStatePolicy.Refresh
            (
                first, true, true, "SQL Editor 2", LeftPanelTargetKind.QueryForm
            );

            Assert.AreEqual(first.ConnectionRevision, second.ConnectionRevision);
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldShowWorkspace(second, true));
        }

        [TestMethod]
        public void W05_PhysicalOracleDisconnectMustNotChangeLogicalWorkspaceVisibility()
        {
            //SELECT may close the physical Oracle Reader, but this policy is logical-only.
            var logicalContext = Connected();

            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldShowWorkspace(logicalContext, true));
        }

        [TestMethod]
        public void W06_NarrowWidthMayDisableLegacyPreviewOfferButMustNotHideWorkspace()
        {
            var state = Connected();

            Assert.IsFalse(SharedSchemaExplorerPreviewPolicy.CanOffer(state, DataSourceType.Oracle, true, 500));
            Assert.IsTrue(SharedSchemaExplorerPreviewPolicy.ShouldShowWorkspace(state, true));
        }

        [TestMethod]
        public void W07_ConnectedWorkspaceIgnoresLegacyUserHideRequest()
        {
            Assert.IsTrue(KeepPinned(Connected(), true, true));
        }

        [TestMethod]
        public void W08_ConnectedWorkspaceIsPinnedWithoutAnyHideRequest()
        {
            Assert.IsTrue(KeepPinned(Connected(), true, false));
        }

        [TestMethod]
        public void W09_DisconnectedWorkspaceDoesNotBecomeVisibleFromHideToggle()
        {
            Assert.IsFalse(KeepPinned(SharedLeftPanelHostStatePolicy.Empty, true, true));
        }

        [TestMethod]
        public void W10_MainWorkspaceNotVisibleCannotShowAnOrphanedLeftPanel()
        {
            Assert.IsFalse(KeepPinned(Connected(), false, true));
        }
    }
}
