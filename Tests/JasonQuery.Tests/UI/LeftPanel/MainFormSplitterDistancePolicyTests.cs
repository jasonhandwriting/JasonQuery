using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class MainFormSplitterDistancePolicyTests
    {
        private static Type PolicyType => typeof(MainForm).Assembly.GetType("JasonQuery.UI.Forms.MainFormSplitterDistancePolicy", true);

        private static int Clamp(int preferred, int total, int splitter, int minLeft, int minRight)
        {
            var method = PolicyType.GetMethod("Clamp", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(method);
            return (int)method.Invoke(null, new object[] { preferred, total, splitter, minLeft, minRight });
        }

        private static bool ShouldPersist(bool dragged, bool restoring, bool visible, bool collapsed, int current, int previous)
        {
            var method = PolicyType.GetMethod("ShouldPersist", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(method);
            return (bool)method.Invoke(null, new object[] { dragged, restoring, visible, collapsed, current, previous });
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LeftPanel")]
        [DataRow(250, 1008, 4, 150, 400, 250)]
        [DataRow(700, 1008, 4, 150, 400, 604)]
        [DataRow(10, 1008, 4, 150, 400, 150)]
        [DataRow(360, 900, 4, 150, 400, 360)]
        [DataRow(360, 600, 4, 150, 400, 196)]
        [DataRow(250, 530, 4, 150, 400, -1)]
        [DataRow(250, 554, 4, 150, 400, 150)]
        [DataRow(250, 1008, 8, 150, 400, 250)]
        [DataRow(900, 1008, 4, 150, 400, 604)]
        [DataRow(0, 1008, 4, 150, 400, 150)]
        public void Clamp_RespectsUserPreferenceAndPanelMinima(int preferred, int width, int divider, int leftMin, int rightMin, int expected)
        {
            Assert.AreEqual(expected, Clamp(preferred, width, divider, leftMin, rightMin));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("LeftPanel")]
        [DataRow(true, false, true, false, 280, 250, true)]
        [DataRow(false, false, true, false, 280, 250, false)]
        [DataRow(true, true, true, false, 280, 250, false)]
        [DataRow(true, false, false, false, 280, 250, false)]
        [DataRow(true, false, true, true, 280, 250, false)]
        [DataRow(true, false, true, false, 250, 250, false)]
        public void ShouldPersist_OnlyAfterActualUserDrag(bool dragged, bool restoring, bool visible, bool collapsed, int current, int previous, bool expected)
        {
            Assert.AreEqual(expected, ShouldPersist(dragged, restoring, visible, collapsed, current, previous));
        }
    }
}
