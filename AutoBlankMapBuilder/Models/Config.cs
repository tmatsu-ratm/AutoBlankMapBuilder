
using System;
using System.IO;
using System.Windows;
using AutoBlankMapBuilder.Views;

namespace AutoBlankMapBuilder.Models
{
    using System.Xml.Linq;

    public class Config
    {
        public static readonly string CONFIG_FILE_PATH = @"..\Config\Config.xml";

        // Server
        public string AllDataDir { get; set; }
        public string NewDataDir { get; set; }
        public string BlankMapDir { get; set; }

        // DB
        public string MapBackupDb { get; set; }

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
                AllDataDir = server.Element("AllDataDirectory").Value.ToString();
                NewDataDir = server.Element("NewDataDirectory").Value.ToString();
                BlankMapDir = server.Element("BlankMap").Value.ToString();
                var win = new ConfigView();
                win.TxtBlock1.Text = AllDataDir;
                win.Show();
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
                            new XText("C:\\TMP\\BLANK_MAP"))
                        ),
                    new XElement("DB",
                        new XElement("MAP_BACKUP",
                            new XElement("ConnectionStrings",
                                new XAttribute("Comment", "接続文字列"),
                                new XText("Persist Security Info=False;User ID='sa';Password='Rohm789';Server=EES-SERVER\\SQL2008R2;Database=MAP_BACKUP"))
                            ))
                    ));

            doc.Save(filename);
        }
    }
}
