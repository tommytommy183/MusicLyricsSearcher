using MeCab;
using musicLine.Models;
using musicLine.Services;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Timers;

namespace musicLine
{
    public partial class Form1 : Form
    {
        private readonly LyricsService _lyricsService;
        private readonly YoutubeService _youtubeService;
        private readonly CommonService _commonService;
        private readonly WanakanaService _wanakanaService;
        private MeCabTagger _tagger;


        private Song? currentSong;
        private int currentLineIndex = 0;
        private System.Timers.Timer lyricTimer;
        private DateTime songStartTime;
        private DateTime? songEndTime = null;
        private bool isAutoPlaying = false;
        private List<YoutubeModel> _playList = new List<YoutubeModel>();
        private int currentPlayListIndex = 0;
        private bool isReplaying = false;

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
            _tagger = MeCabTagger.Create();
            _lyricsService = new LyricsService();
            _youtubeService = new YoutubeService();
            _commonService = new CommonService();
            _wanakanaService = new WanakanaService();
            lblLyricLine.Init(_tagger, _wanakanaService.ToHiragana);

            InitializeUI();
            InitializeTimer();
        }

        private void InitializeUI()
        {
            var iconPath = _commonService.FindIconInProjectFolder("musicLine", "icon_result.ico");
            if (!string.IsNullOrEmpty(iconPath))
            {
                this.Icon = new Icon(iconPath);
            }

            UpdateRoundedCorners();
            this.Resize += (s, e) => UpdateRoundedCorners();
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        private void InitializeTimer()
        {
            lyricTimer = new System.Timers.Timer(100);
            lyricTimer.Elapsed += LyricTimer_Elapsed;
            lyricTimer.AutoReset = true;
        }

        #region Timer & Auto Play
        // Timer 事件處理
        private void LyricTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!isAutoPlaying || currentSong == null || currentSong.SongLineTimes == null)
                return;

            // 計算已經播放的時間
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - songStartTime;

