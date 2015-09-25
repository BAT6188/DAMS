using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Windows.Documents;
using DAMS.VideoManager;

namespace DAMS.MainControl
{
    class MonitorUnitCtrlProcess
    {
        //警官编号
        private String policeNo;
        private Ini ini = new Ini(AppDomain.CurrentDomain.BaseDirectory + "config.ini");
        //从Main.ini中取得缩略图路径
        private String ThumbnailPath = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Thumbnail", "path");
        private String companyName = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Server", "sRemoteUser");
        //从Main.ini中取得缓存路径
        private String localCachePath = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Local", "sCacheDir");
        //从Main.ini中取得是否要上传到服务器（采集站模式或者服务器模式）
        private String serverFlag = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Server", "serverType");
        //从Main.ini中取得上传类型（分类上传或者直接上传）
        private String categoFlag = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Zhimin", "categoFlag");
        //从Main.ini中取得视频压缩模式（压缩或不压缩）
        private String compressFlag = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Zhimin", "compressFlag");
        private String org = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Server", "sOrg");

        private String xmlFile;
        private VideoDisplay vs = new VideoDisplay();
        //LordControl
        private MonitorUnitCtrl lordCtrl;

        public MonitorUnitCtrlProcess(MonitorUnitCtrl lordCtrl)
        {
            this.lordCtrl = lordCtrl;
        }

        //查看是否已经激活过
        public Boolean ActivationCheck(string drive_name)
        {
            string path = drive_name + "sn.txt";

            // 文件不存在，未激活
            if (!FileExistCheck(path))
            {
                return false;
            }

            // 文件存在，根据文件里的内容，判断是否激活
            policeNo = OpenTxt(path);

            if (policeNo == null || string.Empty == policeNo)
            {
                return false;
            }

            this.lordCtrl.PoliceNo = policeNo;

            if (!PoliceNoExistCheck(policeNo))
            {
                return false;
            }

            LogConfig.info(policeNo, "已激活过");
            return true;
        }

        public int GetBorderNo(string name)
        {
            string borderStr = name.Substring(name.Length - 2);
            int borderNo = int.Parse(borderStr);
            return borderNo;
        }

        // 判断警员编号是否存在
        private Boolean PoliceNoExistCheck(string policeNo)
        {
            string police_value = ini.ReadValue("SN", policeNo);
            if ("".Equals(police_value))
            {
                return false;
            }

            return true;
        }
        private List<string> pathList;

