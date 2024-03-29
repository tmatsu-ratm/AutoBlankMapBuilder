﻿using System;
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
            TBlockOrderList.Text = cfg.ListDir;
            TBlockLogFolder.Text = cfg.LogDir;
            TBlockDbUsr.Text = cfg.DbUser;
            TBlockDbPw.Text = new string('*', cfg.DbPwd.Length);
            TBlockDbSrv.Text = cfg.DbServer;
            TBlockDbName.Text = cfg.DbName;
            TBlockDbProvider.Text = "Microsoft.ACE.OLEDB.12.0";
            TimePicker.Value = DateTime.ParseExact( cfg.ExecuteTime, "HH:mm", null);
        }

        private void Button1_OnClick(object sender, RoutedEventArgs e)
        {
            var folderName = GetFolderName();
            if (folderName.Length > 0)
            {
                TBlockOrderList.Text = folderName;
            }
        }

        private void Button2_OnClick(object sender, RoutedEventArgs e)
        {
            var folderName = GetFolderName();
            if (folderName.Length > 0)
            {
                TBlockMapFolder.Text = folderName;
            }
        }

        private void Button3_OnClick(object sender, RoutedEventArgs e)
        {
            var folderName = GetFolderName();
            if (folderName.Length > 0)
            {
                TBlockAllFolder.Text = folderName;
            }
        }

        private void Button4_OnClick(object sender, RoutedEventArgs e)
        {
            var folderName = GetFolderName();
            if (folderName.Length > 0)
            {
                TBlockNewFolder.Text = folderName;
            }
        }

        private static string GetFolderName()
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

        private void Button7_OnClick(object sender, RoutedEventArgs e)
        {
            var folderName = GetFolderName();
            if (folderName.Length > 0)
            {
                TBlockLogFolder.Text = folderName;
            }
        }

    }
}
