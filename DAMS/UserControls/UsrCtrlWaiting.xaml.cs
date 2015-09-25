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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DAMS.UserControls
{
    /// <summary>
    /// UsrCtrlWaiting.xaml 的交互逻辑
    /// </summary>
    public partial class UsrCtrlWaiting : UserControl
    {
        DispatcherTimer timer = new DispatcherTimer();
        List<SolidColorBrush> lstBrush = new List<SolidColorBrush>();
        int iTicks = 0;

        public UsrCtrlWaiting()
        {
            InitializeComponent();

            SolidColorBrush brush;
            Color color;

            for (int i = 0; i <= 11; i++)
            {
                Int32 iOffset = 154 + 9 * i;

                color = new Color();
                color.A = 255;
                color.R = Convert.ToByte(iOffset);
                color.G = Convert.ToByte(iOffset);
                color.B = Convert.ToByte(iOffset);

                brush = new SolidColorBrush();
                brush.Color = color;
                lstBrush.Add(brush);
            }

            //Start the Timer
            StartTimer();
        }

        //Start the Timer
        private void StartTimer()
        {

            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += OnTimer;
            timer.IsEnabled = true;
            timer.Start();
        }

        //Refresh the Date Time on MainWindow
        private void OnTimer(object sender, EventArgs e)
        {
            this.rec0.Fill = lstBrush[(iTicks + 11) % 12];
            this.rec1.Fill = lstBrush[(iTicks + 10) % 12];
            this.rec2.Fill = lstBrush[(iTicks + 9) % 12];
            this.rec3.Fill = lstBrush[(iTicks + 8) % 12];
            this.rec4.Fill = lstBrush[(iTicks + 7) % 12];
            this.rec5.Fill = lstBrush[(iTicks + 6) % 12];
            this.rec6.Fill = lstBrush[(iTicks + 5) % 12];
            this.rec7.Fill = lstBrush[(iTicks + 4) % 12];
            this.rec8.Fill = lstBrush[(iTicks + 3) % 12];
            this.rec9.Fill = lstBrush[(iTicks + 2) % 12];
            this.rec10.Fill = lstBrush[(iTicks + 1) % 12];
            this.rec11.Fill = lstBrush[(iTicks + 0) % 12];
            iTicks++;
            if (12 == iTicks)
            {
                iTicks = 0;
            }
        }

        public void StopWaiting()
        {
            timer.Stop();
            timer = null;
        }
    }
}
