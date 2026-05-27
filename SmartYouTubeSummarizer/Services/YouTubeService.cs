using System;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace SmartYouTubeSummarizer.Services
{
    public class YouTubeService : IYouTubeService
    {
        public async Task<string> GetVideoTranscriptAsync(string videoUrl)
        {
            var youtube = new YoutubeClient();
            var videoId = VideoId.Parse(videoUrl);
            var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoId);

            var trackInfo = trackManifest.Tracks.FirstOrDefault(t => t.Language.Code == "az")
                         ?? trackManifest.Tracks.FirstOrDefault(t => t.Language.Code == "en")
                         ?? trackManifest.Tracks.FirstOrDefault();

            if (trackInfo == null)
                throw new Exception("Bu videoda heç bir altyazı tapılmadı.");

            var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);
            return string.Join(" ", track.Captions.Select(c => c.Text));
        }
    }
}