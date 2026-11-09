using JasonQuery.Core.Database.Execution;
using System;

namespace JasonQuery.UI.Forms
{
    public partial class MainForm
    {
        //F2-C2: one pending structural change per currently selected logical DB scope.
        //Keep the existing F7 form dirty flag; do not redesign the C98 Execute Gate.
        private bool _pendingF7EditorSchemaDirty;
        private string _pendingF7EditorSchemaScope = string.Empty;

        //A pre-DDL, same-scope snapshot may survive a successful ROLLBACK.
        //Never reuse it for a later transaction if Commit/Reload failed.
        private bool _f7RollbackBaselineEligible;
        private string _f7RollbackBaselineScope = string.Empty;
        private object _f7RollbackBaselineSnapshot;

        private void ResetF7RollbackBaseline()
        {
            _f7RollbackBaselineEligible = false;
            _f7RollbackBaselineScope = string.Empty;
            _f7RollbackBaselineSnapshot = null;
        }

        private bool IsF7RollbackSnapshotTrusted()
        {
            return _sharedSchemaMetadataSnapshot != null
                   && _sharedSchemaExplorerGrid != null
                   && !_sharedSchemaExplorerGrid.IsDisposed
                   && _sharedSchemaExplorerGrid.DataSource != null
                   && IsSharedSchemaSnapshotInCurrentScope();
        }

        private void CaptureF7RollbackBaselineBeforeFirstDirty(string currentScope)
        {
            if (_pendingF7EditorSchemaDirty || _pendingF7AddColumnMetadataRefresh)
            {
                //A previous transaction may have failed to refresh. Do not relabel it.
                return;
            }

            _f7RollbackBaselineScope = currentScope;
            _f7RollbackBaselineSnapshot = _sharedSchemaMetadataSnapshot;

            _f7RollbackBaselineEligible = (_currentSourceType == JasonQuery.Core.Database.Connection.DataSourceType.PostgreSql
                                           || _currentSourceType == JasonQuery.Core.Database.Connection.DataSourceType.SqlServer)
                                          && !string.IsNullOrWhiteSpace(currentScope)
                                          && IsF7RollbackSnapshotTrusted();
        }

        private void MarkF7TransactionBoundAddColumn()
        {
            CaptureF7RollbackBaselineBeforeFirstDirty(GetSharedSchemaDatabaseScope());
            _pendingF7AddColumnMetadataRefresh = true;
        }

        private void MarkF7EditorStructuralDdl()
        {
            var currentScope = GetSharedSchemaDatabaseScope();

            if (string.IsNullOrWhiteSpace(currentScope))
            {
                return;
            }

            if (_pendingF7EditorSchemaDirty && !string.Equals(_pendingF7EditorSchemaScope, currentScope, StringComparison.Ordinal))
            {
                _pendingF7EditorSchemaDirty = false;
                ResetF7RollbackBaseline();
            }

            CaptureF7RollbackBaselineBeforeFirstDirty(currentScope);
            _pendingF7EditorSchemaScope = currentScope;

            _pendingF7EditorSchemaDirty = SchemaDirtyPolicy.Evaluate
            (
                _pendingF7EditorSchemaDirty,
                true,
                false,
                false
            ).IsDirty;
        }

        private void RefreshF7MetadataAfterTransactionClosed(string completedAction)
        {
            var currentScope = GetSharedSchemaDatabaseScope();

            if (_pendingF7EditorSchemaDirty && !string.Equals(_pendingF7EditorSchemaScope, currentScope, StringComparison.Ordinal))
            {
                _pendingF7EditorSchemaDirty = false;
                _pendingF7EditorSchemaScope = string.Empty;

                ResetF7RollbackBaseline();
            }

            //Do not apply an old Add Column refresh requirement to a new DB scope.
            if (_pendingF7AddColumnMetadataRefresh && !string.IsNullOrWhiteSpace(_f7RollbackBaselineScope) && !string.Equals(_f7RollbackBaselineScope, currentScope, StringComparison.Ordinal))
            {
                _pendingF7AddColumnMetadataRefresh = false;

                ResetF7RollbackBaseline();
            }

            var hasTransactionBoundDirty = _pendingF7EditorSchemaDirty || _pendingF7AddColumnMetadataRefresh;
            var sameScope = string.Equals(_f7RollbackBaselineScope, currentScope, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(currentScope);
            var trustedBaseline = _f7RollbackBaselineEligible && ReferenceEquals(_f7RollbackBaselineSnapshot, _sharedSchemaMetadataSnapshot) && IsF7RollbackSnapshotTrusted();

            var canSkip = SchemaDirtyPolicy.ShouldSkipMetadataReloadForConfirmedRollback
            (
                _currentSourceType,
                completedAction,
                true, //This method is called only from the confirmed transaction-closed event.
                hasTransactionBoundDirty,
                trustedBaseline,
                sameScope,
                !_f7RollbackBaselineEligible
            );

            //A completed transaction consumes the rollback baseline, even if the subsequent metadata reload is unavailable or fails.
            ResetF7RollbackBaseline();

            if (canSkip)
            {
                _pendingF7EditorSchemaDirty = false;
                _pendingF7EditorSchemaScope = string.Empty;
                _pendingF7AddColumnMetadataRefresh = false;

                if (_sharedSchemaPreviewStatus != null)
                {
                    _sharedSchemaPreviewStatus.Text = "F7: schema unchanged after confirmed rollback.";
                }

                return;
            }

            var decision = SchemaDirtyPolicy.Evaluate
            (
                _pendingF7EditorSchemaDirty,
                false,
                true,
                false
            );

            if (decision.ShouldRefresh || _pendingF7AddColumnMetadataRefresh)
            {
                //If F7 is unavailable or the loader fails, its pending flags stay set.
                RefreshSharedSchemaExplorerPreviewMetadata();
            }
        }

        private void ClearF7SchemaDirtyAfterSuccessfulReload()
        {
            _pendingF7EditorSchemaDirty = SchemaDirtyPolicy.Evaluate
            (
                _pendingF7EditorSchemaDirty,
                false,
                true,
                true
            ).IsDirty;

            _pendingF7EditorSchemaScope = string.Empty;
            _pendingF7AddColumnMetadataRefresh = false;

            ResetF7RollbackBaseline();
        }
    }
}
