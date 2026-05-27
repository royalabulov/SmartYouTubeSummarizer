using System.Collections.Generic;
using SmartYouTubeSummarizer.Models;

namespace SmartYouTubeSummarizer.Services
{
    public interface ISummaryRepository
    {
        List<SummaryHistory> GetAllDescending();
        void Add(SummaryHistory item);
        void Delete(int id);
    }
}