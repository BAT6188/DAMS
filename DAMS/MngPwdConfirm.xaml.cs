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
using System.Security.Cryptography;

namespace DAMS
{
    /// <summary>
    /// MngPwdConfirm.xaml 的交互逻辑
    /// </summary>
    public partial class MngPwdConfirm : Window
    {
        public MngPwdConfirm()
        {
            InitializeComponent();

            this.Top = 90;
            this.Left =930;

            this.lblErrMsg.Content = "";
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            //读取Main.ini  
            Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini");
            string password_content = ini.ReadValue("Option", "sPassword");

            //MD5加密
            byte[] results = Encoding.Default.GetBytes(this.pwdPassword.Password);
            MD5 md5s = new MD5CryptoServiceProvider();
            byte[] outputs = md5s.ComputeHash(results);
            string password = BitConverter.ToString(outputs).Replace("-", "");

            // 密码确认
            if (password_content.Equals(password))
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                //MessageBox.Show("密码错误！请重新输入！");
                this.lblErrMsg.Content = "您输入的密码有误，请重新输入。";
                this.pwdPassword.Clear();
                //this.managementPassword.Focus();
                return;
            }
        }

        private void pwdPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            this.lblErrMsg.Content = "";
        }
    }
}
