using System;
using System.Drawing;
using System.Windows.Forms;
using GpTaskbar.Forms;
using GpTaskbar.Models;

namespace GpTaskbar.Services
{
    public class TaskbarService : IDisposable
    {
        private Form? _floatingForm;
        private Label? _floatingLabel; 
        private System.Windows.Forms.Timer? _topmostTimer;
        private ConfigService _configService;
        private AppConfig _config;
        private string _currentDisplayText = "";
        private bool _isDragging = false;
        private Point _dragStartPosition;
        private Point _formStartPosition;

        public TaskbarService()
        {
            _configService = new ConfigService();
            _config = _configService.LoadConfig();
            InitializeFloatingForm();
         
            InitializeTopmostTimer();
        }

        private void InitializeFloatingForm()
        {
            _floatingForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(45, 45, 48), // 深色背景匹配Windows 11主题
                Size = new Size(250, 35),
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                ShowInTaskbar = false,
                Opacity = 0.6,
                AllowTransparency = true
            };

            _floatingLabel = new Label
            {
                Text = "加载中...",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 45, 48), // 设置与窗体相同的背景色
                Cursor = Cursors.Hand // 添加手型光标
            };

            _floatingForm.Controls.Add(_floatingLabel);
            
            // 添加鼠标事件
            _floatingForm.MouseEnter += OnMouseEnter;
            _floatingForm.MouseLeave += OnMouseLeave;
           // _floatingForm.Click += OnClick;
            _floatingForm.MouseDown += OnMouseDown;
            _floatingForm.MouseMove += OnMouseMove;
            _floatingForm.MouseUp += OnMouseUp;
            
            _floatingLabel.MouseEnter += OnMouseEnter;
            _floatingLabel.MouseLeave += OnMouseLeave;
           // _floatingLabel.Click += OnClick;
            _floatingLabel.MouseDown += OnMouseDown;
            _floatingLabel.MouseMove += OnMouseMove;
            _floatingLabel.MouseUp += OnMouseUp;

