using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GpTaskbar.Services
{
    public class AppConfig
    {
        public List<string> StockSymbols { get; set; } = new List<string>();
        public int RefreshInterval { get; set; } = 60; // 秒
        public int WindowPositionX { get; set; } = -1; // 窗口X坐标，-1表示使用默认位置
        public int WindowPositionY { get; set; } = -1; // 窗口Y坐标，-1表示使用默认位置
        
        // 股票信息显示选项
        public bool ShowSymbol { get; set; } = false;    // 显示股票代码
        public bool ShowName { get; set; } = false;     // 显示股票名称
        public bool ShowPrice { get; set; } = true;     // 显示股票价格
        public bool ShowChangePercent { get; set; } = true; // 显示涨跌幅
        public bool ShowChange { get; set; } = true;    // 显示涨跌差价
        
        // 新增配置选项
        public bool AutoStartup { get; set; } = false;  // 开机自启
        public string DisplayStartTime { get; set; } = "09:12"; // 显示开始时间
        public string DisplayEndTime { get; set; } = "15:10";   // 显示结束时间
        public bool EnableDisplayTimeRange { get; set; } = false; // 启用时间段控制
    }

    public class ConfigService
    {
        private readonly string _configPath;

        public ConfigService()
        {
            // 获取程序所在目录
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _configPath = Path.Combine(appDirectory, "config.json");
        }

        public AppConfig LoadConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    return CreateDefaultConfig();
                }

                var json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? CreateDefaultConfig();
            }
            catch (Exception)
            {
                return CreateDefaultConfig();
            }
        }

        public void SaveConfig(AppConfig config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"保存配置失败: {ex.Message}");
            }
        }

        private AppConfig CreateDefaultConfig()
        {
            var defaultConfig = new AppConfig
            {
                StockSymbols = new List<string> { "sh000001", "sz399001" },
                RefreshInterval = 5
            };

            SaveConfig(defaultConfig);
            return defaultConfig;
        }
    }
}