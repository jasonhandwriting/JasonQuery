using System;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    public static class JasonQueryDbSecurityRuntime
    {
        public static JasonQueryDbSecurityRuntimeMode Mode { get; private set; } = JasonQueryDbSecurityRuntimeMode.Uninitialized;

        public static bool IsLegacy => Mode == JasonQueryDbSecurityRuntimeMode.LegacyDefault || Mode == JasonQueryDbSecurityRuntimeMode.LegacyCustom;

        public static bool IsV2 => Mode == JasonQueryDbSecurityRuntimeMode.WindowsCurrentUser || Mode == JasonQueryDbSecurityRuntimeMode.CustomPassword;

        internal static void SetLegacyDefault()
        {
            Mode = JasonQueryDbSecurityRuntimeMode.LegacyDefault;
        }

        internal static void SetLegacyCustom()
        {
            Mode = JasonQueryDbSecurityRuntimeMode.LegacyCustom;
        }

        internal static void SetV2(JasonQueryDbSecurityMode mode)
        {
            switch (mode)
            {
                case JasonQueryDbSecurityMode.WindowsCurrentUser:
                    {
                        Mode = JasonQueryDbSecurityRuntimeMode.WindowsCurrentUser;
                        break;
                    }
                case JasonQueryDbSecurityMode.CustomPassword:
                    {
                        Mode = JasonQueryDbSecurityRuntimeMode.CustomPassword;
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported database security mode.");
                    }
            }
        }
    }
}
