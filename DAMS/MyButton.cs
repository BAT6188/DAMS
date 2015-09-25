using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
//using System.Workflow.ComponentModel;

namespace DAMS
{
    class MyButton : Button
    {
        public static readonly new System.Windows.DependencyProperty IsDefaultProperty;
        public static readonly System.Windows.DependencyProperty IsSavedProperty;

        static MyButton()
        {
            // 注册属性  
            MyButton.IsDefaultProperty = System.Windows.DependencyProperty.Register("IsDefault",
                typeof(bool), typeof(MyButton),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(OnIsDefaultChanged)));

            // 注册属性  
            MyButton.IsSavedProperty = System.Windows.DependencyProperty.Register("IsSaved",
                typeof(bool), typeof(MyButton),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(OnIsSavedChanged)));
        }

        // .net属性包装器（可选）  
        public bool IsDefalut
        {
            get { return (bool)GetValue(MyButton.IsDefaultProperty); }
            set { SetValue(MyButton.IsDefaultProperty, value); }
        }

        private static void OnIsDefaultChanged(System.Windows.DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        // .net属性包装器（可选）  
        public bool IsSaved
        {
            get { return (bool)GetValue(MyButton.IsSavedProperty); }
            set { SetValue(MyButton.IsSavedProperty, value); }
        }

        private static void OnIsSavedChanged(System.Windows.DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
