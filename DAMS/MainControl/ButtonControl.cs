using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DAMS.MainControl
{
    public class ButtonControl
    {
        MonitorWindow win;

        private Color colorNoFocus = new Color();
        private Color colorFocus = new Color();

        public ButtonControl(MonitorWindow win)
        {
            this.win = win;
            //colorNoFocus = (Color)ColorConverter.ConvertFromString("#C2C2C2");
        }

        public void createButton(Dictionary<string, UsbInfo> dic)
        {
            SolidColorBrush scb = new SolidColorBrush(Colors.Transparent);
            for (int i = 0; i < dic.Count; i++)
            {
                UsbInfo info = null;
                dic.TryGetValue(MainConst.USB_KEY + (i + 1), out info);
                //ControlTemplate template = new ControlTemplate();
                //template.VisualTree = new FrameworkElementFactory(typeof(Button));
                //button.Template = template;
/* COMMENT For merging
                Button button = new Button();

                button.BorderBrush = new SolidColorBrush(colorNoFocus);
                button.FocusVisualStyle = null;
                //ImageBrush b = new ImageBrush();
                //System.Drawing.Bitmap
                //Bitmap hBitmap = Properties.Resources.u235;
                //b.ImageSource = ChangeBitmapToImageSource(hBitmap);
                //b.Stretch = Stretch.Fill;
                //button.Background = b;
                button.Background = scb;
                button.Width = 50;
                button.Height = 30;
                button.Name = info.TerminalNo;
                button.Content = info.TerminalNo;
                button.Foreground = new SolidColorBrush(colorNoFocus);
                Thickness thick = new Thickness(2, 1, 0, 0);
                button.Margin = thick;

                button.Click += button_Click;
                this.win.StackPanel_TerminalNo.Children.Add(button);
 */
            }

            Button btn;
            String btnName;

            for (int i = dic.Count + 1; i <= 24; i++)
            {
                btnName = "btnT" + String.Format("{0:00}", i);
                btn = (Button)this.win.FindName(btnName);
                btn.Visibility = Visibility.Hidden;
            }

        }

        //button事件
        private void button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = sender as Button;
            //改变样式
            colorFocus = ((SolidColorBrush)button.Foreground).Color;
            button.Foreground = new SolidColorBrush(colorNoFocus);
            button.BorderBrush = new SolidColorBrush(colorNoFocus);

            Activity acitivty = new Activity(this.win);
            //acitivty.changeStyle(button);
        }


    }
}
