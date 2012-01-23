using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EPubMaker
{
    public partial class FormMain : Form
    {
        private List<Page> pages;
        private bool gridChanging;
        private Page copy;
        private MouseEventArgs start;
        private MouseEventArgs end;
        private int selectedIndex;

        private Setting setting;

        public FormMain()
        {
            InitializeComponent();

            pages = null;
            gridChanging = false;
            copy = null;
            start = null;
            end = null;
            selectedIndex = -1;

            setting = Setting.Load();

            splitContainer_Panel1_ClientSizeChanged(null, null);
            splitContainer_Panel2_ClientSizeChanged(null, null);

            if (setting.Width > 0)
            {
                this.Left = setting.Left;
                this.Top = setting.Top;
                this.Width = setting.Width;
                this.Height = setting.Height;
                splitContainer.SplitterDistance = setting.SrcWidth;
            }

            editWidth.Value = setting.PageWidth;
            editHeight.Value = setting.PageHeight;

            EnabledButtonsAndMenuItems(false, false);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (setting != null)
            {
                setting.Left = this.Left;
                setting.Top = this.Top;
                setting.Width = this.Width;
                setting.Height = this.Height;
                setting.SrcWidth = splitContainer.SplitterDistance;
                setting.Save();
            }
        }

        private void FormMain_ClientSizeChanged(object sender, EventArgs e)
        {
            splitContainer.Left = pageLabel.Left;
            splitContainer.Width = pageLabel.Width;
            splitContainer.Top = pageLabel.Bottom;
            splitContainer.Height = ClientRectangle.Bottom - splitContainer.Top;
        }

        private void menuItemOpen_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(setting.PrevSrc) && Directory.Exists(setting.PrevSrc))
            {
                folderBrowserDialog.SelectedPath = setting.PrevSrc;
            }
            else
            {
                folderBrowserDialog.SelectedPath = null;
            }
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            setting.PrevSrc = folderBrowserDialog.SelectedPath;
            setting.Save();

            if (pages == null)
            {
                pages = new List<Page>();
            }
            menuItemClose_Click(null, null);

            DirectoryInfo dir = new DirectoryInfo(folderBrowserDialog.SelectedPath);
            foreach (FileInfo file in dir.GetFiles())
            {
                if (String.Compare(file.Extension, ".jpg", true) == 0 || String.Compare(file.Extension, ".png", true) == 0 || String.Compare(file.Extension, ".bmp", true) == 0)
                {
                    Page page = new Page(file.FullName);
                    pages.Add(page);
                }
            }
            if (pages.Count <= 0)
            {
                EnabledButtonsAndMenuItems(false, false);
                return;
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
                editTitle.Text = ary[0].Replace('_', ' ');
                editAuthor.Text = ary[1].Replace('_', ' ');
            }
            else
            {
                editTitle.Text = name;
            }

            EnabledButtonsAndMenuItems(true, pages.Count > 0);

            selectedIndex = pages.Count > 0 ? 0 : -1;
            if (selectedIndex >= 0)
            {
                RedrawImages(selectedIndex);
            }
        }

        private void menuItemClose_Click(object sender, EventArgs e)
        {
            pages.Clear();
            pagesGrid.Rows.Clear();

            pageLabel.Text = "";
            srcPicture.Image = null;
            previewPicture.Image = null;
            srcLabel.Text = "";
            previewLabel.Text = "";

            EnabledButtonsAndMenuItems(false, false);

            selectedIndex = -1;
        }

        private void menuItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MenuItemGenerate_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = Path.GetFileName(folderBrowserDialog.SelectedPath);
            if (!String.IsNullOrEmpty(setting.OutPath) && Directory.Exists(setting.OutPath))
            {
                saveFileDialog.InitialDirectory = setting.OutPath;
            }
            else
            {
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            setting.OutPath = Path.GetDirectoryName(saveFileDialog.FileName);
            setting.PageWidth = (int)editWidth.Value;
            setting.PageHeight = (int)editHeight.Value;
            setting.Save();

            FormProgress formProgress = new FormProgress(pages, editTitle.Text, editAuthor.Text, (int)editWidth.Value, (int)editHeight.Value, saveFileDialog.FileName);
            formProgress.ShowDialog(this);
            formProgress.Dispose();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (pagesGrid.SelectedRows.Count > 0)
            {
                copy = (Page)pages[pagesGrid.SelectedRows[0].Index].Clone();
                EnabledButtonsAndMenuItems(true, true);
            }
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            if (copy == null || pagesGrid.SelectedRows.Count <= 0)
            {
                return;
            }

            for (int i = pagesGrid.Rows.Count - 1; i >= 0; --i)
            {
                if (pagesGrid.Rows[i].Selected)
                {
                    pages[i].Locked = copy.Locked;
                    pages[i].Rotate = copy.Rotate;
                    pages[i].Format = copy.Format;
                    pages[i].ClipLeft = copy.ClipLeft;
                    pages[i].ClipTop = copy.ClipTop;
                    pages[i].ClipRight = copy.ClipRight;
                    pages[i].ClipBottom = copy.ClipBottom;
                }
            }
            RedrawImages(pagesGrid.SelectedRows[0].Index);
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
                if ((bool)pagesGrid.Rows[i].Cells[3].Value)
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
                if ((bool)pagesGrid.Rows[i].Cells[3].Value)
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
                if ((bool)pagesGrid.Rows[i].Cells[3].Value)
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

        private void btnDuplicate_Click(object sender, EventArgs e)
        {
            if (gridChanging || selectedIndex < 0)
            {
                return;
            }

            gridChanging = true;
            pages.Insert(selectedIndex + 1, (Page)pages[selectedIndex].Clone());
            UpdatePageList();
            RedrawImages(selectedIndex);
            gridChanging = false;
        }

        private void btnErase_Click(object sender, EventArgs e)
        {
            if (gridChanging || selectedIndex < 0)
            {
                return;
            }

            gridChanging = true;
            pages.RemoveAt(selectedIndex);
            if (pages.Count <= 0)
            {
                menuItemClose_Click(null, null);
            }
            else
            {
                UpdatePageList();
                if (selectedIndex >= pages.Count)
                {
                    selectedIndex = pages.Count - 1;
                }
                RedrawImages(selectedIndex);
            }
            gridChanging = false;
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (gridChanging || selectedIndex < 0)
            {
                return;
            }

            openFileDialog.InitialDirectory = setting.PrevSrc;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                setting.PrevSrc = Path.GetDirectoryName(openFileDialog.FileName);
                setting.Save();

                gridChanging = true;
                pages.Insert(selectedIndex + 1, new Page(openFileDialog.FileName));
                UpdatePageList();
                RedrawImages(selectedIndex);
                gridChanging = false;
            }
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            if (gridChanging || selectedIndex < 0)
            {
                return;
            }

            FormMove form = new FormMove();
            form.MaxValue = pages.Count;
            form.Page = selectedIndex + 1;
            if (form.ShowDialog() == DialogResult.OK && form.Page != selectedIndex + 1)
            {
                gridChanging = true;

                int move = form.Page - 1;
                if (move < selectedIndex)
                {
                    pages.Insert(move, pages[selectedIndex]);
                    pages.RemoveAt(selectedIndex + 1);
                }
                else
                {
                    pages.Insert(move + 1, pages[selectedIndex]);
                    pages.RemoveAt(selectedIndex);
                }
                selectedIndex = move;

                UpdatePageList();
                RedrawImages(selectedIndex);

                gridChanging = false;
            }
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
                EnabledButtonsAndMenuItems(true, true);
                RedrawImages(idx);

                selectedIndex = idx;
            }
            else
            {
                EnabledButtonsAndMenuItems(true, false);

                selectedIndex = -1;
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
                    pages[row.Index].Rotate = (Page.PageRotate)rotateCombo.SelectedIndex;
                    if (row.Index < idx || idx < 0)
                    {
                        idx = row.Index;
                    }
                }
            }

            if (idx >= 0)
            {
                RedrawImages(idx);
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
                RedrawImages(idx);
            }
            gridChanging = false;
        }

        private void editClipLeft_ValueChanged(object sender, EventArgs e)
        {
            if (gridChanging)
            {
                return;
            }

            gridChanging = true;
            int idx = -1;
            foreach (DataGridViewRow row in pagesGrid.SelectedRows)
            {
                if (row.Index < pages.Count)
                {
                    pages[row.Index].ClipLeft = (int)editClipLeft.Value;
                    if (row.Index < idx || idx < 0)
                    {
                        idx = row.Index;
                    }
                }
            }

            if (idx >= 0)
            {
                RedrawImages(idx);
            }
            gridChanging = false;
        }

        private void editClipTop_ValueChanged(object sender, EventArgs e)
        {
            if (gridChanging)
            {
                return;
            }

            gridChanging = true;
            int idx = -1;
            foreach (DataGridViewRow row in pagesGrid.SelectedRows)
            {
                if (row.Index < pages.Count)
                {
                    pages[row.Index].ClipTop = (int)editClipTop.Value;
                    if (row.Index < idx || idx < 0)
                    {
                        idx = row.Index;
                    }
                }
            }

            if (idx >= 0)
            {
                RedrawImages(idx);
            }
            gridChanging = false;
        }

        private void editClipRight_ValueChanged(object sender, EventArgs e)
        {
            if (gridChanging)
            {
                return;
            }

            gridChanging = true;
            int idx = -1;
            foreach (DataGridViewRow row in pagesGrid.SelectedRows)
            {
                if (row.Index < pages.Count)
                {
                    pages[row.Index].ClipRight = (int)editClipRight.Value;
                    if (row.Index < idx || idx < 0)
                    {
                        idx = row.Index;
                    }
                }
            }

            if (idx >= 0)
            {
                RedrawImages(idx);
            }
            gridChanging = false;
        }

        private void editClipBottom_ValueChanged(object sender, EventArgs e)
        {
            if (gridChanging)
            {
                return;
            }

            gridChanging = true;
            int idx = -1;
            foreach (DataGridViewRow row in pagesGrid.SelectedRows)
            {
                if (row.Index < pages.Count)
                {
                    pages[row.Index].ClipBottom = (int)editClipBottom.Value;
                    if (row.Index < idx || idx < 0)
                    {
                        idx = row.Index;
                    }
                }
            }

            if (idx >= 0)
            {
                RedrawImages(idx);
            }
            gridChanging = false;
        }

        private void splitContainer_Panel1_ClientSizeChanged(object sender, EventArgs e)
        {
            srcPicture.Width = splitContainer.Panel1.ClientSize.Width;
            srcPicture.Height = splitContainer.Panel1.ClientSize.Height - srcLabel.Height;
        }

        private void splitContainer_Panel2_ClientSizeChanged(object sender, EventArgs e)
        {
            previewPicture.Width = splitContainer.Panel2.ClientSize.Width;
            previewPicture.Height = splitContainer.Panel2.ClientSize.Height - previewLabel.Height;
        }

        private void srcPicture_ClientSizeChanged(object sender, EventArgs e)
        {
            if (srcPicture.Image != null)
            {
                int zoom;
                if (srcPicture.ClientRectangle.Width < srcPicture.Image.Width || srcPicture.ClientRectangle.Height < srcPicture.Image.Height)
                {
                    srcPicture.SizeMode = PictureBoxSizeMode.Zoom;
                    double dw = (double)srcPicture.ClientRectangle.Width / srcPicture.Image.Width;
                    double dh = (double)srcPicture.ClientRectangle.Height / srcPicture.Image.Height;
                    zoom = (int)((dw < dh ? dw : dh) * 100);
                }
                else
                {
                    srcPicture.SizeMode = PictureBoxSizeMode.CenterImage;
                    zoom = 100;
                }

                srcLabel.Text = String.Format("{0}x{1} ({2}%)", srcPicture.Image.Width, srcPicture.Image.Height, zoom);
            }
        }

        private void previewPicture_ClientSizeChanged(object sender, EventArgs e)
        {
            if (previewPicture.Image != null)
            {
                int zoom;
                if (previewPicture.ClientRectangle.Width < previewPicture.Image.Width || previewPicture.ClientRectangle.Height < previewPicture.Image.Height)
                {
                    previewPicture.SizeMode = PictureBoxSizeMode.Zoom;
                    double dw = (double)previewPicture.ClientRectangle.Width / previewPicture.Image.Width;
                    double dh = (double)previewPicture.ClientRectangle.Height / previewPicture.Image.Height;
                    zoom = (int)((dw < dh ? dw : dh) * 100);
                }
                else
                {
                    previewPicture.SizeMode = PictureBoxSizeMode.CenterImage;
                    zoom = 100;
                }

                previewLabel.Text = String.Format("{0}x{1} ({2}%)", previewPicture.Image.Width, previewPicture.Image.Height, zoom);
            }
        }

        private void pagesGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // 「目次」「ロック」のみ扱う
            if (gridChanging || (e.ColumnIndex != 2 && e.ColumnIndex != 3) || pages == null || e.RowIndex >= pages.Count)
            {
                return;
            }

            gridChanging = true;

            if (e.ColumnIndex == 2)
            {
                pages[e.RowIndex].Index = pagesGrid.Rows[e.RowIndex].Cells[2].Value.ToString();
            }
            else if (e.ColumnIndex == 3)
            {
                pages[e.RowIndex].Locked = (bool)pagesGrid.Rows[e.RowIndex].Cells[3].Value;
            }

            gridChanging = false;
        }

        private void srcPicture_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && srcPicture.Image != null)
            {
                start = e;
                end = e;
            }
        }

        private void srcPicture_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && start != null && srcPicture.Image != null)
            {
                Graphics g = srcPicture.CreateGraphics();
                Pen pen = new Pen(Color.Red, 2);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                srcPicture.Refresh();
                end = e;
                g.DrawRectangle(pen, Math.Min(start.X, end.X), Math.Min(start.Y, end.Y), Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
                g.Dispose();
            }
        }

        private void srcPicture_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && start != null && srcPicture.Image != null)
            {
                srcPicture_MouseMove(null, e);

                int left = Math.Min(start.X, end.X);
                int top = Math.Min(start.Y, end.Y);
                int width = Math.Abs(end.X - start.X);
                int height = Math.Abs(end.Y - start.Y);
                if (srcPicture.SizeMode == PictureBoxSizeMode.Zoom)
                {
                    double d = Math.Max((double)srcPicture.Image.Width / srcPicture.ClientSize.Width, (double)srcPicture.Image.Height / srcPicture.ClientSize.Height);
                    left = (int)(left * d - (srcPicture.ClientSize.Width * d - srcPicture.Image.Width) / 2);
                    top = (int)(top * d - (srcPicture.ClientSize.Height * d - srcPicture.Image.Height) / 2);
                    width = (int)(width * d);
                    height = (int)(height * d);
                }
                else
                {
                    left -= (srcPicture.ClientSize.Width - srcPicture.Image.Width) / 2;
                    top -= (srcPicture.ClientSize.Height - srcPicture.Image.Height) / 2;
                }
                if (left < 0)
                {
                    width += left;
                    left = 0;
                }
                width = Math.Min(width, srcPicture.Image.Width);
                if (top < 0)
                {
                    height += top;
                    top = 0;
                }
                height = Math.Min(height, srcPicture.Image.Height);

                start = null;
                end = null;

                gridChanging = true;
                foreach (DataGridViewRow row in pagesGrid.SelectedRows)
                {
                    if (row.Index < pages.Count)
                    {
                        pages[row.Index].ClipLeft = left * 100 / srcPicture.Image.Width;
                        pages[row.Index].ClipTop = top * 100 / srcPicture.Image.Height;
                        pages[row.Index].ClipRight = (left + width) * 100 / srcPicture.Image.Width;
                        pages[row.Index].ClipBottom = (top + height) * 100 / srcPicture.Image.Height;
                    }
                }
                RedrawImages(selectedIndex);
                gridChanging = false;
            }
        }

        private void srcPicture_Paint(object sender, PaintEventArgs e)
        {
            if (selectedIndex < 0 || srcPicture.Image == null || start != null)
            {
                return;
            }

            Page page = pages[selectedIndex];
            Pen pen = new Pen(Color.Red, 2);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            int left = srcPicture.Image.Width * page.ClipLeft / 100 - 1;
            int top = srcPicture.Image.Height * page.ClipTop / 100 - 1;
            int width = srcPicture.Image.Width * (page.ClipRight - page.ClipLeft) / 100 - 1;
            int height = srcPicture.Image.Height * (page.ClipBottom - page.ClipTop) / 100 - 1;
            if (srcPicture.SizeMode == PictureBoxSizeMode.Zoom)
            {
                double d = Math.Max((double)srcPicture.Image.Width / srcPicture.ClientSize.Width, (double)srcPicture.Image.Height / srcPicture.ClientSize.Height);
                left = (int)(left / d + (srcPicture.ClientSize.Width - srcPicture.Image.Width / d) / 2);
                top = (int)(top / d + (srcPicture.ClientSize.Height - srcPicture.Image.Height / d) / 2);
                width = (int)(width / d);
                height = (int)(height / d);
            }
            else
            {
                left += (srcPicture.ClientSize.Width - srcPicture.Image.Width) / 2;
                top += (srcPicture.ClientSize.Height - srcPicture.Image.Height) / 2;
            }
            e.Graphics.DrawRectangle(pen, left, top, width, height);
        }

        private void UpdatePageList()
        {
            pagesGrid.Rows.Clear();
            for (int i = 0; i < pages.Count; ++i)
            {
                int idx = pagesGrid.Rows.Add(i + 1, pages[i].Name, pages[i].Index, pages[i].Locked);
            }
        }

        private void RedrawImages(int idx)
        {
            Image src;
            Image preview;
            pages[idx].GenerateImages(out src, (int)editWidth.Value, (int)editHeight.Value, out preview);

            srcPicture.Image = src;
            srcPicture_ClientSizeChanged(null, null);

            previewPicture.Image = preview;
            previewPicture_ClientSizeChanged(null, null);

            pageLabel.Text = String.Format("{0} ({1}ページ)", pages[idx].Name, idx + 1);
            if (!String.IsNullOrEmpty(pages[idx].Index))
            {
                pageLabel.Text += " " + pages[idx].Index;
            }

            rotateCombo.SelectedIndex = (int)pages[idx].Rotate;
            formatCombo.SelectedIndex = (int)pages[idx].Format;

            editClipLeft.Value = pages[idx].ClipLeft;
            editClipTop.Value = pages[idx].ClipTop;
            editClipRight.Value = pages[idx].ClipRight;
            editClipBottom.Value = pages[idx].ClipBottom;

            start = null;
            end = null;
        }

        private void EnabledButtonsAndMenuItems(bool opened, bool selected)
        {
            menuItemClose.Enabled = opened;

            menuItemCopy.Enabled = selected;
            menuItemPaste.Enabled = copy != null;

            menuItemSelectAll.Enabled = opened;
            menuItemSelectOdd.Enabled = opened;
            menuItemSelectEven.Enabled = opened;

            menuItemDuplicate.Enabled = selected;
            menuItemErase.Enabled = selected;
            menuItemInsert.Enabled = selected;
            menuItemMove.Enabled = selected;

            menuItemGenerate.Enabled = opened;

            btnCopy.Enabled = selected;
            btnPaste.Enabled = copy != null;

            btnSelectAll.Enabled = opened;
            btnSelectOdd.Enabled = opened;
            btnSelectEven.Enabled = opened;

            btnDuplicate.Enabled = selected;
            btnErase.Enabled = selected;
            btnInsert.Enabled = selected;
            btnMove.Enabled = selected;

            rotateCombo.Enabled = selected;
            formatCombo.Enabled = selected;

            editClipLeft.Enabled = selected;
            editClipTop.Enabled = selected;
            editClipRight.Enabled = selected;
            editClipBottom.Enabled = selected;
        }
    }
}
