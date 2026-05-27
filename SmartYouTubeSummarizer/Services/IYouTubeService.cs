using System.Threading.Tasks;

namespace SmartYouTubeSummarizer.Services
{
    public interface IYouTubeService
    {
        Task<string> GetVideoTranscriptAsync(string videoUrl);
    }
}