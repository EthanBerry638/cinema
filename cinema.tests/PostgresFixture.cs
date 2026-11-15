using cinema.Data;
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

    private string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var db = new CinemaDbContext(options);
        await db.Database.MigrateAsync();
    }

    public ValueTask DisposeAsync() => _container.DisposeAsync();
}