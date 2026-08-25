using C1.Win.C1TrueDBGrid;
using JasonQuery.Core.Config;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Localization;
using JasonQuery.Core.QueryEngine.Editor.Analysis;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using JasonQuery.UI.QueryEditor.AutoComplete.Contexts;
using JasonQuery.UI.QueryEditor.AutoComplete.Filters;
using JasonQuery.UI.QueryEditor.AutoComplete.Popup;
using JasonQuery.UI.QueryEditor.AutoComplete.State;
using JasonQuery.UI.QueryEditor.AutoComplete.Workflows;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private string _autoCompleteResolveErrorStatusText = string.Empty;

        private sealed class QueryEditorAutoCompleteWorkflowHostAdapter : IQueryEditorAutoCompleteWorkflowHost
        {
            private readonly QueryForm _owner;

            public QueryEditorAutoCompleteWorkflowHostAdapter(QueryForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public int EditorCurrentPosition
            {
                get { return _owner.editor.CurrentPosition; }
            }

            public QueryEditorAutoCompletePeriodResolveContext CreatePeriodResolveContext()
            {
                return _owner._autoCompleteResolveContextFactory.CreatePeriodResolveContext();
            }

            public QueryEditorAutoCompleteSpaceResolveContext CreateSpaceResolveContext()
            {
                return _owner._autoCompleteResolveContextFactory.CreateSpaceResolveContext();
            }

            public void HidePeriodPopup()
            {
                _owner.HidePeriodAutoCompletePopup();
            }

            public void HideSpacePopup()
            {
                _owner.HideSpaceAutoCompletePopup();
            }

            public void ShowResolveErrorStatus()
            {
                _owner.ShowAutoCompleteResolveErrorStatus();
            }

            public void ClearResolveErrorStatus()
            {
                _owner.ClearAutoCompleteResolveErrorStatus();
            }

            public void ShowException(Exception ex)
            {
                _owner.ShowExceptionMessage(ex);
            }
        }

        private void ShowAutoCompleteResolveErrorStatus()
        {
            var message = LocalizationHelper.GetLanguageString
            (
                "AutoComplete failed to retrieve suggestions. See SQL History for details.",
                "form",
                GetType().Name,
                "msg",
                "AutoCompleteResolveError",
                "Text"
            );

            _autoCompleteResolveErrorStatusText = message;

            SetEditorStatusBarInfo(message, Color.DarkRed);

            //Reset the existing status-bar timeout, including when the same error occurs again.
            lblInfoEditor.Tag = message;
            c1StatusBar2.Tag = MyGlobal.DateTimeNow();
        }

        private void ClearAutoCompleteResolveErrorStatus()
        {
            if (string.IsNullOrEmpty(_autoCompleteResolveErrorStatusText))
            {
                return;
            }

            if (!string.Equals(lblInfoEditor.Text, _autoCompleteResolveErrorStatusText, StringComparison.Ordinal))
            {
                _autoCompleteResolveErrorStatusText = string.Empty;
                return;
            }

            SetEditorStatusBarInfo(string.Empty, Color.Black);

            lblInfoEditor.Tag = string.Empty;
            c1StatusBar2.Tag = string.Empty;
            _autoCompleteResolveErrorStatusText = string.Empty;
        }

        private sealed class QueryEditorAutoCompleteFilterCoordinatorHostAdapter : IQueryEditorAutoCompleteFilterCoordinatorHost
        {
            private readonly QueryForm _owner;

            public QueryEditorAutoCompleteFilterCoordinatorHostAdapter(QueryForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public string EditorText
            {
                get { return _owner.editor.Text ?? string.Empty; }
            }

            public int EditorCurrentPosition
            {
                get { return _owner.editor.CurrentPosition; }
            }

            public DataTable PeriodAutoCompleteTable
            {
                get { return _owner._dtPeriodAutoCompleteTable; }
            }

            public DataTable SpaceAutoCompleteTable
            {
                get { return _owner._dtSpaceAutoCompleteTable; }
            }

            public int PeriodTriggerPosition
            {
                get { return _owner._periodAutoCompleteSession?.TriggerPosition ?? 0; }
            }

            public int SpaceTriggerPosition
            {
                get { return _owner._spaceAutoCompleteSession?.TriggerPosition ?? 0; }
            }

            public QueryEditorAutoCompleteSession GetActiveAutoCompleteSession()
            {
                return _owner.GetActiveAutoCompleteSession();
            }

            public void HidePeriodPopup()
            {
                _owner.HidePeriodAutoCompletePopup();
            }

            public void HideSpacePopup()
            {
                _owner.HideSpaceAutoCompletePopup();
            }

            public void ShowException(Exception ex)
            {
                _owner.ShowExceptionMessage(ex);
            }
        }

        private sealed class QueryEditorAutoCompleteResolveContextFactoryHostAdapter : IQueryEditorAutoCompleteResolveContextFactoryHost
        {
            private readonly QueryForm _owner;

            public QueryEditorAutoCompleteResolveContextFactoryHostAdapter(QueryForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public ScintillaNET.Scintilla Editor
            {
                get { return _owner.editor; }
            }

            public DataSourceType CurrentSourceType
            {
                get { return _owner._currentSourceType; }
            }

            public bool IsDataSourceSqlServer
            {
                get { return _owner.IsSqlServer; }
            }

            public bool IsDataSourceMySql
            {
                get { return _owner.IsMySql; }
            }

            public Func<bool, string> SelectCurrentBlock
            {
                get { return _owner.SelectCurrentBlock; }
            }

            public Func<string, bool, bool, string> FormatSql
            {
                get { return (sql, toUpper, space) => QueryEditorSqlNormalizer.GetSingleLineSql(sql, toUpper, space); }
            }

            public string ConnectionDatabase
            {
                get { return DatabaseSqlExecutor.DatabaseName; }
            }

            public DataTable TableAndViews
            {
                get { return DatabaseSqlExecutor.dtTableAndViews; }
            }
        }

        private sealed class QueryEditorPeriodTriggerCoordinatorHostAdapter : IQueryEditorPeriodTriggerCoordinatorHost
        {
            private readonly QueryForm _owner;

            public QueryEditorPeriodTriggerCoordinatorHostAdapter(QueryForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public bool IsEditorFocused
            {
                get { return _owner.editor.Focused; }
            }

            public bool IsAutoListMembersEnabled
            {
                get { return MyGlobal.IsAutoListMembers; }
            }

            public bool IsSqlServer
            {
                get { return _owner.IsSqlServer; }
            }

            public bool IsMySql
            {
                get { return _owner.IsMySql; }
            }

            public string EditorText
            {
                get { return _owner.editor.Text ?? string.Empty; }
            }

            public int EditorCurrentPosition
            {
                get { return _owner.editor.CurrentPosition; }
            }

            public QueryEditorAutoCompleteSession GetActiveAutoCompleteSession()
            {
                return _owner.GetActiveAutoCompleteSession();
            }

            public bool HandleAutoCompletePeriodKey(int triggerPositionOverride)
            {
                return _owner.HandleAutoCompletePeriodKey(triggerPositionOverride);
            }

            public void HideSpacePopup()
            {
                _owner.HideSpaceAutoCompletePopup();
            }

            public void HidePeriodPopup()
            {
                _owner.HidePeriodAutoCompletePopup();
            }

            public void ShowException(Exception ex)
            {
                _owner.ShowExceptionMessage(ex);
            }
        }

        private sealed class QueryEditorAutoCompleteSessionCoordinatorHostAdapter : IQueryEditorAutoCompleteSessionCoordinatorHost
        {
            private readonly QueryForm _owner;

            public QueryEditorAutoCompleteSessionCoordinatorHostAdapter(QueryForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public QueryEditorAutoCompleteUiContext AutoCompleteContext
            {
                get { return _owner._autoCompleteContext; }
            }

            public QueryEditorAutoCompleteSession PeriodSession
            {
                get { return _owner._periodAutoCompleteSession; }
            }

            public QueryEditorAutoCompleteSession SpaceSession
            {
                get { return _owner._spaceAutoCompleteSession; }
            }

            public void ShowException(Exception ex)
            {
                _owner.ShowExceptionMessage(ex);
            }
        }

        private sealed class QueryEditorAutoCompletePopupContextFactoryHostAdapter : IQueryEditorAutoCompletePopupContextFactoryHost
        {
            private readonly QueryForm _owner;

            public QueryEditorAutoCompletePopupContextFactoryHostAdapter(QueryForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public Form OwnerForm
            {
                get { return _owner; }
            }

            public ScintillaNET.Scintilla Editor
            {
                get { return _owner.editor; }
            }

            public string QueryEditorFontSize
            {
                get { return _owner._queryEditorFontSize; }
            }

            public int SplitterDistance
            {
                get { return _owner.splitContainer1.SplitterDistance; }
            }
        }

        private sealed class QueryEditorAutoCompleteVisiblePopupRefreshWorkflowHostAdapter : IQueryEditorAutoCompleteVisiblePopupRefreshWorkflowHost
        {
            private readonly QueryForm _owner;

            public QueryEditorAutoCompleteVisiblePopupRefreshWorkflowHostAdapter(QueryForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public QueryEditorAutoCompleteSession GetActiveAutoCompleteSession()
            {
                return _owner.GetActiveAutoCompleteSession();
            }

            public DataTable BuildPeriodAutoCompleteData(string keyword)
            {
                return _owner.BuildPeriodAutoCompleteData(keyword);
            }

            public DataTable BuildSpaceAutoCompleteData(string keyword)
            {
                return _owner.BuildSpaceAutoCompleteData(keyword);
            }
        }

        private sealed class QueryEditorAutoCompleteEditorKeyUpWorkflowHostAdapter : IQueryEditorAutoCompleteEditorKeyUpWorkflowHost
        {
            private readonly QueryForm _owner;

            public QueryEditorAutoCompleteEditorKeyUpWorkflowHostAdapter(QueryForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public bool IsAutoListMembersEnabled
            {
                get { return MyGlobal.IsAutoListMembers; }
            }

            public void ClearHighlightSelectionCopyState()
            {
                _owner.ClearHighlightSelectionCopyState();
            }

            public bool ShouldSkipEditorContentCheckOnKeyUp(KeyEventArgs e)
            {
                return _owner.ShouldSkipEditorContentCheckOnKeyUp(e);
            }

            public bool TryConsumeCompoundCtrlShiftKeyUp()
            {
                return _owner.TryConsumeCompoundCtrlShiftKeyUp();
            }

            public bool ShouldIgnoreEditorKeyUp(KeyEventArgs e)
            {
                return _owner.ShouldIgnoreEditorKeyUp(e);
            }

            public void HandleAutoReplaceOnSpaceKeyUp(KeyEventArgs e)
            {
                _owner.HandleAutoReplaceOnSpaceKeyUp(e);
            }

            public bool TryHandleAutoCompleteEditorKeyUp(KeyEventArgs e, ref bool checkEditorContent)
            {
                return _owner._autoCompleteEditorKeyUpHandler.TryHandle(_owner.CreateEditorKeyUpContext(), e, ref checkEditorContent);
            }

            public bool TryTriggerPeriodAutoCompleteOnIdentifierKeyUp(KeyEventArgs e)
            {
                return _owner._periodTriggerCoordinator.TryTriggerPeriodAutoCompleteOnIdentifierKeyUp(e);
            }

            public bool TryCloseActiveAutoCompletePopupOnInvalidCharKeyUp(KeyEventArgs e)
            {
                return _owner._autoCompleteFilterCoordinator.TryCloseActiveAutoCompletePopupOnInvalidCharKeyUp(e);
            }

            public void RefreshVisibleAutoCompleteOnEditorKeyUp(KeyEventArgs e)
            {
                _owner._visiblePopupRefreshWorkflow.RefreshOnEditorKeyUp(e);
            }

            public void CheckEditorContent()
            {
                _owner.CheckEditorContent();
            }

            public void ShowException(Exception ex)
            {
                _owner.ShowExceptionMessage(ex);
            }
        }

        private sealed class QueryEditorAutoCompletePopupPresenterHostFactoryHostAdapter : IQueryEditorAutoCompletePopupPresenterHostFactoryHost
        {
            private readonly QueryForm _owner;

            public QueryEditorAutoCompletePopupPresenterHostFactoryHostAdapter(QueryForm owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            public int EditorTextLength
            {
                get { return _owner.editor.TextLength; }
            }

            public void CaptureAutoCompleteMousePosition()
            {
                _owner._autoCompleteMousePosition = Cursor.Position;
            }

            public QueryEditorAutoCompleteSession PeriodSession
            {
                get { return _owner._periodAutoCompleteSession; }
            }

            public QueryEditorAutoCompleteSession SpaceSession
            {
                get { return _owner._spaceAutoCompleteSession; }
            }

            public C1TrueDBGrid PeriodGrid
            {
                get { return _owner.c1GridAutoCompleteForPeriod; }
            }

            public C1TrueDBGrid SpaceGrid
            {
                get { return _owner.c1GridAutoCompleteForSpace; }
            }

            public DataTable PeriodAutoCompleteTable
            {
                get { return _owner._dtPeriodAutoCompleteTable; }
                set { _owner._dtPeriodAutoCompleteTable = value; }
            }

            public DataTable SpaceAutoCompleteTable
            {
                get { return _owner._dtSpaceAutoCompleteTable; }
                set { _owner._dtSpaceAutoCompleteTable = value; }
            }

            public QueryEditorAutoCompletePopupContextFactory PopupContextFactory
            {
                get { return _owner._autoCompletePopupContextFactory; }
            }

            public QueryEditorAutoCompleteFilterCoordinator FilterCoordinator
            {
                get { return _owner._autoCompleteFilterCoordinator; }
            }

            public QueryEditorPeriodTriggerCoordinator PeriodTriggerCoordinator
            {
                get { return _owner._periodTriggerCoordinator; }
            }

            public int[] PeriodPopupFetchStyleColumns
            {
                get { return _periodPopupFetchStyleColumns; }
            }

            public int[] SpacePopupFetchStyleColumns
            {
                get { return _spacePopupFetchStyleColumns; }
            }
        }
    }
}
