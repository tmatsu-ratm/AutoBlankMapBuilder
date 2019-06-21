using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.Sql;
using System.Windows.Navigation;
using AutoBlankMapBuilder.Models;

namespace AutoBlankMapBuilder.Utils
{
    public class SqlFunc
    {
        private SqlConnection mapBackupConnection = null;
        private String connectionStringMapBackup = "";

        private readonly int ECODE_OK = 0;
        private readonly int ECODE_ERROR = -1;

        public String ConnectionStringMapBackup
        {
            get { return connectionStringMapBackup; }
            set { connectionStringMapBackup = value; }
        }

        public int MainInfoAppend(MainTblAppendParam param, ref String msgStr)
        {
            String funcName = "MainInfoAppend : ";
            int errCode = ECODE_ERROR;
            int updateData;
            String sqlCmd;
            SqlCommand cmd;

            msgStr = "";

            try
            {
                sqlCmd = "insert into main (backup_date" +
                         ",backup_pc" +
                         ",type_name" +
                         ",lot_name" +
                         ",pass_chip_count" +
                         ",ng_chip_count" +
                         ",map_count" +
                         ",send_flag" +
                         ",backup_path" +
                         ",OPE_NAME" +
                         ",OPE_SEQ" +
                         ",LAY_NO" +
                         ",INI_PASSCOUNT" +
                         ",INI_MAP_COUNT" +
                         ",INI_MAP_DATE" +
                         ",file_modify" +
                         ",backup_path2) " +
                         " select " + DateTimeToString(param.backup_date) +
                         "," + "" + param.backup_pc + "" +
                         "," + "" + param.type_name + "" +
                         "," + "" + param.lot_name + "" +
                         "," + param.pass_chip_count +
                         "," + param.ng_chip_count +
                         "," + param.map_count +
                         "," + "" + BoolToInt(param.send_flag) + "" +
                         "," + "" + param.backup_path + "" +
                         "," + "" + param.OPE_NAME + "" +
                         "," + param.OPE_SEQ +
                         "," + "" + param.LAY_NO + "" +
                         "," + param.INI_PASSCOUNT +
                         "," + param.INI_MAP_COUNT +
                         "," + DateTimeToString(param.INI_MAP_DATE) +
                         "," + DateTimeToString(param.file_modify) +
                         "," + "" + param.backup_path2 + "";


            }
            catch (Exception)
            {

            }

            return errCode;
        }

        private String DateTimeToString(DateTime dt)
        {
            String str = "null";
            try
            {
                if (dt != new DateTime(0))
                {
                    str = "" + dt.ToString("yyyy/MM/dd HH:mm:ss") + "";
                }
            }
            catch (Exception)
            {
            }

            return str;
        }

        private String BoolToInt(bool val)
        {
            String retVal = "0";

            try
            {
                if (val == true)
                {
                    retVal = "1";
                }
            }
            catch (Exception)
            {
            }

            return retVal;
        }
    }
}
