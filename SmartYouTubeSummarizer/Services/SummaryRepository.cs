using System;
using System.Collections.Generic;
using System.Linq;
using SmartYouTubeSummarizer.Models;
using YoutubeSummarizer.Data;

namespace SmartYouTubeSummarizer.Services
{
    public class SummaryRepository : ISummaryRepository
    {
        public List<SummaryHistory> GetAllDescending()
        {
            using var db = new AppDbContext();
            return db.Summaries.OrderByDescending(s => s.CreatedAt).ToList();
        }

        public void Add(SummaryHistory item)
        {
            using var db = new AppDbContext();
            db.Summaries.Add(item);
            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();
            var item = db.Summaries.Find(id);
            if (item != null)
            {
                db.Summaries.Remove(item);
                db.SaveChanges();
            }
        }
    }
}