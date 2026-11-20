using cinema.DTOs;
using cinema.Models;
using cinema.Repositories;
using cinema.Services;
using cinema.Settings;
using Microsoft.Extensions.Options;
using Moq;

namespace cinema.tests.Unit_Tests.Service_Tests;

public class TripServiceTests
{
    private readonly Mock<ITripRepository> _mockRepo = new();
    private readonly TripService _tripService;

    public TripServiceTests()
    {
        var options = Options.Create(new TmdbOptions { PosterBaseUrl = PosterBaseUrl });
        _tripService = new TripService(_mockRepo.Object, options);
    }
    
    private const string PosterBaseUrl = "https://image.tmdb.org/t/p/w500";

    [Fact]
    public async Task MyTrips_ShouldReturnTripsWithFullPosterUrl_WhenUserHasTrips()
    {
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var userId = Guid.NewGuid();
        var watchedAt = new DateTime(2026, 11, 1, 19, 30, 0, DateTimeKind.Utc);
        var firstTripId = Guid.NewGuid();
        var firstMovieId = Guid.NewGuid();
        var secondTripId = Guid.NewGuid();
        var secondMovieId = Guid.NewGuid();
        var tripSummaries = new List<TripSummary>
        {
            new(firstTripId, firstMovieId, "Dune: Part Two", "/1E5baAaEse26fej7uHcjOgEE2t2.jpg",
                watchedAt, "Odeon Leicester Square", Format.Dolby, 18.50m, "Front row, great sound"),
            new(secondTripId, secondMovieId, "Oppenheimer", "/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg",
                watchedAt.AddDays(-14), "BFI IMAX", Format.Imax, 22.00m, null)
        };
        _mockRepo.Setup(m => m.MyTrips(userId, It.IsAny<CancellationToken>())).ReturnsAsync(tripSummaries);
        var expected = new List<TripSummaryResponse>
        {
            new(firstTripId, firstMovieId, "Dune: Part Two",
                $"{PosterBaseUrl}/1E5baAaEse26fej7uHcjOgEE2t2.jpg",
                watchedAt, "Odeon Leicester Square", Format.Dolby, 18.50m, "Front row, great sound"),
            new(secondTripId, secondMovieId, "Oppenheimer",
                $"{PosterBaseUrl}/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg",
                watchedAt.AddDays(-14), "BFI IMAX", Format.Imax, 22.00m, null)
        };
        
        var result = await _tripService.MyTrips(userId, ct);
        
        Assert.Equal(expected, result);
        _mockRepo.Verify(m => m.MyTrips(userId, ct), Times.Once);
    }

    [Fact]
    public async Task MyTrips_ShouldReturnNullPosterUrl_WhenPosterPathIsNull()
    {
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var userId = Guid.NewGuid();
        var trip = new TripSummary(Guid.NewGuid(), Guid.NewGuid(), "Obscure Indie Film", null,
            new DateTime(2026, 10, 5, 20, 0, 0, DateTimeKind.Utc), "Prince Charles Cinema",
            Format.Dolby, null, null);
        _mockRepo.Setup(m => m.MyTrips(userId, It.IsAny<CancellationToken>())).ReturnsAsync([trip]);

        var result = await _tripService.MyTrips(userId, ct);

        var single = Assert.Single(result);
        Assert.Null(single.PosterUrl);
        _mockRepo.Verify(m => m.MyTrips(userId, ct), Times.Once);
    }

    [Fact]
    public async Task MyTrips_ShouldReturnEmpty_WhenUserHasNoTrips()
    {
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var userId = Guid.NewGuid();
        _mockRepo.Setup(m => m.MyTrips(userId, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await _tripService.MyTrips(userId, ct);

        Assert.Empty(result);
        _mockRepo.Verify(m => m.MyTrips(userId, ct), Times.Once);
    }
}