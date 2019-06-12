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

        public CommonFunc(string logFileName)
        {
            logDir = Environment.CurrentDirectory + "\\" + CommonConstants.LOG_PATH;
            this.logFileName = logFileName;

            if (Directory.Exists(logDir) == false)
            {
                Directory.CreateDirectory(logDir);
            }
        }
    }
}
