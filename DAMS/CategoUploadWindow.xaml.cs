using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Xml;
using DAMS.VideoManager;
using DAMS.UserControls;
using DAMS.MainControl;
using System.Diagnostics;

namespace DAMS
{
    /// <summary>
    /// CategoUploadWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CategoUploadWindow : Window
    {
        
        private Color colorNoFocus;
        private Color colorFocus;

        //private string enterType = "2";//"1":插入USB弹出，"2":点击文件查看弹出
        //终端号
        private string terNumber = "";
        //警员编号
        private string userName = "";
        //记录仪路径
        private List<string> discCode = new List<string>();
        //媒体种类按钮
        private Button[] searchMediaType;
        //数据源按钮
        private Button[] searchDateSource;
        //视频缩略图按钮控件
        private MyButton[] videoImageDisplay;
        //音频按钮控件
        private MyButton[] soundImageDisplay;
        //图片按钮控件
        private MyButton[] pictureImageDisplay;
        //视频缩略图名称
        private String[] videoImage;
        //音频文件名称
        private String[] sounds;
        //图片文件名称
        private String[] pictures;
        //视频缩略图父容器
        private Grid[] videoGrid;
        //音频缩略图父容器
        private Grid[] soundGrid;
        //图片父容器
        private Grid[] pictureGrid;
        //视频左右选择按钮
        private int plusOrMinus = 0;
        //缩略图转换完毕后的个数
        private int maxLength;
        //选中的媒体种类名称
        private string searchByMediaType = "视频";
        //选中的数据源名称
        private string searchByDateSource;
        //左边视频播放器的计时器
        private DispatcherTimer timerLeft = new DispatcherTimer();
        //右边视频播放器播放或暂停的控制器
        private int playOrPauseFlgRight;//1:Play,2:Pause
        //右边视频播放器的计时器
        private DispatcherTimer timerRight = new DispatcherTimer();
        //右边视频播放器点击屏幕播放或暂停的控制器
        private bool isPlayingRight = false;
        //private Uri uriLeft;
        //private Uri uriRight;
        //实例化生成缩略图的类
        VideoDisplay vs = new VideoDisplay();
        //记录仪中所有视频个数
        private int videoLength;
        //记录仪中所有音频个数
        private int soundLength;
        //记录仪中所有图片个数
        private int pictureLength;
        //XML文件的文件个数自增长
        int index = 0;
        //private Button[] searchYear;
        //private Button[] searchMonth;
        //private Button[] searchDay;
        //private Button[] searchClassType;
        //private string searchByYear;
        //private string searchByMonth;
        //private string searchByDay;
        //private string searchByClassType;
        //按鈕類型
        public String buttonType = "";
        //上傳xml文件名
        public String xmlFileName = "";
        //从Main.ini中取得缩略图路径
        private String ThumbnailPath = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Thumbnail", "path");
        private String companyName = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Server", "sRemoteUser");
        private Ini catIni = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini");
        private String org = new Ini(AppDomain.CurrentDomain.BaseDirectory + "Main.ini").ReadValue("Server", "sOrg");
        //保存视频音频图片对应的时长
        private Dictionary<String, String> timeDic = new Dictionary<String, String>();
        private Dictionary<String, String> nameDic ;
        private Dictionary<String, String> nameDicALL;

        //Background worker for changing video to shutcut image in a new background thread.
        System.ComponentModel.BackgroundWorker bkgWorker;

        // 当前表示的种类
        private string currentType = "";
        // 视频全部选择flag
        private Boolean videoSelectAllFlag = false;
        // 音频全部选择flag
        private Boolean audioSelectAllFlag = false;
        // 图片全部选择flag
        private Boolean imageSelectAllFlag = false;

        public CategoUploadWindow(String terNumber, String userName, List<string> discCode)
        {
            InitializeComponent();
            InitialFilterButtons();
            //其他按钮隐藏
            btnP04.Visibility = System.Windows.Visibility.Collapsed;
            //服务器按钮隐藏
            btnT02.Visibility = System.Windows.Visibility.Collapsed;
            //设定数据分类
            string[] sectionList;
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] sectionByte = catIni.IniReadValues("Type", null);
            //编码所有key的string类型
            string sections = ascii.GetString(sectionByte);
            //获取key的数组
            sectionList = sections.Split(new char[1] { '\0' });
            for (int t = 0; t < sectionList.Length; t++)
            {
                if (sectionList[t] == null || sectionList[t].Equals(""))
                {
                    break;
                }
                else
                {
                    this.ComboBoxClass.Items.Add(catIni.ReadValue("Type",sectionList[t]));
                }
            }
            this.ComboBoxClass.SelectedIndex = 0;//设置第一项为默认选择项。

                //警员编号
            this.terNumber = terNumber;
            this.userName = userName;//"陈警官"
            this.lblPoliceNo.Content = userName;

            //记录仪路径
            this.discCode = discCode; //"F:\\DCIM\\20150429";
            nameDic = new Dictionary<String, String>();
            nameDicALL = new Dictionary<String, String>();
            //查找记录仪下所有的文件
            for (int i = 0; i < discCode.Count; i++)
            {
                DirectoryInfo files = new DirectoryInfo(discCode[i]);

                //遍历记录仪下所有文件
                foreach (FileInfo item in files.GetFiles())
                {
                    String ext = item.Extension;
                    if (ext == null || ext.Length < 3)
                        continue;
                    //创建map
                    nameDic.Add(item.Name.Substring(0 , item.Name.Length-4), item.DirectoryName);
                    nameDicALL.Add(item.Name.Substring(0, item.Name.Length - 4), item.FullName);
                }
            }
            //Window's location control
            double workHeight = SystemParameters.WorkArea.Height;
            double workWidth = SystemParameters.WorkArea.Width;
            this.Top = (workHeight - this.Height) / 2 + MainConst.OFFSET_PIXS_VERTICAL;
            this.Left = (workWidth - this.Width) / 2;
            //WindowStartupLocation = WindowStartupLocation.CenterScreen;

            //创建缩略图文件夹
            if (!Directory.Exists(ThumbnailPath))
            {
                Directory.CreateDirectory(ThumbnailPath);
            }

            //------------------------------------BEG
            //定义媒体种类数组
            searchMediaType = new Button[] { btnP01, btnP02, btnP03 };
            //定义数据源类型数组
            searchDateSource = new Button[] { btnT01, btnT02 };

            //ButtonPictureShowLeft.Background = new SolidColorBrush(Color.FromRgb(204, 204, 204));
            ButtonPictureShowRight.Background = new SolidColorBrush(Color.FromRgb(204, 204, 204));

            //设置点击屏幕播放或暂停的控制器的初始值
            playOrPauseFlgRight = 1;

            //Show the waiting animation
            UsrCtrlWaiting ucWaiting = new UsrCtrlWaiting();
            this.wrpWaiting.Children.Add(ucWaiting);
            this.cvsWaiting.Visibility = Visibility.Visible;

            //查找记录仪中所有文件并加载到对应的数组中
            bkgWorker = new System.ComponentModel.BackgroundWorker();
            bkgWorker.WorkerReportsProgress = false;
            bkgWorker.WorkerSupportsCancellation = false;
            bkgWorker.RunWorkerCompleted += DoWorkCompleted;
            bkgWorker.DoWork += DoWork;
            if (bkgWorker.IsBusy)
            {
                return;
            }
            bkgWorker.RunWorkerAsync();
            //GetItemFromMachine();

            //默认加载视频播放器
            VideoOrPictureShow();
            //------------------------------------END
        }

