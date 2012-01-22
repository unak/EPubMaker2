namespace EPubMaker
{
    partial class formMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
            this.pagesPanel = new System.Windows.Forms.Panel();
            this.pagesGrid = new System.Windows.Forms.DataGridView();
            this.file = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.index = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.locked = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.pageSelectButtons = new System.Windows.Forms.ToolStrip();
            this.btnSelectAll = new System.Windows.Forms.ToolStripButton();
            this.btnSelectOdd = new System.Windows.Forms.ToolStripButton();
            this.btnSelectEven = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ファイルFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemClose = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.ePubEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCreate = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.formatCombo = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.editHeight = new System.Windows.Forms.NumericUpDown();
            this.editWidth = new System.Windows.Forms.NumericUpDown();
            this.editAuthor = new System.Windows.Forms.TextBox();
            this.editTitle = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.rotateCombo = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.srcPicture = new System.Windows.Forms.PictureBox();
            this.previewPicture = new System.Windows.Forms.PictureBox();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.pagesPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pagesGrid)).BeginInit();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.pageSelectButtons.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.editHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.editWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.srcPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.previewPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // pagesPanel
            // 
            this.pagesPanel.Controls.Add(this.pagesGrid);
            this.pagesPanel.Controls.Add(this.toolStripContainer1);
            this.pagesPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.pagesPanel.Location = new System.Drawing.Point(0, 26);
            this.pagesPanel.Name = "pagesPanel";
            this.pagesPanel.Size = new System.Drawing.Size(251, 479);
            this.pagesPanel.TabIndex = 0;
            // 
            // pagesGrid
            // 
            this.pagesGrid.AllowUserToAddRows = false;
            this.pagesGrid.AllowUserToDeleteRows = false;
            this.pagesGrid.AllowUserToResizeRows = false;
            this.pagesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.pagesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.file,
            this.index,
            this.locked});
            this.pagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pagesGrid.Location = new System.Drawing.Point(0, 46);
            this.pagesGrid.Name = "pagesGrid";
            this.pagesGrid.RowHeadersVisible = false;
            this.pagesGrid.RowTemplate.Height = 21;
            this.pagesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.pagesGrid.Size = new System.Drawing.Size(251, 433);
            this.pagesGrid.TabIndex = 1;
            this.pagesGrid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.pagesGrid_CellValueChanged);
            this.pagesGrid.SelectionChanged += new System.EventHandler(this.pagesGrid_SelectionChanged);
            // 
            // file
            // 
            this.file.HeaderText = "ファイル";
            this.file.Name = "file";
            this.file.ReadOnly = true;
            // 
            // index
            // 
            this.index.HeaderText = "目次";
            this.index.Name = "index";
            // 
            // locked
            // 
            this.locked.HeaderText = "ﾛｯｸ";
            this.locked.MinimumWidth = 36;
            this.locked.Name = "locked";
            this.locked.Width = 36;
            // 
            // toolStripContainer1
            // 
            this.toolStripContainer1.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.pageSelectButtons);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(251, 46);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolStripContainer1.LeftToolStripPanelVisible = false;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.RightToolStripPanelVisible = false;
            this.toolStripContainer1.Size = new System.Drawing.Size(251, 46);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            this.toolStripContainer1.TopToolStripPanelVisible = false;
            // 
            // pageSelectButtons
            // 
            this.pageSelectButtons.AllowMerge = false;
            this.pageSelectButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pageSelectButtons.Dock = System.Windows.Forms.DockStyle.None;
            this.pageSelectButtons.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSelectAll,
            this.btnSelectOdd,
            this.btnSelectEven});
            this.pageSelectButtons.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.pageSelectButtons.Location = new System.Drawing.Point(0, 0);
            this.pageSelectButtons.Name = "pageSelectButtons";
            this.pageSelectButtons.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.pageSelectButtons.Size = new System.Drawing.Size(81, 25);
            this.pageSelectButtons.TabIndex = 0;
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectAll.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectAll.Image")));
            this.btnSelectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(23, 22);
            this.btnSelectAll.Text = "全ページを選択";
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnSelectOdd
            // 
            this.btnSelectOdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectOdd.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectOdd.Image")));
            this.btnSelectOdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectOdd.Name = "btnSelectOdd";
            this.btnSelectOdd.Size = new System.Drawing.Size(23, 22);
            this.btnSelectOdd.Text = "奇数ページを選択";
            this.btnSelectOdd.Click += new System.EventHandler(this.btnSelectOdd_Click);
            // 
            // btnSelectEven
            // 
            this.btnSelectEven.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectEven.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectEven.Image")));
            this.btnSelectEven.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectEven.Name = "btnSelectEven";
            this.btnSelectEven.Size = new System.Drawing.Size(23, 22);
            this.btnSelectEven.Text = "偶数ページを選択";
            this.btnSelectEven.Click += new System.EventHandler(this.btnSelectEven_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ファイルFToolStripMenuItem,
            this.ePubEToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(902, 26);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ファイルFToolStripMenuItem
            // 
            this.ファイルFToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemOpen,
            this.toolStripSeparator1,
            this.menuItemClose,
            this.toolStripSeparator2,
            this.menuItemExit});
            this.ファイルFToolStripMenuItem.Name = "ファイルFToolStripMenuItem";
            this.ファイルFToolStripMenuItem.Size = new System.Drawing.Size(85, 22);
            this.ファイルFToolStripMenuItem.Text = "ファイル(&F)";
            // 
            // menuItemOpen
            // 
            this.menuItemOpen.Name = "menuItemOpen";
            this.menuItemOpen.Size = new System.Drawing.Size(239, 22);
            this.menuItemOpen.Text = "元データフォルダを開く...(&O)";
            this.menuItemOpen.Click += new System.EventHandler(this.menuItemOpen_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(236, 6);
            // 
            // menuItemClose
            // 
            this.menuItemClose.Name = "menuItemClose";
            this.menuItemClose.Size = new System.Drawing.Size(239, 22);
            this.menuItemClose.Text = "閉じる(&C)";
            this.menuItemClose.Click += new System.EventHandler(this.menuItemClose_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(236, 6);
            // 
            // menuItemExit
            // 
            this.menuItemExit.Name = "menuItemExit";
            this.menuItemExit.Size = new System.Drawing.Size(239, 22);
            this.menuItemExit.Text = "終了(&X)";
            this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
            // 
            // ePubEToolStripMenuItem
            // 
            this.ePubEToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemCreate});
            this.ePubEToolStripMenuItem.Name = "ePubEToolStripMenuItem";
            this.ePubEToolStripMenuItem.Size = new System.Drawing.Size(65, 22);
            this.ePubEToolStripMenuItem.Text = "EPub(&E)";
            // 
            // menuItemCreate
            // 
            this.menuItemCreate.Name = "menuItemCreate";
            this.menuItemCreate.Size = new System.Drawing.Size(130, 22);
            this.menuItemCreate.Text = "生成...(&C)";
            this.menuItemCreate.Click += new System.EventHandler(this.MenuItemCreate_Click);
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.ShowNewFolderButton = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.formatCombo);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.editHeight);
            this.panel1.Controls.Add(this.editWidth);
            this.panel1.Controls.Add(this.editAuthor);
            this.panel1.Controls.Add(this.editTitle);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.rotateCombo);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(751, 26);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(151, 479);
            this.panel1.TabIndex = 2;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 191);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 12);
            this.label9.TabIndex = 14;
            this.label9.Text = "形式";
            // 
            // formatCombo
            // 
            this.formatCombo.FormattingEnabled = true;
            this.formatCombo.Items.AddRange(new object[] {
            "フルカラー",
            "8bitグレイスケール",
            "4bitグレイスケール",
            "白黒"});
            this.formatCombo.Location = new System.Drawing.Point(48, 188);
            this.formatCombo.Name = "formatCombo";
            this.formatCombo.Size = new System.Drawing.Size(95, 20);
            this.formatCombo.TabIndex = 13;
            this.formatCombo.SelectedIndexChanged += new System.EventHandler(this.formatCombo_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label8.Location = new System.Drawing.Point(4, 172);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(57, 12);
            this.label8.TabIndex = 12;
            this.label8.Text = "出力画像";
            // 
            // editHeight
            // 
            this.editHeight.Location = new System.Drawing.Point(50, 82);
            this.editHeight.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.editHeight.Name = "editHeight";
            this.editHeight.Size = new System.Drawing.Size(92, 19);
            this.editHeight.TabIndex = 11;
            // 
            // editWidth
            // 
            this.editWidth.Location = new System.Drawing.Point(50, 62);
            this.editWidth.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.editWidth.Name = "editWidth";
            this.editWidth.Size = new System.Drawing.Size(92, 19);
            this.editWidth.TabIndex = 10;
            // 
            // editAuthor
            // 
            this.editAuthor.Location = new System.Drawing.Point(50, 42);
            this.editAuthor.Name = "editAuthor";
            this.editAuthor.Size = new System.Drawing.Size(92, 19);
            this.editAuthor.TabIndex = 9;
            // 
            // editTitle
            // 
            this.editTitle.Location = new System.Drawing.Point(50, 21);
            this.editTitle.Name = "editTitle";
            this.editTitle.Size = new System.Drawing.Size(92, 19);
            this.editTitle.TabIndex = 8;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 86);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(25, 12);
            this.label7.TabIndex = 7;
            this.label7.Text = "高さ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 66);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 12);
            this.label6.TabIndex = 6;
            this.label6.Text = "幅";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 5;
            this.label5.Text = "著者";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "タイトル";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label3.Location = new System.Drawing.Point(4, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "出力設定";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 138);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "回転";
            // 
            // rotateCombo
            // 
            this.rotateCombo.FormattingEnabled = true;
            this.rotateCombo.Items.AddRange(new object[] {
            "しない",
            "右90°",
            "180°",
            "左90°"});
            this.rotateCombo.Location = new System.Drawing.Point(48, 135);
            this.rotateCombo.Name = "rotateCombo";
            this.rotateCombo.Size = new System.Drawing.Size(95, 20);
            this.rotateCombo.TabIndex = 1;
            this.rotateCombo.SelectedIndexChanged += new System.EventHandler(this.rotateCombo_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(4, 118);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "ソース画像";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(251, 26);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.srcPicture);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.previewPicture);
            this.splitContainer1.Size = new System.Drawing.Size(500, 479);
            this.splitContainer1.SplitterDistance = 262;
            this.splitContainer1.TabIndex = 3;
            // 
            // srcPicture
            // 
            this.srcPicture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.srcPicture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.srcPicture.Location = new System.Drawing.Point(0, 0);
            this.srcPicture.Name = "srcPicture";
            this.srcPicture.Size = new System.Drawing.Size(262, 479);
            this.srcPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.srcPicture.TabIndex = 0;
            this.srcPicture.TabStop = false;
            this.srcPicture.ClientSizeChanged += new System.EventHandler(this.srcPicture_ClientSizeChanged);
            // 
            // previewPicture
            // 
            this.previewPicture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.previewPicture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewPicture.Location = new System.Drawing.Point(0, 0);
            this.previewPicture.Name = "previewPicture";
            this.previewPicture.Size = new System.Drawing.Size(234, 479);
            this.previewPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.previewPicture.TabIndex = 0;
            this.previewPicture.TabStop = false;
            this.previewPicture.ClientSizeChanged += new System.EventHandler(this.previewPicture_ClientSizeChanged);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "epub";
            this.saveFileDialog.Filter = "ePub|*.epub|すべてのファイル|*";
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(902, 505);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pagesPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "formMain";
            this.Text = "EPubMaker";
            this.pagesPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pagesGrid)).EndInit();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.pageSelectButtons.ResumeLayout(false);
            this.pageSelectButtons.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.editHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.editWidth)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.srcPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.previewPicture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pagesPanel;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip pageSelectButtons;
        private System.Windows.Forms.ToolStripButton btnSelectAll;
        private System.Windows.Forms.ToolStripButton btnSelectOdd;
        private System.Windows.Forms.ToolStripButton btnSelectEven;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ファイルFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItemOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuItemClose;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuItemExit;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.DataGridView pagesGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn file;
        private System.Windows.Forms.DataGridViewTextBoxColumn index;
        private System.Windows.Forms.DataGridViewCheckBoxColumn locked;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox srcPicture;
        private System.Windows.Forms.PictureBox previewPicture;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox rotateCombo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown editHeight;
        private System.Windows.Forms.NumericUpDown editWidth;
        private System.Windows.Forms.TextBox editAuthor;
        private System.Windows.Forms.TextBox editTitle;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox formatCombo;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ToolStripMenuItem ePubEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItemCreate;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}

