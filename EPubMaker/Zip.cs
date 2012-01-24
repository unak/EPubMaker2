using System;
using System.IO;
using System.Threading;

namespace EPubMaker
{
    /// <summary>
    /// zipファイル操作
    /// とりあえず生成のみサポート
    /// Windows自体のzipフォルダ機能(zipfldr.dll)を使用しているので、そいつが無効だと実は動かない
    /// </summary>
    public class Zip
    {
        private string path;        /// 出力ファイルパス
        private string zip;         /// 拡張子がzipじゃない場合の一時ファイルパス
        private Shell32.Shell sh;   /// シェルオブジェクト
        private Shell32.Folder dir; /// フォルダオブジェクト

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">出力先パス</param>
        public Zip(string path)
        {
            this.path = path;
            if (String.Compare(Path.GetExtension(path), ".zip", true) != 0)
            {
                zip = Path.ChangeExtension(path, ".zip");
            }
            else
            {
                zip = path;
            }

            Scripting.FileSystemObject fso = new Scripting.FileSystemObject();
            Scripting.TextStream ts = fso.CreateTextFile(zip, true, false);
            ts.Write("PK\x05\x06\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00");
            ts.Close();

            sh = new Shell32.Shell();
            dir = sh.NameSpace(this.zip);
        }

        /// <summary>
        /// ファイル・フォルダ格納
        /// </summary>
        /// <param name="src">コピー元パス</param>
        public void CopyFrom(string src)
        {
            Shell32.FolderItem fi = sh.NameSpace(Path.GetDirectoryName(src)).ParseName(Path.GetFileName(src));
            dir.CopyHere(fi, 0);
            Sync();
        }

        /// <summary>
        /// zipファイルクローズ
        /// </summary>
        public void Close()
        {
            Sync();
            dir = null;
            sh = null;

            if (String.Compare(zip, path, true) != 0)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                while (true)
                {
                    try
                    {
                        File.Move(zip, path);
                        break;
                    }
                    catch
                    {
                        Thread.Yield();
                        Thread.Sleep(500);
                    }
                }
            }

            zip = null;
            path = null;
        }

        /// <summary>
        /// 同期
        /// </summary>
        private void Sync()
        {
            if (zip == null || dir == null || sh == null)
            {
                return;
            }

            // 非同期処理が行われているので、ファイルがオープンできるかどうかで処理終了を判断する
            while (true)
            {
                Thread.Sleep(500);
                try
                {
                    FileStream fs = File.Open(this.zip, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fs.Close();
                    break;
                }
                catch
                {
                    Thread.Yield();
                }
            }
        }
    }
}
