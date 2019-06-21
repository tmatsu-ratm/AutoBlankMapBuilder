using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoBlankMapBuilder.Models
{
    public class LogInfo
    {
        public DateTime Time { get; set; }
        public string Department { get; set; }
        public string OrderNo { get; set; }
        public string Model { get; set; }
        public string StartDate { get; set; }
        public int Quantity { get; set; }
        public string Result { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public string SendPath { get; set; }
    }
}
