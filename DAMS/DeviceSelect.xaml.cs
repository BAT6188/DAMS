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
using DAMS.MainControl;

namespace DAMS
{
    /// <summary>
    /// DeviceSelect.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceSelect : Window
    {
        public String deviceType;

        public DeviceSelect()
        {
            InitializeComponent();

            double workHeight = SystemParameters.WorkArea.Height;
            double workWidth = SystemParameters.WorkArea.Width;

            this.Top = (workHeight - this.Height) / 2 + MainConst.OFFSET_PIXS_VERTICAL;
            this.Left = (workWidth - this.Width) / 2;
            //WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void btnType01_Click(object sender, RoutedEventArgs e)
        {
            //智 敏
            this.deviceType = "0";
            this.Close();
        }

        private void btnType02_Click(object sender, RoutedEventArgs e)
        {
            //警 翼
            this.deviceType = "1";
            this.Close();
        }

        private void btnType03_Click(object sender, RoutedEventArgs e)
        {
            //华德安
            this.deviceType = "2";
            this.Close();
        }
    }
}
