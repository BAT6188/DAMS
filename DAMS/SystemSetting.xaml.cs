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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using DAMS.MainControl;

namespace DAMS
{
    /// <summary>
    /// SystemSetting.xaml 的交互逻辑
    /// </summary>
    public partial class SystemSetting : Window
    {
        private string errormsg;

        public SystemSetting()
        {
            InitializeComponent();

            //this.Owner.Visibility = Visibility.Hidden;

            //Window's location control
            double workHeight = SystemParameters.WorkArea.Height;
            double workWidth = SystemParameters.WorkArea.Width;

            this.Top = (workHeight - this.Height) / 2 + MainConst.OFFSET_PIXS_VERTICAL;
            this.Left = (workWidth - this.Width) / 2;
            //WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // System setting
            cavSetting01.Visibility = Visibility.Visible;
            cavSetting02.Visibility = Visibility.Hidden;
            cavSetting03.Visibility = Visibility.Hidden;
			cavSetting04.Visibility = Visibility.Hidden;
            SystemSettingReset();
        }

        private void OnSystemSetting(object sender, MouseButtonEventArgs e)
        {
            Label lblSender = (Label)sender;
            String name = lblSender.Name;
            switch (name)
            {
                case "lblSettingGroup1":
                    {
                        // System setting
                        cavSetting01.Visibility = Visibility.Visible;
                        cavSetting02.Visibility = Visibility.Hidden;
                        cavSetting03.Visibility = Visibility.Hidden;
                        cavSetting04.Visibility = Visibility.Hidden;
                        SystemSettingReset();
                        break;
                    }
                case "lblSettingGroup2":
                    {
                        // Change password for administrator
                        cavSetting01.Visibility = Visibility.Hidden;
                        cavSetting02.Visibility = Visibility.Visible;
                        cavSetting03.Visibility = Visibility.Hidden;
                        cavSetting04.Visibility = Visibility.Hidden;
                        PasswordChange();
                        break;
                    }
                case "lblSettingGroup3":
                    {
                        // Other settings
                        cavSetting01.Visibility = Visibility.Hidden;
                        cavSetting02.Visibility = Visibility.Hidden;
                        cavSetting03.Visibility = Visibility.Hidden;
                        cavSetting04.Visibility = Visibility.Hidden;
                        OtherSettingReset();
                        break;
                    }
                case "lblSettingGroup4":
                    {
                        // Other settings
                        cavSetting01.Visibility = Visibility.Hidden;
                        cavSetting02.Visibility = Visibility.Hidden;
                        cavSetting03.Visibility = Visibility.Hidden;
                        cavSetting04.Visibility = Visibility.Visible;
                        break;
                    }
            }
        }

        private void ButtonQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SystemSettingReset()
        {
            //读取Main.ini  
            Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini");
            //是否需要上传
            string serverFlag = ini.ReadValue("Server", "serverType");
            if(serverFlag.Equals("local")){
                serverType.IsChecked = true;
                txbNetUri.IsEnabled = false;
                txbUser.IsEnabled = false;
                txbPwd.IsEnabled = false;
                txbIp.IsEnabled = false;
                btnTest.IsEnabled = false;
            }else{
                serverType.IsChecked = false;
                txbNetUri.IsEnabled = true;
                txbUser.IsEnabled = true;
                txbPwd.IsEnabled = true;
                txbIp.IsEnabled = true;
                btnTest.IsEnabled = true;
            }
            
            //网络路径
            string sRemoteDir = ini.ReadValue("Server", "sRemoteDir");
            txbNetUri.Text = sRemoteDir;
            //用户名
            string sRemoteUser = ini.ReadValue("Server", "sRemoteUser");
            txbUser.Text = sRemoteUser;
            //密码
            string sRemotePin = ini.ReadValue("Server", "sRemotePin");
            txbPwd.Password = sRemotePin;
            //网络IP
            string sIP = ini.ReadValue("Server", "sIP");
            txbIp.Text = sIP;
            //系统重启
            string sRebootTime = ini.ReadValue("Timer", "sRebootTime");
            txbRestart.Text = sRebootTime;
            //缩略图目录
            string ThumbnailPath = ini.ReadValue("Thumbnail", "path");
            txbShortcutDir.Text = ThumbnailPath;
            //缓存目录
            string sCacheDir = ini.ReadValue("Local", "sCacheDir");
            txbCacheDir.Text = sCacheDir;
            //声音警告
            string iDriveQuota = ini.ReadValue("Local", "iDriveQuota");
            txbWrnDiscSpace.Text = iDriveQuota;
            //文件缓存期限
            string iDays = ini.ReadValue("Local", "iDays");
            txbFileCachedDays.Text = iDays;
            //空闲时上传文件、时间设定
            //string bIdleUpload = ini.ReadValue("Server", "bIdleUpload");
            //if (bIdleUpload == "1")
            //{
            //    chbUploadOnFree.IsChecked = true;
            //}
            //string sIdleTime = ini.ReadValue("Server", "sIdleTime");
            //txbUploadOnFree.Text = sIdleTime;
        }

