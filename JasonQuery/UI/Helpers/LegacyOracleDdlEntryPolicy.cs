using JasonQuery.Core.Database.Connection;

namespace JasonQuery.UI.Helpers
{
    /// <summary>
    /// Denies legacy Oracle structural DDL from UI routes without a qualified
    /// metadata owner, selected object identity and connection snapshot.
    /// Other providers retain their established form entry behavior only
    /// while the active source type is unchanged.
    /// </summary>
    internal static class LegacyOracleDdlEntryPolicy
    {
        public static bool CanOpenStructuralDdl(DataSourceType menuSource, DataSourceType currentSource)
        {
            if (menuSource != currentSource)
            {
                return false;
            }

            switch (menuSource)
            {
                case DataSourceType.PostgreSql:
                case DataSourceType.SqlServer:
                case DataSourceType.MySql:
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
