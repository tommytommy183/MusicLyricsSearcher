using System.Net.Http;
using System.Text.Json;
using System.Runtime.InteropServices;
using musicLine.Models;

namespace musicLine
{
    public partial class Form1 : Form
    {
        private Song currentSong;
        private int currentLineIndex = 0;
        private static readonly HttpClient httpClient = new HttpClient();

        // 用於拖曳視窗的 Windows API
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public Form1()
        {
            InitializeComponent();
            var iconPath = FindIconInProjectFolder("musicLine", "icon_result.ico");
            if (!string.IsNullOrEmpty(iconPath))
            {
                this.Icon = new Icon(iconPath);
            }
            // 設定 HttpClient
            httpClient.DefaultRequestHeaders.Add("User-Agent", "MusicLineApp/1.0");
            
            // 設定圓角
            UpdateRoundedCorners();
            
            // 視窗大小改變時更新圓角
            this.Resize += (s, e) => UpdateRoundedCorners();

            this.KeyPreview = true;  // 讓 Form 優先接收鍵盤事件
            this.KeyDown += Form1_KeyDown;  // 註冊鍵盤事件
        }

        // 創建圓角視窗
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        // 更新圓角效果
        private void UpdateRoundedCorners()
        {
            // 設定圓角半徑（數字越大越圓）
            int cornerRadius = 30;
            this.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, Width, Height, cornerRadius, cornerRadius)
            );
        }

        // 視窗拖曳功能
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        // 右鍵選單 - 關閉
        private void 關閉ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // 右鍵選單 - 最小化
        private void 最小化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSearch_Click(sender, e);
                e.SuppressKeyPress = true;
            }
        }

        private void SwitchToSearchMode()
        {
            // 顯示搜尋元件
            txtSongSearch.Visible = true;
            txtArtistSearch.Visible = true;
            btnSearch.Visible = true;
            
            // 隱藏歌詞元件
            lblSongTitle.Visible = false;
            lblLyricLine.Visible = false;
            lblLineNumber.Visible = false;
            btnPrevious.Visible = false;
            btnNext.Visible = false;
            btnBackToSearch.Visible = false;
            listBoxResults.Visible = true;
        }

        private void SwitchToLyricsMode()
        {
            // 隱藏搜尋元件
            txtSongSearch.Visible = false;
            txtArtistSearch.Visible = false;
            btnSearch.Visible = false;
            listBoxResults.Visible = false;
            
            // 顯示歌詞元件
            lblSongTitle.Visible = true;
            lblLyricLine.Visible = true;
            lblLineNumber.Visible = true;
            btnPrevious.Visible = true;
            btnNext.Visible = true;
            btnBackToSearch.Visible = true;
        }

        private void listBoxResults_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxResults.SelectedIndex >= 0)
            {
                var results = listBoxResults.Tag as List<Song>;
                if (results != null && listBoxResults.SelectedIndex < results.Count)
                {
                    var selectedSong = results[listBoxResults.SelectedIndex];
                    
                    // 檢查是否有歌詞
                    if (selectedSong.Lyrics == null || selectedSong.Lyrics.Count == 0)
                    {
                        MessageBox.Show("這首歌曲沒有可用的歌詞（可能是純音樂）", "提示", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    
                    LoadSong(selectedSong);
                    listBoxResults.Visible = false;
                }
            }
        }

        private void LoadSong(Song song)
        {
            if (song == null || song.Lyrics == null || song.Lyrics.Count == 0)
            {
                MessageBox.Show("這首歌曲沒有可用的歌詞", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            SwitchToLyricsMode();
            currentSong = song;
            currentLineIndex = 0;

            lblSongTitle.Text = $"🎵 {song.Title} - {song.Artist}";
            DisplayCurrentLine();

            btnPrevious.Enabled = false;
            btnNext.Enabled = song.Lyrics.Count > 1;
        }

        private void DisplayCurrentLine()
        {
            if (currentSong == null || currentSong.Lyrics == null || currentSong.Lyrics.Count == 0)
                return;

            lblLyricLine.Text = currentSong.Lyrics[currentLineIndex];
            lblLineNumber.Text = $"{currentLineIndex + 1} / {currentSong.Lyrics.Count}";

            // 更新按鈕狀態
            btnPrevious.Enabled = currentLineIndex > 0;
            btnNext.Enabled = currentLineIndex < currentSong.Lyrics.Count - 1;
        }

        // 使用 LrcLib API 搜尋歌詞
        private async Task<List<Song>> SearchLyricsByLrcLib(string trackName, string artistName)
        {
            try
            {
                string url;

                // 根據輸入決定搜尋方式
                if (!string.IsNullOrWhiteSpace(trackName))
                {
                    string encodedTrack = Uri.EscapeDataString(trackName);
                    
                    if (!string.IsNullOrWhiteSpace(artistName))
                    {
                        string encodedArtist = Uri.EscapeDataString(artistName);
                        url = $"https://lrclib.net/api/search?track_name={encodedTrack}&artist_name={encodedArtist}";
                    }
                    else
                    {
                        url = $"https://lrclib.net/api/search?track_name={encodedTrack}";
                    }
                }
                else if (!string.IsNullOrWhiteSpace(artistName))
                {
                    string encodedArtist = Uri.EscapeDataString(artistName);
                    url = $"https://lrclib.net/api/search?q={encodedArtist}";
                }
                else
                {
                    return null;
                }

                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var lrcDatas = JsonSerializer.Deserialize<List<LrcLibResponse>>(jsonResponse);

                    if (lrcDatas != null && lrcDatas.Count > 0)
                    {
                        List<Song> songs = new List<Song>();
                        
                        foreach (var lrcData in lrcDatas)
                        {
                            List<string> lyrics = new List<string>();
                            
                            // 處理歌詞（優先使用 plainLyrics）
                            if (!string.IsNullOrWhiteSpace(lrcData.plainLyrics))
                            {
                                lyrics = lrcData.plainLyrics
                                    .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                    .Where(line => !string.IsNullOrWhiteSpace(line))
                                    .Select(line => line.Trim())
                                    .ToList();
                            }

                            // 只加入有歌詞的歌曲（排除純音樂）
                            if (lyrics.Count > 0 || !lrcData.instrumental)
                            {
                                songs.Add(new Song
                                {
                                    Title = lrcData.trackName ?? "未知歌名",
                                    Artist = lrcData.artistName ?? "未知歌手",
                                    Lyrics = lyrics
                                });
                            }
                        }
                        
                        return songs.Count > 0 ? songs : null;
                    }
                }

                return null;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"網路連線錯誤：{ex.Message}", "錯誤", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"搜尋歌詞時發生錯誤：{ex.Message}", "錯誤", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        #region 按鈕事件
        private async void btnSearch_Click(object sender, EventArgs e)
        {
            string songName = txtSongSearch.Text.Trim();
            string artistName = txtArtistSearch.Text.Trim();

            if (string.IsNullOrEmpty(songName) && string.IsNullOrEmpty(artistName))
            {
                MessageBox.Show("請至少輸入歌名或歌手名稱！", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 顯示載入中
            btnSearch.Enabled = false;
            btnSearch.Text = "搜尋中...";
            listBoxResults.Visible = false;

            try
            {
                var songs = await SearchLyricsByLrcLib(songName, artistName);

                if (songs != null && songs.Count > 0)
                {
                    if (songs.Count == 1)
                    {
                        // 只有一個結果，直接載入
                        LoadSong(songs[0]);
                    }
                    else
                    {
                        // 多個結果，顯示列表
                        listBoxResults.Items.Clear();
                        foreach (var song in songs)
                        {
                            string display = $"{song.Title} - {song.Artist}";
                            if (song.Lyrics == null || song.Lyrics.Count == 0)
                            {
                                display += " (純音樂)";
                            }
                            listBoxResults.Items.Add(display);
                        }
                        listBoxResults.Tag = songs;
                        listBoxResults.Visible = true;
                    }
                }
                else
                {
                    MessageBox.Show(
                        "找不到相關歌詞！\n\n" +
                        "提示：\n" +
                        "• 可以只輸入歌名或歌手名稱\n" +
                        "• 日文歌範例：「Lemon」、「米津玄師」\n" +
                        "• 中文歌範例：「晴天」、「周杰倫」",
                        "搜尋結果",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            finally
            {
                btnSearch.Enabled = true;
                btnSearch.Text = "🔍 查詢";
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (currentSong != null && currentLineIndex > 0)
            {
                currentLineIndex--;
                DisplayCurrentLine();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currentSong != null && currentLineIndex < currentSong.Lyrics.Count - 1)
            {
                currentLineIndex++;
                DisplayCurrentLine();
            }
        }

        private void btnBackToSearch_Click(object sender, EventArgs e)
        {
            SwitchToSearchMode();
            currentSong = null;
            currentLineIndex = 0;
        }
        #endregion

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentSong != null && lblLyricLine.Visible)
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:    // ← 前一句
                        if (currentLineIndex > 0)
                        {
                            currentLineIndex--;
                            DisplayCurrentLine();
                        }
                        break;
                        
                    case Keys.Right:   // → 下一句
                        if (currentSong != null && currentLineIndex < currentSong.Lyrics.Count - 1)
                        {
                            currentLineIndex++;
                            DisplayCurrentLine();
                        }
                        break;

                    case Keys.Escape:  // ESC 回到搜尋
                        btnBackToSearch_Click(sender, e);
                        break;
                }
            }
        }

        /// <summary>
        /// 從啟動目錄往上搜尋，尋找形如 {上層}\{projectFolderName}\{iconFileName} 的檔案，找到回傳完整路徑。
        /// </summary>
        private static string? FindIconInProjectFolder(string projectFolderName, string iconFileName)
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, projectFolderName, iconFileName);
                if (File.Exists(candidate))
                    return candidate;

                dir = dir.Parent;
            }

            return null;
        }






    }
}
