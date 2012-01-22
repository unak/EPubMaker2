using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EPubMaker
{
    public partial class formMain : Form
    {
        private List<Page> pages;
        private bool gridChanging;

        public formMain()
        {
            InitializeComponent();

            pages = null;
            gridChanging = false;

            editWidth.Value = 480;
            editHeight.Value = 800;

            menuItemClose.Enabled = false;
            menuItemCreate.Enabled = false;
        }

        private void menuItemOpen_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (pages != null)
            {
                pages.Clear();
            }
            else
            {
                pages = new List<Page>();
            }

            DirectoryInfo dir = new DirectoryInfo(folderBrowserDialog.SelectedPath);
            foreach (FileInfo file in dir.GetFiles())
            {
                if (String.Compare(file.Extension, ".jpg", true) == 0 || String.Compare(file.Extension, ".png", true) == 0)
                {
                    Page page = new Page(file.FullName);
                    pages.Add(page);
                }
            }
            pages.Sort(delegate(Page a, Page b)
            {
                return String.Compare(a.Name, b.Name, true);
            });

            UpdatePageList();

            string name = Path.GetFileNameWithoutExtension(folderBrowserDialog.SelectedPath);
            if (name.Contains('-'))
            {
                string[] ary = name.Split("-".ToCharArray(), 2);
                editTitle.Text = ary[0];
                editAuthor.Text = ary[1];
            }
            else
            {
                editTitle.Text = name;
            }

            menuItemClose.Enabled = true;
            menuItemCreate.Enabled = true;
        }

        private void menuItemClose_Click(object sender, EventArgs e)
        {
            pages.Clear();
            pagesGrid.Rows.Clear();

            menuItemClose.Enabled = false;
            menuItemCreate.Enabled = false;
        }

        private void menuItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MenuItemCreate_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = Path.GetFileName(folderBrowserDialog.SelectedPath);
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Cursor prev = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            string tmpdir = Path.Combine(Path.GetTempPath(), "EPubMaker");
            if (Directory.Exists(tmpdir))
            {
                Directory.Delete(tmpdir, true);
            }
            Directory.CreateDirectory(tmpdir);

            string mime = Path.Combine(tmpdir, "mimetype");
            FileStream fs = File.OpenWrite(mime);
            WriteText(fs, "application/epub+zip");
            fs.Close();

            string meta = Path.Combine(tmpdir, "META-INF");
            Directory.CreateDirectory(meta);
            fs = File.OpenWrite(Path.Combine(meta, "container.xml"));
            WriteText(fs, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">\n<rootfiles>\n<rootfile media-type=\"application/oebps-package+xml\" full-path=\"OEBPS/package.opf\" />\n</rootfiles>\n</container>\n");
            fs.Close();

            string contents = Path.Combine(tmpdir, "OEBPS");
            Directory.CreateDirectory(Path.Combine(contents, "data"));

            fs = File.OpenWrite(Path.Combine(contents, "package.opf"));
            WriteText(fs, "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n<package xmlns=\"http://www.idpf.org/2007/opf\" version=\"3.0\" unique-identifier=\"BookId\" xml:lang=\"ja\">\n<metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\">\n<dc:identifier id=\"BookId\">");
            string bookid = Guid.NewGuid().ToString();
            WriteText(fs, bookid);
            WriteText(fs, "</dc:identifier>\n<dc:title>");
            WriteText(fs, editTitle.Text.Trim());
            WriteText(fs, "</dc:title>\n");
            if (editAuthor.Text.Trim().Length > 0)
            {
                WriteText(fs, "<dc:creator opf:file-as=\"");
                WriteText(fs, editAuthor.Text.Trim());
                WriteText(fs, "\" opf:role=\"aut\">");
                WriteText(fs, editAuthor.Text.Trim());
                WriteText(fs, "</dc:creator>\n");
            }
            WriteText(fs, "<dc:language>ja</dc:language>\n</metadata>\n<manifest>\n");
            WriteText(fs, "<item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\" />\n");

            for (int i = 0; i < pages.Count; ++i)
            {
                Image src;
                Image dst;
                pages[i].GenerateImages(out src, (int)editWidth.Value, (int)editHeight.Value, out dst);
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
            }

            WriteText(fs, "</manifest>\n<spine toc=\"ncx\" page-progression-direction=\"rtl\">\n");
            for (int i = 0; i < pages.Count; ++i)
            {
                string id = i.ToString("d4");
                WriteText(fs, "<itemref idref=\"" + id + "f\"/>\n");
            }
            WriteText(fs, "</spine>\n</package>\n");
            fs.Close();

            fs = File.OpenWrite(Path.Combine(contents, "toc.ncx"));
            WriteText(fs, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE ncx PUBLIC \"-//NISO//DTD ncx 2005-1//EN\" \"http://www.daisy.org/z3986/2005/ncx-2005-1.dtd\">\n<ncx version=\"2005-1\" xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" xml:lang=\"ja\">\n<head>\n<meta name=\"dtb:uid\" content=\"");
            WriteText(fs, bookid);
            WriteText(fs, "\"/>\n<meta name=\"dtb:depth\" content=\"1\"/>\n<meta name=\"dtb:totalPageCount\" content=\"0\"/>\n<meta name=\"dtb:maxPageNumber\" content=\"0\"/>\n</head>\n<docTitle><text>");
            WriteText(fs, editTitle.Text);
            WriteText(fs, "</text></docTitle>\n<navMap>\n");
            for (int i = 0, idx = 0; i < pages.Count; ++i)
            {
                string id = i.ToString("d4");
                if (!String.IsNullOrEmpty(pages[i].Index))
                {
                    WriteText(fs, "<navPoint id=\"" + id + "f\" playOrder=\"" + (++idx).ToString() + "\">\n");
                    WriteText(fs, "<navLabel><text>" + pages[i].Index + "</text></navLabel>\n");
                    WriteText(fs, "<content src=\"data/" + id + "f.xhtml\"/>\n");
                    WriteText(fs, "</navPoint>\n");
                }
            }
            WriteText(fs, "</navMap>\n</ncx>\n");
            fs.Close();

            Zip zip = new Zip(saveFileDialog.FileName);
            zip.CopyFrom(mime);
            zip.CopyFrom(meta);
            zip.CopyFrom(contents);
            zip.Close();

            try
            {
                Directory.Delete(tmpdir, true);
            }
            catch
            {
            }

            this.Cursor = prev;

            MessageBox.Show("ePubファイル生成完了", "EPubMaker", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void WriteText(Stream st, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            st.Write(bytes, 0, bytes.Length);
        }

        private void UpdatePageList()
        {
            pagesGrid.Rows.Clear();
            foreach (Page page in pages)
            {
                int idx = pagesGrid.Rows.Add(page.Name, page.Index, false);
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            if (gridChanging)
            {
                return;
            }

            gridChanging = true;
            for (int i = pagesGrid.Rows.Count - 1; i >= 0; --i)
            {
                if ((bool)pagesGrid.Rows[i].Cells[2].Value)
                {
                    pagesGrid.Rows[i].Selected = false;
                }
                else
                {
                    pagesGrid.Rows[i].Selected = true;
                }
            }
            gridChanging = false;
            pagesGrid_SelectionChanged(sender, e);
        }

        private void btnSelectOdd_Click(object sender, EventArgs e)
        {
            if (gridChanging)
            {
                return;
            }

            gridChanging = true;
            for (int i = pagesGrid.Rows.Count - 1; i >= 0; --i)
            {
                if ((bool)pagesGrid.Rows[i].Cells[2].Value)
                {
                    pagesGrid.Rows[i].Selected = false;
                }
                else
                {
                    pagesGrid.Rows[i].Selected = i % 2 == 0;
                }
            }
            gridChanging = false;
            pagesGrid_SelectionChanged(sender, e);
        }

        private void btnSelectEven_Click(object sender, EventArgs e)
        {
            if (gridChanging)
            {
                return;
            }

            gridChanging = true;
            for (int i = pagesGrid.Rows.Count - 1; i >= 0; --i)
            {
                if ((bool)pagesGrid.Rows[i].Cells[2].Value)
                {
                    pagesGrid.Rows[i].Selected = false;
                }
                else
                {
                    pagesGrid.Rows[i].Selected = i % 2 != 0;
                }
            }
            gridChanging = false;
            pagesGrid_SelectionChanged(sender, e);
        }

        private void pagesGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (gridChanging)
            {
                return;
            }

            gridChanging = true;
            int idx = -1;
            foreach (DataGridViewRow row in pagesGrid.SelectedRows)
            {
                if (/*row.Index < idx ||*/ idx < 0)
                {
                    idx = row.Index;
                }
            }
            if (idx >= 0 && idx < pages.Count)
            {
                rotateCombo.SelectedIndex = pages[idx].Rotate;
                RedrawImages(pages[idx]);
            }
            gridChanging = false;
        }

        private void rotateCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rotateCombo.SelectedIndex < 0 || gridChanging)
            {
                return;
            }

            gridChanging = true;
            int idx = -1;
            foreach (DataGridViewRow row in pagesGrid.SelectedRows)
            {
                if (row.Index < pages.Count)
                {
                    pages[row.Index].Rotate = rotateCombo.SelectedIndex;
                    if (row.Index < idx || idx < 0)
                    {
                        idx = row.Index;
                    }
                }
            }

            if (idx >= 0)
            {
                RedrawImages(pages[idx]);
            }
            gridChanging = false;
        }

        private void formatCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rotateCombo.SelectedIndex < 0 || gridChanging)
            {
                return;
            }

            gridChanging = true;
            int idx = -1;
            foreach (DataGridViewRow row in pagesGrid.SelectedRows)
            {
                if (row.Index < pages.Count)
                {
                    pages[row.Index].Format = (Page.PageFormat)formatCombo.SelectedIndex;
                    if (row.Index < idx || idx < 0)
                    {
                        idx = row.Index;
                    }
                }
            }

            if (idx >= 0)
            {
                RedrawImages(pages[idx]);
            }
            gridChanging = false;
        }

        private void srcPicture_ClientSizeChanged(object sender, EventArgs e)
        {
            if (srcPicture.ClientRectangle.Width < srcPicture.Image.Width || srcPicture.ClientRectangle.Height < srcPicture.Image.Height)
            {
                srcPicture.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                srcPicture.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }

        private void previewPicture_ClientSizeChanged(object sender, EventArgs e)
        {
            if (previewPicture.ClientRectangle.Width < previewPicture.Image.Width || previewPicture.ClientRectangle.Height < previewPicture.Image.Height)
            {
                previewPicture.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                previewPicture.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }

        private void pagesGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // 「目次」のみ扱う
            if (gridChanging || e.ColumnIndex != 1 || pages == null || e.RowIndex >= pages.Count)
            {
                return;
            }

            gridChanging = true;

            pages[e.RowIndex].Index = pagesGrid.Rows[e.RowIndex].Cells[1].Value.ToString();

            gridChanging = false;
        }

        private void RedrawImages(Page page)
        {
            Image src;
            Image preview;
            page.GenerateImages(out src, (int)editWidth.Value, (int)editHeight.Value, out preview);

            if (srcPicture.ClientRectangle.Width < src.Width || srcPicture.ClientRectangle.Height < src.Height)
            {
                srcPicture.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                srcPicture.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            srcPicture.Image = src;

            if (previewPicture.ClientRectangle.Width < preview.Width || previewPicture.ClientRectangle.Height < preview.Height)
            {
                previewPicture.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                previewPicture.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            previewPicture.Image = preview;

            formatCombo.SelectedIndex = (int)page.Format;
        }
    }
}
