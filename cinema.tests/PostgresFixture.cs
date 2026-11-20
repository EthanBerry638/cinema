using cinema.Data;
using cinema.Models;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace cinema.tests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("testdb")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => $"{_container.GetConnectionString()};Include Error Detail=true";
    
    public CinemaDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new CinemaDbContext(options);
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();
    
    public async Task SeedAsync(User[]? users = null, Movie[]? movies = null, Trip[]? trips = null)
    {
        await using var db = CreateDbContext();

        db.Users.AddRange(users ?? []);
        db.Movies.AddRange(movies ?? []);
        db.Trips.AddRange(trips ?? []);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}