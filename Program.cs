using Hangfire;
using Hangfire.PostgreSql;
using HangfireBasicAuthenticationFilter;
using Microsoft.EntityFrameworkCore;
using URLShortener.Data;
using URLShortener.Entity;
using URLShortener.Models;
using URLShortener.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Database
var connectionString = builder.Configuration.GetConnectionString("Database");
//Main Database
builder.Services.AddDbContextPool<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

//Hangfire
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(connectionString));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<UrlShorteningService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

//Hangfire's Dashboard(http://localhost:5000/hangfire)
app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    DashboardTitle = "Shortener",
    Authorization = new[]
    {
        new HangfireCustomBasicAuthenticationFilter()
        {
            //Passwords to dashboard
            Pass = "GJMRTGZEOK",
            User = "L034UURRDV"
        }
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

//Create_Short_URL
app.MapPost("api/shorten", async (
    ShortenUrlRequest request,
    UrlShorteningService urlShorteningService,
    ApplicationDbContext dbContext,
    HttpContext httpContext) =>
{
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
    {
        return Results.BadRequest("The URL is invalid");
    }

    var code = await urlShorteningService.GenerateCode();

    var shortenedUrl = new ShortenedUrl
    {
        Id = Guid.NewGuid(),
        LongUrl = request.Url,
        Code = code,
        ShortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/{code}",
        CreatedOnUtc = DateTime.UtcNow
    };

    dbContext.ShortenedUrls.Add(shortenedUrl);
    await dbContext.SaveChangesAsync();

    return Results.Ok(shortenedUrl.ShortUrl);
});

//Redirection
app.MapGet("api/{code}", async (
    string code,
    ApplicationDbContext dbContext,
    IHttpContextAccessor httpContextAccessor,
    IBackgroundJobClient backgroundJobs) =>
{
    var shortenUrl = await dbContext.ShortenedUrls
        .FirstOrDefaultAsync(s => s.Code == code);

    if (shortenUrl == null)
    {
        return Results.NotFound();
    }

    // Statistics
    var httpContext = httpContextAccessor.HttpContext;
    var ip = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
    var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "unknown";

    // Write statistic's data to hangfire
    backgroundJobs.Enqueue<StatsLogger>(logger =>
    logger.SaveLog(code, ip, userAgent));

    return Results.Redirect(shortenUrl.LongUrl);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Auto apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
