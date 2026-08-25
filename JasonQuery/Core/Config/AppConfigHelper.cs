namespace JasonQuery.Core.Config
{
    public static class AppConfigHelper
    {
        public static string LocalVersion = string.Empty; //JasonQuery.exe 的版本號碼
        public static string JasonQueryVersion = "JasonQuery";
        public static string SupportInfo = string.Empty;
        public static string LogFileName = string.Empty; //20250615 Log FileName for 記錄執行日誌
        public static string BackupPath = string.Empty; //20240301 for 定時備份
        public static bool IsBackupFile = true; //20240301 for 定時備份
        public static int BackupSeconds = 7; //20240301 for 每隔幾秒定時備份
        public static bool IsAfterPasteFocusOnQueryEditor = true;
        public static bool IsNotCommitYet = false;
        public static bool HasMultiOpenSchemaBrowser = false; //20231012 是否已經開啟了多個 SchemaBrowser？
        public static bool AskBeforeOpenUnsavedFiles = false; //20240721 每次啟動時，是否要詢問使用者是否要開啟「所有未儲存的文件」
        public static bool HasGenerateLogFile = false; //20250615

        public static bool IsMainFormMaximized = true;
        public static int MainFormWidth = 1024;
        public static int MainFormHeight = 768;
        public static int MainFormLocationX = 100;
        public static int MainFormLocationY = 100;
        public static int MainFormLeft = 0; //主畫面 Left 值
        public static int MainFormTop = 0; //主畫面 Top 值
        public static int MainFormIconStyle = 0; //20240201 每個資料庫連線，獨立的主畫面圖示樣式；20240210 預設值為第 0 組圖示

        public static bool IsChangeColorThemeNeedRestart = false;
        public static int LargeTextPreviewLength { get; set; } = 50; //50 chars ≈ Preivew 視窗的字串長度邊界
    }
}
