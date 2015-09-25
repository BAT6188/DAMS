using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DAMS.MainControl
{
    public class ControlFactory
    {
        MonitorWindow win;
        UsbInfo info;

        public ControlFactory(MonitorWindow win)
        {
            this.win = win;
        }

        //创建Grid行列并追加一个控件
        public void CreateUsbInfoControl(WrapPanel panelPage, UsbInfo info)
        {
            this.info = info;

            MonitorUnitCtrl mUnit = new MonitorUnitCtrl(info);
            win.Grid_Container.RegisterName(mUnit.Name, mUnit);
            panelPage.Children.Add(mUnit);
        }
    }
}
