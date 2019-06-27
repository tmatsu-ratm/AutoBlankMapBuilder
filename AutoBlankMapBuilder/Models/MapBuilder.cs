using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AutoBlankMapBuilder.Utils;
using AutoBlankMapBuilder.Views;
using EqpBinDataAccess;

namespace AutoBlankMapBuilder.Models
{
    public class MapBuilder
    {
        private CommonFunc commonFunc;
        private FileAccessClass fileAccessClass;
        private FileCopyClass fileCopyClass;
        private Config cfg;
        private MainView view;
        private List<AlarmInfo> alarmList;
        private SqlFunc sqlFunc = new SqlFunc();

        public MapBuilder(Config cfg, MainView view)
        {
            commonFunc = new CommonFunc("AutoBlankMapBuilder");
            fileAccessClass = new FileAccessClass();
            fileCopyClass = new FileCopyClass(commonFunc);
            this.cfg = cfg;
            this.view = view;
            alarmList = new List<AlarmInfo>();
            sqlFunc.ConnectionStringMapBackup = cfg.MapBackupDb;
        }

        public void Process()
        {
            // List作成
            var list = Utils.Utils.GetOrderList(cfg.OrderList);

            // ブランクマップ作成処理
            foreach (var order in list)
            {
                CreateMap(order);
            }

            if (alarmList.Count > 0)
            {
                var alarmView = new AlarmView(alarmList);
                alarmView.Topmost = true;
                alarmView.Show();
            }

            if (WindowManager.IsOpenWindow<AlarmView>() == false)
            {
                Application.Current.Shutdown();
            }
        }

        public void CreateMap(Order order)
        {
            var alarmMessage = "";
            var backupPath = "";
            var canCreate = true;
            int waPass = 0;
            int waFail = 0;

            // 機種フォルダ特定
            var srcDir = cfg.BlankMapDir + "\\" + order.Item;
            if (Directory.Exists(srcDir) == false)
            {
                canCreate = false;
                alarmMessage += AlarmMessage.AMES_BLANK_MAP_UNKNOWN;
                Utils.Utils.WriteLog(view,  AlarmMessage.AMES_BLANK_MAP_UNKNOWN + " (" + order.Item + ")");
            }

            // INS_ALL保管先確認
            var dir = cfg.AllDataDir + "\\" + order.Item;
            if (Utils.Utils.SearchFolder(dir, order.No))
            {
                canCreate = false;
                if (alarmMessage != "")
                {
                    alarmMessage += ", ";
                }
                alarmMessage += AlarmMessage.AMES_INS_ALL_EXIST;
                Utils.Utils.WriteLog(view,  AlarmMessage.AMES_INS_ALL_EXIST + " (" + order.Item + ")");
            }

            // INS_NEW保管先確認
            dir = cfg.NewDataDir + "\\" + order.Item;
            if (Utils.Utils.SearchFolder(dir, order.No))
            {
                canCreate = false;
                if (alarmMessage != "")
                {
                    alarmMessage += ", ";
                }
                alarmMessage += AlarmMessage.AMES_INS_NEW_EXIST;
                Utils.Utils.WriteLog(view,  AlarmMessage.AMES_INS_NEW_EXIST + " (" + order.Item + ")");
            }

            if (canCreate)
            {
                // MAPファイル作成
                var dstDir = CommonConstants.TMP_PATH;
                CreateMapFile(srcDir, dstDir, order.Item, order.No, order.Quantity, CommonConstants.EXPLORER, out waPass, out waFail);

                // INS_ALLにコピー
                srcDir = dstDir;
                dstDir = cfg.AllDataDir + "\\" + order.Item + "\\" + order.No + "\\" + CommonConstants.INS_ALL_FOLDER;
                backupPath = dstDir;
                fileCopyClass.CopyDirectory(srcDir, dstDir, false, true);

                // INS_NEWにコピー
                dstDir = cfg.NewDataDir + "\\" + order.Item + "\\" + order.No;
                fileCopyClass.CopyDirectory(srcDir, dstDir, false, true);
            }

            if (alarmMessage != "")
            {
                AddAlarm(order, alarmMessage);
            }

            var logMes = order.Department + " " + order.No + " " + order.Item + " " + order.Date + " " +
                         order.Quantity + " ";

            if (alarmMessage == "")
            {
                logMes += "作成済 " + DateTime.Now.ToString("yyyy/MM/dd HH:mm " ) + waPass.ToString() + " " + waFail.ToString() + " " + backupPath;

                // MAP保管履歴書込
                //  mainとwamainの"backup_date"を同値にする
                var backupDate = DateTime.Now;
                var dstDir = cfg.AllDataDir + "\\" + order.Item + "\\" + order.No + "\\" + CommonConstants.INS_ALL_FOLDER;
                if (MainInfoAppend(backupDate, order, waPass, waFail, dstDir) != CommonConstants.ECODE_OK)
                {
                    logMes = logMes.Replace("作成済", "作成済（データベース書込失敗 - main）");
                }
                else
                {
                    if (WaMainInfoAppend(backupDate, order, waPass, waFail, dstDir) != CommonConstants.ECODE_OK)
                    {
                        logMes = logMes.Replace("作成済", "作成済（データベース書込失敗 - wamain）");
                    }
                }
            }
            else
            {
                logMes += alarmMessage;
            }

            commonFunc.PutLog(logMes);

            fileCopyClass.DeleteDirectory(CommonConstants.TMP_PATH);
        }

