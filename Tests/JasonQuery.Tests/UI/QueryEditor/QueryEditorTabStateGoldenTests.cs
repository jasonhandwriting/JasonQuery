using JasonQuery.UI.QueryEditor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.UI.QueryEditor
{
    //TAB-R-C1a: pure state transitions only; no editor, dialog or file I/O.
    [TestClass]
    [TestCategory("TABRC1A")]
    public sealed class QueryEditorTabStateGoldenTests
    {
        private const string OriginalPath = @"D:\SQL\Customer.sql";
        private const string OtherPath = @"D:\Archive\Customer.sql";
        private const string NewPath = @"D:\SQL\Customer2026.sql";
        private const string Alias = "Customer Analysis";

        [TestMethod]
        public void TABRC1A01_NewEditorHasNoSavedFileIdentity()
        {
            var state = QueryEditorTabState.CreateNew("SQL Editor 1");

            Assert.AreEqual("SQL Editor 1", state.DisplayName);
            Assert.AreEqual(string.Empty, state.SavedFilePath);
            Assert.IsFalse(state.HasSavedFile);
            Assert.AreEqual("SQL Editor 1", state.BuildTitle(false));
            Assert.AreEqual("*SQL Editor 1", state.BuildTooltip(true));
            Assert.IsFalse(state.MatchesSavedFilePath("SQL Editor 1"));
        }

        [TestMethod]
        public void TABRC1A02_OpenedFileSeparatesDisplayNameAndPath()
        {
            var state = QueryEditorTabState.CreateOpenedFile(OriginalPath);

            Assert.AreEqual("Customer.sql", state.DisplayName);
            Assert.AreEqual(OriginalPath, state.SavedFilePath);
            Assert.IsTrue(state.HasSavedFile);
            Assert.AreEqual(OriginalPath, state.BuildTooltip(false));
            Assert.AreEqual("*" + OriginalPath, state.BuildTooltip(true));
        }

        [TestMethod]
        public void TABRC1A03_UnsavedRenameUpdatesTitleAndTooltipWithoutCreatingFileIdentity()
        {
            var state = QueryEditorTabState.CreateNew("SQL Editor 2");

            Assert.IsTrue(state.TryRename("  " + Alias + "  "));
            Assert.AreEqual(Alias, state.BuildTitle(false));
            Assert.AreEqual("*" + Alias, state.BuildTooltip(true));
            Assert.AreEqual(string.Empty, state.SavedFilePath);
            Assert.IsFalse(state.HasSavedFile);
        }

        [TestMethod]
        public void TABRC1A04_SavedEditorRenameNeverChangesFilePath()
        {
            var state = QueryEditorTabState.CreateOpenedFile(OriginalPath);

            Assert.IsTrue(state.TryRename(Alias));
            Assert.AreEqual(Alias, state.DisplayName);
            Assert.AreEqual(OriginalPath, state.SavedFilePath);
            Assert.IsTrue(state.HasSavedFile);
            Assert.AreEqual(OriginalPath, state.BuildTooltip(false));
            Assert.AreEqual("*" + OriginalPath, state.BuildTooltip(true));
        }

        [TestMethod]
        public void TABRC1A05_RepeatedDirtyRefreshKeepsCurrentAlias()
        {
            var state = QueryEditorTabState.CreateOpenedFile(OriginalPath);

            state.TryRename(Alias);

            for (var index = 0; index < 3; index++)
            {
                Assert.AreEqual("*" + Alias, state.BuildTitle(true));
            }

            Assert.AreEqual(Alias, state.DisplayName);
            Assert.AreEqual(OriginalPath, state.SavedFilePath);
        }

        [TestMethod]
        public void TABRC1A06_DirtyPrefixIsDerivedRatherThanStored()
        {
            var state = QueryEditorTabState.CreateNew("SQL Editor 2");

            state.TryRename(Alias);

            Assert.AreEqual("*" + Alias, state.BuildTitle(true));
            Assert.AreEqual(Alias, state.BuildTitle(false));
            Assert.AreEqual("*" + Alias, state.BuildTitle(true));
            Assert.AreEqual(Alias, state.DisplayName);
        }

        [TestMethod]
        public void TABRC1A07_CleanProjectionAfterOrdinarySaveKeepsAliasAndPath()
        {
            //Ordinary Save uses SavedFilePath without rebinding the editor to a new file.
            //Only the caller's dirty state changes; this test does not perform a save.
            var state = QueryEditorTabState.CreateOpenedFile(OriginalPath);

            state.TryRename(Alias);

            Assert.AreEqual("*" + Alias, state.BuildTitle(true));
            Assert.AreEqual(Alias, state.BuildTitle(false));
            Assert.AreEqual(OriginalPath, state.SavedFilePath);
        }

        [TestMethod]
        public void TABRC1A08_SuccessfulSaveAsRebindsNameAndPath()
        {
            var state = QueryEditorTabState.CreateOpenedFile(OriginalPath);

            state.TryRename(Alias);
            state.ApplySuccessfulSaveAs(NewPath);

            Assert.AreEqual("Customer2026.sql", state.DisplayName);
            Assert.AreEqual(NewPath, state.SavedFilePath);
            Assert.IsTrue(state.HasSavedFile);
            Assert.IsFalse(state.MatchesSavedFilePath(OriginalPath));
            Assert.IsTrue(state.MatchesSavedFilePath(NewPath));

            var newEditor = QueryEditorTabState.CreateNew("SQL Editor 3");

            newEditor.TryRename(Alias);
            newEditor.ApplySuccessfulSaveAs(NewPath);

            Assert.AreEqual("Customer2026.sql", newEditor.DisplayName);
            Assert.AreEqual(NewPath, newEditor.SavedFilePath);
            Assert.IsTrue(newEditor.HasSavedFile);
        }

        [TestMethod]
        public void TABRC1A09_InvalidSaveAsStateInputCannotPartiallyRebind()
        {
            //A cancelled or failed real write must not call ApplySuccessfulSaveAs.
            //Here we only verify that an invalid state input cannot partially change identity.
            var state = QueryEditorTabState.CreateOpenedFile(OriginalPath);

            state.TryRename(Alias);

            Assert.ThrowsExactly<ArgumentException>(() => state.ApplySuccessfulSaveAs(string.Empty));

            Assert.AreEqual(Alias, state.DisplayName);
            Assert.AreEqual(OriginalPath, state.SavedFilePath);
            Assert.IsTrue(state.HasSavedFile);
        }

        [TestMethod]
        public void TABRC1A10_UnsavedBackupRestoresAliasWithoutRetainingDirtyMarkerInName()
        {
            var state = QueryEditorTabState.Restore("*" + Alias, string.Empty, out var wasDirty);

            Assert.IsTrue(wasDirty);
            Assert.AreEqual(Alias, state.DisplayName);
            Assert.IsFalse(state.HasSavedFile);
            Assert.AreEqual("*" + Alias, state.BuildTitle(wasDirty));
            Assert.AreEqual(Alias, state.BuildTitle(false));
        }

        [TestMethod]
        public void TABRC1A11_SavedBackupRestoresAliasAndOriginalFileIdentityIndependently()
        {
            var state = QueryEditorTabState.Restore("*" + Alias, OriginalPath, out var wasDirty);

            Assert.IsTrue(wasDirty);
            Assert.AreEqual(Alias, state.DisplayName);
            Assert.AreEqual(OriginalPath, state.SavedFilePath);
            Assert.IsTrue(state.HasSavedFile);
            Assert.AreEqual("*" + Alias, state.BuildTitle(wasDirty));
            Assert.AreEqual("*" + OriginalPath, state.BuildTooltip(wasDirty));

            var legacy = QueryEditorTabState.Restore("*Customer.sql", OriginalPath, out var legacyWasDirty);

            Assert.IsTrue(legacyWasDirty);
            Assert.AreEqual("Customer.sql", legacy.DisplayName);
            Assert.AreEqual(OriginalPath, legacy.SavedFilePath);

            var clean = QueryEditorTabState.Restore(Alias, OriginalPath, out var cleanWasDirty);

            Assert.IsFalse(cleanWasDirty);
            Assert.AreEqual(Alias, clean.BuildTitle(cleanWasDirty));
        }

        [TestMethod]
        public void TABRC1A12_BackupProjectionUsesLatestAliasAndSeparatePath()
        {
            var state = QueryEditorTabState.CreateOpenedFile(OriginalPath);

            state.TryRename("First Alias");
            state.TryRename(Alias);

            var backupTabTitle = state.BuildTitle(true);
            var savedFilePath = state.SavedFilePath;
            var restored = QueryEditorTabState.Restore(backupTabTitle, savedFilePath, out var wasDirty);

            Assert.AreEqual("*" + Alias, backupTabTitle);
            Assert.AreEqual(OriginalPath, savedFilePath);
            Assert.AreEqual(Alias, restored.DisplayName);
            Assert.AreEqual(OriginalPath, restored.SavedFilePath);
            Assert.IsTrue(wasDirty);
        }

        [TestMethod]
        public void TABRC1A13_EqualFileNamesAndAliasesDoNotConflateDifferentPathsOrInstances()
        {
            var first = QueryEditorTabState.CreateOpenedFile(OriginalPath);
            var second = QueryEditorTabState.CreateOpenedFile(OtherPath);

            Assert.AreEqual(first.DisplayName, second.DisplayName);

            first.TryRename(Alias);

            Assert.AreEqual("Customer.sql", second.DisplayName);

            second.TryRename(Alias);

            Assert.AreEqual(first.DisplayName, second.DisplayName);
            Assert.IsTrue(first.MatchesSavedFilePath(OriginalPath));
            Assert.IsFalse(first.MatchesSavedFilePath(OtherPath));
            Assert.IsTrue(second.MatchesSavedFilePath(OtherPath));
            Assert.IsFalse(second.MatchesSavedFilePath(OriginalPath));
            Assert.IsFalse(first.MatchesSavedFilePath(Alias));
        }

        [TestMethod]
        public void TABRC1A14_BlankUnchangedOrDirtyMarkerRenameDoesNotMutateState()
        {
            var state = QueryEditorTabState.CreateOpenedFile(OriginalPath);

            state.TryRename(Alias);

            foreach (var rejectedName in new[] { null, string.Empty, "   ", Alias, " " + Alias + " ", "*" + Alias, "A*B" })
            {
                Assert.IsFalse(state.TryRename(rejectedName));
                Assert.AreEqual(Alias, state.DisplayName);
                Assert.AreEqual(OriginalPath, state.SavedFilePath);
            }
        }
    }
}
