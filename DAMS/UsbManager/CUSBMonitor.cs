using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using DAMS;
using DAMS.MainControl;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DAMS.UsbManager
{
    /// <summary>
    /// USB插拔监控类
    /// </summary>
    public class CUSBMonitor
    {
        private List<string> usbList;
        private String[] oldList;

        private Activity activity;

        private Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + "config.ini");

        public CUSBMonitor(Activity activity, List<string> usbList)
        {
            this.activity = activity;
            this.usbList = usbList;
        }
        int usbCount;

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        //public static extern bool SetBackgroundWindow(IntPtr hWnd);
        [DllImport("USER32.DLL", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("USER32.DLL", EntryPoint = "mouse_event")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        [DllImport("USER32.DLL", EntryPoint = "SetCursorPos")]
        public static extern void SetCursorPos(int dx, int dy);

        public static readonly int MOUSEEVENTF_LEFTDOWN = 0x2;
        public static readonly int MOUSEEVENTF_LEFTUP = 0x4;
        //兼容flag
        private String compatibleFlag = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Zhimin", "compatibleFlag");

        public void FillData(int msg, IntPtr wParam)
        {
            try
            {
                if (msg == CWndProMsgConst.WM_DEVICECHANGE)
                {
                    //if ((usbList.Count.Equals(usbCount) || usbList.Count == 0)&&!flag)
                    //{
                    //    //兼容upb驱动操作
                    //    flag = true;
                    handleUSBDriver();

                    //}


                    switch (wParam.ToInt32())
                    {
                        case CWndProMsgConst.WM_DEVICECHANGE:
                            break;
                        case CWndProMsgConst.DBT_DEVICEARRIVAL://U盘插入   

                            var s = DriveInfo.GetDrives();
                            foreach (var drive in s)
                            {
                                if (drive.DriveType == DriveType.Removable)
                                {
                                    if (!usbList.Contains(drive.Name))
                                    {
                                        //DeviceSelect pWin = new DeviceSelect();

                                        //Boolean isclose = (Boolean)pWin.ShowDialog();
                                        //if (!isclose)
                                        //{
                                        //    //智 敏
                                        //    if (pWin.deviceType.Equals("0"))
                                        //    {
                                        //        Process pro = new Process();
                                        //        pro.StartInfo.FileName = @".\\zm\\U.bat";
                                        //        pro.StartInfo.CreateNoWindow = false;
                                        //        pro.Start();
                                        //        pro.WaitForExit();
                                        //    }
                                        //    //警 翼
                                        //    else if (pWin.deviceType.Equals("1"))
                                        //    {
                                        //        Process pro = new Process();
                                        //        pro.StartInfo.FileName = @".\\jy\\AutoConn.exe";
                                        //        pro.StartInfo.CreateNoWindow = false;
                                        //        pro.Start();
                                        //        pro.WaitForExit();
                                        //    }
                                        //    //华德安
                                        //    else if (pWin.deviceType.Equals("2"))
                                        //    {
                                        //        Process pro = new Process();
                                        //        pro.StartInfo.FileName = @".\\hda\\AutoConn.exe";
                                        //        pro.StartInfo.CreateNoWindow = false;
                                        //        pro.Start();
                                        //        pro.WaitForExit();
                                        //    }
                                        //}

                                        if (usbList.Count == 22)
                                        {
                                            MessageBox.Show("可用插口已满，请等待有空闲插口再进行操作！");
                                            LogConfig.info("Administrator", "可用插口已满，请等待有空闲插口再进行操作！");
                                            return;
                                        }
                                        usbList.Add(drive.Name);
                                        usbCount = usbList.Count;
                                        
                                        LogConfig.info("Administrator", drive.Name+"接入");
                                        //killProcess();
                                        // 根据激活状态，迁移画面
                                        activity.ForwardOnActivation(drive.Name, usbList);

                                    }
                                }
                            }
                            break;
                        case CWndProMsgConst.DBT_CONFIGCHANGECANCELED:
                            break;
                        case CWndProMsgConst.DBT_CONFIGCHANGED:
                            break;
                        case CWndProMsgConst.DBT_CUSTOMEVENT:
                            break;
                        case CWndProMsgConst.DBT_DEVICEQUERYREMOVE:
                            break;
                        case CWndProMsgConst.DBT_DEVICEQUERYREMOVEFAILED:
                            break;
                        case CWndProMsgConst.DBT_DEVICEREMOVECOMPLETE: //U盘卸载
                            var u = DriveInfo.GetDrives();
                            oldList = new String[usbList.Count];
                            usbList.CopyTo(oldList);

                            usbList.Clear();
                            foreach (var drive in u)
                            {
                                if (drive.DriveType == DriveType.Removable)
                                {
                                    if (!usbList.Contains(drive.Name))
                                    {
                                        usbList.Add(drive.Name);
                                    }
                                }
                            }
                            for (int i = 0; i < oldList.Length; i++)
                            {
                                if (!usbList.Contains(oldList[i]))
                                {
                                    String terminal_no = ini.ReadValue("drive_name_to_terminal_no", oldList[i]);
                                    activity.SetFinishingInfo(terminal_no);
                                    ini.Writue(MainConst.ROOE_VALUE, MainConst.USB_KEY + terminal_no, "T" + String.Format("{0:00}", Convert.ToInt16(terminal_no)) + ",0");
                                    LogConfig.info("Administrator", oldList[i] + "被拔出");
                                    //break;
                                }
                            }
                            CategoUploadWindow.DeleteFolder(new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Thumbnail", "path"));
                            MainWindow mainwin = (MainWindow)System.Windows.Application.Current.MainWindow;
                            if (mainwin.PpUpload != null)
                            {
                                String fileName = ((CategoUploadWindow)mainwin.PpUpload).xmlFileName;
                                CategoUploadWindow.DeleteAfterCopy(@".\\" + fileName);
                                mainwin.PpUpload.Close();
                            }
                            killProcess();
                            break;
                        case CWndProMsgConst.DBT_DEVICEREMOVEPENDING:
                            break;
                        case CWndProMsgConst.DBT_DEVICETYPESPECIFIC:
                            break;
                        case CWndProMsgConst.DBT_DEVNODES_CHANGED:
                            //killProcess();
                            break;
                        case CWndProMsgConst.DBT_QUERYCHANGECONFIG:
                            break;
                        case CWndProMsgConst.DBT_USERDEFINED:
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogConfig.error("Administrator", ex.Message);
                LogConfig.error("Administrator", ex.StackTrace);
            }
        }

        private void handleUSBDriver()
        {
            //System.Threading.Thread.Sleep(3000);
            //兼容模式
            if (compatibleFlag.Equals("1"))
            {  
                //化德安驱动
                hda();
            }
            //智敏记录仪驱动
            zm();

            //killProcess();
        }

        private void zm()
        {
            Process pro = new Process();
            pro.StartInfo.FileName = @".\\zm\\U.bat";
            pro.StartInfo.UseShellExecute = false;
            pro.StartInfo.RedirectStandardInput = true;//可能接受来自调用程序的输入信息 
            pro.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息 
            pro.StartInfo.CreateNoWindow = true;//不显示程序窗口 
            pro.Start();
            //pro.WaitForExit();
        }

        private void hda()
        {
            String appName = ".\\hda\\执法记录仪文件管理系统.exe";
            //Console.WriteLine ("LtAutoRun starts ... ...");
            try
            {
                Process[] aa = Process.GetProcessesByName("执法记录仪文件管理系统");
                //在进程列表中查找指定的QQ进程
                //在进程列表中查找指定的QQ进程
                foreach (Process p in aa)
                {
                    if (!usbList.Contains(this.activity.newDriverName))
                    {
                        //执行kill命令
                        p.Kill();
                        //等待被杀死的进程退出
                        //p.WaitForExit();
                    }
                    else
                    {
                        return;
                    }

                }

                //Starts the object application
                Process proObj = Process.Start(appName);
                proObj.WaitForInputIdle();
                //proObj.WaitForExit(1000);
                //Console.WriteLine("[" + appName + "] was started successfully.");

                // Get a handle to the Calculator application. The window class  
                // and window name were obtained using the Spy++ tool.  
                //IntPtr objHdl = FindWindow(null, "MainWindow");
                IntPtr objHdl = FindWindow(null, "TCL执法记录仪文件管理系统");

                // Verify that Calculator is a running process.  
                if (objHdl == IntPtr.Zero)
                {
                    //Console.WriteLine("TCL执法记录仪文件管理系统 is not running.");
                    //Console.ReadKey();
                    return;
                }

                //Console.WriteLine("Do setting action.");

                // Make Calculator the foreground application and send it  
                // a set of calculations.   
                SetForegroundWindow(objHdl);

                int iX;
                int iY;
                iX = 403;
                iY = 159;
                SetCursorPos(iX, iY);
                mouse_event(MOUSEEVENTF_LEFTDOWN, iX, iY, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, iX, iY, 0, 0);
                //SetCursorPos(iX, iY);
                //mouse_event(MOUSEEVENTF_LEFTDOWN, iX, iY, 0, 0);
                //mouse_event(MOUSEEVENTF_LEFTUP, iX, iY, 0, 0);
                iX = 330;
                iY = 268;

                SetCursorPos(iX, iY);
                mouse_event(MOUSEEVENTF_LEFTDOWN, iX, iY, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, iX, iY, 0, 0);
                //Thread.Sleep(2000);
                //Console.WriteLine("TxtBox focus...Done.");

                SendKeys.SendWait("1");
                SendKeys.SendWait("2");
                SendKeys.SendWait("3");
                SendKeys.SendWait("4");
                SendKeys.SendWait("5");
                SendKeys.SendWait("6");
                //Thread.Sleep(2000);
                //Console.WriteLine("TxtBox input...Done.");
                iX = 700;
                iY = 268;

                SetCursorPos(iX, iY);
                mouse_event(MOUSEEVENTF_LEFTDOWN, iX, iY, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, iX, iY, 0, 0);
                //SetBackgroundWindow(objHdl);
                //Console.WriteLine("Button click...Done.");     
                //proObj.Kill();
                //proObj.WaitForExit(5000);
                //System.Threading.Thread.Sleep(500);
                System.Windows.Application.Current.MainWindow.Activate();


            }
            catch (Exception e)
            {
                Console.WriteLine("Because of exception occured, LtAutoRun shuts down ... ...");
                Console.WriteLine("     Error Detail: " + e.Message);
                Console.WriteLine(e.StackTrace);

            }
        }
        private void killProcess()
        {
            Process[] aa = Process.GetProcessesByName("执法记录仪文件管理系统");
            //在进程列表中查找指定的QQ进程
            foreach (Process p in aa)
            {
                //执行kill命令
                p.Kill();
                //等待被杀死的进程退出
                //p.WaitForExit();
                //跳出foreach循环，可有可无
                //break;
            }
            //Process[] bb = Process.GetProcessesByName("LtAutoRun");
            ////在进程列表中查找指定的QQ进程
            //foreach (Process p in bb)
            //{
            //    //执行kill命令
            //    p.Kill();
            //    //等待被杀死的进程退出
            //    p.WaitForExit();
            //    //跳出foreach循环，可有可无
            //    break;
            //}
        }
    }

    /// <summary>
    /// windows消息常量
    /// </summary>
    class CWndProMsgConst
    {
        public const int WM_DEVICECHANGE = 0x219;
        public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_CONFIGCHANGECANCELED = 0x0019;
        public const int DBT_CONFIGCHANGED = 0x0018;
        public const int DBT_CUSTOMEVENT = 0x8006;
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;
        public const int DBT_DEVICEQUERYREMOVEFAILED = 0x8002;
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int DBT_DEVICEREMOVEPENDING = 0x8003;
        public const int DBT_DEVICETYPESPECIFIC = 0x8005;
        public const int DBT_DEVNODES_CHANGED = 0x0007;
        public const int DBT_QUERYCHANGECONFIG = 0x0017;
        public const int DBT_USERDEFINED = 0xFFFF;
    }

}
