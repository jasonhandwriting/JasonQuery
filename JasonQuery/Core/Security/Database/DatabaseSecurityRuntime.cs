using System;

namespace JasonQuery.Core.Security.Database
{
    public static class DatabaseSecurityRuntime
    {
        public static DatabaseSecurityRuntimeMode Mode { get; private set; } = DatabaseSecurityRuntimeMode.Uninitialized;

        public static bool IsLegacy => Mode == DatabaseSecurityRuntimeMode.LegacyDefault || Mode == DatabaseSecurityRuntimeMode.LegacyCustom;

        public static bool IsV2 => Mode == DatabaseSecurityRuntimeMode.WindowsCurrentUser || Mode == DatabaseSecurityRuntimeMode.CustomPassword;

        internal static void SetLegacyDefault()
        {
            Mode = DatabaseSecurityRuntimeMode.LegacyDefault;
        }

        internal static void SetLegacyCustom()
        {
            Mode = DatabaseSecurityRuntimeMode.LegacyCustom;
        }

        internal static void SetV2(DatabaseSecurityMode mode)
        {
            switch (mode)
            {
                case DatabaseSecurityMode.WindowsCurrentUser:
                //20260902
                    Mode = DatabaseSecurityRuntimeMode.WindowsCurrentUser;
                    break;

                case DatabaseSecurityMode.CustomPassword:

                    Mode = DatabaseSecurityRuntimeMode.CustomPassword;
                    break;

                default:

                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported database security mode.");

            }
        }
    }
}
