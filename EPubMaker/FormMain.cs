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
    public partial class FormMain : Form
    {
        private List<Page> pages;
        private bool gridChanging;

        public FormMain()
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
                if (String.Compare(file.Extension, ".jpg", true) == 0 || String.Compare(file.Extension, ".png", true) == 0 || String.Compare(file.Extension, ".bmp", true) == 0)
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

            FormProgress formProgress = new FormProgress(pages, editTitle.Text, editAuthor.Text, (int)editWidth.Value, (int)editHeight.Value, saveFileDialog.FileName);
            formProgress.ShowDialog(this);
            formProgress.Dispose();
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