        private void PasswordChange()
        {
            Password_old_text.Password = "";
            Password_new_text.Password = "";
            Password_new_confirm_text.Password = "";
            Password_old_error.Content = "";
            Password_new_error.Content = "";
            Password_new_confirm_text_error.Content = "";
        }

        private void OtherSettingReset()
        {
            //读取Main.ini  
            Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini");
            //上传模式（分类上传或直接上传）
            string categoFlag = ini.ReadValue("Zhimin", "categoFlag");
            if (categoFlag == "1")
            {
                chbUploadWithCatego.IsChecked = true;
            }
            else
            {
                chbUploadWithCatego.IsChecked = false;
            }
            //视频是否压缩
            string compressFlag = ini.ReadValue("Zhimin", "compressFlag");
            if (compressFlag == "1")
            {
                chbCompressVedio.IsChecked = true;
            }
            else
            {
               chbCompressVedio.IsChecked = false;
            }
            //兼容模式
            string compatibleFlag = ini.ReadValue("Zhimin", "compatibleFlag");
            if (compatibleFlag == "1")
            {
                chbCompatibilityMode.IsChecked = true;
            }
            else
            {
                chbCompatibilityMode.IsChecked = false;
            }
            //禁用鼠标键盘
            string UsbFlg = ini.ReadValue("Server", "UsbFlg");
            if (UsbFlg == "1")
            {
                chbForbidUsb.IsChecked = true;
            }
            else
            {
                chbForbidUsb.IsChecked = false;
            }

        }

        //清空按钮功能
        private void btnClear_OnClick(object sender, RoutedEventArgs e)
        {
            Password_old_text.Password = "";
            Password_new_text.Password = "";
            Password_new_confirm_text.Password = "";
        }

        private void btnModify_OnClick(object sender, RoutedEventArgs e)
        {
            //读取Main.ini  
            Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini");
            string password_content = ini.ReadValue("Option", "sPassword");

            //MD5加密
            byte[] result = Encoding.Default.GetBytes(Password_old_text.Password);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            string password_content_text = BitConverter.ToString(output).Replace("-", "");

            Password_old_error.Content = "";
            Password_new_error.Content = "";
            Password_new_confirm_text_error.Content = "";
            if (Password_old_text.Password == "")
            {
                Password_old_error.Content = "请输入当前密码";
            }
            else if (Password_new_text.Password == "")
            {
                Password_new_error.Content = "请输入新密码";
            }
            else if (Password_new_text.Password != Password_new_confirm_text.Password)
            {
                Password_new_confirm_text_error.Content = "两次输入的密码不一致";
            }
            else if (password_content_text != password_content)
            {
                Password_old_error.Content = "密码错误,请重新输入!";
            }
            else if (password_content_text == password_content)
            {
                //MD5加密
                byte[] results = Encoding.Default.GetBytes(Password_new_text.Password);
                MD5 md5s = new MD5CryptoServiceProvider();
                byte[] outputs = md5s.ComputeHash(results);
                string password_content_texts = BitConverter.ToString(outputs).Replace("-", "");
                //写入Main.ini
                ini.Writue("Option", "sPassword", password_content_texts);

                Password_old_text.Password = "";
                Password_new_text.Password = "";
                Password_new_confirm_text.Password = "";
                MessageBox.Show("密码修改成功!", MainControl.MainConst.MESSAGE_BOX_TITLE);
            }
        }

