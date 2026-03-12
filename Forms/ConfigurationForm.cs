using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GpTaskbar.Services;

namespace GpTaskbar.Forms
{
    public partial class ConfigurationForm : Form
    {
        private readonly ConfigService _configService;
        private AppConfig _config;
        private TextBox? _symbolsTextBox;
        private NumericUpDown? _intervalNumeric;
        private Button? _saveButton;
        private Button? _cancelButton;
        private TextBox? _errorTextBox;
        
        // 显示选项复选框
        private CheckBox? _showSymbolCheckBox;
        private CheckBox? _showNameCheckBox;
        private CheckBox? _showPriceCheckBox;
        private CheckBox? _showChangePercentCheckBox;
        private CheckBox? _showChangeCheckBox;
        
        // 新增配置选项
        private CheckBox? _autoStartupCheckBox;
        private CheckBox? _enableDisplayTimeRangeCheckBox;
        private TextBox? _displayStartTimeTextBox;
        private TextBox? _displayEndTimeTextBox;

        public ConfigurationForm(ConfigService configService, AppConfig currentConfig)
        {
            _configService = configService;
            _config = currentConfig;
            InitializeComponent();
            LoadConfiguration();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // 窗体设置
            this.Text = "股票配置";
            this.Size = new Size(520, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // 股票符号标签
            var symbolsLabel = new Label
            {
                Text = "股票代码 (用逗号分隔):",
                Location = new Point(20, 20),
                Size = new Size(150, 20)
            };
            
            // 股票符号文本框
            _symbolsTextBox = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(470, 60),
                Multiline = true,
                Height = 60
            };
            
            // 刷新间隔标签
            var intervalLabel = new Label
            {
                Text = "刷新间隔 (秒):",
                Location = new Point(20, 120),
                Size = new Size(100, 20)
            };
            
            // 刷新间隔数值框
            _intervalNumeric = new NumericUpDown
            {
                Location = new Point(120, 120),
                Size = new Size(80, 20),
                Minimum = 5,
                Maximum = 3600,
                Value = 5
            };
            
            // 股票信息显示选项
            var displayOptionsLabel = new Label
            {
                Text = "显示字段选项:",
                Location = new Point(20, 160),
                Size = new Size(150, 20),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };
            
            _showSymbolCheckBox = new CheckBox
            {
                Text = "股票代码",
                Location = new Point(20, 185),
                Size = new Size(80, 20),
                Checked = _config.ShowSymbol
            };
            
            _showNameCheckBox = new CheckBox
            {
                Text = "股票名称",
                Location = new Point(110, 185),
                Size = new Size(80, 20),
                Checked = _config.ShowName
            };
            
            _showPriceCheckBox = new CheckBox
            {
                Text = "股票价格",
                Location = new Point(200, 185),
                Size = new Size(80, 20),
                Checked = _config.ShowPrice
            };
            
            _showChangePercentCheckBox = new CheckBox
            {
                Text = "涨跌幅",
                Location = new Point(20, 210),
                Size = new Size(80, 20),
                Checked = _config.ShowChangePercent
            };
            
            _showChangeCheckBox = new CheckBox
            {
                Text = "涨跌差价",
                Location = new Point(110, 210),
                Size = new Size(80, 20),
                Checked = _config.ShowChange
            };
            
            // 开机自启选项
            var autoStartupLabel = new Label
            {
                Text = "开机自启:",
                Location = new Point(20, 250),
                Size = new Size(80, 20),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };
            
            _autoStartupCheckBox = new CheckBox
            {
                Text = "开机自动启动",
                Location = new Point(100, 250),
                Size = new Size(120, 20),
                Checked = _config.AutoStartup
            };
            
            // 显示时间段控制
            var timeRangeLabel = new Label
            {
                Text = "显示时间段控制:",
                Location = new Point(20, 280),
                Size = new Size(120, 20),
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold)
            };
            
            _enableDisplayTimeRangeCheckBox = new CheckBox
            {
                Text = "启用时间段控制",
                Location = new Point(150, 280),
                Size = new Size(120, 20),
                Checked = _config.EnableDisplayTimeRange
            };
            _enableDisplayTimeRangeCheckBox.CheckedChanged += (s, e) => UpdateTimeRangeControls();
            
            var startTimeLabel = new Label
            {
                Text = "开始时间:",
                Location = new Point(20, 310),
                Size = new Size(60, 20)
            };
            
            _displayStartTimeTextBox = new TextBox
            {
                Text = _config.DisplayStartTime,
                Location = new Point(80, 310),
                Size = new Size(50, 20)
            };
            
            var endTimeLabel = new Label
            {
                Text = "结束时间:",
                Location = new Point(150, 310),
                Size = new Size(60, 20)
            };
            
            _displayEndTimeTextBox = new TextBox
            {
                Text = _config.DisplayEndTime,
                Location = new Point(210, 310),
                Size = new Size(50, 20)
            };
            
            var timeFormatLabel = new Label
            {
                Text = "格式: HH:mm (24小时制)",
                Location = new Point(270, 310),
                Size = new Size(150, 20),
                Font = new Font("Microsoft Sans Serif", 8F),
                ForeColor = Color.Gray
            };
            
