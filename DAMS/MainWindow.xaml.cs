using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DAMS.RecycleManager;
using DAMS.MonitorManager;
//using DAMS.Monitor;
using DAMS.MainControl;

using System.Data.OracleClient;
using System.Data.OleDb;
using System.Data;


namespace DAMS
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MonitorWindow ppWinMonitor;
        private SystemSetting ppWinSetting;
        private CategoUploadWindow ppUpload;

        public CategoUploadWindow PpUpload
        {
            get { return ppUpload; }
            set { ppUpload = value; }
        }


        //TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, 0);
        DispatcherTimer timer = new DispatcherTimer();

        //TimeSpan statusSpan = new TimeSpan(0, 0, 0, 0, 0);
        DispatcherTimer statusTimer = new DispatcherTimer();

        //Disk clean processer
        private SpaceRecycler spcRecycler = new SpaceRecycler("Main.ini");

        //Status Monitor processer
        private DaMonitor daMonitor = new DaMonitor("Main.ini");

        public MainWindow()
        {
            LogConfig.info("Administrator","系统启动");
            InitializeComponent();

            double workHeight = SystemParameters.WorkArea.Height;
            double workWidth = SystemParameters.WorkArea.Width;
            this.Top = (workHeight - this.Height) / 2 + MainConst.OFFSET_PIXS_VERTICAL;
            this.Left = (workWidth - this.Width) / 2;

            //WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.WindowState = WindowState.Maximized;

            //testc();
        }


        private void testc()
        {

            OleDbConnection conn = null;

            String cnnStr = "Provider=OraOLEDB.Oracle;User ID=ZHIMIN;Password=ZHIMIN;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.120)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=orcl)))";
            //String cnnStr = "Provider=OraOLEDB.Oracle.1;User ID=ZHIMIN;Password=ZHIMIN;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.120)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=orcl)))";
            //String cnnStr = "Provider=OraOLEDB.Oracle;User ID=ZHIMIN;Password=ZHIMIN;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.120)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=orcl)))";

            try
            {
                LogConfig.info("Administrator", " -- DB connecting string is as : " + cnnStr);

                if (conn == null)
                    conn = new OleDbConnection(cnnStr);

                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                String sql = "select * from T_DASTATION";
                OleDbCommand cmd = new OleDbCommand(sql, conn);

                DataTable dt = ExecuteDataTable(cmd);
                string sNum = dt.Rows[0]["DAS_NUMBER"].ToString();
                LogConfig.info("Administrator", sNum);
            }
            catch (Exception ex)
            {
                LogConfig.info("Administrator", "!!!! DB Exception: " + ex.Message);
            }
            if (conn != null)
            {
                conn.Close();
                conn = null;
            }
        }

        private DataTable ExecuteDataTable(OleDbCommand cmd)
        {
            DataSet dtSet = new DataSet();
            using (OleDbDataAdapter dtAdpter = new OleDbDataAdapter(cmd))
            {
                try
                {
                    dtAdpter.Fill(dtSet);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            if (dtSet.Tables.Count > 0)
            {
                dtSet.Tables[0].DefaultView.RowStateFilter = DataViewRowState.Unchanged |
                                                          DataViewRowState.Added |
                                                          DataViewRowState.ModifiedCurrent |
                                                          DataViewRowState.Deleted;
                return dtSet.Tables[0];
            }
            else
                return null;
        }





        //Show Monitor Window
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Start the Timer
                StartTimer();

                //For recycler test
                //spcRecycler.DoRecycle();

                //Set IP status for NG as the original
                SolidColorBrush sClrBrush = new SolidColorBrush();
                Uri uriImageCp;
                uriImageCp = new Uri(@"/DAMS;component/Resources/Orage3_S.png", UriKind.Relative);
                this.imgIpStatus.Source = new BitmapImage(uriImageCp);

                //Start the Timer for IP status
                this.ipShow.Content = GetIPAddress();
                StartStatusTimer();

                //When application is started, show monitor window.
                OpenMonitorWindow();
             }catch(Exception exc){
                 LogConfig.error("Administrator", exc.Message);
                 LogConfig.error("Administrator", exc.StackTrace);
             }
        }

        //Show System Setting window
        public void OpenSettingWindow()
        {
            //ppWinMonitor.Visibility = Visibility.Visible;
            ppWinSetting = new SystemSetting();
            ppWinSetting.Owner = this;
            ppWinSetting.ShowDialog();

            //When System Setting window was closed, Monitor window will be opened.
            OpenMonitorWindow();
        }

        //Show Monitor window in modal 
        public void OpenMonitorWindow()
        {
            if (ppWinMonitor != null)
            {
                //ppWinMonitor.Close();
                ppWinMonitor.Top = ppWinMonitor.PixTop;
                ppWinMonitor.Left = ppWinMonitor.PixLeft;
            }
            else
            {
                ppWinMonitor = new MonitorWindow();
                ppWinMonitor.Owner = this;
                Boolean blnExit = (Boolean)ppWinMonitor.ShowDialog();
                if (blnExit)
                {
                    //Shut application down.
                    Application.Current.Shutdown();
                }
            }
        }

        //Start the Timer
        private void StartTimer()
        {
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Tick += OnTimer;
            timer.IsEnabled = true;
            timer.Start();
        }

        //Refresh the Date Time on MainWindow
        private void OnTimer(object sender, EventArgs e)
        {
            //timeSpan += new TimeSpan(0, 0, 0, 1);

            System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //String time = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

            String[] weekdays = { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
            String weekday = weekdays[(int)System.DateTime.Now.DayOfWeek];

            this.lblDateTime.Content = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " " + weekday;
            //如果是server模式  启动自动删除Timer
            String serverFlag = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Server", "serverType");
            if (serverFlag.Equals("server"))
            {
                //Recycling process
                spcRecycler.DoRecycle();
            }
        }

        //Start the Timer
        private void StartStatusTimer()
        {

            statusTimer.Interval = new TimeSpan(0, 0, 0, 1);
            statusTimer.Tick += OnStatusTimer;
            statusTimer.IsEnabled = true;
            statusTimer.Start();
        }

        //Refresh the Date Time on MainWindow
        private void OnStatusTimer(object sender, EventArgs e)
        {
            Ini mainIni = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini");

            //服务器ip
            string sIP = mainIni.ReadValue("Server", "sIP");
            SolidColorBrush sClrBrush = new SolidColorBrush();

            Uri uriImageCp;
            if (PingNetAddress(sIP))
            {
                uriImageCp = new Uri(@"/DAMS;component/Resources/Green3_S.png", UriKind.Relative);
            }
            else
            {
                uriImageCp = new Uri(@"/DAMS;component/Resources/Orage3_S.png", UriKind.Relative);
            }
            //显示硬盘空间
            string iDriveQuota = mainIni.ReadValue("Local", "iDriveQuota");
            string sCacheDir = mainIni.ReadValue("Local", "sCacheDir");
            string hdPath = sCacheDir.Split(':')[0];
            
            this.imgIpStatus.Source = new BitmapImage(uriImageCp);
            long totalHD = GetHardDiskSpace(hdPath);
            long freeHD = GetHardDiskFreeSpace(hdPath);
            this.hdTotal.Content = "合计 " + totalHD + "GB";
            this.hdFree.Content = "剩余 " + freeHD + "GB";
            this.hdStatus.Minimum = 0;
            this.hdStatus.Maximum = 100;
            if (0 != totalHD)
            {
                this.hdStatus.Value = ((totalHD -freeHD) * 100.0 / totalHD);
            }
            else
            {
                this.hdStatus.Value = 0;
            }

            iDriveQuota = (iDriveQuota == "" )? "0" : iDriveQuota;
            
            if (Convert.ToDouble(iDriveQuota) > freeHD)
            {
               Console.Beep(800, 5000);
               MessageBox.Show("硬盘可用空间即将达到设定的上限，请联系管理员！", MainControl.MainConst.MESSAGE_BOX_TITLE);
            }

            //Monitor this DA station and insert info into DB
            daMonitor.Monitor();

            statusTimer.Interval = new TimeSpan(0, 0, 15, 0);
            
        }

        private String GetIPAddress()
        {
            String str;
            String Result = "";
            String hostName = Dns.GetHostName();
            IPAddress[] myIP = Dns.GetHostAddresses(hostName);
            foreach (IPAddress address in myIP)
            {
                str = address.ToString();
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] >= '0' && str[i] <= '9' || str[i] == '.') Result = str;
                }
            }
            return Result;
        }

        /// <summary>
        /// ping 具体的网址看能否ping通
        /// </summary>
        /// <param name="strNetAdd"></param>
        /// <returns></returns>
        private static bool PingNetAddress(string strNetAdd)
        {
            bool Flage = false;
            Ping ping = new Ping();
            try
            {
                PingReply pr = ping.Send(strNetAdd, 3000);
                if (pr.Status == IPStatus.TimedOut)
                {
                    Flage = false;
                }
                if (pr.Status == IPStatus.Success)
                {
                    Flage = true;
                }
                else
                {
                    Flage = false;
                }
            }
            catch
            {
                Flage = false;
            }
            finally
            {
                ping.Dispose();
            }
            return Flage;
        }

        ///   
        /// 获取指定驱动器的空间总大小(单位为B) 
        ///   
        ///  只需输入代表驱动器的字母即可 （大写） 
        ///    
        public static long GetHardDiskSpace(string str_HardDiskName)
        {
            long totalSize = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    totalSize = drive.TotalSize / (1024 * 1024 * 1024);
                }
            }
            return totalSize;
        }

        ///   
        /// 获取指定驱动器的剩余空间总大小(单位为B) 
        ///   
        ///  只需输入代表驱动器的字母即可  
        ///    
        public static long GetHardDiskFreeSpace(string str_HardDiskName)
        {
            long freeSpace = new long();
            str_HardDiskName = str_HardDiskName + ":\\";
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drives)
            {
                if (drive.Name == str_HardDiskName)
                {
                    freeSpace = drive.TotalFreeSpace / (1024 * 1024 * 1024);
                }
            }
            return freeSpace;
        }

    }
}
