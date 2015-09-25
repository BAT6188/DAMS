using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAMS.MainControl
{
    public class Init
    {
        Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + "config.ini");

        public void write()
        {
            // 写入ini 
            for (int i = 1; i <= MainConst.USB_INFO_NUMBER_TOTAL; i++)
            {
                String terminaNo = (i.ToString().Length == 1) ? "T0" + i : "T" + i.ToString();
                ini.Writue(MainConst.ROOE_VALUE, MainConst.USB_KEY + i, terminaNo + ",0");
            }
        }

        public Dictionary<String, UsbInfo> read()
        {
            Dictionary<String, UsbInfo> dic = new Dictionary<String, UsbInfo>();
            UsbInfo usbInfo;
            for (int i = 1; i <= MainConst.USB_INFO_NUMBER_TOTAL; i++)
            {
                usbInfo = new UsbInfo();
                // 获取当前警察所用的端口号
                string infos = ini.ReadValue(MainConst.ROOE_VALUE, MainConst.USB_KEY + i);
                String[] info = infos.Split(',');
                String terminaNo = info[0];
                int acquisitionState = int.Parse(info[1]);
                usbInfo.TerminalNo = terminaNo;
                usbInfo.AcquisitionState = acquisitionState;
                dic.Add(MainConst.USB_KEY + i, usbInfo);
            }
            return dic;
        }
    }
}
