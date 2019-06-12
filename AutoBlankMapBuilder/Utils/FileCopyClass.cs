using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace AutoBlankMapBuilder.Utils
{
    public class FileCopyClass
    {
        private CommonFunc commonFunc;

        public FileCopyClass(CommonFunc commonFunc)
        {
            this.commonFunc = commonFunc;
        }

        public void LotDatFileDelete(String path)
        {
            string fileName;

            try
            {
                if (Directory.Exists(path) == true)
                {
                    File.Delete(path + "\\" + CommonConstants.LOT_DAT_STRING);

                    for (var i = 0; i < CommonConstants.WAFER_MAX; i++)
                    {
                        fileName = path + "\\" + CommonConstants.WAFER_DAT_STRING + string.Format("{0:00}", (i + 1)) +
                                   ".dat";
                        if (File.Exists(fileName) == true)
                        {
                            File.Delete(fileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
