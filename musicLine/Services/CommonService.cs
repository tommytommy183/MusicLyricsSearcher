using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            // 常見垃圾關鍵字
            string[] noiseKeywords = new[]
            {
        "official", "video", "mv", "music video", "lyric", "lyrics",
        "ver", "version", "audio", "hd", "4k","music"
    };

            // 判斷是不是垃圾內容
            bool IsNoise(string text)
            {
                var lower = text.ToLower();
                return noiseKeywords.Any(n => lower.Contains(n));
            }

            // 1️⃣ 嘗試抓最後括號
            var match = System.Text.RegularExpressions.Regex.Match(
                result,
                @"[\(（]([^\(\)（）]*)[\)）](?!.*[\(（])"
            );

            if (match.Success)
            {
                var content = match.Groups[1].Value.Trim();

                // ⭐ 只有不是垃圾才用
                if (!IsNoise(content))
                {
                    result = content;
                }
            }

            // 2️⃣ 處理分隔符
            char[] separators = new[] { '-', '/', '|', '｜', '–', '—' };

            foreach (var sep in separators)
            {
                if (result.Contains(sep))
                {
                    var parts = result.Split(sep);
                    result = parts[parts.Length - 1].Trim();
                }
            }

            // 3️⃣ 去掉垃圾字（避免殘留）
            foreach (var noise in noiseKeywords)
            {
                result = System.Text.RegularExpressions.Regex.Replace(
                    result,
                    noise,
                    "",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }

            // 4️⃣ 清符號
            result = result.Trim('\"', '\'', '「', '」', '『', '』', '(', '(', '（', ')', '）');

            // 5️⃣ 清空白
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[\(（][^\)）]*$", "").Trim();

            return result;
        }

        public string GetSingerFirstFilter(string artist)
        {
            var channelMapping = new Dictionary<string, string>
            {
                { "yoasobi", "YOASOBI" },
                { "kenshi yonezu", "米津玄師" },
                { "優里","yuuri"},
                { "zutomayo","zutomayo"},
                { "backnumber","back number"}
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
                { "back number","back number"}
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
