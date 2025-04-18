using Microsoft.EntityFrameworkCore;
using URLShortener.Entity;
using URLShortener.Models;
using URLShortener.Services;

namespace URLShortener.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }
        
        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }
        public DbSet<VisitLog> VisitedLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShortenedUrl>(builder =>
            {
                builder.Property(s => s.Code).HasMaxLength(UrlShorteningService.NumberOfChars);

                builder.HasIndex(s => s.Code).IsUnique();
            });
        }
    }
}
