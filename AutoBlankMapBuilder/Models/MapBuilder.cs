using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using AutoBlankMapBuilder.Utils;
using EqpBinDataAccess;

namespace AutoBlankMapBuilder.Models
{
    public class MapBuilder
    {
        private CommonFunc commonFunc;
        private FileAccessClass fileAccessClass;
        private FileCopyClass fileCopyClass;

        public MapBuilder()
        {
            commonFunc = new CommonFunc("AutoBlankMapBuilder");
            fileAccessClass = new FileAccessClass();
            fileCopyClass = new FileCopyClass(commonFunc);
        }

        public void CreateMapFile(string srcDir, string dstDir, string typeName, string lotNo, CheckBox[] checkBoxes,
            string explorerPath)
        {
            int i;
            int count = 0;
            string srcFile = "";
            string dstFile = "";
            string errMsg = "";
            bool[] waferList = new bool[CommonConstants.WAFER_MAX];
            LotDatInformation lotDatInfo = null;
            WaferMap waferData = null;

            try
            {
                // LOT.DAT読込
                srcFile = srcDir + "\\" + CommonConstants.LOT_DAT_STRING;
                var rc = fileAccessClass.LotDataReadToClass(srcFile, ref lotDatInfo, ref errMsg);
                if (rc != CommonConstants.ECODE_OK)
                {
                    // TODO
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
                    // TODO
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

                // TODO: チェックボックスではなくリストの枚数で
                for (i = 0; i < checkBoxes.Length; i++)
                {
                    if (checkBoxes[i].IsChecked == false)
                    {
                        continue;
                    }

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

                // LOT.DATのコピー
                srcFile = srcDir + "\\" + CommonConstants.LOT_DAT_STRING;
                dstFile = dstDir + "\\" + CommonConstants.LOT_DAT_STRING;
                File.Copy(srcFile, dstFile);

                // LOT.DATの情報更新
                for (i = 0; i < waferList.Length; i++)
                {
                    waferList[i] = (bool)checkBoxes[i].IsChecked;
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
    }
}