        void DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            //从记录仪中按分类查找，如果有视频文件则制作其缩略图
            GetItemFromMachine();
        }

        void DoWorkCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //查找缩略图文件夹下图片
            DirectoryInfo images = new DirectoryInfo(ThumbnailPath);
            FileInfo[] fi = images.GetFiles();
            videoLength = 0;
            //查找记录仪下所有的文件
            for (int i = 0; i < discCode.Count; i++)
            {
                DirectoryInfo files = new DirectoryInfo(discCode[i]);

                //遍历记录仪下所有文件
                foreach (FileInfo item in files.GetFiles())
                {
                    String ext = item.Extension;
                    if (ext == null || ext.Length < 3)
                        continue;
                    //找到视频文件，记录总个数
                    if ((item.Extension.Substring(1, item.Extension.Length - 1)) == "MP4")
                    {
                        videoLength = videoLength + 1;
                    }
                    //nameDic.Add(item.Name, item.DirectoryName);
                }
            }

            //视频全部转换成缩略图
            if (images.GetFiles().Length == videoLength)
            {
                //定义用于存放动态生成的Grid的数组
                maxLength = videoLength;
                videoGrid = new Grid[videoLength];
                soundGrid = new Grid[soundLength];
                pictureGrid = new Grid[pictureLength];
                //缩略图名称加入到缩略图数组
                for (int i = 0; i < images.GetFiles().Length; i++)
                {
                    videoImage[i] = fi[i].Name;
                }
                // 记住当前类型
                currentType = "MP4";
                //加载生成好的控件
                InsertGrid("MP4");
                
            }

            //Stop showing the waiting animation
            UsrCtrlWaiting ucWaiting = (UsrCtrlWaiting)this.wrpWaiting.Children[0];
            ucWaiting.StopWaiting();
            this.cvsWaiting.Visibility = Visibility.Hidden;
        }


        //从记录仪中按分类查找
        private void GetItemFromMachine()
        {

            //初始化数组长度
            videoLength = 0;
            soundLength = 0;
            pictureLength = 0;
            for (int s=0; s < discCode.Count; s++)
            {
                //遍历记录仪中文件
                DirectoryInfo videoes = new DirectoryInfo(discCode[s]);

                //遍历记录仪下所有文件
                foreach (FileInfo item in videoes.GetFiles())
                {
                    String ext = item.Extension;
                    if (ext == null || ext.Length < 3)
                        continue;
                    //查找视频文件并转换缩略图，记录总长度
                    if ((item.Extension.Substring(1, item.Extension.Length - 1)) == "MP4")
                    {
                        vs.Convert(nameDic[item.Name.Substring(0, item.Name.Length - 4)] + "\\" + item.Name, ThumbnailPath + "\\", item.Name.Substring(0, item.Name.Length - 4), VideoDisplay.VideoType.MP4, true, true);
                        timeDic.Add(item.Name.Substring(0, item.Name.Length - 4), vs.VideoLength);
                        videoLength = videoLength + 1;
                    }
                    //查找音频文件，记录总长度
                    if ((item.Extension.Substring(1, item.Extension.Length - 1)) == "WAV")
                    {
                        string sLength = vs.GetVideoDuration(vs.ffmpegpath, nameDic[item.Name.Substring(0, item.Name.Length - 4)] + "\\" + item.Name);
                        timeDic.Add(item.Name.Substring(0, item.Name.Length - 4), sLength);
                        soundLength = soundLength + 1;
                    }
                    //查找图片文件，记录总长度
                    if ((item.Extension.Substring(1, item.Extension.Length - 1)) == "JPG")
                    {
                        timeDic.Add(item.Name.Substring(0, item.Name.Length - 4), "00:00:00");
                        pictureLength = pictureLength + 1;
                    }
                }
            }

            //定义缩略图图片数组长度
            videoImage = new String[videoLength];
            //定义视频按钮数组长度
            videoImageDisplay = new MyButton[videoLength];
            //定义音频数组长度
            sounds = new String[soundLength];
            //定义音频按钮数组长度
            soundImageDisplay = new MyButton[soundLength];
            //定义图片数组长度
            pictures = new String[pictureLength];
            //定义图片按钮数组长度
            pictureImageDisplay = new MyButton[pictureLength];
            //重新遍历记录仪下所有文件
            int i = 0;
            int j = 0;
            for (int m=0; m < discCode.Count; m++)
            {
                DirectoryInfo videoes = new DirectoryInfo(discCode[m]);

                foreach (FileInfo item in videoes.GetFiles())
                {
                    String ext = item.Extension;
                    if (ext == null || ext.Length < 3)
                        continue;
                    //查找音频文件，加入到音频数组
                    if ((item.Extension.Substring(1, item.Extension.Length - 1)) == "WAV")
                    {
                        sounds[i] = item.Name;
                        i++;
                    }
                    //查找图片文件，加入到图片数组
                    if ((item.Extension.Substring(1, item.Extension.Length - 1)) == "JPG")
                    {
                        pictures[j] = item.Name;
                        j++;
                    }
                }
            }
        }

        //按分类生成Grid控件组
        private void CreateGrid(String type)
        {
            //当类型为视频时，动态生成用于显示的Grid
            if (type == "MP4")
            {
                for (int i = 0; i < maxLength; i++)
                {
                    Grid grid = new Grid();
                    grid.Name = "VideoGrid" + videoImage[i].Substring(0, videoImage[i].Length - 4);
                    //在主窗体中注册Grid
                    MainGrid.RegisterName(grid.Name, grid);
                    grid.Width = 130;
                    grid.Height = 120;
                    //设置Grid在主窗体中的位置
                    grid.HorizontalAlignment = HorizontalAlignment.Left;
                    grid.VerticalAlignment = VerticalAlignment.Top;
                    //设置Grid与主窗体的位移
                    int left = 73 + 129 * i;
                    int top = 683;
                    Thickness tk = new Thickness(left, top, 0, 0);
                    grid.Margin = tk;
                    //动态生成缩略图按钮
                    MyButton button = new MyButton();
                    //自定义属性 IsDefaule  false：取消选中  true:选中
                    button.IsDefalut = false;
                    //自定义属性 IsSave  false：没有保存到XML文件中 true：已经保存到XML文件中
                    button.IsSaved = false;
                    button.Name = "Vide" + videoImage[i].Substring(0, videoImage[i].Length - 4);
                    //grid.RegisterName(fi[i].Name.Substring(0, fi[i].Name.Length - 4), button);
                    button.Content = "";
                    button.HorizontalAlignment = HorizontalAlignment.Center;
                    button.VerticalAlignment = VerticalAlignment.Top;
                    //button.HorizontalContentAlignment = HorizontalAlignment.Right;
                    //button.VerticalContentAlignment = VerticalAlignment.Top;
                    button.Width = 130;
                    button.Height = 86;
                    //int left = 70 + 112 * i;
                    //Thickness tk = new Thickness(left, 462, 0, 0);
                    //button.Margin = tk;
                    //自定义按钮样式 ButtonStyle
                    button.Style = Resources["ButtonStyle"] as Style;

                    //设置背景缩略图图片
                    Uri uri = new Uri(ThumbnailPath + "\\" + videoImage[i], UriKind.Relative);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad; //增加这一行
                    bi.UriSource = uri;
                    bi.EndInit();

                    BitmapImage image = new BitmapImage();
                    image = bi;
                    button.Background = new ImageBrush(image);

                    //动态加载按钮的单击事件
                    button.Click += new RoutedEventHandler(Button_Click);
                    //动态加载按钮的双击事件
                    button.MouseDoubleClick += new MouseButtonEventHandler(MouseLeftButton_DoubleClick);
                    //把生成的按钮加载到按钮数组
                    videoImageDisplay[i] = button;

                    //MainGrid.Children.Add(grid);
                    //在Grid里加载Button
                    grid.Children.Add(button);
                    //动态生成缩略图的名称框
                    TextBlock tb = new TextBlock();
                    tb.Text = button.Name.Substring(4, button.Name.Length - 4) + ".MP4";
                    tb.FontSize = 11;
                    tb.Width = 130;
                    tb.Height = 36;
                    tb.TextWrapping = TextWrapping.Wrap;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Bottom;
                    //tb.HorizontalContentAlignment = HorizontalAlignment.Center;
                    //tb.VerticalContentAlignment = VerticalAlignment.Center;
                    tb.Background = null;
                    //tb.BorderBrush = null;
                    //在Grid中加载名称框
                    grid.Children.Add(tb);
                    //把生成的Grid加载到视频Grid数组
                    videoGrid[i] = grid;
                }
            }
            else if (type == "WAV")
            {
                for (int i = 0; i < soundLength; i++)
                {
                    Grid grid = new Grid();
                    grid.Name = "SoundGrid" + sounds[i].Substring(0, sounds[i].Length - 4);
                    MainGrid.RegisterName(grid.Name, grid);
                    grid.Width = 130;
                    grid.Height = 120;
                    grid.HorizontalAlignment = HorizontalAlignment.Left;
                    grid.VerticalAlignment = VerticalAlignment.Top;
                    int left = 73 + 129 * i;
                    int top = 683;
                    Thickness tk = new Thickness(left, top, 0, 0);

                    grid.Margin = tk;

                    MyButton button = new MyButton();
                    button.IsDefalut = false;
                    button.IsSaved = false;
                    button.Name = "Soun" + sounds[i].Substring(0, sounds[i].Length - 4);
                    //grid.RegisterName(fi[i].Name.Substring(0, fi[i].Name.Length - 4), button);
                    button.Content = "";
                    button.HorizontalAlignment = HorizontalAlignment.Center;
                    button.VerticalAlignment = VerticalAlignment.Top;
                    //button.HorizontalContentAlignment = HorizontalAlignment.Right;
                    //button.VerticalContentAlignment = VerticalAlignment.Top;
                    button.Width = 130;
                    button.Height = 86;
                    //int left = 70 + 112 * i;
                    //Thickness tk = new Thickness(left, 462, 0, 0);
                    //button.Margin = tk;
                    button.Style = Resources["ButtonStyle"] as Style;

                    Uri uri = new Uri(@".\\Resources\\radio.png", UriKind.Relative);
                    BitmapImage image = new BitmapImage(uri);
                    button.Background = new ImageBrush(image);
                    button.Click += new RoutedEventHandler(Button_Click);
                    button.MouseDoubleClick += new MouseButtonEventHandler(MouseLeftButton_DoubleClick);
                    soundImageDisplay[i] = button;

                    //MainGrid.Children.Add(grid);
                    grid.Children.Add(button);

                    TextBlock tb = new TextBlock();
                    tb.Text = button.Name.Substring(4, button.Name.Length - 4) + ".WAV";
                    tb.FontSize = 11;
                    tb.Width = 130;
                    tb.Height = 36;
                    tb.TextWrapping = TextWrapping.Wrap;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Bottom;
                    //tb.HorizontalContentAlignment = HorizontalAlignment.Center;
                    //tb.VerticalContentAlignment = VerticalAlignment.Center;
                    tb.Background = null;
                    //tb.BorderBrush = null;
                    grid.Children.Add(tb);

                    soundGrid[i] = grid;
                }
            }
            else if (type == "JPG")
            {
                for (int i = 0; i < pictureLength; i++)
                {
                    Grid grid = new Grid();
                    grid.Name = "PictuGrid" + pictures[i].Substring(0, pictures[i].Length - 4);
                    MainGrid.RegisterName(grid.Name, grid);
                    grid.Width = 130;
                    grid.Height = 120;
                    grid.HorizontalAlignment = HorizontalAlignment.Left;
                    grid.VerticalAlignment = VerticalAlignment.Top;
                    int left = 73 + 129 * i;
                    int top = 683;
                    Thickness tk = new Thickness(left, top, 0, 0);
                    grid.Margin = tk;

                    MyButton button = new MyButton();
                    button.IsDefalut = false;
                    button.IsSaved = false;
                    button.Name = "Pict" + pictures[i].Substring(0, pictures[i].Length - 4);
                    //grid.RegisterName(fi[i].Name.Substring(0, fi[i].Name.Length - 4), button);
                    button.Content = "";
                    button.HorizontalAlignment = HorizontalAlignment.Center;
                    button.VerticalAlignment = VerticalAlignment.Top;
                    //button.HorizontalContentAlignment = HorizontalAlignment.Right;
                    //button.VerticalContentAlignment = VerticalAlignment.Top;
                    button.Width = 130;
                    button.Height = 86;
                    //int left = 70 + 112 * i;
                    //Thickness tk = new Thickness(left, 462, 0, 0);
                    //button.Margin = tk;
                    button.Style = Resources["ButtonStyle"] as Style;

                    //做不占用资源处理，解决被占用无法删除的问题
                    Uri uri = new Uri(nameDic[pictures[i].Substring(0, pictures[i].Length - 4)] + "\\" + pictures[i], UriKind.Relative);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad; //增加这一行
                    bi.UriSource = uri;
                    bi.EndInit();
                    BitmapImage image = new BitmapImage();
                    image = bi;
                    button.Background = new ImageBrush(image);
                    button.Click += new RoutedEventHandler(Button_Click);
                    button.MouseDoubleClick += new MouseButtonEventHandler(MouseLeftButton_DoubleClick);
                    pictureImageDisplay[i] = button;

                    //MainGrid.Children.Add(grid);
                    grid.Children.Add(button);

                    TextBlock tb = new TextBlock();
                    tb.Text = button.Name.Substring(4, button.Name.Length - 4) + ".JPG";
                    tb.FontSize = 11;
                    tb.Width = 130;
                    tb.Height = 36;
                    tb.TextWrapping = TextWrapping.Wrap;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Bottom;
                    //tb.HorizontalContentAlignment = HorizontalAlignment.Center;
                    //tb.VerticalContentAlignment = VerticalAlignment.Center;
                    tb.Background = null;
                    //tb.BorderBrush = null;
                    grid.Children.Add(tb);

                    pictureGrid[i] = grid;
                }
            }
        }

        private void InsertGrid(string type)
        {
            //生成Grid控件
            CreateGrid(type);
            //在主窗体中遍历加载生成的Grid控件
            if (type == "MP4")
            {
                for (int i = 0; i < maxLength; i++)
                {
                    MainGrid.Children.Add(videoGrid[i]);
                }
                //设置超出部分的隐藏
                if (maxLength > 10)
                {
                    for (int i = 10; i < maxLength; i++)
                    {
                        videoGrid[i].Visibility = Visibility.Hidden;
                    }
                }
            }
            else if (type == "WAV")
            {
                for (int i = 0; i < soundLength; i++)
                {
                    MainGrid.Children.Add(soundGrid[i]);
                }
                //设置超出部分的隐藏
                if (soundLength > 10)
                {
                    for (int i = 10; i < soundLength; i++)
                    {
                        soundGrid[i].Visibility = Visibility.Hidden;
                    }
                }
            }
            else if (type == "JPG")
            {
                for (int i = 0; i < pictureLength; i++)
                {
                    MainGrid.Children.Add(pictureGrid[i]);
                }
                //设置超出部分的隐藏
                if (pictureLength > 10)
                {
                    for (int i = 10; i < pictureLength; i++)
                    {
                        pictureGrid[i].Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        //向左移动显示的项目
        private void ButtonLeftMove_Click(object sender, RoutedEventArgs e)
        {
            //当前媒体种类为视频
            if (searchByMediaType == "视频")
            {
                //位移位数与显示数目不能大于总个数
                if (plusOrMinus + 10 >= maxLength)
                {
                    //MessageBox.Show("到头了", MainControl.MainConst.MESSAGE_BOX_TITLE);
                }
                else
                {
                    //位移加1
                    plusOrMinus = plusOrMinus + 1;
                    //整体向左移动129的倍数
                    for (int i = 0; i < videoGrid.Length; i++)
                    {
                        int left = 73 + 129 * (i - plusOrMinus);
                        int top = 683;
                        Thickness tk = new Thickness(left, top, 0, 0);
                        videoGrid[i].Margin = tk;
                    }
                    //设置前一个视频不可见
                    videoGrid[plusOrMinus - 1].Visibility = Visibility.Hidden;
                    //设置后一个视频可见
                    videoGrid[9 + plusOrMinus].Visibility = Visibility.Visible;
                }
            }
            //当前媒体种类为音频
            else if (searchByMediaType == "音频")
            {
                if (plusOrMinus + 10 >= soundLength)
                {
                    //MessageBox.Show("到头了", MainControl.MainConst.MESSAGE_BOX_TITLE);
                }
                else
                {
                    plusOrMinus = plusOrMinus + 1;
                    for (int i = 0; i < soundGrid.Length; i++)
                    {
                        int left = 73 + 129 * (i - plusOrMinus);
                        int top = 683;
                        Thickness tk = new Thickness(left, top, 0, 0);
                        soundGrid[i].Margin = tk;
                    }
                    soundGrid[plusOrMinus - 1].Visibility = Visibility.Hidden;
                    soundGrid[9 + plusOrMinus].Visibility = Visibility.Visible;
                }
            }
            //当前媒体种类为图片
            else if (searchByMediaType == "图片")
            {
                if (plusOrMinus + 10 >= pictureLength)
                {
                    //MessageBox.Show("到头了", MainControl.MainConst.MESSAGE_BOX_TITLE);
                }
                else
                {
                    plusOrMinus = plusOrMinus + 1;
                    for (int i = 0; i < pictureGrid.Length; i++)
                    {
                        int left = 73 + 129 * (i - plusOrMinus);
                        int top = 683;
                        Thickness tk = new Thickness(left, top, 0, 0);
                        pictureGrid[i].Margin = tk;
                    }
                    pictureGrid[plusOrMinus - 1].Visibility = Visibility.Hidden;
                    pictureGrid[9 + plusOrMinus].Visibility = Visibility.Visible;
                }
            }
        }
        //向右移动显示的项目
        private void ButtonRightMove_Click(object sender, RoutedEventArgs e)
        {
            if (searchByMediaType == "视频")
            {
                if (plusOrMinus <= 0)
                {
                    //MessageBox.Show("到头了", MainControl.MainConst.MESSAGE_BOX_TITLE);
                }
                else
                {
                    plusOrMinus = plusOrMinus - 1;
                    for (int i = 0; i < videoGrid.Length; i++)
                    {
                        int left = 73 + 129 * (i - plusOrMinus);
                        int top = 683;
                        Thickness tk = new Thickness(left, top, 0, 0);
                        videoGrid[i].Margin = tk;
                    }

                    videoGrid[10 + plusOrMinus].Visibility = Visibility.Hidden;
                    videoGrid[plusOrMinus].Visibility = Visibility.Visible;

                    //InsertImage();
                }
            }
            else if (searchByMediaType == "音频")
            {
                if (plusOrMinus <= 0)
                {
                    //MessageBox.Show("到头了", MainControl.MainConst.MESSAGE_BOX_TITLE);
                }
                else
                {
                    plusOrMinus = plusOrMinus - 1;
                    for (int i = 0; i < soundGrid.Length; i++)
                    {
                        int left = 73 + 129 * (i - plusOrMinus);
                        int top = 683;
                        Thickness tk = new Thickness(left, top, 0, 0);
                        soundGrid[i].Margin = tk;
                    }

                    soundGrid[10 + plusOrMinus].Visibility = Visibility.Hidden;
                    soundGrid[plusOrMinus].Visibility = Visibility.Visible;

                    //InsertImage();
                }
            }
            else if (searchByMediaType == "图片")
            {
                if (plusOrMinus <= 0)
                {
                    //MessageBox.Show("到头了", MainControl.MainConst.MESSAGE_BOX_TITLE);
                }
                else
                {
                    plusOrMinus = plusOrMinus - 1;
                    for (int i = 0; i < pictureGrid.Length; i++)
                    {
                        int left = 73 + 129 * (i - plusOrMinus);
                        int top = 683;
                        Thickness tk = new Thickness(left, top, 0, 0);
                        pictureGrid[i].Margin = tk;
                    }

                    pictureGrid[10 + plusOrMinus].Visibility = Visibility.Hidden;
                    pictureGrid[plusOrMinus].Visibility = Visibility.Visible;

                    //InsertImage();
                }
            }
        }

        //动态加载的按钮的点击事件
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //类型转换
            MyButton myButton = (MyButton)sender;
            //得到点击的按钮的父控件Grid
            Grid myGrid = (Grid)myButton.Parent;
            //在Grid中查找是否存在状态按钮图片
            Button bt = myGrid.FindName("Image" + myButton.Name.Substring(4, myButton.Name.Length - 4)) as Button;
            //点击的按钮状态为未保存状态
            if (myButton.IsSaved == false)
            {
                //不存在状态按钮（缩略图按钮当前状态为未选中状态）
                if (bt == null)
                {
                    //动态生成状态按钮
                    Button buttonImage = new Button();
                    buttonImage.Name = "Image" + myButton.Name.Substring(4, myButton.Name.Length - 4);
                    buttonImage.Width = 10;
                    buttonImage.Height = 10;
                    buttonImage.HorizontalAlignment = HorizontalAlignment.Right;
                    buttonImage.VerticalAlignment = VerticalAlignment.Top;
                    Thickness tk = new Thickness(0, 5, 5, 0);
                    buttonImage.Margin = tk;
                    Uri uriImage = new Uri(@".\\Resources\\u199.png", UriKind.Relative);
                    BitmapImage newImage = new BitmapImage(uriImage);
                    buttonImage.Background = new ImageBrush(newImage);

                    myGrid.Children.Add(buttonImage);
                    myGrid.RegisterName("Image" + myButton.Name.Substring(4, myButton.Name.Length - 4), buttonImage);
                    //设置缩略图按钮状态为选中状态
                    myButton.IsDefalut = true;
                }
                //存在状态按钮（缩略图按钮当前状态为选中状态）
                else
                {
                    //移除已生成的状态按钮
                    myGrid.Children.Remove(bt);
                    myGrid.UnregisterName("Image" + myButton.Name.Substring(4, myButton.Name.Length - 4));
                    //设置缩略图按钮为未选中状态
                    myButton.IsDefalut = false;
                    switch (currentType)
                    {
                        case "MP4":
                            videoSelectAllFlag = false;
                            selectAll.IsChecked = videoSelectAllFlag;
                            break;
                        case "WAV":
                            audioSelectAllFlag = false;
                            selectAll.IsChecked = audioSelectAllFlag;
                            break;
                        case "JPG":
                            imageSelectAllFlag = false;
                            selectAll.IsChecked = imageSelectAllFlag;
                            break;
                    }
                }
            }
        }
        //动态加载的缩略图按钮的双击事件
        private void MouseLeftButton_DoubleClick(object sender, RoutedEventArgs e)
        {
            //类型转换
            Button button = (Button)sender;
            //当前媒体类型为视频或音频
            if (searchByMediaType == "视频" || searchByMediaType == "音频")
            {
                //选择左侧播放器播放
                //if (MediaElementLeft.Source == null)
                //{
                //    //设置左侧播放器的播放路径
                //    if (searchByMediaType == "视频")
                //    {
                //        MediaElementLeft.Source = new Uri(discCode + "\\" + button.Name.Substring(4, button.Name.Length - 4) + ".MP4", UriKind.Relative);
                //    }
                //    else
                //    {
                //        MediaElementLeft.Source = new Uri(discCode + "\\" + button.Name.Substring(4, button.Name.Length - 4) + ".WAV", UriKind.Relative);
                //    }

                //    //设置播放或暂停按钮的Flg状态为播放
                //    playOrPauseFlgLeft = 1;
                //    //设置点击屏幕操作的Flg为false
                //    isPlayingLeft = false;
                //    //播放视频
                //    if (playOrPauseFlgLeft == 1)
                //    {
                //        try
                //        {
                //            if (MediaElementLeft.Source != null)
                //            {
                //                MediaElementLeft.Play();
                //                isPlayingLeft = true;
                //                playOrPauseFlgLeft = 2;
                //                Uri uri = new Uri(@".\\Resources\\Pau-01_S2.png", UriKind.Relative);
                //                BitmapImage image = new BitmapImage(uri);
                //                ButtonPlayOrPauseLeft.Background = new ImageBrush(image);
                //            }
                //        }
                //        catch
                //        { }
                //    }
                //    else
                //    {
                //        try
                //        {
                //            if (MediaElementLeft.Source != null)
                //            {
                //                isPlayingLeft = false;
                //                MediaElementLeft.Pause();
                //                playOrPauseFlgLeft = 1;
                //                Uri uri = new Uri(@".\\Resources\\Ply-01_S2.png", UriKind.Relative);
                //                BitmapImage image = new BitmapImage(uri);
                //                ButtonPlayOrPauseLeft.Background = new ImageBrush(image);
                //            }
                //        }
                //        catch { }
                //    }
                //}
                //设置右侧播放器播放
                //else
                //{
                    if (searchByMediaType == "视频")
                    {
                        MediaElementRight.Source = new Uri(nameDic[button.Name.Substring(4, button.Name.Length - 4)] + "\\" + button.Name.Substring(4, button.Name.Length - 4) + ".MP4", UriKind.Relative);
                    }
                    else
                    {
                        MediaElementRight.Source = new Uri(nameDic[button.Name.Substring(4, button.Name.Length - 4)] + "\\" + button.Name.Substring(4, button.Name.Length - 4) + ".WAV", UriKind.Relative);
                    }
                    //MediaElementRight.Source = new Uri(discCode + "\\" + button.Name.Substring(4, button.Name.Length - 4) + ".MP4", UriKind.Relative);
                    playOrPauseFlgRight = 1;
                    isPlayingRight = false;
                    if (playOrPauseFlgRight == 1)
                    {
                        try
                        {
                            if (MediaElementRight.Source != null)
                            {
                                MediaElementRight.Play();
                                isPlayingRight = true;
                                playOrPauseFlgRight = 2;
                                Uri uri = new Uri(@".\\Resources\\Pau-01_S2.png", UriKind.Relative);
                                BitmapImage image = new BitmapImage(uri);
                                ButtonPlayOrPauseRight.Background = new ImageBrush(image);
                            }
                        }
                        catch
                        { }
                    }
                    else
                    {
                        try
                        {
                            if (MediaElementRight.Source != null)
                            {
                                isPlayingRight = false;
                                MediaElementRight.Pause();
                                playOrPauseFlgRight = 1;
                                Uri uri = new Uri(@".\\Resources\\Ply-01_S2.png", UriKind.Relative);
                                BitmapImage image = new BitmapImage(uri);
                                ButtonPlayOrPauseRight.Background = new ImageBrush(image);
                            }
                        }
                        catch { }
                    }
                //}
            }
            //设置图片播放
            if (searchByMediaType == "图片")
            {
                    Uri uri = new Uri(nameDic[button.Name.Substring(4, button.Name.Length - 4)] + "\\" + button.Name.Substring(4, button.Name.Length - 4) + ".JPG", UriKind.Relative);
                    BitmapImage image = new BitmapImage(uri);
                    ButtonPictureShowRight.Background = new ImageBrush(image);

            }
        }


        //变更选择的按钮的背景颜色
        private void ChangeBackColor(string groupType)
        {
            //获取当前获得焦点的按钮
            Button button = (Button)Keyboard.FocusedElement;
            switch (groupType)
            {
                //当选中的按钮为媒体类型时
                case "mediaType":
                    foreach (Button m in searchMediaType)
                    {
                        if (m.Name == button.Name)
                        {
                            //设置当前选中的按钮的背景色
                            m.Background = new SolidColorBrush(Color.FromArgb(255, 0, 102, 255));
                            //保存当前选中的按钮的名称
                            searchByMediaType = m.Content.ToString();
                        }
                        else
                        {
                            m.Background = null;
                        }
                    }
                    break;
                //当选中的按钮为数据源时
                case "dataSourceType":
                    foreach (Button m in searchDateSource)
                    {
                        if (m.Name == button.Name)
                        {
                            m.Background = new SolidColorBrush(Color.FromArgb(255, 0, 102, 255));
                            searchByDateSource = m.Content.ToString();
                        }
                        else
                        {
                            m.Background = null;
                        }
                    }
                    break;
            }
        }

        //媒体类型按钮切换时，清空前一个Grid数组资源
        private void ClearShow()
        {
            //当前一个选择项为视频时
            if (searchByMediaType == "视频")
            {
                //遍历视频Grid数组
                for (int i = 0; i < maxLength; i++)
                {
                    //从MainGrid中找出注册的Grid
                    Grid grid = MainGrid.FindName(videoGrid[i].Name) as Grid;
                    if (grid != null)
                    {
                        //从Grid中找出注册的状态按钮
                        Button button = grid.FindName("Image" + grid.Name.Substring(9, grid.Name.Length - 9)) as Button;
                        if (button != null)
                        {
                            //从Grid中取消注册状态按钮
                            grid.UnregisterName("Image" + grid.Name.Substring(9, grid.Name.Length - 9));
                        }
                        //从MainGrid中删除Grid
                        MainGrid.Children.Remove(grid);
                        //从MainGrid中取消注册Grid
                        MainGrid.UnregisterName(videoGrid[i].Name);
                    }
                }
            }
            if (searchByMediaType == "音频")
            {
                for (int i = 0; i < soundLength; i++)
                {
                    Grid grid = MainGrid.FindName(soundGrid[i].Name) as Grid;
                    if (grid != null)
                    {
                        Button button = grid.FindName("Image" + grid.Name.Substring(9, grid.Name.Length - 9)) as Button;
                        if (button != null)
                        {
                            grid.UnregisterName("Image" + grid.Name.Substring(9, grid.Name.Length - 9));
                        }
                        MainGrid.Children.Remove(grid);
                        MainGrid.UnregisterName(soundGrid[i].Name);
                    }
                }
            }
            if (searchByMediaType == "图片")
            {
                for (int i = 0; i < pictureLength; i++)
                {
                    Grid grid = MainGrid.FindName(pictureGrid[i].Name) as Grid;
                    if (grid != null)
                    {
                        
                        Button button = grid.FindName("Image" + grid.Name.Substring(9, grid.Name.Length - 9)) as Button;
                        if (button != null)
                        {
                            grid.UnregisterName("Image" + grid.Name.Substring(9, grid.Name.Length - 9));
                        }
                        MainGrid.Children.Remove(grid);
                        MainGrid.UnregisterName(pictureGrid[i].Name);
                    }
                }
            }
        }
        //切换媒体类型时选择加载视频播放器或图片播放器
        private void VideoOrPictureShow()
        {
            if (searchByMediaType == "视频" || searchByMediaType == "音频")
            {
                this.CanvasVideoShowRight.Visibility = Visibility.Visible;
                this.CanvasImageShow.Visibility = Visibility.Hidden;
            }
            if (searchByMediaType == "图片")
            {
                this.CanvasVideoShowRight.Visibility = Visibility.Hidden;
                this.CanvasImageShow.Visibility = Visibility.Visible;
            }
        }

        //切换媒体类型时清空视频播放器或图片播放器的数据源
        private void ClearShowBox()
        {
            MediaElementRight.Source = null;
            SliderProgressRight.Value = 0;
            Uri uri2 = new Uri(@".\\Resources\\Ply-01_S2.png", UriKind.Relative);
            BitmapImage image2 = new BitmapImage(uri2);
            ButtonPlayOrPauseRight.Background = new ImageBrush(image2);
            isPlayingRight = false;
            playOrPauseFlgRight = 1;
        }

        //搜索XML文件中已保存的文件
        private void SearchSavedItem(MyButton[] buttons)
        {
            //取得系统当前时间
            DateTime dt = DateTime.Now;
            String xmlDate = dt.ToString("yyyyMMdd");
            //生成XML标题
            String title = userName + "_" + xmlDate;
            //搜索根目录下所有文件
            DirectoryInfo files = new DirectoryInfo(@".\\");
            //遍历根目录下所有文件
            foreach (FileInfo item in files.GetFiles())
            {
                //如果存在已生成的XML
                if (item.Name == title + ".xml")
                {
                    //加载此XML文件
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(item.Name);
                    //查找XML目录下“Name”节点
                    XmlNodeList list = xmlDoc.GetElementsByTagName("Name");
                    //遍历“Name”节点
                    foreach (XmlNode item2 in list)
                    {
                        //查找以保存的文件
                        for (int i = 0; i < buttons.Length; i++)
                        {
                            if (buttons[i].Name.Substring(4, buttons[i].Name.Length - 4) == item2.InnerText)
                            {
                                //找到以保存文件的父控件
                                Grid myGrid = (Grid)buttons[i].Parent;
                                //在此以保存的文件缩略图按钮的父控件上生成状态按钮
                                Button buttonImage = new Button();
                                buttonImage.Name = "Image" + buttons[i].Name.Substring(4, buttons[i].Name.Length - 4);
                                buttonImage.Width = 10;
                                buttonImage.Height = 10;
                                buttonImage.HorizontalAlignment = HorizontalAlignment.Right;
                                buttonImage.VerticalAlignment = VerticalAlignment.Top;
                                Thickness tk = new Thickness(0, 5, 5, 0);
                                buttonImage.Margin = tk;
                                Uri uriImage = new Uri(@".\\Resources\\u215.png", UriKind.Relative);
                                BitmapImage newImage = new BitmapImage(uriImage);
                                buttonImage.Background = new ImageBrush(newImage);
                                myGrid.Children.Add(buttonImage);
                                //在父控件中注册状态按钮
                                myGrid.RegisterName("Image" + buttons[i].Name.Substring(4, buttons[i].Name.Length - 4), buttonImage);
                                //设置缩略图按钮的状态为以保存
                                buttons[i].IsSaved = true;
                            }
                        }
                    }
                }
            }
        }

        //左侧播放器
        //播放器加载事件
        private void MediaElementLeft_Loaded(object sender, RoutedEventArgs e)
        {
            //MediaElementLeft.Source = new Uri("E:\\Program\\Program\\Resources\\Video\\JGR1503_000000_20130321235015_0006.MP4", UriKind.Relative);
            //MediaElementLeft.Source = uriLeft;
        }
        //设置timerLeft
        private void timer_Tick(object sender, EventArgs e)
        {
            this.SliderProgressRight.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(SliderProgressRight_ValueChanged);
            this.SliderProgressRight.Value = this.MediaElementRight.Position.TotalSeconds / 10.0;
            this.SliderProgressRight.ValueChanged += new RoutedPropertyChangedEventHandler<double>(SliderProgressRight_ValueChanged);
        }

        //右侧播放器
        private void ButtonLastRight_Click(object sender, RoutedEventArgs e)
        {
            MediaElementRight.Position = MediaElementRight.Position - TimeSpan.FromSeconds(10);
        }

        private void ButtonPlayOrPauseRight_Click(object sender, RoutedEventArgs e)
        {
            if (playOrPauseFlgRight == 1)
            {
                try
                {
                    if (MediaElementRight.Source != null)
                    {
                        MediaElementRight.Play();
                        isPlayingRight = true;
                        playOrPauseFlgRight = 2;
                        Uri uri = new Uri(@".\\Resources\\Pau-01_S2.png", UriKind.Relative);
                        BitmapImage image = new BitmapImage(uri);
                        ButtonPlayOrPauseRight.Background = new ImageBrush(image);
                    }
                }
                catch
                { }
            }
            else
            {
                try
                {
                    if (MediaElementRight.Source != null)
                    {
                        isPlayingRight = false;
                        MediaElementRight.Pause();
                        playOrPauseFlgRight = 1;
                        Uri uri = new Uri(@".\\Resources\\Ply-01_S2.png", UriKind.Relative);
                        BitmapImage image = new BitmapImage(uri);
                        ButtonPlayOrPauseRight.Background = new ImageBrush(image);
                    }
                }
                catch { }
            }
        }

        private void ButtonNextRight_Click(object sender, RoutedEventArgs e)
        {
            MediaElementRight.Position = MediaElementRight.Position + TimeSpan.FromSeconds(10);
        }

        private void SliderProgressRight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TimeSpan span = new TimeSpan(0, 0, (int)(SliderProgressRight.Value) * 10);
            MediaElementRight.Position = span;
        }

        private void MediaElementRight_MediaOpened(object sender, RoutedEventArgs e)
        {
            //视频总时长
            double seconds = MediaElementRight.NaturalDuration.TimeSpan.TotalSeconds;
            //共分成10等分，每1等分多少秒
            SliderProgressRight.Maximum = seconds / 10;
            //10分之一
            double baseSecond = seconds / SliderProgressRight.Maximum;
            this.timerRight.Interval = new TimeSpan(0, 0, 1);
            this.timerRight.Tick += new EventHandler(timer_Tick);
            this.timerRight.Start();
            Uri uri = new Uri(@".\\Resources\\Pau-01_S2.png", UriKind.Relative);
            BitmapImage image = new BitmapImage(uri);
            ButtonPlayOrPauseRight.Background = new ImageBrush(image);
        }

        private void MediaElementRight_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElementRight.Stop();
            SliderProgressRight.Value = 0;
            Uri uri = new Uri(@".\\Resources\\Ply-01_S2.png", UriKind.Relative);
            BitmapImage image = new BitmapImage(uri);
            ButtonPlayOrPauseRight.Background = new ImageBrush(image);
            isPlayingRight = false;
            playOrPauseFlgRight = 1;
        }

        private void MediaElementRight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isPlayingRight)
            {
                try
                {
                    if (MediaElementRight.Source != null)
                    {
                        MediaElementRight.Pause();
                        isPlayingRight = false;
                        playOrPauseFlgRight = 1;
                        Uri uri = new Uri(@".\\Resources\\Ply-01_S2.png", UriKind.Relative);
                        BitmapImage image = new BitmapImage(uri);
                        ButtonPlayOrPauseRight.Background = new ImageBrush(image);
                    }
                }
                catch { }
            }
            else
            {
                try
                {
                    if (MediaElementRight.Source != null)
                    {
                        MediaElementRight.Play();
                        isPlayingRight = true;
                        playOrPauseFlgRight = 2;
                        Uri uri = new Uri(@".\\Resources\\Pau-01_S2.png", UriKind.Relative);
                        BitmapImage image = new BitmapImage(uri);
                        ButtonPlayOrPauseRight.Background = new ImageBrush(image);
                    }
                }
                catch { }
            }
        }

        private void MediaElementRight_Loaded(object sender, RoutedEventArgs e)
        {
            //MediaElementRight.Source = new Uri("E:\\Program\\Program\\Resources\\Video\\JGR1503_000000_20130321235015_0006.MP4", UriKind.Relative);
            //MediaElementRight.Source = uriRight;
        }
        //退出按钮点击事件
        private void ButtonQuit_Click(object sender, RoutedEventArgs e)
        {
            this.buttonType = "ButtonQuit";
            MediaElementRight.Source = null;
            MediaElementRight.Close();
            this.Close();
            DeleteFolder(ThumbnailPath);
        }
        //点击保存分类按钮的check
        private bool SaveCheck()
        {
            if (TextBoxComments.Text.Trim() == "请输入上传文件的备注信息" || TextBoxComments.Text.Trim().Length==0)
            {
                //TextBoxComments.Text = "其他";
                return true;
            }
            return false;
        }
        //保存分类按钮点击事件
        private void ButtonSaveClass_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            String xmlDate = dt.ToString("yyyyMMdd");
            String title = userName + "_" + xmlDate;
            this.xmlFileName = title + ".xml";
            if (searchByMediaType == "视频")
            {
                CreateXML("Video", title, videoImageDisplay);
            }
            if (searchByMediaType == "音频")
            {
                CreateXML("Sound", title, soundImageDisplay);
            }
            if (searchByMediaType == "图片")
            {
                CreateXML("Picture", title, pictureImageDisplay);
            }
            
        }

        //上传按钮点击事件
        private void ButtonUploadClass_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(@".\\" + this.xmlFileName))
            {
                MessageBox.Show("请先分类再上传！", MainControl.MainConst.MESSAGE_BOX_TITLE);
            }
            else
            {
                this.buttonType = "ButtonUpload";
                CreateOtherXml(this.xmlFileName);
                MediaElementRight.Source = null;
                MediaElementRight.Close();
                this.Close();
                DeleteFolder(ThumbnailPath);
            }
        }
        //为没选择分类的文件创建xml
        private void CreateOtherXml(String xmlFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFileName);
            XmlNode root = xmlDoc.SelectSingleNode("Root");//查找<Root>
            XmlNodeList list = xmlDoc.GetElementsByTagName("Name");
            foreach (XmlNode item2 in list)
            {
                if (nameDicALL.ContainsKey(item2.InnerText))
                {
                    nameDicALL.Remove(item2.InnerText);
                }
            }

            Dictionary<String, String>.KeyCollection fileNamekeys = nameDicALL.Keys;
            foreach (string name in fileNamekeys)
            {
                index++;
                string value = nameDicALL[name].ToString();
                //LogConfig.info(value+"生成成功。");
                FileInfo mtFile = new FileInfo(value);
                String type = "";
                if ((mtFile.Extension.Substring(1, mtFile.Extension.Length - 1)) == "MP4")
                {
                    type = "Video";
                }
                if ((mtFile.Extension.Substring(1, mtFile.Extension.Length - 1)) == "WAV")
                {
                    type = "Sound";
                }
                if ((mtFile.Extension.Substring(1, mtFile.Extension.Length - 1)) == "JPG")
                {
                    type = "Picture";
                }

                XmlElement xe1 = xmlDoc.CreateElement(type);//创建一个<book>节点
                //DateTime dt = DateTime.Now;
                //String uploadTime = dt.ToString("yyyyMMddHHmmss");
                String shootTime = name.Split('_')[2];
                String policeNo = xmlFileName.Split('_')[0];
                xe1.SetAttribute("ChangedName", companyName + "_" + policeNo + "_" + policeNo + "_" + shootTime + "_" + timeDic[name].Replace(":", "-") + "_" + index);//设置该节点genre属性

                XmlElement xesub1 = xmlDoc.CreateElement("Name");
                xesub1.InnerText = name;
                xe1.AppendChild(xesub1);
                XmlElement xesub2 = xmlDoc.CreateElement("Class");
                xe1.AppendChild(xesub2);
                XmlElement xesub3 = xmlDoc.CreateElement("Comments");
                xe1.AppendChild(xesub3);
                XmlElement xesub4 = xmlDoc.CreateElement("SavedFlag");
                xesub4.InnerText = "0";
                xe1.AppendChild(xesub4);

                root.AppendChild(xe1);//添加到<Root>节点中
                xmlDoc.Save(xmlFileName);
            }
        }

        //删除文件夹

        private void DeleteDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                DirectoryInfo[] childs = dir.GetDirectories();
                foreach (DirectoryInfo child in childs)
                {
                    child.Delete(true);
                }
                dir.Delete(true);
            }
        }

        /// <summary>
        /// 清空指定的文件夹，但不删除文件夹
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteFolder(string dir)
        {
            DirectoryInfo fold = new DirectoryInfo(dir);
            if (!fold.Exists)
                return;

            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                   {
                        FileInfo fi = new FileInfo(d);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                           fi.Attributes = FileAttributes.Normal;
                           File.Delete(d);//直接删除其中的文件 
                   }
                   else
                   {
                        DirectoryInfo d1 = new DirectoryInfo(d);
                        if (d1.GetFiles().Length != 0)
                        {
                           DeleteFolder(d1.FullName);////递归删除子文件夹
                        }
                        Directory.Delete(d);
                   }
           }
        }

        //生成XML方法
        private void CreateXML(string type,string title,MyButton[] buttons)
        {
            int itemNum = 0;
            foreach (MyButton item in buttons)
            {
                if (item.IsDefalut == true && item.IsSaved == false)
                {
                    if (SaveCheck())
                    {
                        MessageBox.Show("请输入上传文件的备注信息", MainControl.MainConst.MESSAGE_BOX_TITLE);
                        return;
                    }
                    itemNum++;
                }
            }
            if (itemNum == 0)
            {
                MessageBox.Show("请选择要分类保存的文件！", MainControl.MainConst.MESSAGE_BOX_TITLE);
            }
            String[] savedItem = new String[itemNum];
            int num = 0;
            foreach (MyButton item in buttons)
            {
                if (item.IsDefalut == true && item.IsSaved == false)
                {
                    item.IsSaved = true;
                    savedItem[num] = item.Name.Substring(4, item.Name.Length - 4);
                    num++;
                }
            }

            DirectoryInfo files = new DirectoryInfo(@".\\");
            foreach (FileInfo item in files.GetFiles())
            {
                if (item.Name == title + ".xml")
                {
                    foreach (string item2 in savedItem)
                    {
                        index++;
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(item.Name);
                        XmlNode root = xmlDoc.SelectSingleNode("Root");//查找<bookstore>
                        XmlElement xe1 = xmlDoc.CreateElement(type);//创建一个<book>节点
                        //DateTime dt = DateTime.Now;
                        //String uploadTime = dt.ToString("yyyyMMddHHmmss");
                        String shootTime = item2.Split('_')[2];
                        String policeNo = title.Split('_')[0];
                        xe1.SetAttribute("ChangedName", companyName + "_" + policeNo + "_" + policeNo + "_" + shootTime + "_" + timeDic[item2].Replace(":", "-") + "_" + index);//设置该节点genre属性

                        XmlElement xesub1 = xmlDoc.CreateElement("Name");
                        xesub1.InnerText = item2;
                        xe1.AppendChild(xesub1);//添加到<book>节点中
                        XmlElement xesub2 = xmlDoc.CreateElement("Class");
                        xesub2.InnerText = ComboBoxClass.Text;
                        xe1.AppendChild(xesub2);
                        XmlElement xesub3 = xmlDoc.CreateElement("Comments");
                        xesub3.InnerText = TextBoxComments.Text.ToString();
                        xe1.AppendChild(xesub3);
                        XmlElement xesub4 = xmlDoc.CreateElement("SavedFlag");
                        xesub4.InnerText = "0";
                        xe1.AppendChild(xesub4);

                        root.AppendChild(xe1);//添加到<bookstore>节点中
                        xmlDoc.Save(item.Name);
                    }
                    foreach (MyButton item3 in buttons)
                    {
                        if (item3.IsSaved == true)
                        {
                            Grid myGrid = (Grid)item3.Parent;
                            Button bt = myGrid.FindName("Image" + item3.Name.Substring(4, item3.Name.Length - 4)) as Button;
                            Uri uriImage = new Uri(@".\\Resources\\u215.png", UriKind.Relative);
                            BitmapImage newImage = new BitmapImage(uriImage);
                            bt.Background = new ImageBrush(newImage);
                        }
                    }
                    return;
                }
            }

            XmlTextWriter writer = new XmlTextWriter(title + ".xml", System.Text.Encoding.UTF8);
            //使用自动缩进便于阅读
            writer.Formatting = Formatting.Indented;
            //XML声明 
            writer.WriteStartDocument();
            //书写根元素 
            writer.WriteStartElement("Root");
            writer.WriteAttributeString("Org", org);
            foreach (string item in savedItem)
            {
                index++;
                //开始一个元素 
                writer.WriteStartElement(type);
                //向先前创建的元素中添加一个属性
                //DateTime dt = DateTime.Now;
                //String uploadTime = dt.ToString("yyyyMMddHHmmss");
                //dt.GetDateTimeFormats();
                String shootTime = item.Split('_')[2];
                String policeNo = title.Split('_')[0];
                writer.WriteAttributeString("ChangedName", companyName + "_" + policeNo + "_" + policeNo + "_" + shootTime + "_" + timeDic[item].Replace(":", "-") + "_" + index);
                //添加子元素
                writer.WriteElementString("Name", item);
                writer.WriteElementString("Class", ComboBoxClass.Text);
                writer.WriteElementString("Comments", TextBoxComments.Text.ToString());
                writer.WriteElementString("SavedFlag", "0");
                //关闭item元素
                writer.WriteEndElement();
            }
            writer.Close();

            foreach (MyButton item in buttons)
            {
                if (item.IsSaved == true)
                {
                    Grid myGrid = (Grid)item.Parent;
                    Button bt = myGrid.FindName("Image" + item.Name.Substring(4, item.Name.Length - 4)) as Button;
                    Uri uriImage = new Uri(@".\\Resources\\u215.png", UriKind.Relative);
                    BitmapImage newImage = new BitmapImage(uriImage);
                    bt.Background = new ImageBrush(newImage);
                }
            }
        }

        //备注信息框获取焦点事件
        private void TextBoxComments_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxComments.Text.Trim() == "请输入上传文件的备注信息")
            {
                TextBoxComments.Text = "";
            }
            Process.Start(@"C:\WINDOWS\system32\osk.exe");
            String[] ilStrs = new String[] { "QQ", "搜狗", "微软" };
            System.Windows.Forms.InputLanguage il= null;
            foreach (String ilStr in ilStrs)
            {
                il = GetDesiredInputLanguage(ilStr);

                if (il != null) 
                {
                    break;
                }
            }
            if (il == null)
            {
                System.Windows.Forms.InputLanguage.CurrentInputLanguage = System.Windows.Forms.InputLanguage.DefaultInputLanguage;
                //MessageBox.Show("请安装中文输入法！");
                
            }
            TextBoxComments.Focus();
            System.Windows.Forms.InputLanguage.CurrentInputLanguage = il;
        }
        //备注信息框失去焦点事件
        private void TextBoxComments_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxComments.Text.Trim() == "")
            {
                TextBoxComments.Text = "请输入上传文件的备注信息";
            }
            Process[] aa = Process.GetProcessesByName("osk");
            //在进程列表中查找指定的QQ进程
            foreach (Process p in aa)
            {
                //执行kill命令
                p.Kill();
            }
           
        }

        public static System.Windows.Forms.InputLanguage GetDesiredInputLanguage(string layoutName)
        {
            System.Windows.Forms.InputLanguageCollection ilc = System.Windows.Forms.InputLanguage.InstalledInputLanguages;

            foreach (System.Windows.Forms.InputLanguage il in ilc)
            {
                if (il.LayoutName.IndexOf(layoutName) != -1)
                    return il;
            }
            return null;
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
           
        }






        private void btnT_OnClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            String btnName = btn.Name;

            switch (btnName)
            {
                case "btnT02": //"btnT01 : remote Date source in server

                    break;
                default:       //"btnT01 : local Date source

                    break;
            }
        }

        private void btnP_OnClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            String btnName = btn.Name;

            if (btnName.Equals("btnP04")){
                return;
            } 

            this.btnP01.Foreground = new SolidColorBrush(colorNoFocus);
            this.btnP02.Foreground = new SolidColorBrush(colorNoFocus);
            this.btnP03.Foreground = new SolidColorBrush(colorNoFocus);
            this.btnP04.Foreground = new SolidColorBrush(colorNoFocus);

            btn.Foreground = new SolidColorBrush(colorFocus);

            switch (btnName)
            {
                case "btnP01": //"btnT01 : vedio
                    // 记住当前类型
                    currentType = "MP4";
                    ClearShow();
                    plusOrMinus = 0;
                    //imageTimer.Start();
                    InsertGrid("MP4");
                    ChangeBackColor("mediaType");
                    VideoOrPictureShow();
                    ClearShowBox();
                    SearchSavedItem(videoImageDisplay);
                    setCheckBoxValue();
                    break;
                case "btnP02": //"btnT02 : audio
                    // 记住当前类型
                    currentType = "WAV";
                    ClearShow();
                    plusOrMinus = 0;
                    InsertGrid("WAV");
                    ChangeBackColor("mediaType");
                    VideoOrPictureShow();
                    ClearShowBox();
                    SearchSavedItem(soundImageDisplay);
                    setCheckBoxValue();
                    break;
                case "btnP03": //"btnT03 : pic
                    // 记住当前类型
                    currentType = "JPG";
                    ClearShow();
                    plusOrMinus = 0;
                    InsertGrid("JPG");
                    ChangeBackColor("mediaType");
                    VideoOrPictureShow();
                    ClearShowBox();
                    SearchSavedItem(pictureImageDisplay);
                    setCheckBoxValue();
                    break;
                default:       //"btnP04 : others
                    break;
            }
            this.btnP01.Background = null;
            this.btnP02.Background = null;
            this.btnP03.Background = null;
          
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            MediaElementRight.Source = null;
            MediaElementRight.Close();
            this.Close();
        }

        private void InitialFilterButtons()
        {
            colorNoFocus = (Color)ColorConverter.ConvertFromString("#C2C2C2");
            colorFocus = (Color)ColorConverter.ConvertFromString("#00CCFF");

            this.btnT01.Foreground = new SolidColorBrush(colorFocus);
            this.btnT02.Foreground = new SolidColorBrush(colorNoFocus);

            this.btnP01.Foreground = new SolidColorBrush(colorFocus);
            this.btnP02.Foreground = new SolidColorBrush(colorNoFocus);
            this.btnP03.Foreground = new SolidColorBrush(colorNoFocus);
            this.btnP04.Foreground = new SolidColorBrush(colorNoFocus);
        }
        //拷贝结束后删除操作
        public static void DeleteAfterCopy(String xmlFile)
        {
            if (File.Exists(xmlFile))
            {
                //如果存在则删除
                File.Delete(xmlFile);
            }
        }

        // 全部选择checkbox
        private void selectAll_check(object sender, RoutedEventArgs e)
        {
            CheckBox selectAll = sender as CheckBox;

            // 设定状态
            setSelectStatus((Boolean)selectAll.IsChecked);
        }

        // 设定状态
        private void setSelectStatus(Boolean status)
        {
            switch (currentType)
            {
                case "MP4":
                    videoSelectAllFlag = status;
                    for (int i = 0; i < videoImageDisplay.Length; i++)
                    {
                        MyButton but = videoImageDisplay[i];
                        selectAll_event(but, videoSelectAllFlag);
                    }
                    break;
                case "WAV":
                    audioSelectAllFlag = status;
                    for (int i = 0; i < soundImageDisplay.Length; i++)
                    {
                        MyButton but = soundImageDisplay[i];
                        selectAll_event(but, audioSelectAllFlag);
                    }
                    break;
                case "JPG":
                    imageSelectAllFlag = status;
                    for (int i = 0; i < pictureImageDisplay.Length; i++)
                    {
                        MyButton but = pictureImageDisplay[i];
                        selectAll_event(but, imageSelectAllFlag);
                    }
                    break;
            }
        }
        //全部选择事件
        private void selectAll_event(MyButton myButton, Boolean selectAllFlag)
        {
            if (!myButton.IsSaved)
            {
                //得到点击的按钮的父控件Grid
                Grid myGrid = (Grid)myButton.Parent;
                //在Grid中查找是否存在状态按钮图片
                Button bt = myGrid.FindName("Image" + myButton.Name.Substring(4, myButton.Name.Length - 4)) as Button;
                // 全部按钮选择时
                if (selectAllFlag)
                {
                    if (!myButton.IsDefalut)
                    {
                        //动态生成状态按钮
                        Button buttonImage = new Button();
                        buttonImage.Name = "Image" + myButton.Name.Substring(4, myButton.Name.Length - 4);
                        buttonImage.Width = 10;
                        buttonImage.Height = 10;
                        buttonImage.HorizontalAlignment = HorizontalAlignment.Right;
                        buttonImage.VerticalAlignment = VerticalAlignment.Top;
                        Thickness tk = new Thickness(0, 5, 5, 0);
                        buttonImage.Margin = tk;
                        Uri uriImage = new Uri(@".\\Resources\\u199.png", UriKind.Relative);
                        BitmapImage newImage = new BitmapImage(uriImage);
                        buttonImage.Background = new ImageBrush(newImage);

                        myGrid.Children.Add(buttonImage);
                        myGrid.RegisterName("Image" + myButton.Name.Substring(4, myButton.Name.Length - 4), buttonImage);
                        //设置缩略图按钮状态为选中状态
                        myButton.IsDefalut = true;
                    }
                }
                else
                {
                    if (bt != null)
                    {
                        string buttonName = "Image" + myButton.Name.Substring(4, myButton.Name.Length - 4);
                        //移除已生成的状态按钮
                        myGrid.Children.Remove(bt);
                        myGrid.UnregisterName(buttonName);
                        //设置缩略图按钮为未选中状态
                        myButton.IsDefalut = false;
                    }
                }
            }
        }

        // checkbox设定
        private void setCheckBoxValue() { 
            switch (currentType)
            {
                case "MP4":
                    selectAll.IsChecked = videoSelectAllFlag;
                    setSelectStatus(videoSelectAllFlag);
                    break;
                case "WAV":
                    selectAll.IsChecked = audioSelectAllFlag;
                    setSelectStatus(audioSelectAllFlag);
                    break;
                case "JPG":
                    selectAll.IsChecked = imageSelectAllFlag;
                    setSelectStatus(imageSelectAllFlag);
                    break;
            }
        }

        private void clear_button_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("此操作将会清除已保存的数据，确定重置吗？", "DAMS", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                if (!"".Equals(xmlFileName))
                {
                    //删除xml文件
                    DeleteAfterCopy(@".\\" + xmlFileName, "");
                    // 画面初始化
                    clearData();

                }
            }
        }
        //删除操作
        private void DeleteAfterCopy(String xmlFile, String usbPath)
        {
            if (File.Exists(xmlFile))
            {
                //如果存在则删除
                File.Delete(xmlFile);
            }
        }

        //画面初始化
        private void clearData()
        {
            videoSelectAllFlag = false;
            audioSelectAllFlag = false;
            imageSelectAllFlag = false;
            selectAll.IsChecked = false;
            switch (currentType)
            {
                case "MP4":
                    ClearShow();
                    plusOrMinus = 0;
                    InsertGrid("MP4");
                    ChangeBackColor("mediaType");
                    VideoOrPictureShow();
                    ClearShowBox();
                    SearchSavedItem(videoImageDisplay);
                    break;
                case "WAV":
                     ClearShow();
                    plusOrMinus = 0;
                    InsertGrid("WAV");
                    ChangeBackColor("mediaType");
                    VideoOrPictureShow();
                    ClearShowBox();
                    SearchSavedItem(soundImageDisplay);
                    break;
                case "JPG":
                    ClearShow();
                    plusOrMinus = 0;
                    InsertGrid("JPG");
                    ChangeBackColor("mediaType");
                    VideoOrPictureShow();
                    ClearShowBox();
                    SearchSavedItem(pictureImageDisplay);
                    break;
            }
        }
    }
}
