using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowChromeNativeTItleBar
{
    public static class ConfigManager
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private static readonly string IniPath = Path.Combine(AppContext.BaseDirectory, "settings.ini");

        public static bool GetCustomTitleBarSetting()
        {
            var sb = new StringBuilder(255);
            GetPrivateProfileString("Settings", "CustomTitleBar", "True", sb, sb.Capacity, IniPath);
            if (bool.TryParse(sb.ToString(), out bool result))
            {
                return result;
            }
            return true;
        }

        public static void SaveCustomTitleBarSetting(bool isCustom)
        {
            WritePrivateProfileString("Settings", "CustomTitleBar", isCustom.ToString(), IniPath);
        }
    }
}
