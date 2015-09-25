using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAMS.MainControl
{
    public class MainConst
    {
        // 端口状态：空闲
        public const int STATUS_FLAG_LEISURE = 0;

        // 端口状态：采集中
        public const int STATUS_FLAG_COLLECTION = 1;

        // 端口状态：采集完了
        public const int STATUS_FLAG_COLLECTION_OVER = 2;

        // 端口状态：未激活
        public const int STATUS_FLAG_NOT_ACTIVATED = 99;

        //配置文件的根节点
        public const String ROOE_VALUE = "Setting";

        //配置文件的根节点
        public const String ROOE_VALUE_1 = "Usbinfo";


        //配置文件的Key
        public const String USB_KEY = "number_of_machines";

        //每个画面显示端口的个数
        public const int USB_INFO_NUMBER_BY_PAGE = 25;

        //画面显示端口的总个数
        public static int USB_INFO_NUMBER_TOTAL = Convert.ToInt16(new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Local", "iUSBNumber"));

        //画面纵向调整Offset
        public static int OFFSET_PIXS_VERTICAL = 17;

        //MessageBox 标题
        public static String MESSAGE_BOX_TITLE = "北京智敏科技";
    }
}
