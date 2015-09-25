using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAMS.MainControl
{
    public class MainWindowControl
    {
        MonitorWindow win;

        Dictionary<string, UsbInfo> dic;

        Init init;

        public MainWindowControl(MonitorWindow win)
        {
            this.win = win;
            this.win.InitializeComponent();
            init = new Init();
            load();
        }

        public void load()
        {
            //写入配置文件
            init.write();

            //读取配置文件
            dic = init.read();
            CreateWrapPanel();
            //CreateButton();
        }

        //添加Grid
        private void CreateWrapPanel()
        {
            WrapPanelControl ct = new WrapPanelControl(this.win, dic, 1440, 900);
            ct.CreateWrapPanel();
        }

        //创建Button
        private void CreateButton()
        {
            ButtonControl bControl = new ButtonControl(this.win);
            bControl.createButton(dic);
        }

    }
}
