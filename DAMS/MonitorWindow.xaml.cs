using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using DAMS.MainControl;
using DAMS.UsbManager;

namespace DAMS
{
    /// <summary>
    /// MonitorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MonitorWindow : Window
    {
        //测试用
        Activity activity;

        MainWindowControl control;

        private double pixTop;
        private double pixLeft;

        public double PixTop
        {
            get { return pixTop; }
            set { pixTop = value; }
        }

        public double PixLeft
        {
            get { return pixLeft; }
            set { pixLeft = value; }
        }

        // USBList
        List<string> usbList = new List<string>();

        // 创建USB监听
        CUSBMonitor usbMonitor;

        public MonitorWindow()
        {
            InitializeComponent();

            //Window's location control
            double workHeight = SystemParameters.WorkArea.Height;
            double workWidth = SystemParameters.WorkArea.Width;

            this.Top = (workHeight - this.Height) / 2 +MainConst.OFFSET_PIXS_VERTICAL;
            this.Left = (workWidth - this.Width) / 2;
            this.pixTop = this.Top;
            this.pixLeft = this.Left;

            //WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            //-----------------------------------------------------------------
            control = new MainWindowControl(this);

            //测试用
            activity = new Activity(this);
            
            // 创建USB监听
            usbMonitor = new CUSBMonitor(activity, usbList);
            //-----------------------------------------------------------------
            
            //Set Focus
            //this.bkRec01.Stroke = new SolidColorBrush(colorBKPanelFocus);
            activity.changeStyle(1);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void system_config_Click(object sender, RoutedEventArgs e)
        {
            MngPwdConfirm ppLogin = new MngPwdConfirm();
            ppLogin.Owner = this;

            Boolean blnLogin = (Boolean)ppLogin.ShowDialog();
            if (blnLogin)
            {
                // Invoke the Main Window to show the System Setting Window.
                //this.Close();
                this.Top = -3000;
                this.Owner.Dispatcher.Invoke(new DelegateHandle(AskOwnerToOpenSettingWindow), null);
            }   
        }

        //Claim the delegation.
        private delegate void DelegateHandle();

        // Invoke the Main Window to show the System Setting Window.
        public void AskOwnerToOpenSettingWindow()
        {
            MainWindow ppWinMain = (MainWindow)this.Owner;
            ppWinMain.OpenSettingWindow();
        }
        //-------------------------------------------------------------------

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var source = PresentationSource.FromVisual(this) as HwndSource;
            if (source != null)
            {
                source.AddHook(WndProc);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            usbMonitor.FillData(msg, wParam);
            return IntPtr.Zero;
        }

        private void DoExit(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("确实要退出并关闭采集系统吗？", MainControl.MainConst.MESSAGE_BOX_TITLE, 
                MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        //-------------------------------------------------------------------

    }
}
