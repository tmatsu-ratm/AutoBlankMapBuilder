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
using System.Windows.Shapes;
using Microsoft.Win32;
using  AutoBlankMapBuilder.Models;

namespace AutoBlankMapBuilder.Views
{
    /// <summary>
    /// ConfigView.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigView : Window
    {
        private Config cfg;

        public ConfigView(Config cfg)
        {
            InitializeComponent();
            this.cfg = cfg;
            InitView();
        }

        private void InitView()
        {
            TBlockAllFolder.Text = cfg.AllDataDir;
            TBlockNewFolder.Text = cfg.NewDataDir;
            TBlockMapFolder.Text = cfg.BlankMapDir;
            TBlockOrderList.Text = cfg.OrderList;
            TBlockDbUsr.Text = cfg.DbUser;
            TBlockDbPw.Text = cfg.DbPwd;
            TBlockDbSrv.Text = cfg.DbServer;
            TBlockDbName.Text = cfg.DbName;
            TBlockDbProvider.Text = "Microsoft.ACE.OLEDB.12.0";
        }

        private void Button1_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();

            dlg.Filter = "Excelファイル (*.xlsx)|*.xlsx";

            if (dlg.ShowDialog() == true)
            {
                TBlockOrderList.Text = dlg.FileName;
            }
        }

        private void Button2_OnClick(object sender, RoutedEventArgs e)
        {
            if (GetFolderName().Length > 0)
            {
                TBlockMapFolder.Text = GetFolderName();
            }
        }

        private void Button3_OnClick(object sender, RoutedEventArgs e)
        {
            if (GetFolderName().Length > 0)
            {
                TBlockAllFolder.Text = GetFolderName();
            }
        }

        private void Button4_OnClick(object sender, RoutedEventArgs e)
        {
            if (GetFolderName().Length > 0)
            {
                TBlockNewFolder.Text = GetFolderName();
            }
        }

        private string GetFolderName()
        {
            var folderName = "";

            var dlg = new System.Windows.Forms.FolderBrowserDialog();

            dlg.Description = "フォルダを選択してください";

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderName = dlg.SelectedPath;
            }

            return folderName;
        }

        private void Button5_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button6_OnClick(object sender, RoutedEventArgs e)
        {
            cfg.SaveConfigFile(this, cfg.filePath);
            this.Close();
        }
    }
}
