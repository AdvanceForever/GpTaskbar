using System;
using Microsoft.Win32;
using System.IO;

namespace GpTaskbar.Services
{
    public class AutoStartupService
    {
        private const string AppName = "GpTaskbar";
        private readonly string _executablePath;

        public AutoStartupService()
        {
            _executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        public bool IsAutoStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
                return key?.GetValue(AppName) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool EnableAutoStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    key.SetValue(AppName, _executablePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"启用开机自启失败: {ex.Message}");
                return false;
            }
        }

        public bool DisableAutoStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    key.DeleteValue(AppName, false);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"禁用开机自启失败: {ex.Message}");
                return false;
            }
        }

        public void UpdateAutoStartup(bool enable)
        {
            if (enable)
            {
                EnableAutoStartup();
            }
            else
            {
                DisableAutoStartup();
            }
        }
    }
}