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
            commonFunc = new CommonFunc("AutoBlankMapBuilder", cfg);
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

            if (list == null)
            {
                alarmList.Add(new AlarmInfo()
                {
                    Time = DateTime.Now,
                    Result = "リストの作成に失敗しました"
                });

                var alarmView = new AlarmView(alarmList);
                alarmView.Topmost = true;
                alarmView.Show();
                return;
            }

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
            /*
            if (WindowManager.IsOpenWindow<AlarmView>() == false)
            {
                Application.Current.Shutdown();
            }
            */
        }

        public void CreateMap(Order order)
        {
            var srcDir = "";
            var alarmMessage = "";
            var backupPath = "";
            var canCreate = true;
            int waPassTotal = 0;
            int waFailTotal = 0;
            int[] waPassList = new int[CommonConstants.WAFER_MAX];
            int[] waFailList = new int[CommonConstants.WAFER_MAX];

            var mode = order.Mode;

            // MAPフォルダ特定
            if (mode == (int) CommonConstants.ListMode.Asic)
            {
                srcDir = order.BackupPath;
            }
            else
            {
                srcDir = cfg.BlankMapDir + "\\" + order.Item;
            }

            if (Directory.Exists(srcDir) == false)
            {
                canCreate = false;
                alarmMessage += AlarmMessage.AMES_BLANK_MAP_UNKNOWN[mode];
                Utils.Utils.WriteLog(view,  AlarmMessage.AMES_BLANK_MAP_UNKNOWN[mode] + " (" + order.No + ")");
            }

            // MAPファイル構成確認
            if (canCreate && mode == (int) CommonConstants.ListMode.Asic)
            {
                if (VerifyMapfile(srcDir, order.WaferList) == false)
                {
                    canCreate = false;
                    alarmMessage += AlarmMessage.AMES_BLANK_MAP_UNKNOWN2[mode];
                    Utils.Utils.WriteLog(view, AlarmMessage.AMES_BLANK_MAP_UNKNOWN2[mode] + " (" + order.No + ")");
                }
            }

            // INS_ALL保管先確認
            var dir = cfg.AllDataDir + "\\" + order.Item;
            var searchResult = Utils.Utils.SearchFolder(dir, order.No);
            if (searchResult == 1)
            {
                canCreate = false;
                if (alarmMessage != "")
                {
                    alarmMessage += ", ";
                }
                alarmMessage += AlarmMessage.AMES_INS_ALL_EXIST;
                Utils.Utils.WriteLog(view,  AlarmMessage.AMES_INS_ALL_EXIST + " (" + order.No + ")");
            }
            else if (searchResult == -1)
            {
                canCreate = false;
                if (alarmMessage != "")
                {
                    alarmMessage += ", ";
                }

                alarmMessage += AlarmMessage.AMES_INS_ALL_SEARCH_ERROR;
                Utils.Utils.WriteLog(view, AlarmMessage.AMES_INS_ALL_SEARCH_ERROR + " (" + order.No + ")");
            }

            // INS_NEW保管先確認
            dir = cfg.NewDataDir + "\\" + order.Item;
            searchResult = Utils.Utils.SearchFolder(dir, order.No);
            if (searchResult == 1)
            {
                canCreate = false;
                if (alarmMessage != "")
                {
                    alarmMessage += ", ";
                }
                alarmMessage += AlarmMessage.AMES_INS_NEW_EXIST;
                Utils.Utils.WriteLog(view,  AlarmMessage.AMES_INS_NEW_EXIST + " (" + order.No + ")");
            }
            else if (searchResult == -1)
            {
                canCreate = false;
                if (alarmMessage != "")
                {
                    alarmMessage += ", ";
                }

                alarmMessage += AlarmMessage.AMES_INS_NEW_SEARCH_ERROR;
                Utils.Utils.WriteLog(view, AlarmMessage.AMES_INS_NEW_SEARCH_ERROR + " (" + order.No + ")");
            }

            if (canCreate)
            {
                // MAPファイル作成
                var dstDir = CommonConstants.TMP_PATH;
                var rc = -1;
                if (mode == (int) CommonConstants.ListMode.Asic)
                {
                    rc = CreateAsicMapFile(srcDir, dstDir, order.Item, order.No, order.WaferList, CommonConstants.EXPLORER, out waPassList, out waFailList, out waPassTotal, out waFailTotal);
                }
                else
                {
                    rc = CreateBlankMapFile(srcDir, dstDir, order.Item, order.No, order.WaferList, CommonConstants.EXPLORER, out waPassList, out waFailList, out waPassTotal, out waFailTotal);
                }

                if (rc == CommonConstants.ECODE_OK)
                {
                    // INS_ALLにコピー
                    srcDir = dstDir;
                    dstDir = cfg.AllDataDir + "\\" + order.Item + "\\" + order.No + "\\" + CommonConstants.INS_ALL_FOLDER[mode];
                    backupPath = dstDir;
                    rc = fileCopyClass.CopyDirectory(srcDir, dstDir, false, true);
                    if (rc != CommonConstants.ECODE_OK)
                    {
                        alarmMessage += AlarmMessage.AMES_INS_ALL_COPY_ERROR;
                        Utils.Utils.WriteLog(view,  AlarmMessage.AMES_INS_ALL_COPY_ERROR + " (" + order.No + ")");
                    }
                    // INS_NEWにコピー
                    dstDir = cfg.NewDataDir + "\\" + order.Item + "\\" + order.No;
                    rc = fileCopyClass.CopyDirectory(srcDir, dstDir, false, true);
                    if (rc != CommonConstants.ECODE_OK)
                    {
                        if (alarmMessage != "")
                        {
                            alarmMessage += ", ";
                        }
                        alarmMessage += AlarmMessage.AMES_INS_NEW_COPY_ERROR;
                        Utils.Utils.WriteLog(view,  AlarmMessage.AMES_INS_NEW_COPY_ERROR + " (" + order.Item + ")");
                    }
                }
                else
                {
                    alarmMessage += AlarmMessage.AMES_MAP_CREATE_ERROR;
                    Utils.Utils.WriteLog(view,  AlarmMessage.AMES_MAP_CREATE_ERROR + " (" + order.No + ")");
                }

            }
            var logMes = order.Department + " " + order.No + " " + order.Item + " " + order.Date + " " +
                         order.Quantity + " ";

            if (alarmMessage == "")
            {
                logMes += "作成済 " + DateTime.Now.ToString("yyyy/MM/dd HH:mm " ) + (waPassTotal / order.Quantity).ToString() + " " + (waFailTotal / order.Quantity).ToString() + " " + backupPath;

                // MAP保管履歴書込
                //  mainとwamainの"backup_date"を同値にする
                var backupDate = DateTime.Now;
                var dstDir = cfg.AllDataDir + "\\" + order.Item + "\\" + order.No + "\\" + CommonConstants.INS_ALL_FOLDER;
                if (MainInfoAppend(backupDate, order, waPassTotal, waFailTotal, dstDir) != CommonConstants.ECODE_OK)
                {
                    logMes = logMes.Replace("作成済", "作成済（データベース書込失敗 - main）");
                    alarmMessage += AlarmMessage.AMES_DB_ERROR;
                    Utils.Utils.WriteLog(view,  AlarmMessage.AMES_DB_ERROR + " (" + order.No + ")");
                }
                else
                {
                    if (WaMainInfoAppend(backupDate, order, waPassList, waFailList, dstDir) != CommonConstants.ECODE_OK)
                    {
                        logMes = logMes.Replace("作成済", "作成済（データベース書込失敗 - wamain）");
                        alarmMessage += AlarmMessage.AMES_DB_ERROR;
                        Utils.Utils.WriteLog(view,  AlarmMessage.AMES_DB_ERROR + " (" + order.No + ")");
                    }
                }
            }
            else
            {
                logMes += alarmMessage;
            }

            if (alarmMessage != "")
            {
                AddAlarm(order, alarmMessage);
            }
            else
            {
                Utils.Utils.WriteLog(view,  "投入MAP作成成功 (" + order.No + ")");
            }

            commonFunc.PutLog(logMes);

            fileCopyClass.DeleteDirectory(CommonConstants.TMP_PATH);
        }

        public int CreateMapFile(string srcDir, string dstDir, string typeName, string lotNo,int quantity, string explorerPath, out int waPass, out int waFail)
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
                    return rc;
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
                    return CommonConstants.ECODE_ERROR;
                }

                // 未使用??
                var versionType = fileAccessClass.GetVersionType(srcDir + "\\" + CommonConstants.LOT_DAT_STRING);

                rc = fileAccessClass.WaferDataReadToClass(srcFile, ref lotDatInfo, ref waferData, false, ref errMsg);
                if (rc != CommonConstants.ECODE_OK)
                {
                    Utils.Utils.WriteLog(view, "Error : WaferDataReadToClass");
                    return rc;
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
                    Utils.Utils.WriteLog(view, "Error : LotData_UpdateSomeInfo");
                    return rc;
                }
            }
            catch (Exception ex)
            {
                return CommonConstants.ECODE_ERROR;
            }

            return CommonConstants.ECODE_OK;
        }

        public int CreateBlankMapFile(string srcDir, string dstDir, string typeName, string lotNo, bool[] waferList, string explorerPath, out int[] waPassList, out int[] waFailList, out int passCount, out int failCount)
        {
            int i;
            int count = 0;
            string srcFile = "";
            string dstFile = "";
            string errMsg = "";
            LotDatInformation lotDatInfo = null;
            WaferMap waferData = null;
            waPassList = new int[CommonConstants.WAFER_MAX];
            waFailList = new int[CommonConstants.WAFER_MAX];
            passCount = 0;
            failCount = 0;
            var testCount = 0;

            try
            {
                // LOT.DAT読込
                srcFile = srcDir + "\\" + CommonConstants.LOT_DAT_STRING;
                var rc = fileAccessClass.LotDataReadToClass(srcFile, ref lotDatInfo, ref errMsg);
                if (rc != CommonConstants.ECODE_OK)
                {
                    Utils.Utils.WriteLog(view, "LOT.DAT読込失敗 (" + typeName + ")");
                    return rc;
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
                    return CommonConstants.ECODE_ERROR;
                }
                // 未使用??
                var versionType = fileAccessClass.GetVersionType(srcDir + "\\" + CommonConstants.LOT_DAT_STRING);

                rc = fileAccessClass.WaferDataReadToClass(srcFile, ref lotDatInfo, ref waferData, false, ref errMsg);
                if (rc != CommonConstants.ECODE_OK)
                {
                    Utils.Utils.WriteLog(view, "Error : WaferDataReadToClass");
                    return rc;
                }

                if (Directory.Exists(dstDir) == false)
                {
                    Directory.CreateDirectory(dstDir);
                }

                foreach (var w in waferList.Select((v, j) => new {v, j}))
                {
                    if (w.v)
                    {
                        dstFile = dstDir + "\\" + CommonConstants.WAFER_DAT_STRING + string.Format("{0:00}", w.j + 1) +
                                  ".dat";

                        File.Copy(srcFile,dstFile);

                        // ファイルの情報を更新
                        fileAccessClass.WaDataUpDate(dstFile, (w.j + 1), ref errMsg);
                        waPassList[w.j] = waferData.wafer_test_sum_info.pass_total;
                        waFailList[w.j] = waferData.wafer_test_sum_info.fail_total;

                        passCount += waferData.wafer_test_sum_info.pass_total;
                        failCount += waferData.wafer_test_sum_info.fail_total;
                        testCount += waferData.wafer_test_sum_info.test_total;
                    }
                }

                // LOT.DATのコピー
                srcFile = srcDir + "\\" + CommonConstants.LOT_DAT_STRING;
                dstFile = dstDir + "\\" + CommonConstants.LOT_DAT_STRING;
                File.Copy(srcFile, dstFile);

                rc = fileAccessClass.LotData_UpdateSomeInfo(dstFile, typeName.Substring(0,12), lotNo, waferList, passCount, failCount,
                    testCount, ref errMsg);
                if (rc != CommonConstants.ECODE_OK)
                {
                    Utils.Utils.WriteLog(view, "Error : LotData_UpdateSomeInfo");
                    return rc;
                }
            }
            catch (Exception ex)
            {
                return CommonConstants.ECODE_ERROR;
            }

            return CommonConstants.ECODE_OK;
        }

        public int CreateAsicMapFile(string srcDir, string dstDir, string typeName, string lotNo, bool[]waferList, string explorerPath, out int[] waPassList, out int[] waFailList, out int passCount, out int failCount)
        {
            int count = 0;
            string srcFile = "";
            string dstFile = "";
            string errMsg = "";
            LotDatInformation lotDatInfo = null;
            WaferMap waferData = null;
            waPassList = new int[CommonConstants.WAFER_MAX];
            waFailList = new int[CommonConstants.WAFER_MAX];
            passCount = 0;
            failCount = 0;
            var testCount = 0;

            try
            {
                // LOT.DAT読込
                srcFile = srcDir + "\\" + CommonConstants.LOT_DAT_STRING;
                var rc = fileAccessClass.LotDataReadToClass(srcFile, ref lotDatInfo, ref errMsg);
                if (rc != CommonConstants.ECODE_OK)
                {
                    Utils.Utils.WriteLog(view, "LOT.DAT読込失敗 (" + typeName + ")");
                    return rc;
                }

                // コピー先のファイルを削除
                fileCopyClass.LotDatFileDelete(dstDir);

                foreach (var w in waferList.Select((v, j) => new { v, j }))
                {
                    if (w.v)
                    {
                        srcFile = srcDir + "\\" + CommonConstants.WAFER_DAT_STRING +
                                  string.Format("{0:00}", (w.j + 1)) + ".dat";

                        rc = fileAccessClass.WaferDataReadToClass(srcFile, ref lotDatInfo, ref waferData, false, ref errMsg);

                        if (rc != CommonConstants.ECODE_OK)
                        {
                            Utils.Utils.WriteLog(view, "Error : WaferDataReadToClass");
                            return rc;
                        }

                        if (Directory.Exists(dstDir) == false)
                        {
                            Directory.CreateDirectory(dstDir);
                        }

                        dstFile = dstDir + "\\" + CommonConstants.WAFER_DAT_STRING + string.Format("{0:00}", w.j + 1) +
                                  ".dat";

                        File.Copy(srcFile, dstFile);

                        // ファイルの情報を更新
                        fileAccessClass.WaDataUpDate(dstFile, (w.j + 1), ref errMsg);
                        waPassList[w.j] = waferData.wafer_test_sum_info.pass_total;
                        waFailList[w.j] = waferData.wafer_test_sum_info.fail_total;

                        passCount += waferData.wafer_test_sum_info.pass_total;
                        failCount += waferData.wafer_test_sum_info.fail_total;
                        testCount += waferData.wafer_test_sum_info.test_total;
                    }
                }

                // LOT.DATのコピー
                srcFile = srcDir + "\\" + CommonConstants.LOT_DAT_STRING;
                dstFile = dstDir + "\\" + CommonConstants.LOT_DAT_STRING;
                File.Copy(srcFile, dstFile);

                rc = fileAccessClass.LotData_UpdateSomeInfo(dstFile, typeName, lotNo, waferList, passCount, failCount,
                    testCount, ref errMsg);
                if (rc != CommonConstants.ECODE_OK)
                {
                    Utils.Utils.WriteLog(view, "Error : LotData_UpdateSomeInfo");
                    return rc;
                }
            }
            catch (Exception ex)
            {
                return CommonConstants.ECODE_ERROR;
            }

            return CommonConstants.ECODE_OK;
        }

        private int MainInfoAppend(DateTime dt, Order order, int waPass, int waFail, string dstDir)
        {
            String errMsg = "";
            string fileName = "";
            var errCode = CommonConstants.ECODE_OK;
            var param = new MainTblAppendParam();
            FileInfo fInfo = null;
            var mode = order.Mode;

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
            param.backup_pc = CommonConstants.BACKUP_PC_NAME[mode];
            param.type_name = order.Item;
            param.lot_name = order.No;
            param.pass_chip_count = waPass;
            param.ng_chip_count = waFail;
            param.map_count = order.Quantity;
            param.send_flag = true;
            param.backup_path = dstDir;
            param.OPE_NAME = CommonConstants.OPE_NAME[mode];
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

        private int WaMainInfoAppend(DateTime dt, Order order, int[] waPassList, int[] waFailList, string dstDir)
        {
            String errMsg = "";
            var errCode = CommonConstants.ECODE_OK;
            var param = new WaMainTblAppendParam();
            var mode = order.Mode;

            try
            {
                foreach (var w in order.WaferList.Select((v, i) => new {v,i}))
                {
                    if (w.v)
                    {
                        param.backup_date = dt;
                        param.backup_pc = CommonConstants.BACKUP_PC_NAME[mode];
                        param.type_name = order.Item;
                        param.lot_name = order.No;
                        param.pass_chip_count = waPassList[w.i];
                        param.ng_chip_count = waFailList[w.i];
                        param.send_flag = true;
                        param.backup_path = dstDir;
                        if (sqlFunc.WaMainInfoAppend(param, w.i + 1, ref errMsg) != CommonConstants.ECODE_OK)
                        {
                            errCode = CommonConstants.ECODE_ERROR;
                            break;
                        }
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

        private bool VerifyMapfile(string folderPath, bool[] waferList)
        {
            foreach (var w in waferList.Select((v, i) => new { v, i }))
            {
                if (w.v)
                {
                    var fileName = folderPath + "\\" + CommonConstants.WAFER_DAT_STRING + string.Format("{0:00}", (w.i + 1)) +
                                   ".dat";
                    if (File.Exists(fileName) == false) return false;
                }
            }
            return true;
        }

    }
}
