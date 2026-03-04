using musicLine.Models;

namespace musicLine.Services
{
    /// <summary>
    /// 負責處理歌詞相關的業務邏輯
    /// </summary>
    public class LyricsService
    {
        private readonly LyricsApiService _apiService;

        public LyricsService()
        {
            _apiService = new LyricsApiService();
        }

        /// <summary>
        /// 搜尋並轉換為 Song 物件列表
        /// </summary>
        public async Task<List<Song>?> SearchSongsAsync(string trackName, string artistName)
        {
            try
            {
                var lrcDatas = await _apiService.SearchLyricsAsync(trackName, artistName);

                if (lrcDatas == null || lrcDatas.Count == 0)
                    return null;

                List<Song> songs = new List<Song>();

                foreach (var lrcData in lrcDatas)
                {
                    var songLineTimes = ParseLyrics(lrcData);

                    // 只加入有歌詞的歌曲（排除純音樂）
                    if (songLineTimes.Count > 0 || !lrcData.instrumental)
                    {
                        songs.Add(new Song
                        {
                            Title = lrcData.trackName ?? "未知歌名",
                            Artist = lrcData.artistName ?? "未知歌手",
                            Duration = TimeSpan.FromSeconds(lrcData.duration),
                            SongLineTimes = songLineTimes,
                        });
                    }
                }

                return songs.Count > 0 ? songs : null;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"網路連線錯誤：{ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"搜尋歌詞時發生錯誤：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 解析歌詞資料（支援同步歌詞和純文字歌詞）
        /// </summary>
        private List<SongLineTime> ParseLyrics(LrcLibResponse lrcData)
        {
            List<SongLineTime> songLineTimes = new List<SongLineTime>();

            // 優先處理同步歌詞
            if (!string.IsNullOrWhiteSpace(lrcData.syncedLyrics))
            {
                songLineTimes = ParseSyncedLyrics(lrcData.syncedLyrics);
            }
            // 其次處理純文字歌詞
            else if (!string.IsNullOrWhiteSpace(lrcData.plainLyrics))
            {
                songLineTimes = ParsePlainLyrics(lrcData.plainLyrics);
            }

            return songLineTimes;
        }

        /// <summary>
        /// 解析同步歌詞（帶時間戳）
        /// </summary>
        private List<SongLineTime> ParseSyncedLyrics(string syncedLyrics)
        {
            List<SongLineTime> result = new List<SongLineTime>();
            
            var lines = syncedLyrics
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim());

            foreach (var line in lines)
            {
                try
                {
                    // 格式: [00:00.25] 歌詞內容
                    int closeBracketIndex = line.IndexOf("]");
                    if (closeBracketIndex > 0)
                    {
                        string lyricText = line.Substring(closeBracketIndex + 1).Trim();
                        string timeStr = line.Substring(1, closeBracketIndex - 1);
                        TimeSpan time = TimeSpan.ParseExact(timeStr, @"mm\:ss\.ff", null);
                        
                        result.Add(new SongLineTime
                        {
                            Line = lyricText,
                            Time = time
                        });
                    }
                }
                catch (FormatException)
                {
                    // 如果時間格式錯誤，跳過這一行
                    continue;
                }
            }

            return result;
        }

        /// <summary>
        /// 解析純文字歌詞（無時間戳）
        /// </summary>
        private List<SongLineTime> ParsePlainLyrics(string plainLyrics)
        {
            List<SongLineTime> result = new List<SongLineTime>();
            
            var lines = plainLyrics
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim());

            foreach (var line in lines)
            {
                result.Add(new SongLineTime
                {
                    Line = line,
                    Time = TimeSpan.Zero
                });
            }

            return result;
        }

        /// <summary>
        /// 檢查歌曲是否有時間資訊
        /// </summary>
        public bool HasTimeInfo(Song song)
        {
            return song.SongLineTimes?.Any(line => line.Time != TimeSpan.Zero) ?? false;
        }
    }
}