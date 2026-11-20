using cinema.Settings;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace cinema.tests.Integration_Tests;

public class SettingsValidationTests(PostgresFixture fixture)
{
    private readonly PostgresFixture _fixture = fixture;
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("notavalidurl")]
    public void Startup_ShouldThrow_WhenPosterBaseUrlIsInvalid(string? url)
    {
        using var baseFactory = new CinemaApiFactory(_fixture);
        using var factory = baseFactory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(s =>
                s.Configure<TmdbSettings>(o => o.PosterBaseUrl = url!)));

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Startup_ShouldThrow_WhenTripRateLimitPermitLimitIsInvalid(int permitLimit)
    {
        using var baseFactory = new CinemaApiFactory(_fixture);
        using var factory = baseFactory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(s =>
                s.Configure<TripRateLimitSettings>(o => o.PermitLimit = permitLimit)));

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Theory]
    [InlineData("00:00:00")]
    [InlineData("-00:00:01")]
    [InlineData("1.00:00:01")]
    public void Startup_ShouldThrow_WhenTripRateLimitWindowIsInvalid(string window)
    {
        using var baseFactory = new CinemaApiFactory(_fixture);
        using var factory = baseFactory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(s =>
                s.Configure<TripRateLimitSettings>(o => o.Window = TimeSpan.Parse(window))));

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Startup_ShouldThrow_WhenTripRateLimitSegmentsPerWindowIsInvalid(int segmentsPerWindow)
    {
        using var baseFactory = new CinemaApiFactory(_fixture);
        using var factory = baseFactory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(s =>
                s.Configure<TripRateLimitSettings>(o => o.SegmentsPerWindow = segmentsPerWindow)));

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }
}