        private void btnRest_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要保存现在画面上所有的设定吗？", MainControl.MainConst.MESSAGE_BOX_TITLE,
               MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes )
            {
                return;
            }
            
            //读取Main.ini  
            Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini");
            //是否需要上传到服务器
            string serverFlag = "";
            if (serverType.IsChecked == true)
            {
                serverFlag = "local";
            }else{
                serverFlag = "server";
            }
            
            ini.Writue("Server", "serverType", serverFlag);
            //网络路径
            string sRemoteDir = txbNetUri.Text;
            ini.Writue("Server", "sRemoteDir", sRemoteDir);
            //用户名
            string sRemoteUser = txbUser.Text;
            ini.Writue("Server", "sRemoteUser", sRemoteUser);
            //密码
            string sRemotePin = txbPwd.Password;
            ini.Writue("Server", "sRemotePin", sRemotePin);
            //IP
            string sIP = txbIp.Text;
            ini.Writue("Server", "sIP", sIP);

            //系统重启
            string sRebootTime = txbRestart.Text;
            Regex reg = new Regex(@"^([0-1]?[0-9]|2[0-3]):([0-5][0-9]):([0-5][0-9])$");;
            if (reg.IsMatch(sRebootTime))
                ini.Writue("Timer", "sRebootTime", sRebootTime);
            else
            {
                MessageBox.Show("[系统重启时间]无效的时间!", MainControl.MainConst.MESSAGE_BOX_TITLE);
                return;
            }
            //缩略图目录
            string ThumbnailPath = txbShortcutDir.Text;

             Regex regpath = new Regex(@"^[a-zA-Z]:\\[^/:*?<>()|]+$");
             if (regpath.IsMatch(ThumbnailPath))
                 ini.Writue("Thumbnail", "path", ThumbnailPath);
            else
            {
                //路径不合法 
                MessageBox.Show("[采集站缩略图目录]无效的路径!", MainControl.MainConst.MESSAGE_BOX_TITLE);
                return;
            }
            //缓存目录
            string sCacheDir = txbCacheDir.Text;
            if (regpath.IsMatch(sCacheDir))
                ini.Writue("Local", "sCacheDir", sCacheDir);
            else
            {
                //路径不合法 
                MessageBox.Show("[采集站缓存目录]无效的路径!", MainControl.MainConst.MESSAGE_BOX_TITLE);
                return;
            }

            //声音警告 缓存目录所在分区剩余空间
            string iDriveQuota = txbWrnDiscSpace.Text;
            int n;
            if (int.TryParse(iDriveQuota, out n))
            {
                ini.Writue("Local", "iDriveQuota", iDriveQuota);
            }
            else
            {
                MessageBox.Show("缓存目录所在分区剩余空间必须为数字!", MainControl.MainConst.MESSAGE_BOX_TITLE);
                return;
            }
            
            //文件缓存期限
            string iDays = txbFileCachedDays.Text;
            if (int.TryParse(iDays, out n))
            {
                ini.Writue("Local", "iDays", iDays);
            }
            else
            {
                MessageBox.Show("文件缓存期限必须为数字!", MainControl.MainConst.MESSAGE_BOX_TITLE);
                return;
            }
            