        //遍历记录仪，如果有新媒体文件存在，则弹出上传画面
        public void UploadWindowShow(string drive_name)
        {
            string path = drive_name + "sn.txt";
            try {
                pathList = new List<string>();
                policeNo = OpenTxt(path);

                String folderPath = "";
                String filePath = GetFiles(new DirectoryInfo(drive_name), "*.MP4", folderPath);
                //if (filePath == string.Empty || filePath.Length == 0)
                //if (pathList.Count==0)
                {
                    filePath = GetFiles(new DirectoryInfo(drive_name), "*.WAV", folderPath);
                }
                //if (filePath == string.Empty || filePath.Length == 0)
                //if (pathList.Count == 0)
                { 
                    filePath = GetFiles(new DirectoryInfo(drive_name), "*.JPG", folderPath);
                }

            //if (filePath == String.Empty || filePath.Length == 0)
                if (pathList.Count == 0)
                {
                    //采集进度表示
                    UsbInfo info = new UsbInfo();
                    info.NumFileCopied = 0;
                    info.NumFileAll = 0;
                    this.lordCtrl.Dispatcher.Invoke(new DelegateHandle(AcquisitionStateOfChange), info);
                    MessageBox.Show("该执法记录仪中没有有效的媒体文件！", MainControl.MainConst.MESSAGE_BOX_TITLE);
                    LogConfig.info("Administrator", drive_name+":该执法记录仪中没有有效的媒体文件");
                    return;
                }
            }
            catch (Exception ioe)
            {
                LogConfig.error("Administrator", ioe.Message);
                LogConfig.error("Administrator", ioe.StackTrace);
                UsbInfo info = new UsbInfo();
                this.lordCtrl.Dispatcher.Invoke(new DelegateHandle(SetControlFree), info);
                return;
            }
            //分类模式
            if (categoFlag.Equals("1"))
            {
                LogConfig.info("Administrator", "启动分类上传画面");
                LogConfig.info(policeNo, "启动分类上传画面");
                LogConfig.info(policeNo, "媒体文件存放路径个数：" + pathList.Count);
                //弹出文件上传画面
                MonitorWindow winMonitor = (MonitorWindow)Window.GetWindow(this.lordCtrl);
                Double iTop = winMonitor.Top;
                //Move the monitor window out of the screen
                winMonitor.Top = -3000;

                CategoUploadWindow swp = new CategoUploadWindow(this.lordCtrl.TerminalNo, policeNo, pathList);

                MainWindow mainwin = (MainWindow)Application.Current.MainWindow;
                mainwin.PpUpload = swp;

                swp.Owner = winMonitor;
                Boolean isclose = true;
                try
                {
                    isclose = (Boolean)swp.ShowDialog();
                }catch(Exception e){
                    LogConfig.error("Administrator", e.Message);
                    LogConfig.error("Administrator", e.StackTrace);
                    swp.Close();
                    UsbInfo info = new UsbInfo();
                    this.lordCtrl.Dispatcher.Invoke(new DelegateHandle(SetControlFree), info);
                }
                

                //Restore the monitor window to it's original position
                winMonitor.Top = iTop;
                killProcess();
                if (!isclose)
                {
                    if (swp.buttonType.Equals("ButtonUpload"))
                    {
                        if (!Directory.Exists(localCachePath))
                        {
                            Directory.CreateDirectory(localCachePath);
                        }
                        xmlFile = swp.xmlFileName;
                        ThreadStart threadDelegate = delegate { CopyFile(drive_name, xmlFile); };

                        Thread newThread = new Thread(threadDelegate);
                        newThread.Start();
                    }
                    else if (swp.buttonType.Equals("ButtonQuit"))
                    {
                        //采集进度表示
                        UsbInfo info = new UsbInfo();
                        info.NumFileCopied = 0;
                        info.NumFileAll = 0;
                        this.lordCtrl.Dispatcher.Invoke(new DelegateHandle(AcquisitionStateOfChange), info);
                    }
                    else
                    {
                        //Upload画面中异常断开时，设置USB状态为空闲
                        UsbInfo info = new UsbInfo();
                        this.lordCtrl.Dispatcher.Invoke(new DelegateHandle(SetControlFree), info);
                    }
                }
            }
            //直接上传模式
            else
            {
               //this.lordCtrl.Dispatcher.Invoke(new DelegateHandle(SetReady), new UsbInfo());

                //开始上传
                ThreadStart threadDelegate = delegate { CopyFile(drive_name, xmlFile); };
                Thread newThread = new Thread(threadDelegate);
                newThread.Start();
            }
 
        }