        public void CreateMapFile(string srcDir, string dstDir, string typeName, string lotNo,int quantity, string explorerPath, out int waPass, out int waFail)
        {
            int i;
            int count = 0;
            string srcFile = "";
            string dstFile = "";
            string errMsg = "";
            bool[] waferList = new bool[CommonConstants.WAFER_MAX];
            LotDatInformation lotDatInfo = null;
            WaferMap waferData = null;
            waPass = 0;
            waFail = 0;

            try
            {
                // LOT.DAT読込
                srcFile = srcDir + "\\" + CommonConstants.LOT_DAT_STRING;
                var rc = fileAccessClass.LotDataReadToClass(srcFile, ref lotDatInfo, ref errMsg);
                if (rc != CommonConstants.ECODE_OK)
                {
                    Utils.Utils.WriteLog(view, "LOT.DAT読込失敗 (" + typeName + ")");
                    return;
                }

                // コピー先のファイルを削除
                fileCopyClass.LotDatFileDelete(dstDir);

                // WA-**.DATの検索
                for (i = 0; i < CommonConstants.WAFER_MAX; i++)
                {
                    srcFile = srcDir + "\\" + CommonConstants.WAFER_DAT_STRING + string.Format("{0:00}", (i + 1)) +
                              ".dat";
                    if (File.Exists(srcFile) == true)
                    {
                        break;
                    }
                }
                if (i >= CommonConstants.WAFER_MAX)
                {
                    Utils.Utils.WriteLog(view, "WA-xx.datが存在しません (" + typeName + ")");
                    return;
                }

                // 未使用??
                var versionType = fileAccessClass.GetVersionType(srcDir + "\\" + CommonConstants.LOT_DAT_STRING);

                rc = fileAccessClass.WaferDataReadToClass(srcFile, ref lotDatInfo, ref waferData, false, ref errMsg);
                if (rc != CommonConstants.ECODE_OK)
                {
                    // TODO
                    return;
                }

                if (Directory.Exists(dstDir) == false)
                {
                    Directory.CreateDirectory(dstDir);
                }

                for (i = 0; i < quantity; i++)
                {
                    dstFile = dstDir + "\\" + CommonConstants.WAFER_DAT_STRING + string.Format("{0:00}", (i + 1)) +
                              ".dat";

                    File.Copy(srcFile, dstFile);

                    // ファイルの情報を更新
                    fileAccessClass.WaDataUpDate(dstFile, (i + 1), ref errMsg);
                    count++;
                }

                var passCount = waferData.wafer_test_sum_info.pass_total * count;
                var failCount = waferData.wafer_test_sum_info.fail_total * count;
                var testCount = waferData.wafer_test_sum_info.test_total * count;

                waPass = waferData.wafer_test_sum_info.pass_total;
                waFail = waferData.wafer_test_sum_info.fail_total;

                // LOT.DATのコピー
                srcFile = srcDir + "\\" + CommonConstants.LOT_DAT_STRING;
                dstFile = dstDir + "\\" + CommonConstants.LOT_DAT_STRING;
                File.Copy(srcFile, dstFile);

                // LOT.DATの情報更新
                for (i = 0; i < waferList.Length; i++)
                {
                    if (i < quantity)
                    {
                        waferList[i] = true;
                    }
                    else
                    {
                        waferList[i] = false;
                    }
                }

                rc = fileAccessClass.LotData_UpdateSomeInfo(dstFile, typeName, lotNo, waferList, passCount, failCount,
                    testCount, ref errMsg);
                if (rc != CommonConstants.ECODE_OK)
                {
                    // TODO:
                    return;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private int MainInfoAppend(DateTime dt, Order order, int waPass, int waFail, string dstDir)
        {
            String errMsg = "";
            string fileName = "";
            var errCode = CommonConstants.ECODE_OK;
            var param = new MainTblAppendParam();
            FileInfo fInfo = null;

            try
            {
                fileName = dstDir + "\\" + CommonConstants.LOT_DAT_STRING;
                if (File.Exists(fileName))
                {
                    fInfo = new FileInfo(fileName);
                }
            }
            catch (Exception ex)
            {
            }

            param.backup_date = dt;
            param.backup_pc = CommonConstants.BACKUP_PC_NAME;
            param.type_name = order.Item;
            param.lot_name = order.No;
            param.pass_chip_count = waPass * order.Quantity;
            param.ng_chip_count = waFail * order.Quantity;
            param.map_count = order.Quantity;
            param.send_flag = true;
            param.backup_path = dstDir;
            param.OPE_NAME = CommonConstants.OPE_NAME;
            param.OPE_SEQ = CommonConstants.OPE_SEQ;
            param.LAY_NO = CommonConstants.LAY_NO;
            param.INI_PASSCOUNT = CommonConstants.INI_PASSCOUNT;
            param.INI_MAP_COUNT = CommonConstants.INI_MAP_COUNT;

            if (fInfo != null)
            {
                param.file_modify = fInfo.LastWriteTime;
            }     
            param.backup_path2 = order.Item + "\\" + order.No;

            if (sqlFunc.MainInfoAppend(param, ref errMsg) != CommonConstants.ECODE_OK)
            {
                errCode = CommonConstants.ECODE_ERROR;
            }

            return errCode;

        }

        private int WaMainInfoAppend(DateTime dt, Order order, int waPass, int waFail, string dstDir)
        {
            String errMsg = "";
            var errCode = CommonConstants.ECODE_OK;
            var param = new WaMainTblAppendParam();

            try
            {
                for (var i = 0; i < order.Quantity; i++)
                {
                    param.backup_date = dt;
                    param.backup_pc = CommonConstants.BACKUP_PC_NAME;
                    param.type_name = order.Item;
                    param.lot_name = order.No;
                    param.pass_chip_count = waPass;
                    param.ng_chip_count = waFail;
                    param.send_flag = true;
                    param.backup_path = dstDir;

                    if (sqlFunc.WaMainInfoAppend(param, i + 1, ref errMsg) != CommonConstants.ECODE_OK)
                    {
                        errCode = CommonConstants.ECODE_ERROR;
                        break;
                    }
                }
            }
            catch
            {
                errCode = CommonConstants.ECODE_ERROR;
            }

            return errCode;
        }

        private void AddAlarm(Order order, string result)
        {
            this.alarmList.Add(new AlarmInfo()
            {
                Time = DateTime.Now,
                Department = order.Department,
                OrderNo = order.No,
                Model = order.Item,
                StartDate = order.Date,
                Quantity = order.Quantity,
                Result = result
            });
        }

    }
}
