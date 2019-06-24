
namespace AutoBlankMapBuilder
{
    public static class CommonConstants
    {
        public static readonly int ECODE_OK = 0;
        public static readonly int ECODE_ERROR = -1;
        public static readonly int WAFER_MAX = 25;

        public static readonly string LOT_DAT_STRING = "LOT.DAT";
        public static readonly string WAFER_DAT_STRING = "W-NO-";
        public static readonly string INS_ALL_FOLDER = "DATA000_投入";
        public static readonly string BACKUP_PC_NAME = "AUTO_MAP";
        public static readonly string OPE_NAME = "自動作成";
        public static readonly string LAY_NO = "AUTO";
        public static readonly int OPE_SEQ = 0;
        public static readonly uint INI_PASSCOUNT = 0;
        public static readonly int INI_MAP_COUNT = 0;


        public static readonly string EXPLORER = "EXPLORER.EXE";

        public static readonly string TMP_PATH = "../TMP";

        public static readonly string LOG_PATH = "../LOG";
        public static readonly string FILE_TYPE = ".log";
        public static readonly uint FILE_MAX = 100;
        public static readonly uint FILE_SIZE = 5000000;
    }

    public static class AlarmMessage
    {
        public static readonly string AMES_BLANK_MAP_UNKNOWN = "ブランクMAP登録なし";
        public static readonly string AMES_INS_ALL_EXIST = "INS_ALLあり";
        public static readonly string AMES_INS_NEW_EXIST = "INS_NEWあり";
        public static readonly string AMES_NETWORK_ERROR = "ネットワークエラー";
    }
}