        //自动生成XML
        private void AutoXML()
        {
            DateTime dt = DateTime.Now;
            String xmlDate = dt.ToString("yyyyMMdd");
            String title = policeNo + "_" + xmlDate;
            //获取文件名
            xmlFile = title + ".xml";

            XmlTextWriter writer = new XmlTextWriter(xmlFile, System.Text.Encoding.UTF8);
            //使用自动缩进便于阅读
            writer.Formatting = Formatting.Indented;
            //XML声明 
            writer.WriteStartDocument();
            //书写根元素 
            writer.WriteStartElement("Root");
            writer.WriteAttributeString("Org", org);
            //查找记录仪下所有的文件
            for (int i = 0; i < pathList.Count; i++)
            {
                DirectoryInfo files = new DirectoryInfo(pathList[i]);
                int j = 0;
                //遍历记录仪下所有文件
                foreach (FileInfo item in files.GetFiles())
                {
                    String ext = item.Extension;
                    if (ext == null || ext.Length < 3)
                        continue;

                    //找到视频文件，记录总个数(MP4)
                    if ((item.Extension.Substring(1, item.Extension.Length - 1)) == "MP4")
                    {
                        //vs.Convert(item.DirectoryName + "\\" + item.Name, ThumbnailPath + "\\", item.Name.Substring(0, item.Name.Length - 4), VideoDisplay.VideoType.MP4, false, true);
                        //CreateXML("Video", title, j, vs.VideoLength, item, writer);
                        string sMp4Length = vs.GetVideoDuration(vs.ffmpegpath, item.DirectoryName + "\\" + item.Name);
                        CreateXML("Video", title, j, sMp4Length, item, writer);
                        j++;
                    }
                    //找到视频文件，记录总个数(MOV)
                    else if ((item.Extension.Substring(1, item.Extension.Length - 1)) == "MOV")
                    {
                        //vs.Convert(item.DirectoryName + "\\" + item.Name, ThumbnailPath + "\\", item.Name.Substring(0, item.Name.Length - 4), VideoDisplay.VideoType.MOV, false, true);
                        //CreateXML("Video", title, j, vs.VideoLength, item, writer);
                        string sMovLength = vs.GetVideoDuration(vs.ffmpegpath, item.DirectoryName + "\\" + item.Name);
                        CreateXML("Video", title, j, sMovLength, item, writer);
                        j++;
                    }
                    //查找音频文件，记录总长度
                    else if ((item.Extension.Substring(1, item.Extension.Length - 1)) == "WAV")
                    {
                        string sLength = vs.GetVideoDuration(vs.ffmpegpath, item.DirectoryName + "\\" + item.Name);
                        CreateXML("Sound", title, j, sLength, item, writer);
                        j++;
                    }
                    //查找图片文件，记录总长度
                    else if ((item.Extension.Substring(1, item.Extension.Length - 1)) == "JPG")
                    {
                        CreateXML("Picture", title, j, "00:00:00", item, writer);
                        j++;
                    }

                }
            }
            //关闭文件流
            writer.Close();
        }

        //生成XML方法
        private void CreateXML(string type, string title, int index, string lendth, FileInfo item, XmlTextWriter writer)
        {

            //开始一个元素 
            writer.WriteStartElement(type);
            //向先前创建的元素中添加一个属性
            //DateTime dt = DateTime.Now;
            //String uploadTime = dt.ToString("yyyyMMddHHmmss");
            //dt.GetDateTimeFormats();
            String policeNo = title.Split('_')[0];
            String shootTime = item.Name.Split('_')[2];
            writer.WriteAttributeString("ChangedName", companyName + "_" + policeNo + "_" + policeNo + "_" + shootTime + "_" + lendth.Replace(":", "-") + "_" + index);
            //添加子元素
            writer.WriteElementString("Name", System.IO.Path.GetFileNameWithoutExtension(item.Name));
            writer.WriteElementString("Class", "");
            writer.WriteElementString("Comments", "");
            writer.WriteElementString("SavedFlag", "0");
            //关闭item元素
            writer.WriteEndElement();

        }


        private void killProcess()
        {
            Process[] aa = Process.GetProcessesByName("执法记录仪文件管理系统");
            //在进程列表中查找指定的QQ进程
            foreach (Process p in aa)
            {
                //执行kill命令
                p.Kill();
                //等待被杀死的进程退出
                //p.WaitForExit();
                //跳出foreach循环，可有可无
                //break;
            }
        }

