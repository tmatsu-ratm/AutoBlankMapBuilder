using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AutoBlankMapBuilder.Utils
{
    public static class WindowManager
    {
        public static bool IsOpenWindow<TWindow>() where TWindow : Window
        {
            var window = Application.Current.Windows.OfType<TWindow>().FirstOrDefault();
            return window != null;
        }
    }
}
