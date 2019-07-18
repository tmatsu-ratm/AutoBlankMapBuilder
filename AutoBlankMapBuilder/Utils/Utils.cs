using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using OfficeOpenXml;
using AutoBlankMapBuilder.Models;
using AutoBlankMapBuilder.Views;

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
                        order.Department = sheet.Cells[i, 1].Text;
                        order.No = sheet.Cells[i, 2].Text;
                        order.Item = sheet.Cells[i, 3].Text;
                        order.Date = sheet.Cells[i, 4].Text;
                        order.Quantity = int.Parse(sheet.Cells[i, 5].Text);
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
    }
}
