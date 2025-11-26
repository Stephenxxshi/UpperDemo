using Microsoft.Win32;

namespace Plant01.Core.Utilities
{
    /// <summary>
    /// 硬件信息
    /// </summary>
    public static class Win32Utility
    {
        #region 添加开机自动运行
        /// <summary>
        /// 添加开机自动运行
        /// </summary>
        public static void SetAutoStart()
        {
            try
            {
                string appPath = Environment.ProcessPath;
                string appName = Path.GetFileName(appPath);
#if x64
                using RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run", true);
#else
                using RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
#endif
                if (key.GetValue(appName) != null)
                {
                    key.DeleteValue(appName);
                }
                key.SetValue(appName, appPath);
                key.Close();
            }
            catch
            {
                ;
            }
        }
        #endregion
    }
}