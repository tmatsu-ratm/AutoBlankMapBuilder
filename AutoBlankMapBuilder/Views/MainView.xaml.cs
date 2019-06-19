using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoBlankMapBuilder.Models;
using AutoBlankMapBuilder.Utils;

namespace AutoBlankMapBuilder.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainView: Window
    {
        private Config cfg;

        public MainView()
        {
            cfg = new Config(Config.CONFIG_FILE_PATH);
            InitializeComponent();
        }

        private void Button1_OnClick(object sender, RoutedEventArgs e)
        {
            Utils.Utils.WriteLog(this, "設定変更画面");
            ConfigView view = new ConfigView(cfg);
            view.ShowDialog();
            AlarmView view2 = new AlarmView();
            view2.ShowDialog();
        }

        private void Button2_OnClick(object sender, RoutedEventArgs e)
        {
            Utils.Utils.WriteLog(this, "手動ブランクMAP作成");
            var builder = new MapBuilder(cfg, this);
            builder.Process();
        }

    }
}
