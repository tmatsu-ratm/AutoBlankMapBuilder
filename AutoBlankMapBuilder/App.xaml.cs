using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;

namespace AutoBlankMapBuilder
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private Mutex mutex = new Mutex(false, "AutoBlankMapBuilder");
        public static string[] CommandLineArgs { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!mutex.WaitOne(0, false))
            {
                MessageBox.Show("AutoBlankMapBuilderは既に起動しています．", "二重起動防止", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                mutex.Close();
                mutex = null;
                this.Shutdown();
            }

            if (e.Args.Length == 0)
            {
                return;
            }

            CommandLineArgs = e.Args;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Close();
            }
        }
    }
}
