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
}