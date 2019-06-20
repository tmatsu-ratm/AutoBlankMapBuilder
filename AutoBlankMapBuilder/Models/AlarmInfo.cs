using System;

namespace AutoBlankMapBuilder.Models
{
    public class AlarmInfo
    {
        public DateTime Time { get; set; }
        public string Department { get; set; }
        public string OrderNo { get; set; }
        public string Model { get; set; }
        public string StartDate { get; set; }
        public int Quantity { get; set; }
        public string Result { get; set; }
    }
}