        // 复制文件
        private void CopyFile(String drive_name, String xmlFile)
        {
            if (!categoFlag.Equals("1")) { 
                    LogConfig.info("Administrator", "自动上传");
                    LogConfig.info(policeNo, "自动上传");
                    LogConfig.info(policeNo, "媒体文件存放路径个数：" + pathList.Count);
                    //遍历文件，自动生成XML
                    try
                    {
                        AutoXML();
                        xmlFile = this.xmlFile;
                    }
                    catch (Exception e)
                    {
                        LogConfig.error(policeNo, e.Message);
                        LogConfig.error(policeNo, e.StackTrace);
                        LogConfig.error("Administrator", e.Message);
                        LogConfig.error("Administrator", e.StackTrace);
                        if (File.Exists(xmlFile))
                        {
                            //如果存在则删除
                            File.Delete(xmlFile);
                        }
                        MessageBox.Show("执法记录仪中可能存在被病毒感染的文件！", MainControl.MainConst.MESSAGE_BOX_TITLE);
                        return;
                    }
            }
            try
            {
                // 获取采集文件信息
                List<string> upload_file_list = new List<string>();
                List<string> new_file_list = new List<string>();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(@".\\" + xmlFile);
                String[] inf = xmlFile.Split('_');
                string policeNo = inf[0];
                XmlNodeList list = xmlDoc.GetElementsByTagName("Name");
                XmlNodeList newList = xmlDoc.SelectNodes("//@ChangedName");

                foreach (XmlNode item2 in list)
                {
                    upload_file_list.Add(item2.InnerText);
                }
                foreach (XmlNode newItem in newList)
                {
                    new_file_list.Add(newItem.Value);
                }
                LogConfig.info(policeNo, "媒体文件个数:" + new_file_list.Count);
                // 情报
                DateTime dt = DateTime.Now;
                String uploadTime = dt.ToString("yyyyMMddHHmmss");

                //移走根目录下xml文件
                String xmlPath = localCachePath + "\\" + policeNo + "\\" + policeNo + "\\" + uploadTime + "\\";
                if (!Directory.Exists(xmlPath))
                {
                    Directory.CreateDirectory(xmlPath);
                }
                File.Copy(@".\\" + xmlFile, xmlPath + System.IO.Path.GetFileName(xmlFile), true);
                LogConfig.info(policeNo, xmlFile + "被移到" + xmlPath);
                //删除xml文件
                DeleteAfterCopy(@".\\" + xmlFile, "");
                LogConfig.info(policeNo, xmlFile + "被删除");
                UsbInfo info = new UsbInfo();
                info.NumFileCopied = 1;
                info.NumFileAll = upload_file_list.Count;

                //初始采集进度表示
                //this.SetInfoCopyOnly(0, upload_file_list.Count);
                this.lordCtrl.Dispatcher.Invoke(new DelegateHandle(SetInfoForFirst), info);

                int i;
                for (i = 0; i < upload_file_list.Count; i++)
                {
                    try { 
                            // 拷贝文件并且重新命名
                            CopyAndRename(drive_name, localCachePath + "\\" + policeNo + "\\" + policeNo + "\\" + uploadTime + "\\", upload_file_list[i], new_file_list[i]);
                            LogConfig.info(policeNo, upload_file_list[i] + "被重新命名为" + new_file_list[i]);
                            LogConfig.info(policeNo, new_file_list[i] + "被复制或压缩");
                            setDeleteFlagForPC(xmlPath + System.IO.Path.GetFileName(xmlFile), new_file_list[i]);
                            String filePath = GetFileName(new DirectoryInfo(drive_name), "*" + upload_file_list[i] + "*", "");
                            File.Delete(filePath);
                            LogConfig.info(policeNo, upload_file_list[i] + "从执法记录仪中删除");
                            //设定参数
                            info.NumFileCopied = i + 1;
                            //采集进度表示
                            this.lordCtrl.Dispatcher.Invoke(new DelegateHandle(AcquisitionStateOfChange), info);
                        }catch(Exception copye){
                            LogConfig.error(policeNo, copye.Message);
                            LogConfig.error(policeNo, copye.StackTrace);
                            LogConfig.error("Administrator", copye.Message);
                            LogConfig.error("Administrator", copye.StackTrace);
                            MessageBox.Show("上传失败，上传过程中请不要随意拔掉执法记录仪！", MainControl.MainConst.MESSAGE_BOX_TITLE);
                            return;
                        }
                }
                
                //设定参数
                info.NumFileCopied = upload_file_list.Count;
                //采集进度表示
                this.lordCtrl.Dispatcher.Invoke(new DelegateHandle(AcquisitionStateOfChange), info);

                //拷贝完了
                Ini mainIni = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini");

                //服务器路径
                string sRemoteDir = mainIni.ReadValue("Server", "sRemoteDir");
                string sIP = mainIni.ReadValue("Server", "sIP");
                string sRemoteUser = mainIni.ReadValue("Server", "sRemoteUser");
                string sRemotePin = mainIni.ReadValue("Server", "sRemotePin");
                string iDays = mainIni.ReadValue("Local", "iDays");
                
                if (i == upload_file_list.Count)
                {
                    //删除执法记录仪里文件(保留sn.txt)
                    //DeleteFolder(drive_name);
                    //sRemoteDir = @"\\" + sIP + "\\" + sRemoteDir;
                    String localPath = localCachePath + "\\" + policeNo + "\\" + policeNo + "\\" + uploadTime + "\\";
                    String policePath = localCachePath + "\\" + policeNo + "\\" + policeNo + "\\";
                    String serverPath = @"\\" + sIP + "\\" + sRemoteDir + "\\" + policeNo + "\\" + uploadTime + "\\";//+ policeNo + "\\"
                    //非分布式模式 
                    if (serverFlag.Equals("server"))
                    {
                        LogConfig.info(policeNo, "服务器模式，准备上传。");
                        // 拷贝文件到服务器
                        if (Connect(sIP + "\\" + sRemoteDir, sRemoteUser, sRemotePin) || ExistConnect(sIP + "\\" + sRemoteDir, sRemoteUser, sRemotePin))
                        {
                            for (i = 0; i < new_file_list.Count; i++)
                            {
                                LogConfig.info(policeNo, "服务器连接成功，开始上传。");
                                // 拷贝文件到服务器
                                //CopyFolder(localCachePath + "\\", sRemoteDir, new_file_list[i]);
                                CopyFolder(localPath, serverPath, new_file_list[i]);
                                //CopyFolder(localCachePath + "\\" + policeNo + "\\" + policeNo + "\\" + uploadTime + "\\", sRemoteDir + "\\" + policeNo + "\\" + policeNo + "\\" + uploadTime + "\\", new_file_list[i]);
                                LogConfig.info(policeNo, new_file_list[i]+"上传成功。");
                                setDeleteFlagForXML(xmlPath + System.IO.Path.GetFileName(xmlFile), new_file_list[i]);
                                File.Copy(xmlPath + System.IO.Path.GetFileName(xmlFile), serverPath + System.IO.Path.GetFileName(xmlFile), true);
                                LogConfig.info(policeNo, iDays + "天后删除。");
                                if (iDays.Equals("0"))
                                {
                                    deleteFileFromFolder(new DirectoryInfo(localPath), new_file_list[i]);
                                    LogConfig.info(policeNo, new_file_list[i] + "删除成功。");
                                }
                            }
                            //查找未上传的文件
                            Dictionary<String, String> unUploadList = findUnUploadFile(new DirectoryInfo(policePath));
                            Dictionary<String, String>.KeyCollection fileNamekeys = unUploadList.Keys;
                            LogConfig.info(policeNo, "查找到"+unUploadList.Count + "件未上传成功的文件。");
                            foreach (string name in fileNamekeys)
                            {
                                string value = unUploadList[name].ToString();
                                CopyFolder(policePath, @"\\" + sIP + "\\" + sRemoteDir + "\\" + policeNo + "\\", name);// + policeNo + "\\"
                                LogConfig.info(policeNo, name + "再次上传成功。");
                                setDeleteFlagForXML(value, name);
                                String[] fileInfomation = name.Split('_');
                                String oldUploadTime = fileInfomation[3];
                                FileInfo oldXmlFile = new FileInfo(value);
                                File.Copy(value, @"\\" + sIP + "\\" + sRemoteDir + "\\" + policeNo + "\\"  + oldXmlFile.Directory.Name + "\\" + oldXmlFile.Name, true);//+ policeNo + "\\"
                                LogConfig.info(policeNo, iDays + "天后删除。");
                                if (iDays.Equals("0"))
                                {
                                    deleteFileFromFolder(new DirectoryInfo(policePath), name);
                                    LogConfig.info(policeNo, name + "删除成功。");
                                }
                            }
                            this.unUploadList = new Dictionary<String, String>();
                        }
                        else
                        {
                            LogConfig.error(policeNo, "上传失败，与服务器连接存在问题，请稍后再试！");
                            LogConfig.error("Administrator", "上传失败，与服务器连接存在问题，请稍后再试！");
                            MessageBox.Show("上传失败，与服务器连接存在问题，请稍后再试！", MainControl.MainConst.MESSAGE_BOX_TITLE);
                        }
                    }
                    else
                    {
                        LogConfig.info(policeNo, "采集站模式，操作结束。");
                    }
                }
                
            }catch(Exception e){
                //MessageBox.Show(e.StackTrace);
                LogConfig.error(policeNo,e.Message);
                LogConfig.error(policeNo, e.StackTrace);
                LogConfig.error("Administrator", e.Message);
                LogConfig.error("Administrator", e.StackTrace);
                //MessageBox.Show("上传失败，与服务器连接出现问题！", MainControl.MainConst.MESSAGE_BOX_TITLE);
            }
        }

