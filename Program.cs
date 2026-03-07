using System;
using System.Windows.Forms;
using GpTaskbar.Forms;

namespace GpTaskbar
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // 确保只有一个实例运行
            bool createdNew;
            var mutex = new System.Threading.Mutex(true, "GpTaskbar", out createdNew);
            
            if (!createdNew)
            {
                MessageBox.Show("程序已经在运行中", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"程序启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                mutex?.Close();
            }
        }
    }
}