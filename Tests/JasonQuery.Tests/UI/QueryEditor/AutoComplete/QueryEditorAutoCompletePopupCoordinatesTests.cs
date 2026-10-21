using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Reflection;

namespace JasonQuery.Tests.UI.QueryEditor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompletePopupCoordinatesTests
    {
        //Covers Period and Space: popup coordinates follow the actual parent and the current styled text-line height.
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(800, 250, 500, 100, 20, 299, 170)]
        [DataRow(800, 250, 200, 100, 20, 599, 170)]
        [DataRow(800, 250, 0, 0, 17, 799, 267)]
        [DataRow(500, 600, 300, 400, 25, 199, 225)]
        [DataRow(100, 100, 100, 100, 1, -1, 1)]
        [DataRow(50, 90, 300, 50, 10, -251, 50)]
        [DataRow(900, 700, 500, 300, 12, 399, 412)]
        [DataRow(900, 700, 500, 300, 30, 399, 430)]
        [DataRow(220, 155, 100, 80, 15, 119, 90)]
        [DataRow(220, 155, 150, 110, 15, 69, 60)]
        [DataRow(220, 155, 150, 110, 0, 69, 46)]
        [DataRow(220, 155, 150, 110, -4, 69, 46)]
        [DataRow(900, 300, 900, 300, 40, -1, 40)]
        [DataRow(500, 400, 0, 0, 18, 499, 418)]
        public void PopupCoordinates_FollowActualParentAndOpenBelowCaretLine(int caretX, int caretY, int parentX, int parentY, int lineHeight, int expectedX, int expectedY)
        {
            var mainAssembly = typeof(JasonQuery.UI.Forms.QueryForm).Assembly;
            var controller = mainAssembly.GetType("JasonQuery.UI.QueryEditor.AutoComplete.Popup.QueryEditorAutoCompletePopupController", true);
            var method = controller.GetMethod("CalculateClientPosition", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(method);

            var actual = (Point)method.Invoke(null, new object[] { new Point(caretX, caretY), new Point(parentX, parentY), lineHeight });

            Assert.AreEqual(expectedX, actual.X);
            Assert.AreEqual(expectedY, actual.Y);
        }
    }
}
