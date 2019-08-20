
using System;
using System.IO;
using System.Windows;
using AutoBlankMapBuilder.Views;

namespace AutoBlankMapBuilder.Models
{
    using System.Xml.Linq;

    public class Config
    {
        public static readonly string CONFIG_FILE_PATH = @".\Config\Config.xml";

        // Server
        public string AllDataDir { get; set; }
        public string NewDataDir { get; set; }
        public string BlankMapDir { get; set; }
        public string LogDir { get; set; }
        public string ListDir { get; set; }

        // DB
        public string MapBackupDb { get; set; }
        public string DbUser { get; set; }
        public string DbPwd { get; set; }
        public string DbServer { get; set; }
        public string DbName { get; set; }

        // 実行時刻
        public string ExecuteTime { get; set; }

        public string OrderList { get; set; }
        public string ASICList { get; set; }
        public string NEXTList { get; set; }

        public string filePath { get; set; }

        public Config(string filename)
        {
            try
            {
                if (File.Exists(filename) == false)
                {
                    var dirName = Path.GetDirectoryName(filename);
                    if (dirName.Length > 0)
                    {
                        if (Directory.Exists(dirName) == false)
                        {
                            Directory.CreateDirectory(dirName);
                        }
                    }

                    CreateDefaultXml(filename);
                }

                LoadConfigFile(filename);
                filePath = Path.GetFullPath(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void LoadConfigFile(string filename)
        {
            try
            {
                var doc = XDocument.Load(filename);
                var server = doc.Element("Config").Element("Server");
                AllDataDir = server.Element("AllDataDirectory").Value;
                NewDataDir = server.Element("NewDataDirectory").Value;
                BlankMapDir = server.Element("BlankMap").Value;
                LogDir = server.Element("Log").Value;
                ListDir = server.Element("List").Value;
                OrderList = ListDir + "\\" + CommonConstants.T_LIST;
                ASICList = ListDir + "\\" + CommonConstants.T_ASIC;
                NEXTList = ListDir + "\\" + CommonConstants.T_NEXT;
                var db = doc.Element("Config").Element("DB").Element("MAP_BACKUP");
                var conString = db.Element("ConnectionStrings").Value;
                MapBackupDb = conString;
                DbUser = Utils.Utils.GetItemString(conString, "User ID");
                DbPwd = Utils.Utils.GetItemString(conString, "Password");
                DbServer = Utils.Utils.GetItemString(conString, "Server");
                DbName = Utils.Utils.GetItemString(conString, "Database");
                var application = doc.Element("Config").Element("Application");
                ExecuteTime = application.Element("ExecuteTime").Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        public void SaveConfigFile(ConfigView view, string fileName)
        {
            if (view.TBlockAllFolder.Text.Length <= 0)
            {
                return;
            }

            if (view.TBlockMapFolder.Text.Length <= 0)
            {
                return;
            }

            if (view.TBlockNewFolder.Text.Length <= 0)
            {
                return;
            }

            if (view.TBlockOrderList.Text.Length <= 0)
            {
                return;
            }

            if (view.TBlockLogFolder.Text.Length <= 0)
            {
                return;
            }

            try
            {
                var doc = XDocument.Load(fileName);
                var server = doc.Element("Config").Element("Server");
                server.Element("AllDataDirectory").Value = view.TBlockAllFolder.Text; 
                server.Element("NewDataDirectory").Value = view.TBlockNewFolder.Text;
                server.Element("BlankMap").Value = view.TBlockMapFolder.Text;
                server.Element("Log").Value = view.TBlockLogFolder.Text;
                server.Element("List").Value = view.TBlockOrderList.Text;
                var application = doc.Element("Config").Element("Application");
                application.Element("ExecuteTime").Value = view.TimePicker.Value.ToString("HH:mm");

                doc.Save(fileName);
                LoadConfigFile(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }            
        }

        private void CreateDefaultXml(string filename)
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "true"),
                new XElement("Config",
                    new XElement("Server",
                        new XElement("AllDataDirectory",
                            new XAttribute("Comment", "個々のMAP保管フォルダ"),
                            new XText("C:\\TMP\\INS_ALL")),
                        new XElement("NewDataDirectory",
                            new XAttribute("Comment", "直近のMAP保管フォルダ"),
                            new XText("C:\\TMP\\INS_NEW")),
                        new XElement("BlankMap",
                            new XAttribute("Comment", "マップファイルを新規作成する際に元になるファイルが保管されているフォルダ"),
                            new XText("C:\\TMP\\BLANK_MAP")),
                        new XElement("Log",
                            new XAttribute("Comment", "マップ作成ログ保管フォルダ"),
                            new XText("C:\\TMP\\LOG")),
                        new XElement("List",
                            new XAttribute("Comment", "リスト保管フォルダ"),
                            new XText("C:\\TMP"))
                        ),
                    new XElement("DB",
                        new XElement("MAP_BACKUP",
                            new XElement("ConnectionStrings",
                                new XAttribute("Comment", "接続文字列"),
                                new XText("Persist Security Info=False;User ID='sa';Password='Rohm789';Server=EES-SERVER\\SQL2008R2;Database=MAP_BACKUP"))
                            )),
                    new XElement("Application",
                        new XElement("ExecuteTime",
                            new XAttribute("Comment", "MAP自動作成実行時刻"),
                            new XText("22:00")))
                    ));

            doc.Save(filename);
        }
    }
}
