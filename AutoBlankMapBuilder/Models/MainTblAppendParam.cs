using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoBlankMapBuilder.Models
{
    public class MainTblAppendParam
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
        public String backup_path2;

        public String OPE_NAME;
        public int OPE_SEQ;
        public String LAY_NO;
        public uint INI_PASSCOUNT;
        public int INI_MAP_COUNT;
        public DateTime INI_MAP_DATE;
        public DateTime file_modify;

        public MainTblAppendParam()
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
            backup_path2 = "";

            OPE_NAME = "";
            OPE_SEQ = 0;
            LAY_NO = "";
            INI_PASSCOUNT = 0;
            INI_MAP_COUNT = 0;
            INI_MAP_DATE = new DateTime(0);
            file_modify = new DateTime(0);
        }
    }
}