        private delegate void DelegateHandle(UsbInfo info);

        //采集进度改变
        public void AcquisitionStateOfChange(UsbInfo info)
        {
            this.lordCtrl.SetInfoCopyOnly(info.NumFileCopied, info.NumFileAll);
            //this.lordCtrl.SetInfoChargeOnly(0.4, 6, 0);
        }

        //采集进度改变
        public void SetReady(UsbInfo info)
        {
            this.lordCtrl.SetReady(0.4, 6, 0);
        }

        //采集进度改变为空闲
        public void SetControlFree(UsbInfo info)
        {
            this.lordCtrl.SetFree();
        }

        public void SetInfoForFirst(UsbInfo info)
        {
            this.lordCtrl.SetInfoForFirst(info.NumFileCopied,info.NumFileAll);
        }
        //查找视频、音频、图片文件名
        public static string GetFileName(DirectoryInfo directory, string pattern, string path)
        {
            if (directory.Exists || pattern.Trim() != string.Empty)
            {

                foreach (FileInfo info in directory.GetFiles(pattern))
                {
                    path = info.FullName.ToString();
                    break;
                }

                foreach (DirectoryInfo info in directory.GetDirectories())
                {
                    path = GetFileName(info, pattern, path);
                    if (path != string.Empty)
                    {
                        break;
                    }
                }
            }
            return path;
        }

