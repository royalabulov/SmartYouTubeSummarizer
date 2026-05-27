using System;

namespace SmartYouTubeSummarizer.Models
{
    public class SummaryHistory
    {
        public int Id { get; set; }
        public string VideoUrl { get; set; }
        public string Title { get; set; }
        public string SummaryText { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}