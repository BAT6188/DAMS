using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.IO;
using System.Xml;

namespace DAMS.RecycleManager
{
    class SpaceRecycler
    {
        //系统重启
        private String sRebootTime;
        //缓存目录
        private String sCacheDir;
        //声音警告
        private String iDriveQuota;
        //文件缓存期限
        private int iDays;

        public SpaceRecycler(String iniFileName)
        {
            //读取Main.ini  
            Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + iniFileName);

            //系统重启时刻
            sRebootTime = ini.ReadValue("Timer", "sRebootTime");
            if ("" == sRebootTime || "24:00" == sRebootTime || "24:00:00" == sRebootTime)
            {
                sRebootTime = " 23:59:58";
            }
            else
            {
                sRebootTime = " " + sRebootTime;
            }

            //缓存目录
            sCacheDir = ini.ReadValue("Local", "sCacheDir");

            //声音警告
            iDriveQuota = ini.ReadValue("Local", "iDriveQuota");

            //文件缓存期限
            String sDays = ini.ReadValue("Local", "iDays");
            if ("" == sDays)
            {
                //If cache duration is not setted, using default as 7 days
                iDays = 7;
            }
            else
            {
                iDays = Convert.ToInt16(sDays);
            }
        }

        // Execute the recycling job
        public void DoRecycle()
        {
            String strDayTimeRestart = System.DateTime.Now.ToString("yyyy/MM/dd");
            strDayTimeRestart = strDayTimeRestart + sRebootTime;
            DateTime dtRestart = Convert.ToDateTime(strDayTimeRestart);

            //After system restarted 30 minutes, recycle process is excuted
            DateTime dtRecycle = dtRestart.AddMinutes(30);
            if (dtRecycle == DateTime.Now)
            {
                // Do recycle
                ThreadStart threadDelegate = delegate { DeleteUpLoadedFiles(sCacheDir, iDays); };
                Thread newThread = new Thread(threadDelegate);
                newThread.Start();
            }
            else
            {
                // Do nothing when real running.
                // This is for Debug
                //DeleteUpLoadedFiles(sCacheDir, iDays); 
            }
        }

