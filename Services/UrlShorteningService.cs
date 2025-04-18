using Microsoft.EntityFrameworkCore;
using URLShortener.Data;

namespace URLShortener.Services
{
    public class UrlShorteningService
    {
        public const int NumberOfChars = 5;
        private const string Alphabet = "ABCDEFGHIKLMNOPQRSTVXYZabcdefghiklmnopqrstvxyz0123456789";

        //Create random class
        private readonly Random _random = new();
        //Connectiong to DB
        private readonly ApplicationDbContext _dbContext;
        public UrlShorteningService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //Generate key
        public async Task<string> GenerateCode()
        {
            var codeChars = new char[NumberOfChars];

            while (true)
            {
                for (var i = 0; i < NumberOfChars; i++)
                {
                    var random = _random.Next(Alphabet.Length - 1);

                    codeChars[i] = Alphabet[random];
                }

                var code = new string(codeChars);

                if (!await _dbContext.ShortenedUrls.AnyAsync(s => s.Code == code))
                {
                    return code;
                }
            }
        }
    }
}
