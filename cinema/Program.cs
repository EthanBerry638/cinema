using cinema;
using cinema.Data;
using cinema.Models;
using cinema.Repositories;
using cinema.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<TmdbOptions>(builder.Configuration.GetSection(TmdbOptions.SectionName));

builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Cinema")));

builder.Services.AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<CinemaDbContext>();

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddScoped<TripService>();
builder.Services.AddScoped<ITripRepository, TripRepository>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

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