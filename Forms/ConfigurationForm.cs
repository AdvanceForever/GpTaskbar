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
            this.Size = new Size(520, 520);
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
            
            // 保存按钮
            _saveButton = new Button
            {
                Text = "保存",
                Location = new Point(200, 440),
                Size = new Size(75, 30)
            };
            _saveButton.Click += SaveButton_Click;
            
            // 取消按钮
            _cancelButton = new Button
            {
                Text = "取消",
                Location = new Point(285, 440),
                Size = new Size(75, 30)
            };
            _cancelButton.Click += (s, e) => this.Close();
            
            // 示例标签
            var exampleLabel = new Label
            {
                Text = "示例: sh000001 (上证指数), sz399001 (深证成指), sh600036 (招商银行)",
                Location = new Point(20, 240),
                Size = new Size(400, 30),
                Font = new Font("Microsoft Sans Serif", 8F),
                ForeColor = Color.Gray
            };
            
            // 错误信息标签
            var errorLabel = new Label
            {
                Text = "错误信息:",
                Location = new Point(20, 280),
                Size = new Size(100, 20)
            };
            
            // 错误信息文本框
            _errorTextBox = new TextBox
            {
                Location = new Point(20, 305),
                Size = new Size(470, 100),
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
                exampleLabel, errorLabel, _errorTextBox, _saveButton, _cancelButton
            });
            
            this.ResumeLayout(false);
        }

        private void LoadConfiguration()
        {
            if (_symbolsTextBox != null)
                _symbolsTextBox.Text = string.Join(", ", _config.StockSymbols);
            if (_intervalNumeric != null)
                _intervalNumeric.Value = _config.RefreshInterval;
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

                _config.StockSymbols = symbols;
                _config.RefreshInterval = (int)_intervalNumeric.Value;
                
                // 保存显示选项
                _config.ShowSymbol = _showSymbolCheckBox?.Checked ?? true;
                _config.ShowName = _showNameCheckBox?.Checked ?? false;
                _config.ShowPrice = _showPriceCheckBox?.Checked ?? true;
                _config.ShowChangePercent = _showChangePercentCheckBox?.Checked ?? true;
                _config.ShowChange = _showChangeCheckBox?.Checked ?? true;

                _configService.SaveConfig(_config);
                
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