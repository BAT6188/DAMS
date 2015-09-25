using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace DAMS.MainControl
{
    /// <summary>
    /// MonitorUnitCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class MonitorUnitCtrl : UserControl
    {
        private String nameT;
        private int usbNo;
        private String nameD;
        private Color colorGray;
        private Color colorBlue;
        private Color colorOrange;
        private Color colorTransparent;

        private int powerPercent;
        private long  millisecondToFull;
        //DispatcherTimer chargeTimer = new DispatcherTimer();
        DispatcherTimer chargeTimer;

        //处理类
        private MonitorUnitCtrlProcess process;

        //USB编号
        public String TerminalNo
        {
            get { return nameT; }
        }

        //警官编号
        private String policeNo;
        public String PoliceNo
        {
            get { return policeNo; }
            set { policeNo = value; }
        }

        //构造函数
        public MonitorUnitCtrl()
        {
            InitializeComponent();

            //创建处理类
            process = new MonitorUnitCtrlProcess(this);
        }

        //构造函数
        public MonitorUnitCtrl(UsbInfo info)
        {
            InitializeComponent();
            InitializeColor();
            //创建处理类
            process = new MonitorUnitCtrlProcess(this);

            policeNo = "";
            nameT = info.TerminalNo;
            usbNo = process.GetBorderNo(nameT);

            this.Name = "Unit" + nameT;
            this.lblTerInfo.Content = nameT;

            this.lblCpStatus.Content = "拷贝";
            this.pgbCopy.Minimum = 0;
            this.pgbCopy.Maximum = 100;
            lblCpPerc.Content = "0% (-- / --)";

            this.lblChStatus.Content = "充电";
            this.pgbCharge.Minimum = 0;
            this.pgbCharge.Maximum = 100;
            lblChPerc.Content = "40% (06 : 00)";

            SetFocus(false);
            SetFree();
        }

        public void SetFocus(bool isFocused)
        {
            if (isFocused)
            {
                SolidColorBrush sClrBrush = new SolidColorBrush();
                sClrBrush.Color = colorBlue;
                this.BorderLight.BorderBrush = sClrBrush;

                DropShadowEffect sdwEffect = new DropShadowEffect();
                sdwEffect.Color = colorBlue;
                sdwEffect.ShadowDepth = 1;
                sdwEffect.Opacity = 0.9;
                this.BorderLight.Effect = sdwEffect;
            }
            else
            {
                SolidColorBrush sClrBrush = new SolidColorBrush();
                sClrBrush.Color = colorTransparent;
                this.BorderLight.BorderBrush = sClrBrush;
                this.BorderLight.Effect = null;
            }
        }

        //刷新充电状态
        public void SetInfoChargeOnly(double chargePercent, int hours, int minutes)
        {
            SolidColorBrush sClrBrush = new SolidColorBrush();
            sClrBrush.Color = colorBlue;
            this.lblTerStatus.Foreground = sClrBrush;
            this.lblChPerc.Foreground = sClrBrush;
            this.lblChStatus.Foreground = sClrBrush;

            this.lblTerStatus.Content = " - 使用中  " + policeNo;

            string number1;
            string number2;
            string strPercent;

            this.pgbCharge.Value = chargePercent * 100.0;
            this.pgbCharge.IsIndeterminate = false;
            strPercent = String.Format("{0:0%}", chargePercent);
            number1 = String.Format("{0:00}", hours);
            number2 = String.Format("{0:00}", minutes);

            lblChPerc.Content = strPercent + " (" + number1 + " : " + number2 + ")";
        }
        //刷新准备
        public void SetReady(double chargePercent, int hours, int minutes)
        {
            SolidColorBrush sClrBrush = new SolidColorBrush();
            sClrBrush.Color = colorBlue;
            this.lblTerStatus.Foreground = sClrBrush;
            this.lblChPerc.Foreground = sClrBrush;
            this.lblChStatus.Foreground = sClrBrush;

            this.lblTerStatus.Content = " - 检测中  " + policeNo;

            string number1;
            string number2;
            string strPercent;

            this.pgbCharge.Value = chargePercent * 100.0;
            this.pgbCharge.IsIndeterminate = false;
            strPercent = String.Format("{0:0%}", chargePercent);
            number1 = String.Format("{0:00}", hours);
            number2 = String.Format("{0:00}", minutes);

            lblChPerc.Content = strPercent + " (" + number1 + " : " + number2 + ")";
        }

        //刷新文件采集状态
        public void SetInfoCopyOnly(int filesCopied, int filesAll)
        {
            SolidColorBrush sClrBrush = new SolidColorBrush();
            sClrBrush.Color = colorBlue;
            this.lblTerInfo.Foreground = sClrBrush;
            this.lblTerStatus.Foreground = sClrBrush;
            this.lblCpPerc.Foreground = sClrBrush;
            this.lblCpStatus.Foreground = sClrBrush;

            this.lblTerStatus.Content = " - 使用中  " + policeNo;

            string number1;
            string number2;
            double dblPercent;
            string strPercent;

            if (0 == filesAll)
            {
                number1 = "0";
                number2 = "0";
                strPercent = "0%";
                dblPercent = 0.0;
            }
            else
            {
                number1 = String.Format("{0:00}", filesCopied);
                number2 = String.Format("{0:00}", filesAll);
                dblPercent = (filesCopied * 1.0) / (filesAll * 1.0);
                strPercent = String.Format("{0:0%}", dblPercent);
            }
            this.pgbCopy.Value = dblPercent * 100.0;
            this.pgbCopy.IsIndeterminate = false;
            lblCpPerc.Content = strPercent + " (" + number1 + " / " + number2 + ")";

            //this.pgbCharge.Value = 100;
            //this.pgbCharge.IsIndeterminate = false;
        }

        //刷新文件采集状态和充电状态
        public void SetInfoAll(int filesCopied, int filesAll, double chargePercent, int hours, int minutes)
        {
            SolidColorBrush sClrBrush = new SolidColorBrush();
            sClrBrush.Color = colorBlue;
            this.lblTerInfo.Foreground = sClrBrush;
            this.lblTerStatus.Foreground = sClrBrush;
            this.lblCpPerc.Foreground = sClrBrush;
            this.lblCpStatus.Foreground = sClrBrush;
            this.lblChPerc.Foreground = sClrBrush;
            this.lblChStatus.Foreground = sClrBrush;
 
            this.lblTerStatus.Content = " - 使用中  " + policeNo;

            string number1;
            string number2;
            double dblPercent;
            string strPercent;

            if (0 == filesAll)
            {
                number1 = "0";
                number2 = "0";
                strPercent = "0%";
                dblPercent = 0.0;
            }
            else
            {
                number1 = String.Format("{0:00}", filesCopied);
                number2 = String.Format("{0:00}", filesAll);
                dblPercent = (filesCopied * 1.0) / (filesAll * 1.0);
                strPercent = String.Format("{0:0%}", dblPercent);
            }
            this.pgbCopy.Value = dblPercent * 100.0;
            this.pgbCopy.IsIndeterminate = false;
            lblCpPerc.Content = strPercent + " (" + number1 + " / " + number2 + ")";

            this.pgbCharge.Value = chargePercent * 100.0;
            this.pgbCharge.IsIndeterminate = false;

            strPercent = String.Format("{0:0%}", chargePercent);
            number1 = String.Format("{0:00}", hours);
            number2 = String.Format("{0:00}", minutes);
            lblChPerc.Content = strPercent + " (" + number1 + " : " + number2 + ")";
        }

        //刷新端口状态为空闲
        public void SetFree()
        {
            SolidColorBrush sClrBrush = new SolidColorBrush();
            sClrBrush.Color = colorGray;
            this.lblTerInfo.Foreground = sClrBrush;
            this.lblTerStatus.Foreground = sClrBrush;
            this.lblCpPerc.Foreground = sClrBrush;
            this.lblCpStatus.Foreground = sClrBrush;
            this.lblChPerc.Foreground = sClrBrush;
            this.lblChStatus.Foreground = sClrBrush;

            this.lblTerStatus.Content = " - 空闲";
            this.pgbCopy.Value = 0;
            lblCpPerc.Content = "0% (-- / --)";
            this.pgbCharge.Value = 0;
            this.pgbCharge.IsIndeterminate = false;
            lblChPerc.Content = "0% (-- : --)";

            if (this.btn_Activate.Visibility == Visibility.Visible)
            {
                this.btn_Activate.Visibility = Visibility.Hidden;
            }

            //If timer exists, then close and release it.
            if (chargeTimer != null)
            {
                chargeTimer.Stop();
                chargeTimer = null;
            }
        }

        //刷新文件采集状态set 1%
        public void SetInfoForFirst(int filesCopied, int filesAll)
        {
            SolidColorBrush sClrBrush = new SolidColorBrush();
            sClrBrush.Color = colorBlue;
            this.lblTerInfo.Foreground = sClrBrush;
            this.lblTerStatus.Foreground = sClrBrush;
            this.lblCpPerc.Foreground = sClrBrush;
            this.lblCpStatus.Foreground = sClrBrush;

            this.lblTerStatus.Content = " - 使用中  " + policeNo;

            string number1;
            string number2;
            double dblPercent;
            string strPercent;

            number1 = filesCopied.ToString();
            number2 = filesAll.ToString();
            strPercent = "1%";
            dblPercent = 0.01;


            this.pgbCopy.Value = dblPercent * 100.0;
            this.pgbCopy.IsIndeterminate = false;
            lblCpPerc.Content = strPercent + " (" + number1 + " / " + number2 + ")";

            //this.pgbCharge.Value = 100;
            //this.pgbCharge.IsIndeterminate = false;
        }

        //刷新端口状态为未激活
        public void SetActivate(string driverName)
        {
            this.nameD = driverName;
            this.btn_Activate.Visibility = Visibility.Visible;

            SolidColorBrush sClrBrush = new SolidColorBrush();
            sClrBrush.Color = colorOrange;
            SolidColorBrush tClrBrush = new SolidColorBrush();
            tClrBrush.Color = colorGray;

            this.lblTerInfo.Foreground = sClrBrush;
            this.lblTerStatus.Foreground = sClrBrush;
            this.lblCpPerc.Foreground = sClrBrush;
            this.lblCpStatus.Foreground = sClrBrush;
            this.lblChPerc.Foreground = sClrBrush;
            this.lblChStatus.Foreground = sClrBrush;

            this.lblTerStatus.Content = " - 未激活";
            this.pgbCopy.Value = 0;
            lblCpPerc.Content = "0% (-- / --)";
            this.pgbCharge.Value = 0;
            lblChPerc.Content = "0% (-- : --)";
        }

        //刷新端口状态为空闲
        public void SetPreparing()
        {
            SolidColorBrush sClrBrush = new SolidColorBrush();
            sClrBrush.Color = colorBlue;
            this.lblTerInfo.Foreground = sClrBrush;
            this.lblTerStatus.Foreground = sClrBrush;
            this.lblCpPerc.Foreground = sClrBrush;
            this.lblCpStatus.Foreground = sClrBrush;
            this.lblChPerc.Foreground = sClrBrush;
            this.lblChStatus.Foreground = sClrBrush;

            this.lblTerStatus.Content = " - 识别中";
            this.pgbCopy.Value = 0;
            lblCpPerc.Content = "0% (-- / --)";
            this.pgbCharge.Value = 0;
            this.pgbCharge.IsIndeterminate = false;
            lblChPerc.Content = "0% (-- : --)";

            //if (this.btn_Activate.Visibility == Visibility.Visible)
            //{
            //    this.btn_Activate.Visibility = Visibility.Hidden;
            //}

        }


        //定义各个状态的样式颜色
        private void InitializeColor()
        {
            Color tmpColor = new Color();
            tmpColor.A = 255;
            tmpColor.R = 205;
            tmpColor.G = 200;
            tmpColor.B = 255;
            colorGray = tmpColor;

            tmpColor.A = 255;
            tmpColor.R = 0;
            tmpColor.G = 204;
            tmpColor.B = 255;
            colorBlue = tmpColor;

            tmpColor.A = 255;
            tmpColor.R = 255;
            tmpColor.G = 102;
            tmpColor.B = 0;
            colorOrange = tmpColor;

            tmpColor.A = 0;
            tmpColor.R = 255;
            tmpColor.G = 255;
            tmpColor.B = 255;
            colorTransparent = tmpColor;
        }

        //激活按钮状态被按下
        private void btn_Activate_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            Registration registration = new Registration(this.nameD, usbNo);            
            registration.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Boolean isClose = (Boolean)registration.ShowDialog();

            if (!isClose)
            {
                // 点击应用按钮，关闭画面时
                if (registration.isRegistFlg == 1)
                {
                    btn.Visibility = Visibility.Hidden;
                    
                    this.policeNo = registration.PoliceNo;

                    //Start the power-charging Timer
                    SetInfoChargeOnly(0.4, 6, 0);
                    StartTimer();

                    UploadWindowShow(this.nameD);
                }
            }
        }

        // 根据激活状态，迁移画面
        public void ForwardOnActivation(String driveName, String newDriverName)
        {
            Boolean is_activation = false;
            try
            {
            // 激活状态
             is_activation = this.process.ActivationCheck(driveName);
            }
            catch (Exception e)
            {
                LogConfig.error("Administrator", e.Message);
                LogConfig.error("Administrator", e.StackTrace);
                return;
            }
            LogConfig.info("Administrator", driveName + "激活状态：" + is_activation);
            // 已激活
            if (is_activation)
            {
                //Start the power-charging Timer
                SetInfoChargeOnly(0.4, 6, 0);
                StartTimer();
                this.UploadWindowShow(driveName);
            }
            else
            {
                //未激活 For Activate
                SetActivate(newDriverName);
            }


        }

        //Call process' UpdateWindowShow
        private void UploadWindowShow(string drive_name)
        {
            //MonitorWindow winMonitor = (MonitorWindow)Window.GetWindow(this);
            //winMonitor.Visibility = Visibility.Hidden;

            //显示分类上传画面
            this.process.UploadWindowShow(drive_name);

            //winMonitor.Visibility = Visibility.Visible;
            

        }

        //On Mouse Click, set the focus on this unit
        private void OnMouseEnter(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WrapPanel wrpParent = (WrapPanel)this.Parent;
            MonitorUnitCtrl mUnitCtrl;
            for (int i = 0; i < wrpParent.Children.Count; i++)
            {
                mUnitCtrl = (MonitorUnitCtrl) wrpParent.Children[i];
                mUnitCtrl.SetFocus(false);
            }
                SetFocus(true);
        }

        //Start the power-charging timer
        private void StartTimer()
        {
            powerPercent = 0;
            millisecondToFull = 28800000;  //(8 * 60) * 60 * 1000;
            //millisecondToFull = 301000;      //(5 * 60 + 1) * 1000;
            //minutesToFull = 21;
            //powerPercent = 0.9;
            //minutesToFull = 61;

            RefreshPowerCharge();

            chargeTimer = new DispatcherTimer();
            chargeTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            chargeTimer.Tick += OnTimer;
            chargeTimer.IsEnabled = true;
            chargeTimer.Start();
        }

        //Refresh the power-charging status
        private void OnTimer(object sender, EventArgs e)
        {
            RefreshPowerCharge();
        }

        private void RefreshPowerCharge()
        {
            long iHours;
            long iMinutes;
            long iSecond;
            long iMsec;

            millisecondToFull -= 500;
            if (millisecondToFull < 0)
            {
                millisecondToFull = 0;
            }

            iHours = millisecondToFull / (1000 * 60 * 60);
            iMinutes = (millisecondToFull - iHours * (1000 * 60 * 60)) / (1000 * 60);
            iSecond = (millisecondToFull - iHours * (1000 * 60 * 60) - iMinutes * (1000 * 60)) / 1000;
            iMsec = millisecondToFull - iHours * (1000 * 60 * 60) - iMinutes * (1000 * 60) - iSecond * 1000;

            //if (minutesToFull % 6 == 0)
            //{
            //    powerPercent += 0.01;
            //    if (powerPercent > 1.0)
            //    {
            //        powerPercent = 1.0;
            //    }
            //}
            powerPercent += 100;
            
            //if (powerPercent == 1.0)
            //LogConfig.info("Administrator", "******* MillisecondToFull = " + millisecondToFull.ToString() + "mm (" + iHours.ToString() + "h : " + iMinutes.ToString() +
            //                                                                "m : " + iSecond.ToString() + "s );  Percent = " + powerPercent.ToString());
            if (millisecondToFull > 0)
            {
                SetInfoChargeOnly((Convert.ToDouble(powerPercent)/1000.0 - 0.01), 0, 0);
                if (powerPercent >= 1000)
                {
                    powerPercent = 0;
                }
            }
            else
            {
                SetInfoChargeOnly(1.0, 0, 0);
                chargeTimer.Stop();
            }
        }

    }
}
