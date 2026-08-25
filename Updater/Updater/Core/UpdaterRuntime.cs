namespace Updater.Core
{
    internal static class UpdaterRuntime
    {
        public static UpdaterRequest Request { get; set; }

        public static string RequestPath { get; set; }

        public static string StartupError { get; set; }
    }
}