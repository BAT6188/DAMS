using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace DAMS.MainControl
{
    public class Activity
    {
        private System.Windows.Media.Color colorOnFocus;

        private MonitorWindow win;

        private Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + "config.ini");

        //从Main.ini中取得缓存路径
        private String localCachePath = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Local", "sCacheDir");

        //private String xmlFile;

        //private String driverName;

        //警官编号
        //private String policeNo;

        SolidColorBrush scb = new SolidColorBrush(Colors.Transparent);

        List<string> usbList;

        public string newDriverName;

        int usbNo;

        public Activity(MonitorWindow win)
        {
            this.win = win;
            colorOnFocus = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF00CCFF");
        }

        //获取记录仪的API，根据记录仪的编号获取信息
        public UsbInfo readUsbInfo(String name)
        {
            UsbInfo info = new UsbInfo();
            string infos = ini.ReadValue(MainConst.ROOE_VALUE_1, name);
            String[] inf = infos.Split(',');
            info.GrapherNo = name;
            info.TerminalNo = inf[0];
            info.PoliceNo = inf[1];

            return info;
        }

        //激活时，写入录入信息
        //s1:记录仪编号
        //s2端口编号
        //s3警察编号
        public void writeUsbInfo(String s1, String s2, String s3)
        {
            ini.Writue(MainConst.ROOE_VALUE_1, s1, s2 + "," + s3);
        }

        public void changeStyle(int usbNo)
        {
            //获取目标页
            int currentPage = (usbNo % MainConst.USB_INFO_NUMBER_BY_PAGE == 0) ? usbNo / MainConst.USB_INFO_NUMBER_BY_PAGE : usbNo / MainConst.USB_INFO_NUMBER_BY_PAGE + 1;
            WrapPanel panel = this.win.Grid_Container.FindName("Page" + currentPage) as WrapPanel;

            MonitorUnitCtrl mUnitCtrl;
            for (int i = 0; i < panel.Children.Count; i++)
            {
                mUnitCtrl = panel.Children[i] as MonitorUnitCtrl;
                mUnitCtrl.SetFocus(false);
            }

            String ctrName = "UnitT" + String.Format("{0:00}", usbNo);
            mUnitCtrl = panel.FindName(ctrName) as MonitorUnitCtrl;

            //设置边框
            mUnitCtrl.SetFocus(true);

            //设置显示页
            CurrentCommon.wrapPanel_page.Visibility = Visibility.Hidden;
            CurrentCommon.wrapPanel_page = panel;
            CurrentCommon.wrapPanel_page.Visibility = Visibility.Visible;
        }

        // 根据激活状态，迁移画面
        public void ForwardOnActivation(string driveName, List<string> usbList)
        {
            this.usbList = usbList;
            this.newDriverName = driveName;
            Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + "config.ini");
            String terminalNo ="";
            for (int i = 1; i <= MainConst.USB_INFO_NUMBER_TOTAL; i++)
            {
                // 获取当前警察所用的端口号
                string infos = ini.ReadValue(MainConst.ROOE_VALUE, MainConst.USB_KEY + i);
                String[] info = infos.Split(',');
                String freeTerminaNo = info[0];
                int acquisitionState = int.Parse(info[1]);
                if (acquisitionState.Equals(0))
                {
                    terminalNo =  freeTerminaNo;
                    ini.Writue(MainConst.ROOE_VALUE, MainConst.USB_KEY + i, freeTerminaNo + ",1");
                    break;
                }
            }
            ini.Writue("drive_name_to_terminal_no", driveName, Convert.ToInt16(terminalNo.Replace("T", "")).ToString());
            this.usbNo = Convert.ToInt16(terminalNo.Replace("T",""));

            //获取目标页
            int currentPage = (usbNo % MainConst.USB_INFO_NUMBER_BY_PAGE == 0) ? usbNo / MainConst.USB_INFO_NUMBER_BY_PAGE : usbNo / MainConst.USB_INFO_NUMBER_BY_PAGE + 1;
            WrapPanel panel = this.win.Grid_Container.FindName("Page" + currentPage) as WrapPanel;
            MonitorUnitCtrl mUnitCtrl;
            String ctrName = "Unit" + terminalNo;
            mUnitCtrl = panel.FindName(ctrName) as MonitorUnitCtrl;
            //上传或激活画面表示
            mUnitCtrl.ForwardOnActivation(driveName, this.newDriverName);

            newDriverName = "";
        }

        //拔出设备后重置为闲置状态
        public void SetFinishingInfo(String TerminalNo)
        {
            //获取目标页
            int currentPage = (usbNo % MainConst.USB_INFO_NUMBER_BY_PAGE == 0) ? usbNo / MainConst.USB_INFO_NUMBER_BY_PAGE : usbNo / MainConst.USB_INFO_NUMBER_BY_PAGE + 1;
            WrapPanel panel = this.win.Grid_Container.FindName("Page" + currentPage) as WrapPanel;

            MonitorUnitCtrl mUnitCtrl;
            String ctrName = "UnitT" + String.Format("{0:00}", Convert.ToInt16(TerminalNo));
            mUnitCtrl = panel.FindName(ctrName) as MonitorUnitCtrl;
            LogConfig.info("Administrator", "Page" + currentPage+"页"+ctrName + "被更新！");
            //上传或激活画面表示
            mUnitCtrl.SetFree();
        }
    }
}
