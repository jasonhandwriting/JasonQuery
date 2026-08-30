using JasonQuery.Core.SchemaExplorer.LazyLoading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.SchemaExplorer.LazyLoading
{
    [TestClass]
    public class SchemaBrowserLazyLoadStateTests
    {
        [TestMethod]
        public void TryBegin_LoadedTab_ReturnsFalseUntilSelectionChanges()
        {
            var state = new SchemaBrowserLazyLoadState();

            state.Reset("SqlServer|Tables|dbo|Orders|1");

            Assert.IsTrue(state.TryBegin(SchemaBrowserLazyTab.TableData));
            state.Complete(SchemaBrowserLazyTab.TableData);
            Assert.IsFalse(state.TryBegin(SchemaBrowserLazyTab.TableData));

            state.Reset("SqlServer|Tables|dbo|Customers|2");
            Assert.IsTrue(state.TryBegin(SchemaBrowserLazyTab.TableData));
        }

        [TestMethod]
        public void TryBegin_LoadingTab_PreventsDuplicateLoad()
        {
            var state = new SchemaBrowserLazyLoadState();

            state.Reset("SqlServer|Views|dbo|CustomerView|3");

            Assert.IsTrue(state.TryBegin(SchemaBrowserLazyTab.ViewData));
            Assert.IsFalse(state.TryBegin(SchemaBrowserLazyTab.ViewData));
        }

        [TestMethod]
        public void Fail_AllowsTabToBeRetried()
        {
            var state = new SchemaBrowserLazyLoadState();

            state.Reset("SqlServer|Tables|dbo|Orders|1");

            Assert.IsTrue(state.TryBegin(SchemaBrowserLazyTab.TableStructure));
            state.Fail(SchemaBrowserLazyTab.TableStructure);

            Assert.IsTrue(state.TryBegin(SchemaBrowserLazyTab.TableStructure));
        }

        [TestMethod]
        public void TryBegin_WithoutSelectionOrContentTab_ReturnsFalse()
        {
            var state = new SchemaBrowserLazyLoadState();

            Assert.IsFalse(state.TryBegin(SchemaBrowserLazyTab.SqlPane));

            state.Reset("SqlServer|Tables|dbo|Orders|1");
            Assert.IsFalse(state.TryBegin(SchemaBrowserLazyTab.None));
        }

        [DataTestMethod]
        [DataRow((int)SchemaBrowserLazyTab.TableData, true, false, (int)SchemaBrowserLazyTab.TableData)]
        [DataRow((int)SchemaBrowserLazyTab.TableStructure, true, false, (int)SchemaBrowserLazyTab.TableStructure)]
        [DataRow((int)SchemaBrowserLazyTab.ViewData, false, true, (int)SchemaBrowserLazyTab.ViewData)]
        [DataRow((int)SchemaBrowserLazyTab.TableData, false, true, (int)SchemaBrowserLazyTab.SqlPane)]
        [DataRow((int)SchemaBrowserLazyTab.ViewData, true, false, (int)SchemaBrowserLazyTab.SqlPane)]
        [DataRow((int)SchemaBrowserLazyTab.ViewData, false, false, (int)SchemaBrowserLazyTab.SqlPane)]
        public void ResolveSupportedTab_ReturnsExpectedTab(int requestedTabValue, bool isTable, bool isView, int expectedTabValue)
        {
            var requestedTab = (SchemaBrowserLazyTab)requestedTabValue;
            var expectedTab = (SchemaBrowserLazyTab)expectedTabValue;
            var actual = SchemaBrowserLazyLoadState.ResolveSupportedTab(requestedTab, isTable, isView);

            Assert.AreEqual(expectedTab, actual);
        }
    }
}
