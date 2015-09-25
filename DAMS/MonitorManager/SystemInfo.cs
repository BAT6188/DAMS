using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text;
using System.Net;
using System.Management;
using System.Runtime.InteropServices;

namespace DAMS.MonitorManager
{
    class SystemInfo
    {
        private int m_ProcessorCount = 0;       //CPUs number
        private PerformanceCounter pcCpuLoad;   //CPU Performance Counter
        private long m_PhysicalMemory = 0;      //Physical memory

        private string sIPv4 = "";

        private const int GW_HWNDFIRST = 0;
        private const int GW_HWNDNEXT = 2;
        private const int GWL_STYLE = (-16);
        private const int WS_VISIBLE = 268435456;
        private const int WS_BORDER = 8388608;

        #region AIP声明
        [DllImport("IpHlpApi.dll")]
        extern static public uint GetIfTable(byte[] pIfTable, ref uint pdwSize, bool bOrder);

        [DllImport("User32")]
        private extern static int GetWindow(int hWnd, int wCmd);

        [DllImport("User32")]
        private extern static int GetWindowLongA(int hWnd, int wIndx);

        [DllImport("user32.dll")]
        private static extern bool GetWindowText(int hWnd, StringBuilder title, int maxBufSize);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static int GetWindowTextLength(IntPtr hWnd);
        #endregion

        #region 构造函数
        ///  
        /// Construct 
        ///  
        public SystemInfo()
        {
            //Initialize CPU count 
            pcCpuLoad = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            pcCpuLoad.MachineName = ".";
            pcCpuLoad.NextValue();

            //CPU count  
            m_ProcessorCount = Environment.ProcessorCount;

            //Physical Memory
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo["TotalPhysicalMemory"] != null)
                {
                    m_PhysicalMemory = long.Parse(mo["TotalPhysicalMemory"].ToString());
                }
            }
        }
        #endregion

        //Return CPU loading status
        public float CpuLoad
        {
            get
            {
                return pcCpuLoad.NextValue();
            }
        }

        //Return available memory
        public long MemoryAvailable
        {
            get
            {
                long availablebytes = 0;
                ManagementClass mos = new ManagementClass("Win32_OperatingSystem");
                foreach (ManagementObject mo in mos.GetInstances())
                {
                    if (mo["FreePhysicalMemory"] != null)
                    {
                        availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                    }
                }
                return availablebytes;
            }
        }

        //Return physical memory
        public long PhysicalMemory
        {
            get
            {
                return m_PhysicalMemory;
            }
        }

        //Return HDD information
        public List<DiskInfo> GetLogicalDrives()
        {
            List<DiskInfo> drives = new List<DiskInfo>();
            ManagementClass diskClass = new ManagementClass("Win32_LogicalDisk");
            ManagementObjectCollection disks = diskClass.GetInstances();
            foreach (ManagementObject disk in disks)
            {
                // DriveType.Fixed 为固定磁盘(硬盘) 
                if (int.Parse(disk["DriveType"].ToString()) == (int)DriveType.Fixed)
                {
                    drives.Add(new DiskInfo(disk["Name"].ToString(), long.Parse(disk["Size"].ToString()), long.Parse(disk["FreeSpace"].ToString())));
                }
            }
            return drives;
        }

        //Return IP address (IPv4)
        public String getIpAddress()
        {
            if (this.sIPv4.Length > 0) return this.sIPv4;

            sIPv4 = "";
            string hostName = Dns.GetHostName();
            System.Net.IPAddress[] lsAddress = Dns.GetHostAddresses(hostName);
            foreach (IPAddress ipAddr in lsAddress)
            {
                if (!ipAddr.IsIPv6LinkLocal)
                {
                    //Return the first IPv4 address
                    sIPv4 = ipAddr.ToString();
                    break;
                }
            }
            return sIPv4;
        }
    }

    //Disk information bean class
    public class DiskInfo
    {
        public DiskInfo(String sName, long iSize, long iFreeSize)
        {
            name = sName;
            size = iSize;
            freeSize = iFreeSize;
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private long size;

        public long Size
        {
            get { return size; }
            set { size = value; }
        }
        private long freeSize;

        public long FreeSize
        {
            get { return freeSize; }
            set { freeSize = value; }
        }
    }
}