            PositionFloatingForm();
        }

        private void OnMouseEnter(object? sender, EventArgs e)
        {
            if (_floatingForm != null && !_floatingForm.IsDisposed)
            {
                _floatingForm.Opacity = 1.0;
            }
        }

        private void OnMouseLeave(object? sender, EventArgs e)
        {
            if (_floatingForm != null && !_floatingForm.IsDisposed)
            {
                _floatingForm.Opacity = 0.6;
            }
        }

        // private void OnClick(object? sender, EventArgs e)
        // {
        //     ShowConfigurationForm();
        // }

        private void OnMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                
                // 正确处理鼠标位置：将控件坐标转换为屏幕坐标
                if (sender is Control control)
                {
                    Point screenPoint = control.PointToScreen(e.Location);
                    _dragStartPosition = screenPoint;
                }
                else
                {
                    _dragStartPosition = e.Location;
                }
                
                _formStartPosition = _floatingForm!.Location;
                _floatingForm!.Capture = true; 
            }
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging && _floatingForm != null && !_floatingForm.IsDisposed)
            {
                // 获取当前的鼠标屏幕位置
                Point currentScreenPoint;
                if (sender is Control control)
                {
                    currentScreenPoint = control.PointToScreen(e.Location);
                }
                else
                {
                    currentScreenPoint = e.Location;
                }
                
                // 计算相对于开始拖拽时的位移
                int deltaX = currentScreenPoint.X - _dragStartPosition.X;
                int deltaY = currentScreenPoint.Y - _dragStartPosition.Y;
                
                // 直接设置窗体位置，避免累积误差
                _floatingForm.Location = new Point(_formStartPosition.X + deltaX, _formStartPosition.Y + deltaY);
            }
        }

        private void OnMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
                if (_floatingForm != null && !_floatingForm.IsDisposed)
                {
                    _floatingForm.Capture = false;
                    
                    // 拖拽结束后保存位置到配置文件
                    _config.WindowPositionX = _floatingForm.Location.X;
                    _config.WindowPositionY = _floatingForm.Location.Y;
                    try
                    {
                        _configService.SaveConfig(_config);
                    }
                    catch (Exception ex)
                    {
                        // 位置保存失败不影响主要功能，可以忽略
                        System.Diagnostics.Debug.WriteLine($"保存窗口位置失败: {ex.Message}");
                    }
                   
                }
            }
        }


        private void InitializeTopmostTimer()
        {
            _topmostTimer = new System.Windows.Forms.Timer
            {
                Interval = 3000 // 每100毫秒置顶一次
            };
            _topmostTimer!.Tick += (s, e) => EnsureTopmost();
            _topmostTimer.Start();
        }

        private void EnsureTopmost()
        {
            if (_floatingForm != null && !_floatingForm.IsDisposed && _floatingForm.Visible)
            {
                _floatingForm.TopMost = false;
                if (!_floatingForm.TopMost)
                {
                    _floatingForm.TopMost = true;
                }
            }
        }

        private void PositionFloatingForm()
        {
            if (_floatingForm == null || _floatingForm.IsDisposed || _isDragging) return;

            // 如果配置中保存了有效位置，则使用保存的位置
            if (_config.WindowPositionX >= 0 && _config.WindowPositionY >= 0)
            {
                _floatingForm.Location = new Point(_config.WindowPositionX, _config.WindowPositionY);
            }
            else
            {
                // 使用默认位置：任务栏上方，靠近系统时间区域
                var screen = Screen.PrimaryScreen!;
                var taskbarHeight = screen.Bounds.Height - screen.WorkingArea.Height;
                
                _floatingForm.Location = new Point(
                    screen.Bounds.Width - _floatingForm.Width - 150, // 在系统时间左侧
                    screen.Bounds.Height - taskbarHeight - _floatingForm.Height - 2
                );
            }
        }

        public void UpdateStockDisplay(List<Models.StockData> stocks)
        {
            if (_floatingLabel == null || _floatingLabel.IsDisposed || stocks.Count == 0) return;
            
            // 根据配置选项生成显示文本
            var displayText = stocks[0].GetDisplayText(
                _config.ShowSymbol,
                _config.ShowName,
                _config.ShowPrice,
                _config.ShowChangePercent,
                _config.ShowChange
            );
            
            if (stocks.Count > 1)
            {
                displayText += " | " + stocks[1].GetDisplayText(
                    _config.ShowSymbol,
                    _config.ShowName,
                    _config.ShowPrice,
                    _config.ShowChangePercent,
                    _config.ShowChange
                );
            }
            
            _currentDisplayText = displayText;
            _floatingLabel.Text = displayText;
            
            // 根据涨跌设置颜色
            if (!stocks[0].IsRising)
            {
                _floatingLabel.ForeColor = Color.FromArgb(76, 175, 80); // 绿色
            }
            else
            {
                _floatingLabel.ForeColor = Color.FromArgb(244, 67, 54); // 红色
            }

            // 确保窗体可见
            if (_floatingForm != null && !_floatingForm.Visible)
            {
                _floatingForm.Show();
            }
        }

        private void ShowConfigurationForm()
        {
            // 暂停定时置顶
            _topmostTimer?.Stop();
            
            // 直接创建并显示配置窗口
            try
            {
                var configService = new ConfigService();
                var config = configService.LoadConfig();
                var configForm = new ConfigurationForm(configService, config);
                configForm.FormClosed += (s, e) =>
                {
                    // 配置窗口关闭后恢复定时置顶
                    _topmostTimer?.Start();
                };
                configForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法打开配置窗口: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // 如果打开失败，也恢复定时器
                _topmostTimer?.Start();
            }
        }

        public void Show()
        {
            if (_floatingForm != null && !_floatingForm.Visible)
            {
                _floatingForm.Show();
            }
        }

        public void Hide()
        {
            if (_floatingForm != null && _floatingForm.Visible)
            {
                _floatingForm.Hide();
            }
        }

        public void PauseTopmostTimer()
        {
            _topmostTimer?.Stop();
        }

        public void ResumeTopmostTimer()
        {
            _topmostTimer?.Start();
        }

        public void Dispose()
        {
            
            _topmostTimer?.Stop();
            _topmostTimer?.Dispose();
            
            _floatingForm?.Close();
            _floatingForm?.Dispose();
        }
    }
}