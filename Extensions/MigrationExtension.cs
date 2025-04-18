using Microsoft.EntityFrameworkCore;
using URLShortener.Data;

namespace URLShortener.Extensions
{
    public static class MigrationExtension
    {
        public static void ApplyMigrations(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
        }
    }
}
