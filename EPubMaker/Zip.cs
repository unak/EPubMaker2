using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace EPubMaker
{
    public class Zip
    {
        private string path;
        private string zip;
        private Shell32.Shell sh;
        private Shell32.Folder dir;

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

        public void CopyFrom(string src)
        {
            Shell32.FolderItem fi = sh.NameSpace(Path.GetDirectoryName(src)).ParseName(Path.GetFileName(src));
            dir.CopyHere(fi, 0);
            Sync();
        }

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

        public void Sync()
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
