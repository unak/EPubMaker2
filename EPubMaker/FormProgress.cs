using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EPubMaker
{
    /// <summary>
    /// ePub生成進捗表示ダイアログ
    /// 実際の生成処理もこちら
    /// </summary>
    public partial class FormProgress : Form
    {
        #region 内部構造体
        private struct WorkerArg
        {
            public List<Page> pages;    /// ページ
            public string title;        /// 書籍タイトル
            public string author;       /// 著者
            public int width;           /// 出力幅
            public int height;          /// 出力高さ
            public string path;         /// 出力ファイルパス
        }
        #endregion

        #region 列挙体定義
        private enum State
        {
            METADATA = -1,
            CONVERT = -2,
            TOC = -3,
            PACK = -4,
        }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// フォームコンストラクタ
        /// </summary>
        /// <param name="pages">ページ</param>
        /// <param name="title">書籍タイトル</param>
        /// <param name="author">著者</param>
        /// <param name="width">出力幅</param>
        /// <param name="height">出力高さ</param>
        /// <param name="path">出力ファイルパス</param>
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
        #endregion

        #region イベント
        #region フォーム
        /// <summary>
        /// フォームが閉じられそう
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.None)
            {
                e.Cancel = true;
                backgroundWorker.CancelAsync();
            }
        }
        #endregion

        #region ボタン
        /// <summary>
        /// キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            backgroundWorker.CancelAsync();
        }
        #endregion

        #region バックグラウンド処理
        /// <summary>
        /// バックグラウンド処理本体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerArg arg = (WorkerArg)e.Argument;

            string tmpdir = Path.Combine(Path.GetTempPath(), "EPubMaker");
            string mime = Path.Combine(tmpdir, "mimetype");
            string meta = Path.Combine(tmpdir, "META-INF");
            string contents = Path.Combine(tmpdir, "OEBPS");
            string bookid = Guid.NewGuid().ToString();

            // まずメタデータを生成する
            FileStream fs = GenerateMetadata(bookid, arg.title, arg.author, tmpdir, mime, meta, contents);

            // 画像を変換しPNGを出力する
            if (!ConvertPictures(arg.pages, arg.width, arg.height, Path.Combine(contents, "data"), fs))
            {
                fs.Close();
                e.Cancel = true;
                return;
            }

            // 目次を生成する
            if (!GenerateToc(arg.pages, bookid, arg.title, arg.author, contents, fs))
            {
                fs.Close();
                e.Cancel = true;
                return;
            }
            fs.Close();

            // ePubファイルを生成する
            PackEPub(arg.path, tmpdir, mime, meta, contents);

            e.Result = true;
        }


        /// <summary>
        /// バックグラウンド処理進捗報告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case (int)State.METADATA:
                    totalProgress.Value = 0;
                    label.Text = "メタデータを生成しています...";
                    break;
                case (int)State.CONVERT:
                    totalProgress.Value = 0;
                    label.Text = "画像を変換しています...";
                    break;
                case (int)State.TOC:
                    totalProgress.Value = 0;
                    label.Text = "目次を生成しています...";
                    break;
                case (int)State.PACK:
                    totalProgress.Value = 0;
                    label.Text = "ePubファイルを生成しています...";
                    break;
                default:
                    totalProgress.Maximum = (int)e.UserState;
                    totalProgress.Value = e.ProgressPercentage;
                    break;
            }
        }

        /// <summary>
        /// バックグラウンド処理完了報告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        #endregion
        #endregion

        #region プライベートメソッド
        /// <summary>
        /// 文字列出力
        /// </summary>
        /// <param name="st">出力先ストリーム</param>
        /// <param name="text">出力文字列</param>
        private static void WriteText(Stream st, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            st.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 文字列エスケープ(XML)
        /// </summary>
        /// <param name="str">対象文字列</param>
        /// <returns>エスケープ結果</returns>
        private static string Escape(string str)
        {
            return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        /// <summary>
        /// メタデータ生成
        /// </summary>
        /// <param name="bookid">ブックID</param>
        /// <param name="title">タイトル</param>
        /// <param name="author">著者</param>
        /// <param name="tmpdir">作業ディレクトリ</param>
        /// <param name="mimefile">MIMEファイル</param>
        /// <param name="metadir">METAディレクトリ</param>
        /// <param name="contentsdir">コンテンツディレクトリ</param>
        /// <returns>OPFファイルストリーム</returns>
        private FileStream GenerateMetadata(string bookid, string title, string author, string tmpdir, string mimefile, string metadir, string contentsdir)
        {
            backgroundWorker.ReportProgress((int)State.METADATA);

            if (Directory.Exists(tmpdir))
            {
                Directory.Delete(tmpdir, true);
            }
            Directory.CreateDirectory(tmpdir);
            backgroundWorker.ReportProgress(50, 100);

            FileStream fs = File.OpenWrite(mimefile);
            WriteText(fs, "application/epub+zip");
            fs.Close();
            backgroundWorker.ReportProgress(60, 100);

            Directory.CreateDirectory(metadir);
            fs = File.OpenWrite(Path.Combine(metadir, "container.xml"));
            WriteText(fs, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
            WriteText(fs, "<container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">\n");
            WriteText(fs, "<rootfiles>\n");
            WriteText(fs, "<rootfile media-type=\"application/oebps-package+xml\" full-path=\"OEBPS/content.opf\"/>\n");
            WriteText(fs, "</rootfiles>\n");
            WriteText(fs, "</container>\n");
            fs.Close();
            backgroundWorker.ReportProgress(70, 100);

            Directory.CreateDirectory(Path.Combine(contentsdir, "data"));
            backgroundWorker.ReportProgress(80, 100);

            fs = File.OpenWrite(Path.Combine(contentsdir, "content.opf"));
            WriteText(fs, "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n");
            WriteText(fs, "<package xmlns=\"http://www.idpf.org/2007/opf\" version=\"3.0\" unique-identifier=\"pub-id\" xml:lang=\"ja\">\n");
            WriteText(fs, "<metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\">\n");
            WriteText(fs, "<dc:identifier id=\"pub-id\">urn:uuid:" + Escape(bookid) + "</dc:identifier>\n");
            WriteText(fs, "<meta refines=\"#pub-id\" property=\"identifier-type\" scheme=\"xsd:string\">uuid</meta>\n");
            WriteText(fs, "<dc:language>ja</dc:language>\n");
            WriteText(fs, "<dc:title>" + Escape(title) + "</dc:title>\n");
            if (!string.IsNullOrEmpty(author))
            {
                WriteText(fs, "<dc:creator id=\"creator\">" + Escape(author) + "</dc:creator>\n");
                WriteText(fs, "<meta refines=\"#creator\" property=\"role\" scheme=\"marc:relators\">aut</meta>\n");
            }
            WriteText(fs, "<meta property=\"dcterms:modified\">" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "</meta>\n");
            WriteText(fs, "</metadata>\n");
            WriteText(fs, "<manifest>\n");
            WriteText(fs, "<item id=\"nav\" href=\"nav.html\" media-type=\"application/xhtml+xml\" properties=\"nav\"/>\n");
            WriteText(fs, "<item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>\n");
            backgroundWorker.ReportProgress(100, 100);

            return fs;
        }

        /// <summary>
        /// 画像変換
        /// </summary>
        /// <param name="pages">ページリスト</param>
        /// <param name="width">出力幅</param>
        /// <param name="height">出力高さ</param>
        /// <param name="datadir">出力ディレクトリ</param>
        /// <param name="opf">OPFファイルストリーム</param>
        /// <returns>正常終了?</returns>
        private bool ConvertPictures(List<Page> pages, int width, int height, string datadir, FileStream opf)
        {
            backgroundWorker.ReportProgress((int)State.CONVERT);
            for (int i = 0; i < pages.Count; ++i)
            {
                if (backgroundWorker.CancellationPending)
                {
                    return false;
                }

                Image src;
                Image dst = pages[i].GenerateImages(width, height, out src);
                src.Dispose();

                if (src != null)
                {
                    string id = i.ToString("d4");
                    string file = String.Format(id + ".png");
                    string full = Path.Combine(datadir, file);
                    dst.Save(full, ImageFormat.Png);
                    dst.Dispose();

                    WriteText(opf, "<item id=\"r" + id + "\" href=\"data/" + file + "\" media-type=\"image/png\" fallback=\"p" + id + "\"/>\n");
                    WriteText(opf, "<item id=\"p" + id + "\" href=\"data/" + id + ".html\" media-type=\"application/xhtml+xml\"/>\n");

                    FileStream xhtml = File.OpenWrite(Path.Combine(datadir, id + ".html"));
                    WriteText(xhtml, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
                    WriteText(xhtml, "<!DOCTYPE html>\n");
                    WriteText(xhtml, "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"ja\" lang=\"ja\">\n");
                    WriteText(xhtml, "<head>\n");
                    WriteText(xhtml, "<title>-</title>\n");
                    WriteText(xhtml, "</head>\n");
                    WriteText(xhtml, "<body>\n");
                    WriteText(xhtml, "<img src=\"./" + file + "\" />\n");
                    WriteText(xhtml, "</body>\n");
                    WriteText(xhtml, "</html>\n");
                    xhtml.Close();
                }
                else
                {
                    DialogResult ret = MessageBox.Show("何らかの理由でページ " + i + " の画像を生成できませんでした。\n処理を継続しますか?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (ret != DialogResult.Yes)
                    {
                        return false;
                    }
                }

                backgroundWorker.ReportProgress(i, pages.Count);
            }

            return true;
        }

        /// <summary>
        /// 目次生成
        /// </summary>
        /// <param name="pages">ページリスト</param>
        /// <param name="bookid">ブックID</param>
        /// <param name="title">タイトル</param>
        /// <param name="author">著者</param>
        /// <param name="contentsdir">出力先ディレクトリ</param>
        /// <param name="opf">OPFファイルストリーム</param>
        /// <returns>正常終了?</returns>
        private bool GenerateToc(List<Page> pages, string bookid, string title, string author, string contentsdir, FileStream opf)
        {
            backgroundWorker.ReportProgress((int)State.TOC);
            WriteText(opf, "</manifest>\n");
            WriteText(opf, "<spine toc=\"ncx\" page-progression-direction=\"rtl\">\n");
            for (int i = 0; i < pages.Count; ++i)
            {
                if (backgroundWorker.CancellationPending)
                {
                    return false;
                }

                string id = i.ToString("d4");
                WriteText(opf, "<itemref idref=\"p" + id + "\"/>\n");

                backgroundWorker.ReportProgress(i, pages.Count * 2);
            }
            WriteText(opf, "</spine>\n</package>\n");

            FileStream nav = File.OpenWrite(Path.Combine(contentsdir, "nav.html"));
            WriteText(nav, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
            WriteText(nav, "<!DOCTYPE html>\n");
            WriteText(nav, "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"ja\" lang=\"ja\">\n");
            WriteText(nav, "<head>\n");
            WriteText(nav, "<title>-</title>\n");
            WriteText(nav, "</head>\n");
            WriteText(nav, "<body>\n");
            WriteText(nav, "<nav epub:type=\"toc\" id=\"toc\"/>\n");
            WriteText(nav, "<h1>目次</h1>\n");
            WriteText(nav, "<ol>\n");
            for (int i = 0; i < pages.Count; ++i)
            {
                if (backgroundWorker.CancellationPending)
                {
                    nav.Close();
                    return false;
                }

                string id = i.ToString("d4");
                if (!String.IsNullOrEmpty(pages[i].Index))
                {
                    WriteText(nav, "<li><a href=\"data/\"" + id + ".html\">" + Escape(pages[i].Index) + "</a></li>\n");
                }

                backgroundWorker.ReportProgress(i + pages.Count, pages.Count * 2);
            }
            WriteText(nav, "</ol>\n");
            WriteText(nav, "</nav>\n");
            WriteText(nav, "</body>\n");
            WriteText(nav, "</html>\n");
            nav.Close();

            FileStream toc = File.OpenWrite(Path.Combine(contentsdir, "toc.ncx"));
            WriteText(toc, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
            WriteText(toc, "<ncx version=\"2005-1\" xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" xml:lang=\"ja\">\n");
            WriteText(toc, "<head>\n");
            WriteText(toc, "<meta name=\"dtb:uid\" content=\"urn:uuid:" + Escape(bookid) + "\"/>\n");
            WriteText(toc, "<meta name=\"dtb:depth\" content=\"1\"/>\n");
            WriteText(toc, "<meta name=\"dtb:totalPageCount\" content=\"0\"/>\n");
            WriteText(toc, "<meta name=\"dtb:maxPageNumber\" content=\"0\"/>\n");
            WriteText(toc, "</head>\n");
            WriteText(toc, "<docTitle><text>" + Escape(title) + "</text></docTitle>\n");
            WriteText(toc, "<docAuthor><text>" + Escape(author) + "</text></docAuthor>\n");
            WriteText(toc, "<navMap>\n");
            for (int i = 0, idx = 0; i < pages.Count; ++i)
            {
                if (backgroundWorker.CancellationPending)
                {
                    toc.Close();
                    return false;
                }

                string id = i.ToString("d4");
                if (!String.IsNullOrEmpty(pages[i].Index))
                {
                    WriteText(toc, "<navPoint id=\"p" + id + "\" playOrder=\"" + (++idx).ToString() + "\">\n");
                    WriteText(toc, "<navLabel><text>" + Escape(pages[i].Index) + "</text></navLabel>\n");
                    WriteText(toc, "<content src=\"data/" + id + ".html\"/>\n");
                    WriteText(toc, "</navPoint>\n");
                }

                backgroundWorker.ReportProgress(i + pages.Count, pages.Count * 2);
            }
            WriteText(toc, "</navMap>\n");
            WriteText(toc, "</ncx>\n");
            toc.Close();

            return true;
        }
        /// <summary>
        /// ePubファイル生成
        /// </summary>
        /// <param name="path">出力ePubファイル</param>
        /// <param name="tmpdir">作業ディレクトリ</param>
        /// <param name="mimefile">MIMEファイル</param>
        /// <param name="metadir">METAディレクトリ</param>
        /// <param name="contentsdir">コンテンツディレクトリ</param>
        private void PackEPub(string path, string tmpdir, string mimefile, string metadir, string contentsdir)
        {
            backgroundWorker.ReportProgress((int)State.PACK);
            Zip zip = new Zip(path);
            zip.CopyFrom(mimefile);
            backgroundWorker.ReportProgress(10, 100);
            zip.CopyFrom(metadir);
            backgroundWorker.ReportProgress(30, 100);
            zip.CopyFrom(contentsdir);
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
        }
        #endregion
    }
}
