using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace DAMS
{
    /// <summary>
    /// Registration.xaml 的交互逻辑
    /// </summary>
    public partial class Registration : Window
    {
        private Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + "config.ini");
        // 设备
        private string drive_name;
        // 终端号
        private int terminal_no;

        //警官编号
        private String policeNo;
        public String PoliceNo
        {
            get { return policeNo; }
            set { policeNo = value; }
        }

        // 判断以何种方式关闭画面
        public int isRegistFlg = 0;

        public Registration(string drive_name, int terminal_no)
        {
            this.InitializeComponent();

            this.drive_name = drive_name;
            this.terminal_no = terminal_no;
            //drive_no_regist.Text = "";
            police_no_regist.Text = "";
        }

        private void use_button_Click(object sender, RoutedEventArgs e)
        {
            String s = this.police_no_regist.Text;
            if (s == null || s.Trim().Equals(""))
            {
                MessageBox.Show("警员编号不能为空！");
                return;
            }
            foreach (char c in s)
            {
                if (!char.IsLetter(c) && !char.IsNumber(c))
                {
                    MessageBox.Show("警员编号必须为英数字！");
                    return;
                }
            }
            ini.Writue("SN", this.police_no_regist.Text, this.police_no_regist.Text);
            registPolice();

        }

        // 把sn.txt文件放入设备中
        private void registPolice()
        {

            string path = drive_name + "sn.txt";

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            //Create the file.
            using (FileStream fs = File.Create(path))
            {
                AddText(fs, this.police_no_regist.Text);
            }

            policeNo = this.police_no_regist.Text;
            isRegistFlg = 1;
            this.Close();
        }

        private static void AddText(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            isRegistFlg = 2;
            this.Close();
        }

        private void grid_load(object sender, RoutedEventArgs e)
        {
           // this.terminal_no_regist.Text = (this.terminal_no < 10) ? "T0" + this.terminal_no : "T" + this.terminal_no;
        }

    }
}
