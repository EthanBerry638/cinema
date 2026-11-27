using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using cinema.DTOs;
using cinema.Models;
using cinema.Repositories;
using cinema.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace cinema.tests.Integration_Tests.Controller_Tests;

public class TripControllerTests : IAsyncDisposable
{
    private const string MyTripsUrl = "/api/me/trips";
    private const int TestRateLimit = 3;
    
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly PostgresFixture _fixture;
    private readonly CinemaApiFactory _baseFactory;
    private readonly WebApplicationFactory<Program> _factory;

    public TripControllerTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _baseFactory = new CinemaApiFactory(fixture);
        _factory = _baseFactory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(s => s.Configure<TripRateLimitSettings>(o =>
                o.PermitLimit = TestRateLimit)));
    }

    [Fact]
    public async Task MyTrips_ShouldReturn200WithTripsNewestFirst_WhenUserHasTrips()
    {
        var user = TestData.NewUser();
        var otherUser = TestData.NewUser();
        var firstMovie = TestData.NewMovie("First Movie", "/first.jpg");
        var secondMovie = TestData.NewMovie("Second Movie", "/second.jpg");
        var olderTrip = TestData.NewTrip(user.Id, firstMovie.Id, DateTime.UtcNow.AddDays(-7), Format.Dolby);
        var newerTrip = TestData.NewTrip(user.Id, secondMovie.Id, DateTime.UtcNow.AddDays(-1), Format.FourDx);
        var someoneElsesTrip = TestData.NewTrip(otherUser.Id, firstMovie.Id, DateTime.UtcNow);
        await _fixture.SeedAsync(
            users: [user, otherUser],
            movies: [firstMovie, secondMovie],
            trips: [olderTrip, newerTrip, someoneElsesTrip]);

        using var client = CreateClientFor(user.Id);

        using var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var trips = await response.Content.ReadFromJsonAsync<List<TripSummaryResponse>>(
            JsonOptions, TestContext.Current.CancellationToken);
        
        Assert.Equal([newerTrip.Id, olderTrip.Id], trips!.Select(t => t.Id));
    }

    [Fact]
    public async Task MyTrips_ShouldReturn200WithEmptyList_WhenUserHasNoTrips()
    {
        var user = TestData.NewUser();
        var otherUser = TestData.NewUser();
        var movie = TestData.NewMovie("Test Movie Title");
        await _fixture.SeedAsync(
            users: [user, otherUser],
            movies: [movie],
            trips: [TestData.NewTrip(otherUser.Id, movie.Id, DateTime.UtcNow)]);

        using var client = CreateClientFor(user.Id);

        using var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Empty(
            (await response.Content.ReadFromJsonAsync<List<TripSummaryResponse>>(TestContext.Current.CancellationToken))
            !);
    }

    [Fact]
    public async Task MyTrips_ShouldReturn401_WhenNotAuthenticated()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task MyTrips_ShouldReturn429_WhenRateLimitExceeded()
    {
        using var client = CreateClientFor(Guid.NewGuid());

        for (var i = 0; i < TestRateLimit; i++)
        {
            using var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }

        using var limited = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }
    
    [Fact]
    public async Task MyTrips_RateLimit_ShouldBePerUser()
    {
        using var greedy = CreateClientFor(Guid.NewGuid());
        using var other = CreateClientFor(Guid.NewGuid());

        for (var i = 0; i < TestRateLimit; i++)
        {
            using var _ = await greedy.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
        }

        using var greedyLimited = await greedy.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.TooManyRequests, greedyLimited.StatusCode);
        
        using var response = await other.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
        Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task MyTrips_ShouldReturn401_WhenUserDoesNotHaveAnId()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "not a guid");
        
        using var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MyTrips_ShouldConvertFormat_ToString()
    {
        var user = TestData.NewUser();
        var movie = TestData.NewMovie("Movie");
        var trip = TestData.NewTrip(user.Id, movie.Id, DateTime.UtcNow.AddDays(-7), Format.Dolby);
        await _fixture.SeedAsync([user], [movie], [trip]);

        using var client = CreateClientFor(user.Id);

        using var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var node = json![0]!["format"];
        
        Assert.Equal(JsonValueKind.String, node!.GetValueKind());
        Assert.Equal("Dolby", node.GetValue<string>());
    }

    [Fact]
    public async Task MyTrips_ShouldReturn500ProblemDetails_WhenUnhandledExceptionThrown()
    {
        var throwingRepo = new Mock<ITripRepository>();
        throwingRepo.Setup(r => r.MyTrips(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());
        using var client = _factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            s.RemoveAll<ITripRepository>().AddScoped(_ => throwingRepo.Object))).CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, Guid.NewGuid().ToString());
        
        using var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);

        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.False(string.IsNullOrWhiteSpace(problem.Title));
    }
    
    [Fact]
    public async Task MyTrips_ShouldNotLeakExceptionMessage_WhenUnhandledExceptionThrown()
    {
        var exceptionMessage = "a very specific exception message";
        var throwingRepo = new Mock<ITripRepository>();
        throwingRepo.Setup(r => r.MyTrips(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));
        using var client = _factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            s.RemoveAll<ITripRepository>().AddScoped(_ => throwingRepo.Object))).CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, Guid.NewGuid().ToString());
        
        using var response = await client.GetAsync(MyTripsUrl, TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        
        Assert.DoesNotContain(exceptionMessage, body);
    }

    public ValueTask DisposeAsync() => _baseFactory.DisposeAsync();

    private HttpClient CreateClientFor(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        return client;
    }
}