using System.Net.Http;
using System.Text.Json;
using musicLine.Models;

namespace musicLine.Services
{
    /// <summary>
    /// 負責與 LrcLib API 進行通訊
    /// </summary>
    public class LyricsApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public LyricsApiService()
        {
            if (_httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "MusicLineApp/1.0");
            }
        }

        /// <summary>
        /// 從 LrcLib API 搜尋歌詞資料
        /// </summary>
        public async Task<List<LrcLibResponse>?> SearchLyricsAsync(string trackName, string artistName)
        {
            try
            {
                string url = BuildSearchUrl(trackName, artistName);
                
                if (string.IsNullOrEmpty(url))
                    return null;

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<LrcLibResponse>>(jsonResponse);
                }

                return null;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string BuildSearchUrl(string trackName, string artistName)
        {
            if (!string.IsNullOrWhiteSpace(trackName))
            {
                string encodedTrack = Uri.EscapeDataString(trackName);
                
                if (!string.IsNullOrWhiteSpace(artistName))
                {
                    string encodedArtist = Uri.EscapeDataString(artistName);
                    return $"https://lrclib.net/api/search?track_name={encodedTrack}&artist_name={encodedArtist}";
                }
                
                return $"https://lrclib.net/api/search?track_name={encodedTrack}";
            }
            else if (!string.IsNullOrWhiteSpace(artistName))
            {
                string encodedArtist = Uri.EscapeDataString(artistName);
                return $"https://lrclib.net/api/search?q={encodedArtist}";
            }

            return string.Empty;
        }
    }
}