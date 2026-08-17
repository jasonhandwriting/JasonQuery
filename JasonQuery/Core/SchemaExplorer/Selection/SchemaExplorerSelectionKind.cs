namespace JasonQuery.Core.SchemaExplorer.Selection
{
    public enum SchemaExplorerSelectionKind
    {
        None = 0,

        //點到連線名稱、SchemaType 群組、SchemaNode 群組等，不是實際物件
        GroupNode = 1,

        //點到可顯示內容的 Table / View / Function / Procedure / Trigger / Index / Package
        SchemaObject = 2,

        //點到欄位資訊列，例如 MyGlobal.IsShowColumnInfo 開啟後顯示的欄位列
        ColumnInfo = 3,

        //從 QueryForm 或外部指定物件開啟，不是從 Grid 點擊
        ExternalObject = 4,

        //解析失敗或資料不足
        Invalid = 9
    }
}