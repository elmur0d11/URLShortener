namespace URLShortener.Models
{
    public class VisitLog
    {
        public int Id { get; set; }
        public string ShortKey { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime VisitTime { get; set; }

    }
}