            // 保存按钮
            _saveButton = new Button
            {
                Text = "保存",
                Location = new Point(200, 480),
                Size = new Size(75, 30)
            };
            _saveButton.Click += SaveButton_Click;
            
            // 取消按钮
            _cancelButton = new Button
            {
                Text = "取消",
                Location = new Point(285, 480),
                Size = new Size(75, 30)
            };
            _cancelButton.Click += (s, e) => this.Close();
            
            // 示例标签
            var exampleLabel = new Label
            {
                Text = "示例: sh000001 (上证指数), sz399001 (深证成指), sh600036 (招商银行)",
                Location = new Point(20, 340),
                Size = new Size(400, 30),
                Font = new Font("Microsoft Sans Serif", 8F),
                ForeColor = Color.Gray
            };
            
            // 错误信息标签
            var errorLabel = new Label
            {
                Text = "错误信息:",
                Location = new Point(20, 380),
                Size = new Size(100, 20)
            };
            
            // 错误信息文本框
            _errorTextBox = new TextBox
            {
                Location = new Point(20, 405),
                Size = new Size(470, 60),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.LightGray,
                Font = new Font("Consolas", 9F)
            };
            
            // 添加控件到窗体
            this.Controls.AddRange(new Control[] {
                symbolsLabel, _symbolsTextBox, intervalLabel, _intervalNumeric,
                 displayOptionsLabel, _showSymbolCheckBox, _showNameCheckBox,
                _showPriceCheckBox, _showChangePercentCheckBox, _showChangeCheckBox,
                autoStartupLabel, _autoStartupCheckBox, timeRangeLabel, _enableDisplayTimeRangeCheckBox,
                startTimeLabel, _displayStartTimeTextBox, endTimeLabel, _displayEndTimeTextBox,
                timeFormatLabel, exampleLabel, errorLabel, _errorTextBox, _saveButton, _cancelButton
            });
            
            // 初始化时间范围控件状态
            UpdateTimeRangeControls();
            
            this.ResumeLayout(false);
        }

        private void LoadConfiguration()
        {
            if (_symbolsTextBox != null)
                _symbolsTextBox.Text = string.Join(", ", _config.StockSymbols);
            if (_intervalNumeric != null)
                _intervalNumeric.Value = _config.RefreshInterval;
        }

        private void UpdateTimeRangeControls()
        {
            bool enabled = _enableDisplayTimeRangeCheckBox?.Checked ?? false;
            if (_displayStartTimeTextBox != null)
                _displayStartTimeTextBox.Enabled = enabled;
            if (_displayEndTimeTextBox != null)
                _displayEndTimeTextBox.Enabled = enabled;
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_symbolsTextBox == null || _intervalNumeric == null )
                {
                    MessageBox.Show("控件初始化失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var symbols = _symbolsTextBox.Text
                    .Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                if (!symbols.Any())
                {
                    MessageBox.Show("请输入至少一个股票代码", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 验证时间格式
                if (_enableDisplayTimeRangeCheckBox?.Checked == true)
                {
                    if (!TimeSpan.TryParseExact(_displayStartTimeTextBox?.Text ?? "", "hh\\:mm", null, out _))
                    {
                        MessageBox.Show("开始时间格式不正确，请使用HH:mm格式", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    if (!TimeSpan.TryParseExact(_displayEndTimeTextBox?.Text ?? "", "hh\\:mm", null, out _))
                    {
                        MessageBox.Show("结束时间格式不正确，请使用HH:mm格式", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                _config.StockSymbols = symbols;
                _config.RefreshInterval = (int)_intervalNumeric.Value;
                
                // 保存显示选项
                _config.ShowSymbol = _showSymbolCheckBox?.Checked ?? true;
                _config.ShowName = _showNameCheckBox?.Checked ?? false;
                _config.ShowPrice = _showPriceCheckBox?.Checked ?? true;
                _config.ShowChangePercent = _showChangePercentCheckBox?.Checked ?? true;
                _config.ShowChange = _showChangeCheckBox?.Checked ?? true;
                
                // 保存新配置选项
                _config.AutoStartup = _autoStartupCheckBox?.Checked ?? false;
                _config.EnableDisplayTimeRange = _enableDisplayTimeRangeCheckBox?.Checked ?? false;
                _config.DisplayStartTime = _displayStartTimeTextBox?.Text ?? "09:12";
                _config.DisplayEndTime = _displayEndTimeTextBox?.Text ?? "15:10";

                _configService.SaveConfig(_config);
                
                // 更新开机自启设置
                var autoStartupService = new AutoStartupService();
                autoStartupService.UpdateAutoStartup(_config.AutoStartup);
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public AppConfig GetUpdatedConfig()
        {
            return _config;
        }

        public void SetErrorText(string errorText)
        {
            if (_errorTextBox != null)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                _errorTextBox.Text = $"[{timestamp}] {errorText}" + Environment.NewLine + _errorTextBox.Text;
            }
        }
    }
}