            // 檢查是否需要切換到下一句
            if (currentLineIndex < currentSong.SongLineTimes.Count - 1)
            {
                var nextLine = currentSong.SongLineTimes[currentLineIndex + 1];

                // 如果有時間資訊且已到達下一句的時間
                if (nextLine.Time != TimeSpan.Zero && elapsed >= nextLine.Time)
                {
                    // 使用 Invoke 在 UI 執行緒上更新
                    this.Invoke(new Action(() =>
                    {
                        currentLineIndex++;
                        DisplayCurrentLine();
                    }));
                }
            }
            else if (songEndTime == null)
            {
                // 已經是最後一句，計算出結束時間(現在時間 + (整首歌的時間 - 最後一句的時間 := 最後一句到完全結束的時間))
                songEndTime = now + (currentSong.Duration - currentSong.SongLineTimes[currentLineIndex].Time);
            }
            else if (now >= songEndTime)
            {
                if (isReplaying)
                {
                    currentLineIndex = 0;
                    RestartAutoPlay();
                    this.Invoke(new Action(() =>
                    {
                        //因為這邊切回去時，還沒到line 0的部分所以不會回到第一句，要手動改
                        lblLyricLine.SetText(currentSong.SongLineTimes[0].Line);
                    }));
                }
                else
                {
                    songEndTime = null;

                    this.Invoke(new Action(() =>
                    {
                        PlayedPlayList();
                    }));
                }
            }
        }

        // 開始自動播放
        private void StartAutoPlay()
        {
            if (currentSong == null || currentSong.SongLineTimes == null || currentSong.SongLineTimes.Count == 0)
                return;

            // 檢查是否有時間資訊
            bool hasTimeInfo = _lyricsService.HasTimeInfo(currentSong);

            if (hasTimeInfo)
            {
                if (currentLineIndex == 0)
                {
                    songStartTime = DateTime.Now;
                }
                else
                {
                    songStartTime = DateTime.Now - currentSong.SongLineTimes[currentLineIndex].Time;
                }
                isAutoPlaying = true;
                lyricTimer.Start();
            }
        }

        // 停止自動播放
        private void StopAutoPlay()
        {
            isAutoPlaying = false;
            songEndTime = null;
            lyricTimer?.Stop();
        }

        // 重新開始自動播放（從當前位置）
        private void RestartAutoPlay()
        {
            StopAutoPlay();
            StartAutoPlay();
        }
        #endregion

        #region UI Methods
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

        private void SwitchToSearchMode()
        {
            // 停止自動播放
            StopAutoPlay();

            // 顯示搜尋元件
            txtSongSearch.Visible = true;
            txtArtistSearch.Visible = true;
            btnSearch.Visible = true;
            listBoxResults.Visible = true;
            txtYoutubeURL.Visible = true;

            // 隱藏歌詞元件
            lblSongTitle.Visible = false;
            lblLyricLine.Visible = false;
            lblLineNumber.Visible = false;
            btnPrevious.Visible = false;
            btnNext.Visible = false;
            btnBackToSearch.Visible = false;
            btnPreviousSong.Visible = false;
            btnNextSong.Visible = false;
            btnShowSongList.Visible = false;
        }

        private void SwitchToLyricsMode()
        {
            // 隱藏搜尋元件
            txtSongSearch.Visible = false;
            txtArtistSearch.Visible = false;
            btnSearch.Visible = false;
            listBoxResults.Visible = false;
            txtYoutubeURL.Visible = false;

            // 顯示歌詞元件
            lblSongTitle.Visible = true;
            lblLyricLine.Visible = true;
            lblLineNumber.Visible = true;
            btnPrevious.Visible = true;
            btnNext.Visible = true;
            btnBackToSearch.Visible = true;
            btnPreviousSong.Visible = true;
            btnNextSong.Visible = true;
            btnShowSongList.Visible = true;
            // 開始自動播放歌詞
            StartAutoPlay();
        }

        private void LoadSong(Song song)
        {
            if (song == null || song.SongLineTimes == null || song.SongLineTimes.Count == 0)
            {
                MessageBox.Show("這首歌曲沒有可用的歌詞", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            StopAutoPlay(); // 先停止之前的播放
            //第一句前加上空白，讓畫面好看一點
            song.SongLineTimes.Insert(0, new SongLineTime
            {
                Line = " ",
                Time = TimeSpan.FromSeconds(0.1)
            });
            currentSong = song;
            currentLineIndex = 0;

            lblSongTitle.Text = $"🎵 {song.Title} - {song.Artist}";

            // 檢查是否有時間資訊
            bool hasTimeInfo = _lyricsService.HasTimeInfo(song);
            if (hasTimeInfo)
            {
                lblSongTitle.Text += " ⏱️"; // 加上時鐘圖示表示有自動播放
            }

            SwitchToLyricsMode();
            DisplayCurrentLine();

            btnPrevious.Enabled = false;
            btnNext.Enabled = song.SongLineTimes.Count > 1;
        }

        private void DisplayCurrentLine()
        {
            if (currentSong == null || currentSong.SongLineTimes == null || currentSong.SongLineTimes.Count == 0)
                return;


            lblLyricLine.SetText(currentSong.SongLineTimes[currentLineIndex].Line);

            // 顯示時間資訊（如果有的話）
            string timeInfo = "";
            if (currentSong.SongLineTimes[currentLineIndex].Time != TimeSpan.Zero)
            {
                timeInfo = $" [{currentSong.SongLineTimes[currentLineIndex].Time:mm\\:ss}]";
            }

            lblLineNumber.Text = $"{currentLineIndex + 1} / {currentSong.SongLineTimes.Count}{timeInfo}";

            // 更新按鈕狀態
            btnPrevious.Enabled = currentLineIndex > 0;
            btnNext.Enabled = currentLineIndex < currentSong.SongLineTimes.Count - 1;
        }
        #endregion

        #region Event Handlers
        private async void btnSearch_Click(object sender, EventArgs e)
        {
            string songName = txtSongSearch.Text.Trim();
            string artistName = txtArtistSearch.Text.Trim();
            string youtubeUrl = txtYoutubeURL.Text.Trim();

            if (string.IsNullOrEmpty(songName) && string.IsNullOrEmpty(artistName) && string.IsNullOrEmpty(youtubeUrl))
            {
                MessageBox.Show("請至少輸入歌名或歌手名稱，或是輸入歌單網址！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 顯示載入中
            btnSearch.Enabled = false;
            btnSearch.Text = "搜尋中...";
            listBoxResults.Visible = false;

            try
            {
                //如果沒有輸入 YouTube URL，則進行歌詞搜尋；如果有輸入，則嘗試從 YouTube 取得資料
                if (string.IsNullOrEmpty(youtubeUrl))
                {
                    var songs = await _lyricsService.SearchSongsAsync(songName, artistName);

                    if (songs != null && songs.Count > 0)
                    {
                        isReplaying = true;
                        if (songs.Count == 1)
                        {
                            // 只有一個結果，直接載入
                            LoadSong(songs[0]);
                        }
                        else
                        {
                            // 多個結果，顯示列表
                            DisplaySearchResults(songs);
                        }
                    }
                    else
                    {
                        ShowNoResultsMessage();
                    }
                }
                else
                {
                    _playList = await _youtubeService.GetYoutubePlayListData(youtubeUrl);
                    await PlayedPlayList();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSearch.Enabled = true;
                btnSearch.Text = "🔍 查詢";
            }
        }

        private void DisplaySearchResults(List<Song> songs)
        {
            listBoxResults.Items.Clear();
            foreach (var song in songs)
            {
                string display = $"{song.Title} - {song.Artist}";
                if (song.SongLineTimes == null || song.SongLineTimes.Count == 0)
                {
                    display += " (純音樂)";
                }
                else if (_lyricsService.HasTimeInfo(song))
                {
                    display += " ⏱️";
                }
                listBoxResults.Items.Add(display);
            }
            listBoxResults.Tag = songs;
            listBoxResults.Visible = true;
        }

        private void ShowNoResultsMessage()
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

        private void listBoxResults_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxResults.SelectedIndex >= 0)
            {
                var results = listBoxResults.Tag as List<Song>;
                if (results != null && listBoxResults.SelectedIndex < results.Count)
                {
                    var selectedSong = results[listBoxResults.SelectedIndex];

                    // 檢查是否有歌詞
                    if (selectedSong.SongLineTimes == null || selectedSong.SongLineTimes.Count == 0)
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


        private void btnPreviousSong_Click(object sender, EventArgs e)
        {
            currentPlayListIndex = Math.Max(0, currentPlayListIndex - 2);
            PlayedPlayList();
        }

        private void btnNextSong_Click(object sender, EventArgs e)
        {
            PlayedPlayList();
        }

        private void btnShowSongList_Click(object sender, EventArgs e)
        {
            MessageBox.Show(string.Join("\n", _playList.Select((s, i) => $"{i + 1}. {s.SongName} - {s.ChannnelName}")), "播放列表");
        }
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (currentSong != null && currentLineIndex > 0)
            {
                currentLineIndex--;
                DisplayCurrentLine();
                RestartAutoPlay();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currentSong != null && currentLineIndex < currentSong.SongLineTimes.Count - 1)
            {
                currentLineIndex++;
                DisplayCurrentLine();
                RestartAutoPlay();
            }
        }

        private void btnBackToSearch_Click(object sender, EventArgs e)
        {
            SwitchToSearchMode();
            currentSong = null;
            currentLineIndex = 0;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void 關閉ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StopAutoPlay();
            Application.Exit();
        }

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

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentSong != null && lblLyricLine.Visible)
            {
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        if (currentLineIndex > 0)
                        {
                            currentLineIndex--;
                            DisplayCurrentLine();
                            RestartAutoPlay();
                        }
                        break;

                    case Keys.Right:
                        if (currentSong != null && currentLineIndex < currentSong.SongLineTimes.Count - 1)
                        {
                            currentLineIndex++;
                            DisplayCurrentLine();
                            RestartAutoPlay();
                        }
                        break;

                    case Keys.Space:
                        if (isAutoPlaying)
                        {
                            StopAutoPlay();
                        }
                        else
                        {
                            StartAutoPlay();
                        }
                        e.Handled = true;
                        break;

                    case Keys.Escape:
                        btnBackToSearch_Click(sender, e);
                        break;
                }
            }
        }
        #endregion

        private async Task PlayedPlayList()
        {
            isReplaying = false;

            if (_playList == null || currentPlayListIndex >= _playList.Count)
                return;

            var current = _playList[currentPlayListIndex];
            currentPlayListIndex++;

            var songName = _commonService.GetSongNameFilter(current.SongName);
            var singer1 = _commonService.GetSingerFirstFilter(current.ChannnelName);
            var singer2 = _commonService.GetSingerSecondFilter(current.ChannnelName);
            var hiraName = _wanakanaService.ToHiragana(songName);
            var kanaName = _wanakanaService.ToKatakana(songName);

            var searchPatterns = new List<(string name, string singer)>
    {
        (songName, singer1),
        (songName, singer2),
        (hiraName, singer1),
        (hiraName, singer2),
        (kanaName, singer1),
        (kanaName, singer2),
        (songName, "")
    };

            foreach (var (name, singer) in searchPatterns)
            {
                var songs = await _lyricsService.SearchSongsAsync(name, singer);

                if (songs?.Any() == true)
                {
                    LoadSong(songs[0]);
                    return;
                }
            }

            // 找不到就播下一首（但不要遞迴爆炸）
            await PlayedPlayList();
        }
    }
}
