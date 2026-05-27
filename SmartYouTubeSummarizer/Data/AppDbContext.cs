using Microsoft.EntityFrameworkCore;
using SmartYouTubeSummarizer.Models;

namespace YoutubeSummarizer.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<SummaryHistory> Summaries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // BURA ÖZ MS SQL SERVER BAĞLANTI SƏTRİNİ (CONNECTION STRING) YAZMALISAN
                // Məsələn: Server=SƏNİN_SERVER_ADIN;Database=YoutubeSummarizerDb;Trusted_Connection=True;TrustServerCertificate=True;
                string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SmartYouTubeSummarizer1;Trusted_Connection=True;TrustServerCertificate=True;";

                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}