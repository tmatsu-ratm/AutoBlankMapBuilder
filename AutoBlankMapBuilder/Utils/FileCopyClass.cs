using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace AutoBlankMapBuilder.Utils
{
    public class FileCopyClass
    {
        private CommonFunc commonFunc;

        public FileCopyClass(CommonFunc commonFunc)
        {
            this.commonFunc = commonFunc;
        }

        // 指定フォルダ以下のファイル及びフォルダを全てコピーする
        public int CopyDirectory(string stSourcePath, string stDestPath, bool subDirCopyFlg, bool bOverwrite)
        {
            string stCopyTo;
            string[] stCopyFrom;
            var errCode = CommonConstants.ECODE_OK;
            var isOverwrite = bOverwrite;

            try
            {
                // コピー先のフォルダがなければ作成する
                if (!Directory.Exists(stDestPath))
                {
                    Directory.CreateDirectory(stDestPath);
                    File.SetAttributes(stDestPath, File.GetAttributes(stSourcePath));
                    isOverwrite = true;
                }

                // コピー元のフォルダにあるすべてのファイルをコピーする
                if (isOverwrite)
                {
                    stCopyFrom = Directory.GetFiles(stSourcePath);
                    foreach (var f in stCopyFrom)
                    {
                        stCopyTo = Path.Combine(stDestPath, Path.GetFileName(f));
                        File.Copy(f, stCopyTo, true);
                    }
                }
                else
                {
                    // 上書き不可能な場合は存在しない時のみコピーする
                    stCopyFrom = Directory.GetFiles(stSourcePath);
                    foreach (var f in stCopyFrom)
                    {
                        stCopyTo = Path.Combine(stDestPath, Path.GetFileName(f));
                        if (!File.Exists(stCopyTo))
                        {
                            File.Copy(f, stCopyTo, false);
                        }
                        
                    }
                }

                // コピー元のフォルダをすべてコピーする（再帰）
                // フラグでサブフォルダのコピーを行うか判断する
                if (subDirCopyFlg)
                {
                    stCopyFrom = Directory.GetDirectories(stSourcePath);
                    foreach (var f in stCopyFrom)
                    {
                        stCopyTo = Path.Combine(stDestPath, Path.GetFileName(f));
                        errCode = CopyDirectory(f, stCopyTo, subDirCopyFlg, isOverwrite);
                        if (errCode != CommonConstants.ECODE_OK)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return errCode;
        }

        // 指定フォルダの下にあるLOT.DAT及び, WA-xx.DATを削除する
        public void LotDatFileDelete(String path)
        {
            string fileName;

            try
            {
                if (Directory.Exists(path) == true)
                {
                    File.Delete(path + "\\" + CommonConstants.LOT_DAT_STRING);

                    for (var i = 0; i < CommonConstants.WAFER_MAX; i++)
                    {
                        fileName = path + "\\" + CommonConstants.WAFER_DAT_STRING + string.Format("{0:00}", (i + 1)) +
                                   ".dat";
                        if (File.Exists(fileName) == true)
                        {
                            File.Delete(fileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // 指定フォルダとその中身を全て削除する
        public void DeleteDirectory(string targetDirectoryPath)
        {
            if (!Directory.Exists(targetDirectoryPath))
            {
                return;
            }

            // ディレクトリ以外の全ファイルを削除
            var filePaths = Directory.GetFiles(targetDirectoryPath);
            foreach (var filePath in filePaths)
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }

            // ディレクトリの中のディレクトリも再帰的に削除
            var directoryPaths = Directory.GetDirectories(targetDirectoryPath);
            foreach (var directoryPath in directoryPaths)
            {
                DeleteDirectory(directoryPath);
            }

            // 中が空になったらディレクトリ自身も削除
            Directory.Delete(targetDirectoryPath, false);
        }
    }
}
