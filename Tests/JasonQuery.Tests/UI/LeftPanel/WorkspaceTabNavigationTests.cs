using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    public sealed class WorkspaceTabNavigationTests
    {
        private static IReadOnlyList<string> Tabs(params string[] values)
        {
            return values;
        }

        [TestMethod]
        public void NullTabList_IsRejected()
        {
            Assert.AreEqual(-1, WorkspaceTabNavigationPolicy.FindTargetIndex(null, "q1"));
        }

        [TestMethod]
        public void NullIdentity_IsRejected()
        {
            Assert.AreEqual(-1, WorkspaceTabNavigationPolicy.FindTargetIndex(Tabs("q1"), null));
        }

        [TestMethod]
        public void EmptyOrWhitespaceIdentity_IsRejected()
        {
            Assert.AreEqual(-1, WorkspaceTabNavigationPolicy.FindTargetIndex(Tabs("q1"), ""));
            Assert.AreEqual(-1, WorkspaceTabNavigationPolicy.FindTargetIndex(Tabs("q1"), "  "));
        }

        [TestMethod]
        public void EmptyTabList_HasNoTarget()
        {
            Assert.AreEqual(-1, WorkspaceTabNavigationPolicy.FindTargetIndex(Tabs(), "q1"));
        }

        [TestMethod]
        public void ExistingTab_ResolvesExactIndex()
        {
            Assert.AreEqual(2, WorkspaceTabNavigationPolicy.FindTargetIndex(Tabs("q1", "browser", "q3"), "q3"));
        }

        [TestMethod]
        public void IdentityComparison_IsCaseSensitive()
        {
            Assert.AreEqual(-1, WorkspaceTabNavigationPolicy.FindTargetIndex(Tabs("q1"), "Q1"));
        }

        [TestMethod]
        public void DisplayTitle_IsNotATabIdentity()
        {
            Assert.AreEqual(-1, WorkspaceTabNavigationPolicy.FindTargetIndex(Tabs("20261018112233555"), "SQL Editor 1"));
        }

        [TestMethod]
        public void StaleClosedTabIdentity_IsNotRedirected()
        {
            Assert.AreEqual(-1, WorkspaceTabNavigationPolicy.FindTargetIndex(Tabs("q1", "q3"), "q2"));
        }

        [TestMethod]
        public void ExactFirstMatch_IsStableWithDuplicates()
        {
            Assert.AreEqual(0, WorkspaceTabNavigationPolicy.FindTargetIndex(Tabs("q1", "q2", "q1"), "q1"));
        }

        [TestMethod]
        public void SpaceInIdentity_IsNotTrimmedToAnotherTab()
        {
            Assert.AreEqual(-1, WorkspaceTabNavigationPolicy.FindTargetIndex(Tabs("q1"), " q1 "));
        }

        [TestMethod]
        public void SchemaBrowserAndOtherForms_UseSameTabIdentityPolicy()
        {
            Assert.AreEqual(1, WorkspaceTabNavigationPolicy.FindTargetIndex(Tabs("query", "schema", "options"), "schema"));
            Assert.AreEqual(2, WorkspaceTabNavigationPolicy.FindTargetIndex(Tabs("query", "schema", "options"), "options"));
        }
    }
}
