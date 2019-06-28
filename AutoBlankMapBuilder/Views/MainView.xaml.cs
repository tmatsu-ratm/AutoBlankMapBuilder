using AutoBlankMapBuilder.Models;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AutoBlankMapBuilder.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainView: Window
    {
        private Config cfg;
        private DateTime executeTime;
        private DateTime executedTime;
        private DispatcherTimer _timer;

        public MainView()
        {
            cfg = new Config(Config.CONFIG_FILE_PATH);
            InitializeComponent();

            executeTime = GetExecuteTime();
            SetupTimer();

            if (App.CommandLineArgs != null && App.CommandLineArgs[0] == "1")
            {
                Utils.Utils.WriteLog(this, "起動時ブランクMAP作成");
                Execute();
            }
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += new EventHandler(MyTimerMethod);
            _timer.Start();
            var dt = executeTime - DateTime.Now;
            this.LabelTimer.Content = "次回実行時刻: " + executeTime.ToString("MM/dd HH:mm") + " (" + ((int)(dt.TotalHours)).ToString("D2") + ":" + dt.Minutes.ToString("D2") + " 後)";
        }

        private void MyTimerMethod(object sender, EventArgs e)
        {
            var dt = executeTime - DateTime.Now;
            if (dt.TotalSeconds < 0)
            {
                this._timer.Stop();
                Utils.Utils.WriteLog(this, "定時ブランクMAP作成");
                Execute();
                this._timer.Start();
            }
            executeTime = GetExecuteTime();
            this.LabelTimer.Content = "次回実行時刻: " + executeTime.ToString("MM/dd HH:mm") + " (" + ((int)(dt.TotalHours)).ToString("D2") + ":" + dt.Minutes.ToString("D2") + " 後)";
        }

        private DateTime GetExecuteTime()
        {
            var arr = cfg.ExecuteTime.Split(':');
            var executeHour = int.Parse(arr[0]);
            var executeMinute = int.Parse(arr[1]);
            var executeDay = DateTime.Now.Day;
            var executeMonth = DateTime.Now.Month;
            var executeYear = DateTime.Now.Year;

            var executeTime = new DateTime(
                executeYear,
                executeMonth,
                executeDay,
                executeHour,
                executeMinute,
                0);

            if (DateTime.Now.Subtract(DateTime.ParseExact(cfg.ExecuteTime, "HH:mm", null)).TotalSeconds > 0)
            {
                executeTime = executeTime.AddDays(1);
            }

            return executeTime;
        }       

        private void Button1_OnClick(object sender, RoutedEventArgs e)
        {
            Utils.Utils.WriteLog(this, "設定変更画面");
            ConfigView view = new ConfigView(cfg);
            view.ShowDialog();
        }

        private void Button2_OnClick(object sender, RoutedEventArgs e)
        {            
            Utils.Utils.WriteLog(this, "手動ブランクMAP作成");
            Execute();
        }

        private void Execute()
        {
            this.Cursor = Cursors.Wait;
            var builder = new MapBuilder(cfg, this);
            builder.Process();
            this.Cursor = null;
            executedTime = DateTime.Now;
            this.LabelTimer2.Content = "前回実行時間: " + executedTime.ToString("MM/dd HH:mm:ss");            
        }

    }
}
