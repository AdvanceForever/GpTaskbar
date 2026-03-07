using System;
using System.Drawing;
using System.Windows.Forms;
using GpTaskbar.Models;
using GpTaskbar.Services;

namespace GpTaskbar.Forms
{
    public partial class MainForm : Form
    {
        private readonly StockService _stockService;
        private readonly ConfigService _configService;
        private readonly TaskbarService _taskbarService;
        private NotifyIcon? _notifyIcon;
        private System.Windows.Forms.Timer? _refreshTimer;
        private AppConfig _config;

        public MainForm()
        {
            InitializeComponent();
            
            _stockService = new StockService();
            _configService = new ConfigService();
            _taskbarService = new TaskbarService();
            _config = _configService.LoadConfig();
            
            InitializeNotifyIcon();
            InitializeTimer();
            
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "股票监控",
                Visible = true
            };

            _notifyIcon.DoubleClick += (s, e) => ShowConfigurationForm();
            
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("配置股票", null, (s, e) => ShowConfigurationForm());
            contextMenu.Items.Add("退出", null, (s, e) => Application.Exit());
            
            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void InitializeTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer
            {
                Interval = _config.RefreshInterval * 1000
            };
            
            _refreshTimer.Tick += async (s, e) => await RefreshStockData();
            _refreshTimer.Start();
            
            // 立即获取一次数据
            _ = RefreshStockData();
        }

        private async Task RefreshStockData()
        {
            try
            {
                if (_config.StockSymbols.Count == 0) return;

                var stocks = await _stockService.GetMultipleStocksAsync(_config.StockSymbols);
                UpdateNotifyIconText(stocks);
            }
            catch (Exception ex)
            {
                // 将错误信息发送到配置窗体
                if (_configForm != null && !_configForm.IsDisposed)
                {
                    _configForm.SetErrorText($"获取股票数据失败: {ex.Message}");
                }
            }
        }

        private void UpdateNotifyIconText(List<StockData> stocks)
        {
            if (stocks.Count == 0) return;

            // 直接传递股票数据列表给任务栏服务
            _taskbarService.UpdateStockDisplay(stocks);
        }

        private ConfigurationForm? _configForm;

        private void ShowConfigurationForm()
        {
            // 检查是否已经打开配置窗口
            if (_configForm != null && !_configForm.IsDisposed)
            {
                _configForm.Activate();
                return;
            }

            // 暂停定时置顶
            _taskbarService?.PauseTopmostTimer();

            _configForm = new ConfigurationForm(_configService, _config);
            _configForm.FormClosed += (s, e) => OnConfigFormClosed();
            _configForm.Show();
        }

        private void OnConfigFormClosed()
        {
            if (_configForm != null && !_configForm.IsDisposed)
            {
                _config = _configForm.GetUpdatedConfig();
                if (_refreshTimer != null)
                    _refreshTimer.Interval = _config.RefreshInterval * 1000;
                _ = RefreshStockData();
            }
            
            // 恢复定时置顶
            _taskbarService?.ResumeTopmostTimer();
            
            _configForm = null;
        }

        // 公开方法供TaskbarService调用
        public void ShowConfigForm()
        {
            ShowConfigurationForm();
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false); // 始终隐藏主窗体
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
            }
            
            base.OnFormClosing(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 200);
            this.Name = "MainForm";
            this.Text = "股票监控";
            this.ResumeLayout(false);
        }
    }
}