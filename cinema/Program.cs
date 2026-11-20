using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using cinema;
using cinema.Data;
using cinema.Models;
using cinema.Repositories;
using cinema.Services;
using cinema.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddOptions<TmdbSettings>()
    .BindConfiguration(TmdbSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<TripRateLimitSettings>()
    .BindConfiguration(TripRateLimitSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Cinema")));

builder.Services.AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<CinemaDbContext>();

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddScoped<TripService>();
builder.Services.AddScoped<ITripRepository, TripRepository>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("trips", context =>
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var rateLimitOptions = context.RequestServices.
            GetRequiredService<IOptions<TripRateLimitSettings>>().Value;

        if (userId is null)
            // Anonymous users are not rate limited yet: https://github.com/EthanBerry638/cinema/issues/2
            return RateLimitPartition.GetNoLimiter("anonymous");

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: userId,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = rateLimitOptions.PermitLimit,
                Window = rateLimitOptions.Window,
                SegmentsPerWindow = rateLimitOptions.SegmentsPerWindow,
                QueueLimit = 0
            });
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<CinemaDbContext>().Database.MigrateAsync();
}

app.MapGroup("/api/auth").MapIdentityApi<User>();

app.Run();