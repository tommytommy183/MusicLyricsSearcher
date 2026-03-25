using musicLine.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;

namespace musicLine.Services
{
    public class YoutubeService
    {
        YoutubeClient _youtubeClient = new YoutubeClient();

        public async Task<List<YoutubeModel>> GetYoutubePlayListData(string youtubeUrl)
        {
            List<YoutubeModel> youtubeModels = new List<YoutubeModel>();

            await foreach (var video in _youtubeClient.Playlists.GetVideosAsync(youtubeUrl))
            {
                string title = video.Title;
                string channel = video.Author.ChannelTitle;

                //最大100筆
                if(youtubeModels.Count > 100)
                {
                    break;
                }

                if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(channel))
                {
                    YoutubeModel youtubeModel = new YoutubeModel()
                    {
                        SongName = title,
                        ChannnelName = channel
                    };

                    youtubeModels.Add(youtubeModel);
                }
            }

            return youtubeModels;
        }
    }
}
