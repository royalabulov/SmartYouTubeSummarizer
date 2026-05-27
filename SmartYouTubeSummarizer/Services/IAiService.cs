using System.Threading.Tasks;

namespace SmartYouTubeSummarizer.Services
{
    public interface IAiService
    {
        Task<string> SummarizeTextAsync(string text, string lengthOption);
        Task<string> AskQuestionAboutVideoAsync(string videoTranscript, string userQuestion);
    }
}