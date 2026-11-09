using JasonQuery.UI.Forms;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F2-C3-B: focused integration contracts, no DB or C1 instance is created.
    [TestClass]
    [TestCategory("F2C3B")]
    public sealed class SchemaExplorerTargetFocusIntegrationGoldenTests
    {
        private static bool Policy(bool trustedBefore, bool sameScope, bool bindSucceeded, bool trustedAfter)
        {
            var type = typeof(SharedSchemaMetadataSnapshot).Assembly.GetType
            (
                "JasonQuery.UI.LeftPanel.SchemaExplorerTargetFocusPolicy", true
            );

            var method = type.GetMethod
            (
                "ShouldApplyAfterBinding",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
            );

            Assert.IsNotNull(method);

            return (bool)method.Invoke
            (
                null,
                new object[] { trustedBefore, sameScope, bindSucceeded, trustedAfter }
            );
        }

        [TestMethod]
        public void S312_RebindHasASeparateCurrentGridFocusCaptureEntry()
        {
            var type = typeof(MainForm);

            Assert.IsNotNull(type.GetMethod
            (
                "CaptureSchemaExplorerFocusBeforeRebind",
                BindingFlags.NonPublic | BindingFlags.Instance
            ));

            Assert.IsNotNull(type.GetMethod
            (
                "RestoreSchemaExplorerFocusAfterSuccessfulRebind",
                BindingFlags.NonPublic | BindingFlags.Instance
            ));

            Assert.IsTrue(Policy(true, true, true, true));
        }

        [TestMethod]
        public void S313_NoSuccessfulRebindNeverReappliesOldFocus()
        {
            Assert.IsFalse(Policy(true, true, false, true));
            Assert.IsFalse(Policy(false, true, true, true));
        }

        [TestMethod]
        public void S314_ChangedScopeOrUntrustedMetadataNeverReappliesOldFocus()
        {
            Assert.IsFalse(Policy(true, false, true, true));
            Assert.IsFalse(Policy(true, true, true, false));
        }
    }
}
