using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EPubMaker
{
    public partial class FormProgress : Form
    {
        private struct WorkerArg
        {
            public List<Page> pages;
            public string title;
            public string author;
            public int width;
            public int height;
            public string path;
        }

        public FormProgress(List<Page> pages, string title, string author, int width, int height, string path)
        {
            InitializeComponent();

            WorkerArg arg = new WorkerArg();
            arg.pages = pages;
            arg.title = title.Trim();
            arg.author = author.Trim();
            arg.width = width;
            arg.height = height;
            arg.path = path;

            this.DialogResult = DialogResult.None;
            backgroundWorker.RunWorkerAsync(arg);
        }

        private void FormProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.None)
            {
                e.Cancel = true;
                backgroundWorker.CancelAsync();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            backgroundWorker.CancelAsync();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            backgroundWorker.ReportProgress(-1);    // 開始

            WorkerArg arg = (WorkerArg)e.Argument;

            string tmpdir = Path.Combine(Path.GetTempPath(), "EPubMaker");
            if (Directory.Exists(tmpdir))
            {
                Directory.Delete(tmpdir, true);
            }
            Directory.CreateDirectory(tmpdir);
            backgroundWorker.ReportProgress(50, 100);

            string mime = Path.Combine(tmpdir, "mimetype");
            FileStream fs = File.OpenWrite(mime);
            WriteText(fs, "application/epub+zip");
            fs.Close();
            backgroundWorker.ReportProgress(60, 100);

            string meta = Path.Combine(tmpdir, "META-INF");
            Directory.CreateDirectory(meta);
            fs = File.OpenWrite(Path.Combine(meta, "container.xml"));
            WriteText(fs, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">\n<rootfiles>\n<rootfile media-type=\"application/oebps-package+xml\" full-path=\"OEBPS/package.opf\" />\n</rootfiles>\n</container>\n");
            fs.Close();
            backgroundWorker.ReportProgress(70, 100);

            string contents = Path.Combine(tmpdir, "OEBPS");
            Directory.CreateDirectory(Path.Combine(contents, "data"));
            backgroundWorker.ReportProgress(80, 100);

            fs = File.OpenWrite(Path.Combine(contents, "package.opf"));
            WriteText(fs, "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n<package xmlns=\"http://www.idpf.org/2007/opf\" version=\"3.0\" unique-identifier=\"BookId\" xml:lang=\"ja\">\n<metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\">\n<dc:identifier id=\"BookId\">");
            string bookid = Guid.NewGuid().ToString();
            WriteText(fs, bookid);
            WriteText(fs, "</dc:identifier>\n<dc:title>");
            WriteText(fs, arg.title);
            WriteText(fs, "</dc:title>\n");
            if (arg.author.Length > 0)
            {
                WriteText(fs, "<dc:creator opf:file-as=\"");
                WriteText(fs, arg.author);
                WriteText(fs, "\" opf:role=\"aut\">");
                WriteText(fs, arg.author);
                WriteText(fs, "</dc:creator>\n");
            }
            WriteText(fs, "<dc:language>ja</dc:language>\n</metadata>\n<manifest>\n");
            WriteText(fs, "<item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\" />\n");
            backgroundWorker.ReportProgress(100, 100);

            backgroundWorker.ReportProgress(-2);    // 画像変換
            for (int i = 0; i < arg.pages.Count; ++i)
            {
                if (backgroundWorker.CancellationPending)
                {
                    fs.Close();
                    e.Cancel = true;
                    return;
                }

                Image src;
                Image dst;
                arg.pages[i].GenerateImages(out src, arg.width, arg.height, out dst);
                src.Dispose();

                string id = i.ToString("d4");
                string file = String.Format(id + ".png");
                string full = Path.Combine(contents, "data", file);
                dst.Save(full, ImageFormat.Png);
                dst.Dispose();

                WriteText(fs, "<item id=\"" + id + "\" href=\"data/" + file + "\" media-type=\"image/png\" fallback=\"" + id + "f\"/>\n");
                WriteText(fs, "<item id=\"" + id + "f\" href=\"data/" + id + "f.xhtml\" media-type=\"application/xhtml+xml\"/>\n");

                FileStream xhtml = File.OpenWrite(Path.Combine(contents, "data", id + "f.xhtml"));
                WriteText(xhtml, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"\n\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"ja\" lang=\"ja\">\n<head>\n<title>-</title>\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/>\n</head>\n<body>\n<img src=\"./");
                WriteText(xhtml, file);
                WriteText(xhtml, "\" />\n</body>\n</html>\n");
                xhtml.Close();

                backgroundWorker.ReportProgress(i, arg.pages.Count);
            }

            backgroundWorker.ReportProgress(-3);    // 目次生成
            WriteText(fs, "</manifest>\n<spine toc=\"ncx\" page-progression-direction=\"rtl\">\n");
            for (int i = 0; i < arg.pages.Count; ++i)
            {
                if (backgroundWorker.CancellationPending)
                {
                    fs.Close();
                    e.Cancel = true;
                    return;
                }

                string id = i.ToString("d4");
                WriteText(fs, "<itemref idref=\"" + id + "f\"/>\n");

                backgroundWorker.ReportProgress(i, arg.pages.Count * 2);
            }
            WriteText(fs, "</spine>\n</package>\n");
            fs.Close();

            fs = File.OpenWrite(Path.Combine(contents, "toc.ncx"));
            WriteText(fs, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE ncx PUBLIC \"-//NISO//DTD ncx 2005-1//EN\" \"http://www.daisy.org/z3986/2005/ncx-2005-1.dtd\">\n<ncx version=\"2005-1\" xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" xml:lang=\"ja\">\n<head>\n<meta name=\"dtb:uid\" content=\"");
            WriteText(fs, bookid);
            WriteText(fs, "\"/>\n<meta name=\"dtb:depth\" content=\"1\"/>\n<meta name=\"dtb:totalPageCount\" content=\"0\"/>\n<meta name=\"dtb:maxPageNumber\" content=\"0\"/>\n</head>\n<docTitle><text>");
            WriteText(fs, arg.title);
            WriteText(fs, "</text></docTitle>\n<navMap>\n");
            for (int i = 0, idx = 0; i < arg.pages.Count; ++i)
            {
                if (backgroundWorker.CancellationPending)
                {
                    fs.Close();
                    e.Cancel = true;
                    return;
                }

                string id = i.ToString("d4");
                if (!String.IsNullOrEmpty(arg.pages[i].Index))
                {
                    WriteText(fs, "<navPoint id=\"" + id + "f\" playOrder=\"" + (++idx).ToString() + "\">\n");
                    WriteText(fs, "<navLabel><text>" + arg.pages[i].Index + "</text></navLabel>\n");
                    WriteText(fs, "<content src=\"data/" + id + "f.xhtml\"/>\n");
                    WriteText(fs, "</navPoint>\n");
                }

                backgroundWorker.ReportProgress(i + arg.pages.Count, arg.pages.Count * 2);
            }
            WriteText(fs, "</navMap>\n</ncx>\n");
            fs.Close();

            backgroundWorker.ReportProgress(-4);    // zip生成
            Zip zip = new Zip(arg.path);
            zip.CopyFrom(mime);
            backgroundWorker.ReportProgress(10, 100);
            zip.CopyFrom(meta);
            backgroundWorker.ReportProgress(30, 100);
            zip.CopyFrom(contents);
            backgroundWorker.ReportProgress(80, 100);
            zip.Close();
            backgroundWorker.ReportProgress(90, 100);

            try
            {
                Directory.Delete(tmpdir, true);
            }
            catch
            {
            }
            backgroundWorker.ReportProgress(100, 100);

            e.Result = true;
        }

        private static void WriteText(Stream st, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            st.Write(bytes, 0, bytes.Length);
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case -1:
                    totalProgress.Value = 0;
                    label.Text = "メタデータを生成しています...";
                    break;
                case -2:
                    totalProgress.Value = 0;
                    label.Text = "画像を変換しています...";
                    break;
                case -3:
                    totalProgress.Value = 0;
                    label.Text = "目次を生成しています...";
                    break;
                case -4:
                    totalProgress.Value = 0;
                    label.Text = "ePubファイルを生成しています...";
                    break;
                default:
                    totalProgress.Maximum = (int)e.UserState;
                    totalProgress.Value = e.ProgressPercentage;
                    break;
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.DialogResult = DialogResult.Cancel;
            }
            else if (e.Error != null)
            {
                this.DialogResult = DialogResult.Abort;
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
            this.Close();
        }
    }
}
