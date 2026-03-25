using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace musicLine.Services
{
    public class CommonService
    {
        /// <summary>
        /// 從啟動目錄往上搜尋，尋找形如 {上層}\{projectFolderName}\{iconFileName} 的檔案，找到回傳完整路徑。
        /// </summary>
        public string? FindIconInProjectFolder(string projectFolderName, string iconFileName)
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

        public string GetSongNameFilter(string songName)
        {
            if (string.IsNullOrWhiteSpace(songName))
                return songName;

            string result = songName;

            // ========================
            // 垃圾字區（你可以自己維護）
            // ========================
            string[] noiseKeywords = new[]
            {
        "official", "video", "mv", "music video", "lyric", "lyrics",
        "ver", "version", "audio", "hd", "4k", "music",
        "【中日羅歌詞】", "official music video", "zutomayo",
        "優里", "主題歌", "劇場版", "back number", "yuuri"
        ,"tokyo mer～走る緊急救命室～南海ミッション","歌詞"
        ,"aimer","『","』","TV Anime","小林明子"
    };

            bool IsNoise(string text)
            {
                var lower = text.ToLower();
                return noiseKeywords.Any(n => lower.Contains(n));
            }

            // 1️⃣ 先清掉中括號（通常100%垃圾）
            result = Regex.Replace(result, @"\[.*?\]", "");

            // 2️⃣ 分隔符切割（🔥 主力）
            char[] separators = new[] { '-', '/', '|', '｜', '–', '—' };

            var parts = result.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                              .Select(p => p.Trim())
                              .Where(p => !string.IsNullOrEmpty(p))
                              .ToList();

            // 👉 找最像歌名的（優先非垃圾 + 含日文）
            string best = parts
                .Where(p => !IsNoise(p))
                .OrderByDescending(p => Score(p))
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(best))
                result = best;

            // 3️⃣ 括號只當 fallback（不要優先）
            var match = Regex.Match(songName, @"[\(（](.*?)[\)）]");
            if (match.Success)
            {
                var content = match.Groups[1].Value.Trim();
                if (!IsNoise(content) && Score(content) > Score(result))
                {
                    result = content;
                }
            }

            // 4️⃣ 清垃圾字
            foreach (var noise in noiseKeywords)
            {
                result = Regex.Replace(result, noise, "", RegexOptions.IgnoreCase);
            }

            // 5️⃣ 清符號
            result = result.Trim('\"', '\'', '「', '」', '『', '』', '(', ')', '（', '）');

            Debug.WriteLine(@$"修改前: {songName} ,修改後: {result}");

            return result.Trim();
        }

        // 🔥 核心：評分（判斷是不是歌名）
        private int Score(string text)
        {
            int score = 0;

            // 有日文 → 加分
            if (text.Any(c => (c >= 0x3040 && c <= 0x30FF) || (c >= 0x4E00 && c <= 0x9FFF)))
                score += 5;

            // 長度適中 → 加分
            if (text.Length >= 2 && text.Length <= 20)
                score += 3;

            // 太長（通常是描述）→ 扣分
            if (text.Length > 30)
                score -= 3;

            return score;
        }

        public string GetSingerFirstFilter(string artist)
        {
            var channelMapping = new Dictionary<string, string>
            {
                { "yoasobi", "YOASOBI" },
                { "kenshi yonezu", "米津玄師" },
                { "優里","yuuri"},
                { "zutomayo","zutomayo"},
                { "backnumber","back number"},
                { "tuki.","tuki."},
            };


            foreach (var kv in channelMapping)
            {
                if (artist.ToLower().Contains(kv.Key))
                {
                    artist = kv.Value;
                    break;
                }
            }

            return artist;
        }

        public string GetSingerSecondFilter(string artist)
        {
            var channelMapping = new Dictionary<string, string>
            {
                { "yoasobi", "yoasobi" },
                { "kenshi yonezu", "kenshi yonezu" },
                { "優里","優里"},
                { "zutomayo","ずっと真夜中でいいのに"},
                { "back number","back number"},
                { "tuki.","tuki."},

            };


            foreach (var kv in channelMapping)
            {
                if (artist.ToLower().Contains(kv.Key))
                {
                    artist = kv.Value;
                    break;
                }
            }

            return artist;
        }
    }
}
