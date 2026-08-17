using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteKeyUpHelperTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void ShouldProcessAutoCompleteTextKeyUp_WithNullEventReturnsFalse()
        {
            Assert.IsFalse(QueryEditorAutoCompleteKeyUpHelper.ShouldProcessAutoCompleteTextKeyUp(null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(Keys.Left)]
        [DataRow(Keys.Right)]
        [DataRow(Keys.Up)]
        [DataRow(Keys.Down)]
        [DataRow(Keys.Home)]
        [DataRow(Keys.End)]
        [DataRow(Keys.PageUp)]
        [DataRow(Keys.PageDown)]
        [DataRow(Keys.ControlKey)]
        [DataRow(Keys.ShiftKey)]
        [DataRow(Keys.Menu)]
        [DataRow(Keys.ProcessKey)]
        [DataRow(Keys.Apps)]
        [DataRow(Keys.Tab)]
        [DataRow(Keys.Enter)]
        [DataRow(Keys.Escape)]
        [DataRow(Keys.Back)]
        [DataRow(Keys.Delete)]
        public void ShouldProcessAutoCompleteTextKeyUp_WithNonTextKeyReturnsFalse(Keys key)
        {
            Assert.IsFalse(QueryEditorAutoCompleteKeyUpHelper.ShouldProcessAutoCompleteTextKeyUp(new KeyEventArgs(key)));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(Keys.A)]
        [DataRow(Keys.Z)]
        [DataRow(Keys.D0)]
        [DataRow(Keys.NumPad1)]
        [DataRow(Keys.OemPeriod)]
        [DataRow(Keys.Decimal)]
        [DataRow(Keys.Oemcomma)]
        [DataRow(Keys.Space)]
        public void ShouldProcessAutoCompleteTextKeyUp_WithTextKeyReturnsTrue(Keys key)
        {
            Assert.IsTrue(QueryEditorAutoCompleteKeyUpHelper.ShouldProcessAutoCompleteTextKeyUp(new KeyEventArgs(key)));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(Keys.Control | Keys.A)]
        [DataRow(Keys.Control | Keys.Space)]
        [DataRow(Keys.Alt | Keys.A)]
        [DataRow(Keys.Alt | Keys.Space)]
        public void ShouldProcessAutoCompleteTextKeyUp_WithControlOrAltReturnsFalse(Keys keyData)
        {
            Assert.IsFalse(QueryEditorAutoCompleteKeyUpHelper.ShouldProcessAutoCompleteTextKeyUp(new KeyEventArgs(keyData)));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void ShouldProcessAutoCompleteTextKeyUp_WithShiftLetterReturnsTrue()
        {
            Assert.IsTrue(QueryEditorAutoCompleteKeyUpHelper.ShouldProcessAutoCompleteTextKeyUp(new KeyEventArgs(Keys.Shift | Keys.A)));
        }
    }
}