            //空闲时上传文件、时间设定
            //if (chbUploadOnFree.IsChecked == true)
            //{
            //    ini.Writue("Server", "bIdleUpload", "1");
            //}
            //else
            //{
            //    ini.Writue("Server", "bIdleUpload", "0");
            //}
            //string sIdleTime = txbUploadOnFree.Text;
            //ini.Writue("Server", "sIdleTime", sIdleTime);


            MessageBox.Show("画面上的设定保存成功！", MainControl.MainConst.MESSAGE_BOX_TITLE);
        }

        private void btnTest_OnClick(object sender, RoutedEventArgs e)
        {
            if (Connect(txbIp.Text + "\\" + txbNetUri.Text, txbUser.Text, txbPwd.Password))
            {
                MessageBox.Show("连接成功！", MainControl.MainConst.MESSAGE_BOX_TITLE);
            }
            else
            {
                MessageBox.Show(errormsg, MainControl.MainConst.MESSAGE_BOX_TITLE);
            }
        }

        public bool Connect(string remoteHost, string userName, string passWord)
        {
            bool Flag = true;
            Process proc = new Process();

            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;
            try
            {

                proc.Start();
                string command = @"net  use  \\" + remoteHost + "  " + passWord + "  " + "  /user:" + userName + ">NUL";
                proc.StandardInput.WriteLine(command);

                command = @"net  use * /del /y";
                proc.StandardInput.WriteLine(command);

                command = "exit";
                proc.StandardInput.WriteLine(command);

                errormsg = proc.StandardError.ReadToEnd();
                while (proc.HasExited == false)
                {
                    proc.WaitForExit(1000);
                }
                if (errormsg != "")
                    Flag = false;

                proc.StandardError.Close();
            }
            catch (Exception ex)
            {
                Flag = false;
                errormsg = ex.Message;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }

            return Flag;
        }

        private void btnShutdown_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确实要退出并关闭采集系统吗？", MainControl.MainConst.MESSAGE_BOX_TITLE,
            MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void serverType_OnChick(object sender, RoutedEventArgs e)
        {
            txbNetUri.IsEnabled = false;
            txbUser.IsEnabled = false;
            txbPwd.IsEnabled = false;
            txbIp.IsEnabled = false;
            btnTest.IsEnabled = false;
        }

        private void serverType_UnChick(object sender, RoutedEventArgs e)
        {
            txbNetUri.IsEnabled = true;
            txbUser.IsEnabled = true;
            txbPwd.IsEnabled = true;
            txbIp.IsEnabled = true;
            btnTest.IsEnabled = true;
        }

        private void btnSaveSetting_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要保存现在画面上所有的设定吗？", MainControl.MainConst.MESSAGE_BOX_TITLE,
                MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
            {
                return;
            }

            //读取Main.ini  
            Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini");

            //上传模式（分类上传或直接上传）
            string categoFlag;
            if (chbUploadWithCatego.IsChecked == true)
            {
                categoFlag = "1";
            }
            else
            {
                categoFlag = "0";
            }
            ini.Writue("Zhimin", "categoFlag", categoFlag);
            //视频是否压缩
            string compressFlag;
            if (chbCompressVedio.IsChecked == true)
            {
                compressFlag = "1";
            }
            else
            {
                compressFlag = "0";
            }
            ini.Writue("Zhimin", "compressFlag", compressFlag);
            //兼容模式
            string compatibleFlag;
            if (chbCompatibilityMode.IsChecked == true)
            {
                compatibleFlag = "1";
            }
            else
            {
                compatibleFlag = "0";
            }
            ini.Writue("Zhimin", "compatibleFlag", compatibleFlag);
            //鼠标键盘禁用
            if (chbForbidUsb.IsChecked == true)
            {
                ini.Writue("Server", "UsbFlg", "1");
            }
            else
            {
                ini.Writue("Server", "UsbFlg", "0");
            }

            MessageBox.Show("画面上的设定保存成功！", MainControl.MainConst.MESSAGE_BOX_TITLE);
        }
    }
}
