using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Threading;

namespace DAMS
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Process thisProc = Process.GetCurrentProcess();
            // Check whether a processes with the same name is already running
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                // If ther is more than one, than it is already running.
                MessageBox.Show("已经有同名的进程在运行。本系统不能同时打开多个进程。", MainControl.MainConst.MESSAGE_BOX_TITLE);
                Application.Current.Shutdown();
                return;
            }
            // Start up this process
            base.OnStartup(e);
        }
    }

    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            DAMS.App app = new DAMS.App();
            app.InitializeComponent();
            MainWindow windows = new MainWindow();
            app.MainWindow = windows;
            app.Run();
        }
    }



}
