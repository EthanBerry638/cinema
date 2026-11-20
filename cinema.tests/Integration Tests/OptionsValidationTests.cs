using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace cinema.tests.Integration_Tests;

public class OptionsValidationTests(PostgresFixture fixture)
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("notavalidurl")]
    public void Startup_ShouldThrow_WhenPosterBaseUrlIsInvalid(string? url)
    {
        using var baseFactory = new CinemaApiFactory(fixture);
        using var factory = baseFactory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(s =>
                s.Configure<TmdbOptions>(o => o.PosterBaseUrl = url!)));

        Assert.Throws<OptionsValidationException>(() => factory.CreateClient());
    }
}