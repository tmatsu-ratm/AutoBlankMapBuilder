﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using OfficeOpenXml;
using AutoBlankMapBuilder.Models;
using AutoBlankMapBuilder.Views;
using OfficeOpenXml.ConditionalFormatting;

namespace AutoBlankMapBuilder.Utils
{
    public static class Utils
    {
        public static int SearchFolder(string path, string folderName)
        {
            // 機種フォルダがない場合
            // とりあえずフォルダ作成
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
                return 0;
            }
            
            string name = "";
            
            string[] subFolders;

            try
            {
                subFolders = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
            }
            catch
            {
                return -1;
            }

            foreach (var folder in subFolders)
            {
                name = folder.Substring(folder.LastIndexOf(@"\") + 1, folder.Length - folder.LastIndexOf(@"\") - 1);
                if (name == folderName)
                {
                    return 1;
                }
            }

            return 0;
        }

        public static List<Order> GetOrderList(string fileName)
        {
            try
            {
                using (var package = new ExcelPackage())
                {

                    using (var stream = File.OpenRead(fileName))
                    {
                        package.Load(stream);
                    }

                    var list = new List<Order>();
                    var sheet = package.Workbook.Worksheets[1];

                    var i = 2;

                    while (true)
                    {
                        if (sheet.Cells[i, 1].Text == "")
                        {
                            break;
                        }
                        var order = new Order();
                        var listName = Path.GetFileName(fileName);

                        switch (listName)
                        {
                            case "T_LIST.xlsx":
                                order.Department = sheet.Cells[i, 1].Text;
                                order.No = sheet.Cells[i, 2].Text;
                                order.Item = sheet.Cells[i, 3].Text;
                                order.Date = sheet.Cells[i, 4].Text;
                                order.Quantity = int.Parse(sheet.Cells[i, 5].Text);
                                order.WaferList = GetWaferList(order.Quantity);
                                order.Mode = (int)CommonConstants.ListMode.Blank;
                                break;
                            case "T_ASIC.xlsx":
                                order.No = sheet.Cells[i, 7].Text;
                                order.Item = sheet.Cells[i, 6].Text;
                                order.Quantity = int.Parse(sheet.Cells[i, 9].Text);
                                order.WaferList = GetWaferList(sheet.Cells[i, 8].Text);
                                order.BackupPath = sheet.Cells[i, 4].Text;
                                order.Mode = (int)CommonConstants.ListMode.Asic;
                                break;
                            case "T_NEXT.xlsx":
                                order.No = sheet.Cells[i, 2].Text;
                                order.Item = sheet.Cells[i, 1].Text;
                                order.Quantity = int.Parse(sheet.Cells[i, 5].Text);
                                order.WaferList = GetWaferList(sheet.Cells[i, 4].Text);
                                order.Mode = (int)CommonConstants.ListMode.Next;
                                break;
                        }

                        list.Add(order);
                        i++;
                    }

                    return list;
                }
            }
            catch
            {
                return null;
            }
        }

        public static string GetItemString(string baseStr, string keyStr)
        {
            var itemStr = "";

            try
            {
                var topIndex = baseStr.IndexOf(keyStr);
                if (topIndex < 0)
                {
                    return string.Empty;
                }

                var endIndex = baseStr.Substring(topIndex).IndexOf(";");
                if (endIndex < 0)
                {
                    itemStr = baseStr.Substring(topIndex);
                }
                else
                {
                    itemStr = baseStr.Substring(topIndex, endIndex);
                }

                // 不要な文字列の削除
                itemStr = itemStr.Replace(keyStr, "");
                itemStr = itemStr.Replace("=", "");
                itemStr = itemStr.Replace("'", "");
            }
            catch (Exception ex)
            {

            }           
            return itemStr;
        }

        public static void WriteLog(MainView view, string text)
        {
            const int LOG_MAX_SIZE = 200000;

            var n = view.LogText.Text.Length;
            if (n > LOG_MAX_SIZE)
            {
                n = view.LogText.Text.IndexOf("\r\n", n/2);
                view.LogText.Text = view.LogText.Text.Substring(n+2);
            }
          
            view.LogText.AppendText("[" + DateTime.Now.ToString()+"] " + text + "\r\n");
            view.LogText.ScrollToEnd();
        }

        public static bool[] GetWaferList(string listString)
        {
            var waferList = new bool[CommonConstants.WAFER_MAX];

            foreach (var w in listString.Select((v,i) => new {v, i}))
            {
                if (w.v == '1') waferList[w.i] = true;
            }

            return waferList;
        }

        public static bool[] GetWaferList(int waferCount)
        {
            var waferList = new bool[CommonConstants.WAFER_MAX];

            for (int i = 0; i < CommonConstants.WAFER_MAX; i++)
            {
                if (waferCount >= (i + 1)) waferList[i] = true;
                else break;
            }

            return waferList;
        }
    }
}
