using DAMS;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        //Ini zhiminKey = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Key");
        Ini zhiminColor = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Color");

        //Has been registered or not yet
        private Boolean isRegistered = false;
        public Boolean IsRegistered
        {
            get { return isRegistered; }
            set { isRegistered = value; }
        }        

        public LoginWindow()
        {
            InitializeComponent();

            double workHeight = SystemParameters.WorkArea.Height;
            double workWidth = SystemParameters.WorkArea.Width;

            this.Top = (workHeight - this.Height) / 2 + MainConst.OFFSET_PIXS_VERTICAL;
            this.Left = (workWidth - this.Width) / 2;
            //WindowStartupLocation = WindowStartupLocation.CenterScreen;

            String key = getKey();
            String value = zhiminColor.ReadValue("Color", "value");

            byte[] result = Encoding.Default.GetBytes(key);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            if (BitConverter.ToString(output) != value || key.Length == 0)
            {
                isRegistered = false;
                tbxSerialNumber.Visibility = Visibility.Visible;
                btnStart.Content = "激 活";
            }
            else
            {
                isRegistered = true;
                tbxSerialNumber.Visibility = Visibility.Hidden;
                btnStart.Content = "启 动";
            }

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确实要退出系统吗？", MainControl.MainConst.MESSAGE_BOX_TITLE, 
                MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (tbxSerialNumber.Visibility == Visibility.Hidden)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                String key = tbxUserName.Text;//zhiminKey.ReadValue("Key", "key");
                //String value = zhiminColor.ReadValue("Color", "value");

                //byte[] result = Encoding.Default.GetBytes(key);
                //MD5 md5 = new MD5CryptoServiceProvider();
                //byte[] output = md5.ComputeHash(result);
                //if (BitConverter.ToString(output) != value || key.Length == 0)//
                //{
                //    MessageBox.Show("请输入有效产品序列号！", MainControl.MainConst.MESSAGE_BOX_TITLE);
                //    return;
                //}
                //else
                {
                    //Do anything here.
                    setKey(key);
                    isRegistered = true;
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        private void ShowMonitorWindow()
        {
            Boolean  blnExit = false;
            MonitorWindow ppWin;
            while (!blnExit) {
                ppWin = new MonitorWindow();
                ppWin.Owner = this.Owner;
                blnExit = (Boolean)ppWin.ShowDialog();          
            }
            Application.Current.Shutdown();
        }

        private void user_id_gotFocus(object sender, RoutedEventArgs e)
        {
            TextBox user_id = sender as TextBox;
            if ("请输入管理员ID".Equals(user_id.Text))
            {
                user_id.Text = "";
            }
        }

        private void password_gotFocus(object sender, RoutedEventArgs e)
        {
            TextBox password = sender as TextBox;
            if ("请输入管理员密码".Equals(password.Text))
            {
                password.Text = "";
            }
        }

        private string getKey()
        {
            String key = null;
            try {
                RegistryKey jly = Registry.CurrentUser.OpenSubKey("SOFTWARE\\jly",true);
                if (jly != null)
                {
                    if (jly.GetValue("key")!=null)
                    {
                        key = (String)jly.GetValue("key");//取得

                    }
                    else
                    {
                        jly.SetValue("key","");
                    }
                }
                else
                {
                    //创建                                            
                    Registry.CurrentUser.CreateSubKey("SOFTWARE\\jly");
                }
                jly.Close();
            }catch(Exception e){
                MessageBox.Show(e.Message, MainControl.MainConst.MESSAGE_BOX_TITLE);
            }
            return key==null?"":key;

        }

        private void setKey(string key)
        {
            try{
                RegistryKey rsg = null;//声明一个变量
                if (Registry.CurrentUser.OpenSubKey("SOFTWARE\\jly") == null)
                {
                    //创建                                            
                    Registry.CurrentUser.CreateSubKey("SOFTWARE\\jly");
                }
                rsg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\jly", true);    //true表可以修改
                rsg.SetValue("key", key);//写入
                rsg.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, MainControl.MainConst.MESSAGE_BOX_TITLE);
            }

        }
    }
}