        // 复制文件并改名
        private void CopyAndRename(string from, string to, string file_name, string new_name)
        {
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            // 子文件夹  
            foreach (string sub in Directory.GetDirectories(from))
            {
                CopyAndRename(sub + "\\", to + System.IO.Path.GetFileName(sub) + "\\", file_name, new_name);
            }
            // 文件 
            foreach (string file in Directory.GetFiles(from))
            {
                if (file_name.Equals(System.IO.Path.GetFileNameWithoutExtension(file)))
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                    {
                        fi.Attributes = FileAttributes.Normal;
                    }

                    if (compressFlag.Equals("1"))
                    {
                        if (System.IO.Path.GetExtension(file).Equals(".MP4"))
                        {
                            vs.Compress(System.IO.Path.GetFullPath(file), to, new_name, DAMS.VideoManager.VideoDisplay.VideoType.MP4);
                        }
                        else if (System.IO.Path.GetExtension(file).Equals(".MOV"))
                        {
                            vs.Compress(System.IO.Path.GetFullPath(file), to, new_name, DAMS.VideoManager.VideoDisplay.VideoType.MOV);
                        }
                        else
                        {
                            File.Copy(file, to + new_name + System.IO.Path.GetExtension(file), true);
                        }
                    }
                    else
                    {
                        File.Copy(file, to + new_name + System.IO.Path.GetExtension(file), true);
                    }

                    if (System.IO.Path.GetExtension(file).Equals(".MP4") || System.IO.Path.GetExtension(file).Equals(".MOV"))
                    {
                        vs.ConvertImageForAll(System.IO.Path.GetFullPath(file), to, new_name);
                    }

                }
                if (file.Contains("log-"))
                {
                    File.Copy(file, to + System.IO.Path.GetFileName(file), true);
                    File.Delete(file);
                }
            }
        }


