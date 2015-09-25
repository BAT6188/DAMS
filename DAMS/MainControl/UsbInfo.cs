using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAMS.MainControl
{
    public class UsbInfo
    {


        //
        public int NumFileCopied { set; get; }
        public int NumFileAll { set; get; }

        //端口号
        private String terminalNo;

        public String TerminalNo
        {
            get { return terminalNo; }
            set { terminalNo = value; }
        }

        //警官编号
        private String policeNo;

        public String PoliceNo
        {
            get { return policeNo; }
            set { policeNo = value; }
        }

        //记录仪的编号
        private String grapherNo;

        public String GrapherNo
        {
            get { return grapherNo; }
            set { grapherNo = value; }
        }


        //采集状态:进行中
        private int acquisitionState;

        public int AcquisitionState
        {
            get { return acquisitionState; }
            set { acquisitionState = value; }
        }

        //采集百分比:100%
        private String acquisitionShow;

        public String AcquisitionShow
        {
            get { return acquisitionShow; }
            set { acquisitionShow = value; }
        }
        //采集进度:16/16
        private string acquisitionRun;

        public string AcquisitionRun
        {
            get { return acquisitionRun; }
            set { acquisitionRun = value; }
        }

        //电池状态:进行中
        private int batteryState;

        public int BatteryState
        {
            get { return batteryState; }
            set { batteryState = value; }
        }

        //电池百分比:100%
        private String batteryShow;

        public String BatteryShow
        {
            get { return batteryShow; }
            set { batteryShow = value; }
        }

        //电池充电进度:16/16
        private String batteryRun;

        public String BatteryRun
        {
            get { return batteryRun; }
            set { batteryRun = value; }
        }

        //是否激活状态：未激活
        private Boolean isActivation;

        public Boolean IsActivation
        {
            get { return isActivation; }
            set { isActivation = value; }
        }
    }
}

