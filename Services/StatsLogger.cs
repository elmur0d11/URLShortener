using URLShortener.Data;
using URLShortener.Models;

namespace URLShortener.Services
{
    public class StatsLogger
    {
        //Connecting DB
        private readonly ApplicationDbContext _context;
        public StatsLogger(ApplicationDbContext context)
        {
            _context = context;
        }

        //Save Logs to DB
        public void SaveLog(string shortKey, string ip, string userAgent)
        {
            //Mapping
            var log = new VisitLog
            {
                ShortKey = shortKey,
                IpAddress = ip,
                UserAgent = userAgent,
                VisitTime = DateTime.UtcNow
            };

            _context.VisitedLogs.Add(log);
            _context.SaveChanges();
        }
    }
}