        // 判断文件是否存在
        private Boolean FileExistCheck(string path)
        {
            if (File.Exists(path))
            {
                return true;
            }

            return false;
        }


        //清空执法记录仪
        /// <summary>
        /// 清空指定的文件夹，但不删除文件夹
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteFolder(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                    {
                        fi.Attributes = FileAttributes.Normal;
                    }
                    //不删除sn.txt文件
                    if (!d.ToLower().Contains("sn.txt"))
                    {
                        File.Delete(d);//直接删除其中的文件 
                    }
                }
                else
                {
                    DirectoryInfo d1 = new DirectoryInfo(d);
                    if (d1.GetFiles().Length != 0 || d1.GetDirectories().Length != 0)
                    {
                        DeleteFolder(d1.FullName);////递归删除子文件夹
                    }
                    Directory.Delete(d);
                }
            }
        }


        //拷贝结束后删除操作
        private void DeleteAfterCopy(String xmlFile, String usbPath)
        {
            if (File.Exists(xmlFile))
            {
                //如果存在则删除
                File.Delete(xmlFile);
            }
        }


        // 复制文件
        private void CopyFolder(string from, string to, string file_name)
        {
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            // 子文件夹  
            foreach (string sub in Directory.GetDirectories(from))
            {
                CopyFolder(sub + "\\", to + System.IO.Path.GetFileName(sub) + "\\", file_name);
            }
            // 文件 
            foreach (string file in Directory.GetFiles(from))
            {
                if (file_name.Equals(System.IO.Path.GetFileNameWithoutExtension(file)))
                {
                    File.Copy(file, to + System.IO.Path.GetFileName(file), true);
                }
                if (file.Contains("log-"))// || file.Contains("xml"))
                {
                    File.Copy(file, to + System.IO.Path.GetFileName(file), true);
                }
            }
        }


        //查找视频、音频、图片文件
        public string GetFiles(DirectoryInfo directory, string pattern, string path)
        {
            
            if (directory.Exists || pattern.Trim() != string.Empty)
            {

                foreach (FileInfo info in directory.GetFiles(pattern))
                {
                    path = info.DirectoryName.ToString();
                    if (!pathList.Contains(path))
                    {
                        if(path.Contains("DCIM"))
                        pathList.Add(path);
                    }
                }

                foreach (DirectoryInfo info in directory.GetDirectories())
                {
                    path = GetFiles(info, pattern, path);
                    if (path != string.Empty)
                    {
                        if (!pathList.Contains(path))
                        {
                            if (path.Contains("DCIM"))
                            pathList.Add(path);
                        }
                    }
                }
            }
            return path;
        }


        // 读取文件里内容
        private string OpenTxt(string path)
        {
            //FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            //仅 对文本 执行  读写操作     
            StreamReader sr = new StreamReader(path);
            //定位操作点,begin 是一个参考点     
            sr.BaseStream.Seek(0, SeekOrigin.Begin);
            //读一下，看看文件内有没有内容，为下一步循环 提供判断依据     
            //sr.ReadLine() 这里是 StreamReader的要领  可不是 console 中的~      
            string str = sr.ReadLine();//假如  文件有内容      
            //C#读取TXT文件之关上文件，留心顺序，先对文件内部执行 关上，然后才是文件~     
            sr.Close();
            //fs.Close();

            return str;
        }


        //跟映射盘建立连接
        public bool Connect(string remoteHost, string userName, string passWord)
        {
            bool Flag = true;
            Process proc = new Process();

            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;
            try
            {

                proc.Start();
                string command = @"net  use  \\" + remoteHost + "  " + passWord + "  " + "  /user:" + userName + ">NUL";

                proc.StandardInput.WriteLine(command);
                command = "exit";

                proc.StandardInput.WriteLine(command);
                while (proc.HasExited == false)
                {
                    proc.WaitForExit(1000);
                }

                string errormsg = proc.StandardError.ReadToEnd();
                if (errormsg != "")
                    Flag = false;
                proc.StandardError.Close();
            }
            catch
            {
                Flag = false;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }

            return Flag;
        }

        //判断跟映射盘已经建立连接
        public bool ExistConnect(string remoteHost, string userName, string passWord)
        {
            bool Flag = true;
            Process proc = new Process();

            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;
            try
            {

                proc.Start();
                string command = @"net  use  \\" + remoteHost;

                proc.StandardInput.WriteLine(command);
                command = "exit";

                proc.StandardInput.WriteLine(command);
                while (proc.HasExited == false)
                {
                    proc.WaitForExit(1000);
                }

                string errormsg = proc.StandardError.ReadToEnd();
                if (errormsg != "")
                    Flag = false;
                proc.StandardError.Close();
            }
            catch
            {
                Flag = false;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }

            return Flag;
        }

        //上传到服务器后设置SavedFlag=1
        private void setDeleteFlagForXML(String xmlFile,String Name)
        {
           //加载此XML文件
           XmlDocument xmlDoc = new XmlDocument();
           xmlDoc.Load(xmlFile);
           //查找XML目录下“Name”节点
           //XmlNodeList list = xmlDoc.GetElementsByTagName("Name");
           XmlNodeList list = xmlDoc.SelectNodes("//*[@ChangedName='" + Name + "']");
           //遍历“Name”节点
             foreach (XmlNode item2 in list)
               {
                //查找要设置deleteFlag的节点
                item2.LastChild.InnerText = "1";
                xmlDoc.Save(xmlFile);
               }
        }

        //完全上传到采集站后设置SavedFlag=9
        private void setDeleteFlagForPC(String xmlFile, String Name)
        {
            //加载此XML文件
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFile);
            //查找XML目录下“Name”节点
            //XmlNodeList list = xmlDoc.GetElementsByTagName("Name");
            XmlNodeList list = xmlDoc.SelectNodes("//*[@ChangedName='" + Name + "']");
            //遍历“Name”节点
            foreach (XmlNode item2 in list)
            {
                //查找要设置deleteFlag的节点
                item2.LastChild.InnerText = "9";
                xmlDoc.Save(xmlFile);
            }
        }
        //遍历文件夹删除指定的文件
        private void deleteFileFromFolder(DirectoryInfo pathName, String fileName)
        {
            FileInfo[] allfile = pathName.GetFiles("*"+fileName+"*"); 
            foreach (FileInfo tt in allfile)
            {            
               tt.Delete();
            }
            
            DirectoryInfo[] direct = pathName.GetDirectories(); 
            
            foreach (DirectoryInfo dirTemp in direct)  
            {  
                deleteFileFromFolder(dirTemp,fileName);
            }       
        }
        //List<string> unUploadList = new List<string>();
        Dictionary<String, String> unUploadList = new Dictionary<String, String>();
        //遍历文件夹找到没有上传成功的文件
        private Dictionary<String, String> findUnUploadFile(DirectoryInfo pathName)
        {
            
            FileInfo[] allfile = pathName.GetFiles("*.xml");
            foreach (FileInfo tt in allfile)
            {
                //加载此XML文件
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(tt.FullName);
                //查找XML目录下“Name”节点
                XmlNodeList list = xmlDoc.GetElementsByTagName("SavedFlag");
                //遍历“Name”节点
                foreach (XmlNode item2 in list)
                {
                    //查找要设置deleteFlag的节点
                    if (item2.InnerText.Equals("9"))
                    {
                        String changedName = item2.ParentNode.Attributes["ChangedName"].Value;
                        if (!unUploadList.ContainsKey(changedName))
                        unUploadList.Add(changedName, tt.FullName);
                    }

                }
            }

            DirectoryInfo[] direct = pathName.GetDirectories();

            foreach (DirectoryInfo dirTemp in direct)
            {
                findUnUploadFile(dirTemp);
            }

            return unUploadList;
        }
    }
}
