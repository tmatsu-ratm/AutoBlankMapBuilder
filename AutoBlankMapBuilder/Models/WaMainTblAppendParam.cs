using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoBlankMapBuilder.Models
{
    public class WaMainTblAppendParam
    {
        public DateTime backup_date;
        public String backup_pc;
        public String type_name;
        public String lot_name;
        public int pass_chip_count;
        public int ng_chip_count;
        public int map_count;
        public bool send_flag;
        public String backup_path;
        public String judge;

        public WaMainTblAppendParam()
        {
            backup_date = new DateTime(0);
            backup_pc = "";
            type_name = "";
            lot_name = "";
            pass_chip_count = 0;
            ng_chip_count = 0;
            map_count = 0;
            send_flag = false;
            backup_path = "";
            judge = "";
        }
    }
}
