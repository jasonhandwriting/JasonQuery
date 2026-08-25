using System.Data;

namespace JasonQuery.Core.Data.DataTables
{
    internal static class DataTableLifecycleHelper
    {
        public static void DisposeDataTable(ref DataTable dt)
        {
            if (dt == null)
            {
                return;
            }

            try
            {
                dt.Dispose();
            }
            finally
            {
                dt = null;
            }
        }

        public static void ReplaceDataTable(ref DataTable current, ref DataTable replacement)
        {
            if (replacement == null)
            {
                return;
            }

            if (ReferenceEquals(current, replacement))
            {
                replacement = null;
                return;
            }

            var old = current;

            current = replacement;
            replacement = null;

            DisposeDataTable(ref old);
        }
    }
}
