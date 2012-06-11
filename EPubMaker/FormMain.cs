using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace EPubMaker
{
    /// <summary>
    /// メインフォーム
    /// </summary>
    public partial class FormMain : Form
    {
        /// <summary>
        /// プロジェクト保存データ
        /// </summary>
        [Serializable]
        public struct EPubMakerData
        {
            public string Path;
            public string Title;
            public string Author;
            public int Width;
            public int Height;
            public List<Page> Pages;
        }

        private List<Page> pages;       /// ページ
        private bool gridChanging;      /// ページ一覧変更中フラグ
        private Page copy;              /// ページ設定コピーバッファ
        private MouseEventArgs start;   /// 画像範囲選択開始位置
        private MouseEventArgs end;     /// 画像範囲選択終了位置
        private int selectedIndex;      /// 選択中ページインデックス
        private bool saved;             /// プロジェクト保存済み?

        private Setting setting;        /// アプリ設定

        /// <summary>
        /// フォームコンストラクタ
        /// </summary>
        public FormMain()
        {
            InitializeComponent();

            pages = null;
            gridChanging = false;
            copy = null;
            start = null;
            end = null;
            selectedIndex = -1;
            saved = true;

            setting = Setting.Load();

            splitContainer_Panel1_ClientSizeChanged(null, null);
            splitContainer_Panel2_ClientSizeChanged(null, null);

            editWidth.Value = setting.PageWidth;
            editHeight.Value = setting.PageHeight;

            EnabledButtonsAndMenuItems(false, false);
        }

        /// <summary>
        /// フォームがロードされた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_Load(object sender, EventArgs e)
        {
            if (setting.Width > 0)
            {
                this.Left = setting.Left;
                this.Top = setting.Top;
                this.Width = setting.Width;
                this.Height = setting.Height;
                splitContainer.SplitterDistance = setting.Distance;
            }
        }

        /// <summary>
        /// フォームが閉じられそう
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!saved)
            {
                DialogResult result = MessageBox.Show("プロジェクトが保存されていません。保存しますか?", Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.OK:
                        menuItemSave_Click(null, null);
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        e.Cancel = true;
                        return;
                }
            }

            if (setting != null)
            {
                setting.Left = this.Left;
                setting.Top = this.Top;
                setting.Width = this.Width;
                setting.Height = this.Height;
                setting.Distance = splitContainer.SplitterDistance;
                setting.Save();
            }
        }

        /// <summary>
        /// フォームのクライアント領域のサイズが変わった
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_ClientSizeChanged(object sender, EventArgs e)
        {
            splitContainer.Left = pageLabel.Left;
            splitContainer.Width = pageLabel.Width;
            splitContainer.Top = pageLabel.Bottom;
            splitContainer.Height = ClientRectangle.Bottom - splitContainer.Top;
        }

        /// <summary>
        /// プロジェクトオープン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemOpenProject_Click(object sender, EventArgs e)
        {
            openFileDialog.DefaultExt = "epmd";
            openFileDialog.Filter = "EPubMakerプロジェクトデータ|*.epmd";
            if (!string.IsNullOrEmpty(folderBrowserDialog.SelectedPath) && Directory.Exists(folderBrowserDialog.SelectedPath))
            {
                openFileDialog.InitialDirectory = folderBrowserDialog.SelectedPath;
            }
            else if (!String.IsNullOrEmpty(setting.PrevSrc) && Directory.Exists(setting.PrevSrc))
            {
                openFileDialog.InitialDirectory = setting.PrevSrc;
            }
            else
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            // 読み込み
            XmlSerializer serializer = new XmlSerializer(typeof(EPubMakerData));
            StreamReader reader = new StreamReader(openFileDialog.FileName, new UTF8Encoding());
            EPubMakerData data = (EPubMakerData)serializer.Deserialize(reader);
            reader.Close();

            folderBrowserDialog.SelectedPath = data.Path;
            editTitle.Text = data.Title;
            editAuthor.Text = data.Author;
            editWidth.Value = data.Width;
            editHeight.Value = data.Height;
            pages = data.Pages;
            saved = true;

            SetupPagesGrid();
            EnabledButtonsAndMenuItems(true, pages.Count > 0);

            selectedIndex = pages.Count > 0 ? 0 : -1;
            if (selectedIndex >= 0)
            {
                DrawImages(selectedIndex);
            }
        }

        /// <summary>
        /// 元データフォルダオープン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            SetupPagesGrid();

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
                DrawImages(selectedIndex);
            }

            saved = false;
        }

        /// <summary>
        /// 元データフォルダクローズ(別にフォルダを閉じるわけじゃないけど)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemClose_Click(object sender, EventArgs e)
        {
            if (!saved)
            {
                DialogResult result = MessageBox.Show("プロジェクトが保存されていません。保存しますか?", Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.OK:
                        menuItemSave_Click(null, null);
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        return;
                }
            }

            pages.Clear();
            pagesGrid.Rows.Clear();

            pageLabel.Text = "";
            srcPicture.Image = null;
            previewPicture.Image = null;
            srcLabel.Text = "";
            previewLabel.Text = "";

            EnabledButtonsAndMenuItems(false, false);

            selectedIndex = -1;
            saved = true;
        }

        /// <summary>
        /// プロジェクト保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemSave_Click(object sender, EventArgs e)
        {
            saveFileDialog.DefaultExt = "epmd";
            saveFileDialog.Filter = "EPubMakerプロジェクトデータ|*.epmd";
            saveFileDialog.FileName = Path.GetFileName(folderBrowserDialog.SelectedPath);
            saveFileDialog.InitialDirectory = folderBrowserDialog.SelectedPath;
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            // 保存
            EPubMakerData data = new EPubMakerData();
            data.Path = folderBrowserDialog.SelectedPath;
            data.Title = editTitle.Text;
            data.Author = editAuthor.Text;
            data.Width = (int)editWidth.Value;
            data.Height = (int)editHeight.Value;
            data.Pages = pages;
            XmlSerializer serializer = new XmlSerializer(typeof(EPubMakerData));
            StreamWriter writer = new StreamWriter(saveFileDialog.FileName, false, new UTF8Encoding());
            serializer.Serialize(writer, data);
            writer.Close();

            saved = true;
        }

        /// <summary>
        /// 終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// ePub生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemGenerate_Click(object sender, EventArgs e)
        {
            saveFileDialog.DefaultExt = "epub";
            saveFileDialog.Filter = "ePub|*.epub|すべてのファイル|*";
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

        /// <summary>
        /// ページ設定コピー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (pagesGrid.SelectedRows.Count > 0)
            {
                copy = (Page)pages[pagesGrid.SelectedRows[0].Index].Clone();
                EnabledButtonsAndMenuItems(true, true);
            }
        }

        /// <summary>
        /// ページ設定貼り付け
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    pages[i].Bold = copy.Bold;
                    pages[i].Contrast = copy.Contrast;
                }
            }
            DrawImages(pagesGrid.SelectedRows[0].Index);
        }

        /// <summary>
        /// 全ページ選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            SelectPages(delegate(int i) { return true; });
        }

        /// <summary>
        /// 奇数ページ選択(ページ番号が奇数ということはインデックスは偶数)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectOdd_Click(object sender, EventArgs e)
        {
            SelectPages(delegate(int i) { return i % 2 == 0; });
        }

        /// <summary>
        /// 偶数ページ選択(ページ番号が奇数ということはインデックスは奇数)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectEven_Click(object sender, EventArgs e)
        {
            SelectPages(delegate(int i) { return i % 2 == 1; });
        }

        /// <summary>
        /// ページ複製
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDuplicate_Click(object sender, EventArgs e)
        {
            if (gridChanging || selectedIndex < 0)
            {
                return;
            }

            gridChanging = true;
            pages.Insert(selectedIndex + 1, (Page)pages[selectedIndex].Clone());

            SetupPagesGrid();
            pagesGrid.ClearSelection();
            pagesGrid.CurrentCell = pagesGrid.Rows[selectedIndex].Cells[2];

            gridChanging = false;
            pagesGrid_SelectionChanged(null, null);

            saved = false;
        }

        /// <summary>
        /// ページ削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                SetupPagesGrid();
                if (selectedIndex >= pages.Count)
                {
                    selectedIndex = pages.Count - 1;
                }
                pagesGrid.ClearSelection();
                pagesGrid.CurrentCell = pagesGrid.Rows[selectedIndex].Cells[2];
            }
            gridChanging = false;
            pagesGrid_SelectionChanged(null, null);

            saved = false;
        }

        /// <summary>
        /// ページ追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (gridChanging || selectedIndex < 0)
            {
                return;
            }

            openFileDialog.DefaultExt = "";
            openFileDialog.Filter = "画像ファイル|*.jpg;*.png;*.bmp|すべてのファイル|*";
            openFileDialog.InitialDirectory = setting.PrevSrc;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                setting.PrevSrc = Path.GetDirectoryName(openFileDialog.FileName);
                setting.Save();

                gridChanging = true;
                pages.Insert(selectedIndex + 1, new Page(openFileDialog.FileName));

                SetupPagesGrid();
                pagesGrid.ClearSelection();
                pagesGrid.CurrentCell = pagesGrid.Rows[selectedIndex].Cells[2];

                gridChanging = false;
                pagesGrid_SelectionChanged(null, null);

                saved = false;
            }
        }

        /// <summary>
        /// ページ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                SetupPagesGrid();
                pagesGrid.ClearSelection();
                pagesGrid.CurrentCell = pagesGrid.Rows[selectedIndex].Cells[2];

                gridChanging = false;
                pagesGrid_SelectionChanged(null, null);

                saved = false;
            }
        }

        /// <summary>
        /// 選択ページが変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                DrawImages(idx);

                selectedIndex = idx;
            }
            else
            {
                EnabledButtonsAndMenuItems(true, false);

                selectedIndex = -1;
            }
            gridChanging = false;
        }

        /// <summary>
        /// ページ一覧のセルの値が変わった
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                if (pagesGrid.Rows[e.RowIndex].Cells[2].Value == null)
                {
                    pages[e.RowIndex].Index = "";
                }
                else
                {
                    pages[e.RowIndex].Index = pagesGrid.Rows[e.RowIndex].Cells[2].Value.ToString();
                }
            }
            else if (e.ColumnIndex == 3)
            {
                pages[e.RowIndex].Locked = (bool)pagesGrid.Rows[e.RowIndex].Cells[3].Value;
            }

            gridChanging = false;
            
            saved = false;
        }

        /// <summary>
        /// 回転コンボボックスが変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rotateCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangePageSettings(delegate(int idx) { pages[idx].Rotate = (Page.PageRotate)rotateCombo.SelectedIndex; });
        }

        /// <summary>
        /// 形式コンボボックスが変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void formatCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangePageSettings(delegate(int idx) { pages[idx].Format = (Page.PageFormat)formatCombo.SelectedIndex; });

            saved = false;
        }

        /// <summary>
        /// 切り抜き(左)が変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editClipLeft_ValueChanged(object sender, EventArgs e)
        {
            ChangePageSettings(delegate(int idx) { pages[idx].ClipLeft = (int)editClipLeft.Value; });

            saved = false;
        }

        /// <summary>
        /// 切り抜き(上)が変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editClipTop_ValueChanged(object sender, EventArgs e)
        {
            ChangePageSettings(delegate(int idx) { pages[idx].ClipTop = (int)editClipTop.Value; });
            
            saved = false;
        }

        /// <summary>
        /// 切り抜き(右)が変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editClipRight_ValueChanged(object sender, EventArgs e)
        {
            ChangePageSettings(delegate(int idx) { pages[idx].ClipRight = (int)editClipRight.Value; });

            saved = false;
        }

        /// <summary>
        /// 切り抜き(下)が変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editClipBottom_ValueChanged(object sender, EventArgs e)
        {
            ChangePageSettings(delegate(int idx) { pages[idx].ClipBottom = (int)editClipBottom.Value; });

            saved = false;
        }

        /// <summary>
        /// 太字化率が変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editBold_ValueChanged(object sender, EventArgs e)
        {
            ChangePageSettings(delegate(int idx) { pages[idx].Bold = (float)editBold.Value; });

            saved = false;
        }

        /// <summary>
        /// コントラストが変更された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editContrast_ValueChanged(object sender, EventArgs e)
        {
            ChangePageSettings(delegate(int idx) { pages[idx].Contrast = (float)editContrast.Value; });

            saved = false;
        }

        /// <summary>
        /// 画像表示領域左ペインのサイズが変わった
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitContainer_Panel1_ClientSizeChanged(object sender, EventArgs e)
        {
            srcPicture.Width = splitContainer.Panel1.ClientSize.Width;
            srcPicture.Height = splitContainer.Panel1.ClientSize.Height - srcLabel.Height;
        }

        /// <summary>
        /// 画像表示領域右ペインのサイズが変わった
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitContainer_Panel2_ClientSizeChanged(object sender, EventArgs e)
        {
            previewPicture.Width = splitContainer.Panel2.ClientSize.Width;
            previewPicture.Height = splitContainer.Panel2.ClientSize.Height - previewLabel.Height;
        }

        /// <summary>
        /// 元画像表示領域のサイズが変わった
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void srcPicture_ClientSizeChanged(object sender, EventArgs e)
        {
            PictureSizeChanged(srcPicture, srcLabel);
        }

        /// <summary>
        /// プレビュー画像表示領域のサイズが変わった
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void previewPicture_ClientSizeChanged(object sender, EventArgs e)
        {
            PictureSizeChanged(previewPicture, previewLabel);
        }

        /// <summary>
        /// 元画像表示領域でマウスボタンが押された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void srcPicture_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && srcPicture.Image != null)
            {
                start = e;
                end = e;
            }
        }

        /// <summary>
        /// 元画像表示領域でマウスが動いた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 元画像表示領域でマウスボタンが離された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                DrawImages(selectedIndex);
                gridChanging = false;

                saved = false;
            }
        }

        /// <summary>
        /// 元画像表示領域描画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            int width = srcPicture.Image.Width * (page.ClipRight - page.ClipLeft) / 100;
            int height = srcPicture.Image.Height * (page.ClipBottom - page.ClipTop) / 100;
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

        /// <summary>
        /// ページ一覧の中身設定
        /// </summary>
        private void SetupPagesGrid()
        {
            pagesGrid.Rows.Clear();
            for (int i = 0; i < pages.Count; ++i)
            {
                int idx = pagesGrid.Rows.Add(i + 1, pages[i].Name, pages[i].Index, pages[i].Locked);
            }
        }

        private delegate bool SelectCond(int idx);  // SelectPages用デリゲート(引数はページインデックス)

        /// <summary>
        /// ページ選択
        /// </summary>
        /// <param name="cond">ページ選択条件指定デリゲート</param>
        private void SelectPages(SelectCond cond)
        {
            if (gridChanging)
            {
                return;
            }

            gridChanging = true;
            for (int i = pagesGrid.Rows.Count - 1; i >= 0; --i)
            {
                if ((bool)pagesGrid.Rows[i].Cells[3].Value || !cond(i))
                {
                    pagesGrid.Rows[i].Selected = false;
                }
                else
                {
                    pagesGrid.Rows[i].Selected = true;
                    //pagesGrid.CurrentCell = pagesGrid.Rows[i].Cells[2];
                }
            }
            gridChanging = false;
            pagesGrid_SelectionChanged(null, null);
        }

        private delegate void SetPageSettings(int idx); // ChangePageSettings用デリゲート(引数はページインデックス)

        /// <summary>
        /// ページ設定変更
        /// </summary>
        /// <param name="setter">設定変更デリゲート</param>
        private void ChangePageSettings(SetPageSettings setter)
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
                    setter(row.Index);
                    if (row.Index < idx || idx < 0)
                    {
                        idx = row.Index;
                    }
                }
            }

            if (idx >= 0)
            {
                DrawImages(idx);
            }
            gridChanging = false;
        }

        /// <summary>
        /// 画像表示領域サイズ変更
        /// </summary>
        /// <param name="box">対象画像表示領域</param>
        /// <param name="label">内容表示用ラベル</param>
        private void PictureSizeChanged(PictureBox box, Label label)
        {
            if (box.Image != null)
            {
                int zoom;
                if (box.ClientRectangle.Width < box.Image.Width || box.ClientRectangle.Height < box.Image.Height)
                {
                    box.SizeMode = PictureBoxSizeMode.Zoom;
                    double dw = (double)box.ClientRectangle.Width / box.Image.Width;
                    double dh = (double)box.ClientRectangle.Height / box.Image.Height;
                    zoom = (int)((dw < dh ? dw : dh) * 100);
                }
                else
                {
                    box.SizeMode = PictureBoxSizeMode.CenterImage;
                    zoom = 100;
                }

                label.Text = String.Format("{0}x{1} ({2}%)", box.Image.Width, box.Image.Height, zoom);
            }
        }

        /// <summary>
        /// 元画像およびプレビュー画像表示
        /// </summary>
        /// <param name="idx">対象ページのインデックス</param>
        private void DrawImages(int idx)
        {
            Image src;
            Image preview = pages[idx].GenerateImages((int)editWidth.Value, (int)editHeight.Value, out src);

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

            editClipLeft.Value = Math.Min(Math.Max(0, pages[idx].ClipLeft), 100);
            editClipTop.Value = Math.Min(Math.Max(0, pages[idx].ClipTop), 100);
            editClipRight.Value = Math.Max(Math.Min(100, pages[idx].ClipRight), 0);
            editClipBottom.Value = Math.Max(Math.Min(100, pages[idx].ClipBottom), 0);

            editBold.Value = (decimal)pages[idx].Bold;
            editContrast.Value = (decimal)pages[idx].Contrast;

            start = null;
            end = null;
        }

        /// <summary>
        /// ボタン等のコントロールの有効・無効状態設定
        /// </summary>
        /// <param name="opened">元データオープン済み？</param>
        /// <param name="selected">選択ページがある？</param>
        private void EnabledButtonsAndMenuItems(bool opened, bool selected)
        {
            menuItemClose.Enabled = opened;
            menuItemSave.Enabled = opened;

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

            editBold.Enabled = selected;
            editContrast.Enabled = selected;
        }
    }
}
