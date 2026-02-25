namespace musicLine
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Color gray = Color.FromArgb(255, 96, 96, 101);

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            txtSongSearch = new TextBox();
            listBoxResults = new ListBox();
            lblSongTitle = new Label();
            btnPrevious = new Button();
            btnNext = new Button();
            lblLyricLine = new Label();
            lblLineNumber = new Label();
            btnSearch = new Button();
            btnBackToSearch = new Button();
            txtArtistSearch = new TextBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            SmallestToolStripMenuItem = new ToolStripMenuItem();
            ClosedToolStripMenuItem = new ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // txtSongSearch
            // 
            txtSongSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSongSearch.BackColor = Color.FromArgb(74, 74, 80);
            txtSongSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSongSearch.Font = new Font("Microsoft JhengHei UI", 12F);
            txtSongSearch.ForeColor = Color.White;
            txtSongSearch.Location = new Point(30, 30);
            txtSongSearch.Name = "txtSongSearch";
            txtSongSearch.PlaceholderText = "🎵 請輸入歌名";
            txtSongSearch.Size = new Size(212, 33);
            txtSongSearch.TabIndex = 0;
            txtSongSearch.KeyDown += txtSearch_KeyDown;
            // 
            // listBoxResults
            // 
            listBoxResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBoxResults.BackColor = Color.FromArgb(37, 37, 38);
            listBoxResults.BorderStyle = BorderStyle.FixedSingle;
            listBoxResults.Font = new Font("Microsoft JhengHei UI", 11F);
            listBoxResults.ForeColor = gray;
            listBoxResults.FormattingEnabled = true;
            listBoxResults.ItemHeight = 23;
            listBoxResults.Location = new Point(30, 130);
            listBoxResults.Name = "listBoxResults";
            listBoxResults.Size = new Size(458, 186);
            listBoxResults.TabIndex = 1;
            listBoxResults.Visible = false;
            listBoxResults.DoubleClick += listBoxResults_DoubleClick;
            // 
            // lblSongTitle
            // 
            lblSongTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblSongTitle.Font = new Font("Microsoft JhengHei UI", 16F, FontStyle.Bold);
            lblSongTitle.ForeColor = Color.FromArgb(255, 75, 87, 146);
            lblSongTitle.Location = new Point(30, 30);
            lblSongTitle.Name = "lblSongTitle";
            lblSongTitle.Size = new Size(458, 40);
            lblSongTitle.TabIndex = 2;
            lblSongTitle.Text = "歌名 - 歌手";
            lblSongTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblSongTitle.Visible = false;
            // 
            // btnPrevious
            // 
            btnPrevious.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPrevious.BackColor = gray;
            btnPrevious.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnPrevious.FlatStyle = FlatStyle.Flat;
            btnPrevious.Font = new Font("Microsoft JhengHei UI", 11F);
            btnPrevious.Location = new Point(30, 270);
            btnPrevious.Name = "btnPrevious";
            btnPrevious.Size = new Size(120, 45);
            btnPrevious.TabIndex = 3;
            btnPrevious.Text = "◀ 前一句";
            btnPrevious.UseVisualStyleBackColor = false;
            btnPrevious.Visible = false;
            btnPrevious.Click += btnPrevious_Click;
            // 
            // btnNext
            // 
            btnNext.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnNext.BackColor = gray;
            btnNext.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnNext.FlatStyle = FlatStyle.Flat;
            btnNext.Font = new Font("Microsoft JhengHei UI", 11F);
            btnNext.Location = new Point(368, 270);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(120, 45);
            btnNext.TabIndex = 4;
            btnNext.Text = "下一句 ▶";
            btnNext.UseVisualStyleBackColor = false;
            btnNext.Visible = false;
            btnNext.Click += btnNext_Click;
            // 
            // lblLyricLine
            // 
            lblLyricLine.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblLyricLine.BackColor = gray;
            lblLyricLine.BorderStyle = BorderStyle.FixedSingle;
            lblLyricLine.Font = new Font("Microsoft JhengHei UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 136);
            lblLyricLine.ForeColor = Color.FromArgb(51, 51, 51);
            lblLyricLine.Location = new Point(30, 84);
            lblLyricLine.Name = "lblLyricLine";
            lblLyricLine.Padding = new Padding(20);
            lblLyricLine.Size = new Size(458, 174);
            lblLyricLine.TabIndex = 5;
            lblLyricLine.Text = "歌詞內容會顯示在這裡";
            lblLyricLine.TextAlign = ContentAlignment.MiddleCenter;
            lblLyricLine.Visible = false;
            // 
            // lblLineNumber
            // 
            lblLineNumber.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblLineNumber.Font = new Font("Microsoft JhengHei UI", 10F);
            lblLineNumber.ForeColor = Color.Gray;
            lblLineNumber.Location = new Point(30, 233);
            lblLineNumber.Name = "lblLineNumber";
            lblLineNumber.Size = new Size(458, 25);
            lblLineNumber.TabIndex = 6;
            lblLineNumber.Text = "1 / 10";
            lblLineNumber.TextAlign = ContentAlignment.MiddleCenter;
            lblLineNumber.Visible = false;
            // 
            // btnSearch
            // 
            btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSearch.BackColor = Color.FromArgb(70, 130, 180);
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);
            btnSearch.ForeColor = Color.White;
            btnSearch.Location = new Point(198, 84);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(120, 38);
            btnSearch.TabIndex = 7;
            btnSearch.Text = "🔍 查詢";
            btnSearch.UseVisualStyleBackColor = false;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnBackToSearch
            // 
            btnBackToSearch.Anchor = AnchorStyles.Bottom;
            btnBackToSearch.BackColor = gray;
            btnBackToSearch.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnBackToSearch.FlatStyle = FlatStyle.Flat;
            btnBackToSearch.Font = new Font("Microsoft JhengHei UI", 10F);
            btnBackToSearch.Location = new Point(198, 271);
            btnBackToSearch.Name = "btnBackToSearch";
            btnBackToSearch.Size = new Size(120, 45);
            btnBackToSearch.TabIndex = 8;
            btnBackToSearch.Text = "🔙 回到搜尋";
            btnBackToSearch.UseVisualStyleBackColor = false;
            btnBackToSearch.Visible = false;
            btnBackToSearch.Click += btnBackToSearch_Click;
            // 
            // txtArtistSearch
            // 
            txtArtistSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtArtistSearch.BackColor = Color.FromArgb(74, 74, 80);
            txtArtistSearch.Font = new Font("Microsoft JhengHei UI", 12F);
            txtArtistSearch.Location = new Point(276, 29);
            txtArtistSearch.Name = "txtArtistSearch";
            txtArtistSearch.PlaceholderText = "🎤 請輸入歌手名";
            txtArtistSearch.Size = new Size(212, 33);
            txtArtistSearch.TabIndex = 9;
            txtArtistSearch.KeyDown += txtSearch_KeyDown;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.BackColor = Color.FromArgb(45, 45, 48);
            contextMenuStrip1.Font = new Font("Microsoft JhengHei UI", 10F);
            contextMenuStrip1.ImageScalingSize = new Size(20, 20);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { SmallestToolStripMenuItem, ClosedToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.RenderMode = ToolStripRenderMode.System;
            contextMenuStrip1.Size = new Size(159, 56);
            // 
            // SmallestToolStripMenuItem
            // 
            SmallestToolStripMenuItem.ForeColor = Color.White;
            SmallestToolStripMenuItem.Name = "SmallestToolStripMenuItem";
            SmallestToolStripMenuItem.Size = new Size(158, 26);
            SmallestToolStripMenuItem.Text = "➖ 最小化";
            SmallestToolStripMenuItem.Click += 最小化ToolStripMenuItem_Click;
            // 
            // ClosedToolStripMenuItem
            // 
            ClosedToolStripMenuItem.ForeColor = Color.White;
            ClosedToolStripMenuItem.Name = "ClosedToolStripMenuItem";
            ClosedToolStripMenuItem.Size = new Size(158, 26);
            ClosedToolStripMenuItem.Text = "❌ 關閉";
            ClosedToolStripMenuItem.Click += 關閉ToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 30, 34);
            ClientSize = new Size(500, 350);
            ContextMenuStrip = contextMenuStrip1;
            Controls.Add(txtArtistSearch);
            Controls.Add(btnBackToSearch);
            Controls.Add(btnSearch);
            Controls.Add(lblLineNumber);
            Controls.Add(lblLyricLine);
            Controls.Add(btnNext);
            Controls.Add(btnPrevious);
            Controls.Add(lblSongTitle);
            Controls.Add(listBoxResults);
            Controls.Add(txtSongSearch);
            ForeColor = Color.DarkBlue;
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(500, 350);
            Name = "Form1";
            Opacity = 0.95D;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "🎵 歌詞查詢器";
            MouseDown += Form1_MouseDown;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtSongSearch;
        private ListBox listBoxResults;
        private Label lblSongTitle;
        private Button btnPrevious;
        private Button btnNext;
        private Label lblLyricLine;
        private Label lblLineNumber;
        private Button btnSearch;
        private Button btnBackToSearch;
        private TextBox txtArtistSearch;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem ClosedToolStripMenuItem;
        private ToolStripMenuItem SmallestToolStripMenuItem;
    }
}