        /*Delete media file which has been uploaded to the server (sigined by "SavedFlag=1")
          When all files have been uploaded to the server, the folder will be deleted too.
          Folder structure is as follow
           D:\\Cache
               \\policeNo\\policeNo
                  \\yyyymmdd
                     \\DCIM\\100MEDIA
                        mediaFile.JPG      // Picture file
                        mediaFile.MP4      // Vedio file
                        mediaFile.WAV      // Sound file
                     \\MISC
                       log-yyyymmdd.txt    // Log file
                  \\policeNo_yyyymmdd.xml  // XML file
        */
        private void DeleteUpLoadedFiles(String sDirectory, int days)
        {
            DirectoryInfo diSub;
            String sPoliceNo;
            String sPath;
            String sDateTime;
            String pathXML;
            String pathMedia;
            String sFileName;
            String sSaved;
            String sNodeName;
            String sType;
            Boolean blnDeleteAll;
            DateTime dt1 = System.DateTime.Now;
            DateTime dt2;
            List<string> lstFileToDelete = new List<string>();

            //Cache file's root path
            DirectoryInfo diRoot = new DirectoryInfo(sDirectory);

            if (diRoot.Exists)
            {
                foreach (DirectoryInfo di01 in diRoot.GetDirectories())
                {
                    sPoliceNo = di01.ToString();
                    sPath = sDirectory + "\\" + sPoliceNo + "\\" + sPoliceNo;

                    diSub = new DirectoryInfo(sPath);
                    if (diSub.Exists)
                    {
                        foreach (DirectoryInfo di02 in diSub.GetDirectories())
                        {
                            sDateTime = di02.ToString();

                            String sTmp = sDateTime.Substring(0, 4) + "-" + sDateTime.Substring(4, 2) + "-" + sDateTime.Substring(6, 2);
                            sTmp = sTmp + " " + sDateTime.Substring(8, 2) + ":" + sDateTime.Substring(10, 2) + ":" + sDateTime.Substring(12, 2);

                            dt2 = Convert.ToDateTime(sTmp);
                            TimeSpan ts = dt1.Subtract(dt2).Duration();
                            if (ts.Days <= days)
                            {
                                //folder still in caching period will not be processed.
                                continue;
                            }

                            pathXML = sPath + "\\" + sDateTime + "\\" + sPoliceNo + "_" + sDateTime.Substring(0, 8) + ".xml";
                            FileInfo fi = new FileInfo(pathXML);

                            //yyyymmdd folder is to be deleted.
                            blnDeleteAll = true;

                            if (fi.Exists)
                            {
                                //Parse XML
                                XmlDocument xmlDoc = new XmlDocument();
                                xmlDoc.Load(pathXML);
                                XmlNodeList nodeList = xmlDoc.SelectSingleNode("Root").ChildNodes;
                                foreach (XmlNode xn in nodeList)//遍历所有子节点
                                {
                                    sNodeName = xn.Name;
                                    if (sNodeName == "Video" || sNodeName == "Picture" || sNodeName == "Sound")
                                    {
                                        sType = sNodeName;
                                        switch (sType)
                                        {
                                            case "Video":
                                                {
                                                    sType = ".MP4";
                                                    break;
                                                }
                                            case "Picture":
                                                {
                                                    sType = ".JPG";
                                                    break;
                                                }
                                            case "Sound":
                                                {
                                                    sType = ".WAV";
                                                    break;
                                                }
                                        }

                                        XmlElement xe = (XmlElement)xn;//将子节点类型转换为XmlElement类型
                                        sFileName = xe.GetAttribute("ChangedName");

                                        foreach (XmlNode xn1 in xn)
                                        {
                                            sNodeName = xn1.Name;
                                            if (sNodeName == "SavedFlag")
                                            {
                                                sSaved = xn1.InnerText;//显示子节点点文本
                                                // sSaved: 0/original; 9/recoderToADStationOK; 1/ADStationToServerOK
                                                if ("1" == sSaved || "0" == sSaved)
                                                {
                                                    // Add midia file name into tobe deleted list
                                                    pathMedia = sPath + "\\" + sDateTime + "\\DCIM\\100MEDIA\\" + sFileName;
                                                    pathMedia += sType;

                                                    lstFileToDelete.Add(pathMedia);
                                                }
                                                else
                                                {
                                                    // Because of unsaved file, folder should not to be deleted.
                                                    blnDeleteAll = false;
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }

                                //Delete this folder or media files in this folder
                                DoDelete((sPath + "\\" + sDateTime), blnDeleteAll, lstFileToDelete);
                            }
                            else
                            {
                                //Delete this folder
                                DoDelete((sPath + "\\" + sDateTime), blnDeleteAll, lstFileToDelete);
                            }
                        }
                    }
                }
            }
        }

        //Delete files or folder
        private void DoDelete(String folderPath, Boolean blnDeleteFolder, List<string> lstFileToDelete)
        {
            DirectoryInfo diObj;
            diObj = new DirectoryInfo(folderPath);
            if (diObj.Exists)
            {
                if (blnDeleteFolder)
                {
                    //Clear object folder
                    DeleteFolder(folderPath);

                    //Delete object folder at last
                    Directory.Delete(folderPath);
                }
                else
                {
                    //Loop to delete each file in the list
                    foreach (String objFile in lstFileToDelete)
                    {
                        if (File.Exists(objFile))
                        {
                            FileInfo fi = new FileInfo(objFile);
                            if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                            {
                                fi.Attributes = FileAttributes.Normal;
                            }
                            File.Delete(objFile);  //直接删除其中的文件 
                        }
                    }
                }
            }
        }

        //清空指定的文件夹
        /// <summary>
        /// 清空指定的文件夹，但不删除文件夹
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteFolder(string dir)
        {
            foreach (String d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                    {
                        fi.Attributes = FileAttributes.Normal;
                    }
                    File.Delete(d);  //直接删除其中的文件 
                }
                else
                {
                    DirectoryInfo d1 = new DirectoryInfo(d);
                    if (d1.GetFiles().Length != 0 || d1.GetDirectories().Length != 0)
                    {
                        DeleteFolder(d1.FullName);  //递归删除子文件夹
                    }
                    Directory.Delete(d);
                }
            }
        }
    }
}
