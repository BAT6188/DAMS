using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DAMS.MainControl
{
    public class CurrentCommon
    {
        //设置当前点击按钮
        public static Button button_usb = new Button();

        //设置当前显示页面
        public static WrapPanel wrapPanel_page = new WrapPanel();

        //设置当前的设备的边框
        public static Border border_info = new Border();

        //设备显示的总数
        public static int sub_count = 0;

        //设置最大页数
        public static int page_total = 0;
    }
}
