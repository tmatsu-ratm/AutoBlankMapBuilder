using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AutoBlankMapBuilder.Models;

namespace AutoBlankMapBuilder.Utils
{
    public class CommonFunc
    {
        private Config cfg;
        private string logFileName = "";
        private string logDir = "";
        private uint fileCount = 0;

        public CommonFunc(string logFileName, Config cfg)
        {
            this.cfg = cfg;
//            logDir = Environment.CurrentDirectory + "\\" + CommonConstants.LOG_PATH;
            logDir = this.cfg.LogDir;
            this.logFileName = logFileName;

            if (Directory.Exists(logDir) == false)
            {
                Directory.CreateDirectory(logDir);
            }

            CheckFileNo();
    
        }

        private void CheckFileNo()
        {
            var di = new DirectoryInfo(logDir);
            var fInfo = di.GetFiles(logFileName + "*" + CommonConstants.FILE_TYPE);
            uint lastNo = 1, i, no;
            var lastTime = new DateTime(0);

            foreach (var fileInfo in fInfo)
            {
                try
                {
                    no = Convert.ToUInt32(fileInfo.Name.Replace(logFileName, "")
                        .Replace(CommonConstants.FILE_TYPE, ""));
                    if (lastTime < fileInfo.LastWriteTime)
                    {
                        lastTime = fileInfo.LastWriteTime;
                        lastNo = no;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            fileCount = lastNo;
        }

        public void PutLog(string str)
        {
            string msg;
            var sjisEnc = Encoding.GetEncoding("Shift_JIS");
            long count;
            string fileName;

            msg = str;

            try
            {
                count = sjisEnc.GetByteCount(msg);
                FileSizeCheck(count);
            }
            catch (Exception)
            {
            }

            fileName = logDir + "\\" + logFileName + string.Format("{0:000}", fileCount) + CommonConstants.FILE_TYPE;

            try
            {
                var writer = new StreamWriter(fileName, true, Encoding.GetEncoding("Shift_JIS"));
                writer.WriteLine(msg);
                writer.Close();
            }
            catch (Exception)
            {
            }    
        }

        private void FileSizeCheck(long size)
        {
            string logFile = null;

            logFile = logDir + "\\" + logFileName + string.Format("{0:000}", fileCount) + CommonConstants.FILE_TYPE;

            if (File.Exists(logFile) == false)
            {
                FileStream fs = File.Create(logFile);
                fs.Close();
            }

            var oFile = new FileInfo(logFile);

            if ((oFile.Length + size) > CommonConstants.FILE_SIZE)
            {
                fileCount = (fileCount + 1) % CommonConstants.FILE_MAX;

                logFile = logDir + "\\" + logFileName + string.Format("{0:000}", fileCount) + CommonConstants.FILE_TYPE;

                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }
                else
                {
                    var fs = File.Create(logFile);
                    fs.Close();
                }
            }

            oFile = null;
        }
    }
}
