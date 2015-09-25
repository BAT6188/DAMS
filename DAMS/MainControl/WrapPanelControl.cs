using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DAMS.MainControl
{
    public class WrapPanelControl
    {
        private double width = 0;
        private double height = 0;

        MonitorWindow win;
        List<WrapPanel> panels = new List<WrapPanel>();

        Dictionary<string, UsbInfo> dic;

        ControlFactory control;

        //构造函数
        public WrapPanelControl(MonitorWindow win, Dictionary<string, UsbInfo> dic)
        {
            this.win = win;
            this.dic = dic;
            control = new ControlFactory(win);
        }

        //构造函数
        public WrapPanelControl(MonitorWindow win, Dictionary<string, UsbInfo> dic, double width, double height)
        {
            this.win = win;
            this.dic = dic;
            this.width = width;
            this.height = height;
            control = new ControlFactory(win);
        }

        public void CreateWrapPanel()
        {
            //根据设备信息创建Grid控件
            CalculationOfNumber();

            //创建并添加已有的设备
            Create_Canvas();
        }

        //创建WrapPanel控件
        private void CalculationOfNumber()
        {
            int pageTotal = (dic.Count % MainConst.USB_INFO_NUMBER_BY_PAGE == 0) ? dic.Count / MainConst.USB_INFO_NUMBER_BY_PAGE : dic.Count / MainConst.USB_INFO_NUMBER_BY_PAGE + 1;

            CurrentCommon.page_total = pageTotal;

            Thickness thick = new Thickness(20, 99, 0, 0);

            for (int i = 0; i < pageTotal; i++)
            {
                //创建显示页
                WrapPanel panel = new WrapPanel()
                {
                    Width = this.width,
                    Height = this.height,
                    Visibility = Visibility.Hidden,
                    Name = "Page" + (i + 1),
                    Margin = thick,
                };

                //设定名称
                this.win.Grid_Container.RegisterName(panel.Name, panel);

                if (i == 0)
                {
                    panel.Visibility = Visibility.Visible;
                    CurrentCommon.wrapPanel_page = panel;
                }
                panels.Add(panel);

            }
        }

        //追加已有的设备
        public void Create_Canvas()
        {
            WrapPanel panel = null;

            if (panels != null || panels.Count != 0)
            {
                int j = 0;
                for (int i = 0; i < dic.Count; i++)
                {
                    if (i % MainConst.USB_INFO_NUMBER_BY_PAGE == 0)
                    {
                        panel = panels[j];
                        j++;
                    }
                    UsbInfo info = new UsbInfo();
                    dic.TryGetValue(MainConst.USB_KEY + (i + 1), out info);
                    control.CreateUsbInfoControl(panel, info);

                    if (i % MainConst.USB_INFO_NUMBER_BY_PAGE == 0)
                    {
                        this.win.Grid_Container.Children.Add(panel);
                    }
                }
            }
        }
    }
}
