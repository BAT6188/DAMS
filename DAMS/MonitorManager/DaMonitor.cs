using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data;
using System.Threading.Tasks;
using System.Net;
using DAMS.MonitorManager;

namespace DAMS.MonitorManager
{
    class DaMonitor
    {
        private Ini ini;
        private String isMnt;
        private DbManager dbm = null;
        private SystemInfo sysInfo = null;
        private String sOrgCode;

        public DaMonitor(String iniFileName)
        {
            ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + iniFileName);
            isMnt = ini.ReadValue("DbServer", "isMnt");
            if (isMnt == "1")
            {
                //Unit code
                sOrgCode = ini.ReadValue("Server", "sOrg");

                //Turn on the monitoring function
                //sIP = ini.ReadValue("DbServer", "sIP");
                //sPort = ini.ReadValue("DbServer", "sPort");
                //sSID = ini.ReadValue("DbServer", "sSID");
                //sUID = ini.ReadValue("DbServer", "sUID");
                //sPWD = ini.ReadValue("DbServer", "sPWD");
                LogConfig.info("Administrator", "    Monitoring function is turned on, key is as :" + isMnt);
                //LogConfig.info("            IP :", sIP);
                //LogConfig.info("    Port is as :", sPort);
                //LogConfig.info("     UID is as :", sUID);
                //LogConfig.info("     PWD is as :", sPWD);

                dbm = new DbManager();

                //Create SystemInfo object
                sysInfo = new SystemInfo();
            }
            else
            {
                LogConfig.info("Administrator", "Monitoring function is turned off, key is as :" + isMnt);
            }
        }

        public void Monitor()
        {
            if (isMnt != "1") return;

            //Get all monitoering information of this DA
            String sIPv4 = sysInfo.getIpAddress();
            //LogConfig.info("Administrator", "         IP address(v4) is as : " + sIPv4);

            float iUsage = sysInfo.CpuLoad;
            //LogConfig.info("Administrator", "         CPU use factor is as : " + iUsage.ToString() + "%");

            long iFree = sysInfo.MemoryAvailable;
            long iTotal = sysInfo.PhysicalMemory;
            iFree = iFree / (1024 * 1024);
            iTotal = iTotal / (1024 * 1024);
            //LogConfig.info("Administrator", "            Free memory is as : " + String.Format("{0:00}", iFree));
            //LogConfig.info("Administrator", "        Physical memory is as : " + String.Format("{0:00}", iTotal));

            List<DiskInfo> logicalDriv = sysInfo.GetLogicalDrives();
            //Insert information into DB
            Dictionary<string, string> alParam = new Dictionary<string, string>();
            alParam.Add("@DAS_NUMBER", this.sOrgCode);              //DAS_NUMBER
            alParam.Add("@DAS_SERIAL_NUMBER", "");                  //DAS_SERIAL_NUMBER
            alParam.Add("@DAS_NAME", "");                           //DAS_NAME
            alParam.Add("@UNIT_UUID", this.sOrgCode);               //UNIT_UUID
            alParam.Add("@IP", sIPv4);                              //IP
            alParam.Add("@AUTHORITY_STATUS", "2");                  //AUTHORITY_STATUS
            alParam.Add("@AUTHORITY_PERIOD", "");                   //AUTHORITY_PERIOD
            alParam.Add("@AUTHORITY_PERIOD_UNIT", "");              //AUTHORITY_PERIOD_UNIT
            alParam.Add("@DT_AUTHORITY", "");                       //DT_AUTHORITY
            alParam.Add("@AUTHORITY_OP", "");                       //AUTHORITY_OP
            alParam.Add("@STATUS_1", "NORMAL");                     //STATUS_1 : network 
            alParam.Add("@STATUS_2", String.Format("{0:#.##}", iUsage)); //STATUS_2 : cpu
            alParam.Add("@STATUS_3", String.Format("{0:00}", iFree) + ":" + String.Format("{0:00}", iTotal)); //STATUS_3 : memory
            int stateIndex = 4;
            foreach (DiskInfo info in logicalDriv)
            {
                iFree = info.FreeSize;
                iTotal = info.Size;
                iFree = iFree / (1024 * 1024);
                iTotal = iTotal / (1024 * 1024);
                String value = info.Name + ":" + String.Format("{0:00}", iFree) + ":" + String.Format("{0:00}", iTotal);
                //LogConfig.info("Administrator", "         Disk information : " + info.Name);
                //LogConfig.info("Administrator", "                     Size : " + info.Size.ToString() + "    FreeSize : " + info.FreeSize.ToString());
                alParam.Add("@STATUS_" + stateIndex, value);
                stateIndex++;
            }
            try
            {
                //DB insert
                dbm.InsertSystemInfo(alParam);
            }
            catch (Exception ex)
            {
                LogConfig.error("Administrator", ex.Message);
            }
        }
    }
}
