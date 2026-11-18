using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using cinema.DTOs;
using cinema.Models;

namespace cinema.tests.Integration_Tests.Controller_Tests;

public class TripControllerTests : IAsyncDisposable
{
    private const string MyTripsUrl = "/api/trip";
    
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly PostgresFixture _fixture;
    private readonly CinemaApiFactory _factory;

    public TripControllerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _factory = new CinemaApiFactory(fixture);
    }

    [Fact]
    public async Task MyTrips_ShouldReturn200WithTripsNewestFirst_WhenUserHasTrips()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var firstMovie = NewMovie("First Movie", "/first.jpg");
        var secondMovie = NewMovie("Second Movie", "/second.jpg");

        var olderTrip = NewTrip(userId, firstMovie.Id, Format.Dolby, DateTime.UtcNow.AddDays(-7));
        var newerTrip = NewTrip(userId, secondMovie.Id, Format.FourDx, DateTime.UtcNow.AddDays(-1));
        var someoneElsesTrip = NewTrip(otherUserId, firstMovie.Id, Format.Standard, DateTime.UtcNow);

        await SeedAsync(
            users: [userId, otherUserId],
            movies: [firstMovie, secondMovie],
            trips: [olderTrip, newerTrip, someoneElsesTrip]);

        using var client = CreateClientFor(userId);

        var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var trips = await response.Content.ReadFromJsonAsync<List<TripSummaryResponse>>(
            JsonOptions, TestContext.Current.CancellationToken);
        
        Assert.Equal([newerTrip.Id, olderTrip.Id], trips!.Select(t => t.Id));
    }

    [Fact]
    public async Task MyTrips_ShouldReturn200WithEmptyList_WhenUserHasNoTrips()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var movie = NewMovie("Test Movie Title");

        await SeedAsync(
            users: [userId, otherUserId],
            movies: [movie],
            trips: [NewTrip(otherUserId, movie.Id, Format.Standard, DateTime.UtcNow)]);

        using var client = CreateClientFor(userId);

        var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Empty(
            (await response.Content.ReadFromJsonAsync<List<TripSummaryResponse>>(TestContext.Current.CancellationToken))
            !);
    }

    [Fact]
    public async Task MyTrips_ShouldReturn401_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task MyTrips_ShouldReturn429_WhenRateLimitExceeded()
    {
        using var client = CreateClientFor(Guid.NewGuid());

        for (var i = 0; i < 60; i++)
        {
            var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }

        var limited = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }
    
    [Fact]
    public async Task MyTrips_RateLimit_ShouldBePerUser()
    {
        using var greedy = CreateClientFor(Guid.NewGuid());
        using var other = CreateClientFor(Guid.NewGuid());

        for (var i = 0; i < 61; i++)
            await greedy.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);

        var response = await other.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
        Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task MyTrips_ShouldReturn401_WhenUserDoesNotHaveAnId()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "not a guid");
        
        var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MyTrips_ShouldConvertFormat_ToString()
    {
        var userId = Guid.NewGuid();
        using var client = CreateClientFor(userId);
        var movie = NewMovie("Movie");
        var trip = NewTrip(userId, movie.Id, Format.Dolby, DateTime.UtcNow.AddDays(-7));
        await SeedAsync([userId], [movie], [trip]);
        
        var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
        var json  = JsonNode.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var node = json![0]!["format"];
        
        Assert.Equal(JsonValueKind.String, node!.GetValueKind());
        Assert.Equal("Dolby", node.GetValue<string>());
    }

    public ValueTask DisposeAsync() => _factory.DisposeAsync();

    private HttpClient CreateClientFor(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        return client;
    }

    private async Task SeedAsync(Guid[] users, Movie[] movies, Trip[] trips)
    {
        await using var db = _fixture.CreateDbContext();

        db.Users.AddRange(users.Select(id => new User
        {
            Id = id, UserName = $"user-{id}", Email = $"{id}@test.com"
        }));
        db.Movies.AddRange(movies);
        db.Trips.AddRange(trips);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static Movie NewMovie(string title, string? posterPath = null) => new()
    {
        Id = Guid.NewGuid(), TmdbId = Random.Shared.Next(), Title = title, PosterPath = posterPath
    };

    private static Trip NewTrip(Guid userId, Guid movieId, Format format, DateTime watchedAt) => new()
    {
        Id = Guid.NewGuid(), UserId = userId, MovieId = movieId,
        CinemaName = $"Cinema {Guid.NewGuid():N}", Format = format, WatchedAt = watchedAt
    };
}