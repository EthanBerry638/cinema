using cinema.Models;
using cinema.Repositories;

namespace cinema.tests.Integration_Tests.Repository_Tests;

public class TripRepositoryTests(PostgresFixture fixture)
{
    private readonly PostgresFixture _fixture = fixture;

    [Fact]
    public async Task MyTrips_ShouldReturnTripsNewestFirst_WhenUserHasTrips()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var firstMovie = new Movie
        {
            Id = Guid.NewGuid(), TmdbId = Random.Shared.Next(), Title = "First Movie", PosterPath = "/first.jpg"
        };
        var secondMovie = new Movie
        {
            Id = Guid.NewGuid(), TmdbId = Random.Shared.Next(), Title = "Second Movie", PosterPath = "/second.jpg"
        };

        var olderTrip = new Trip
        {
            Id = Guid.NewGuid(), UserId = userId, MovieId = firstMovie.Id, CinemaName = "Test Name 1",
            Format = Format.Dolby, WatchedAt = DateTime.UtcNow.AddDays(-7)
        };
        var newerTrip = new Trip
        {
            Id = Guid.NewGuid(), UserId = userId, MovieId = secondMovie.Id, CinemaName = "Test Name 2",
            Format = Format.FourDx, WatchedAt = DateTime.UtcNow.AddDays(-1)
        };
        var someoneElsesTrip = new Trip
        {
            Id = Guid.NewGuid(), UserId = otherUserId, MovieId = firstMovie.Id, CinemaName = "Test Name 3",
            Format = Format.Standard, WatchedAt = DateTime.UtcNow
        };

        await using (var seedDb = _fixture.CreateDbContext())
        {
            seedDb.Users.AddRange(
                new User { Id = userId, UserName = $"user-{userId}", Email = $"{userId}@test.com" },
                new User { Id = otherUserId, UserName = $"user-{otherUserId}", Email = $"{otherUserId}@test.com" });
            seedDb.Movies.AddRange(firstMovie, secondMovie);
            seedDb.Trips.AddRange(olderTrip, newerTrip, someoneElsesTrip);
            await seedDb.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var db = _fixture.CreateDbContext();
        var repository = new TripRepository(db);

        var result = await repository.MyTrips(userId);
        
        Assert.Equal([newerTrip.Id, olderTrip.Id], result.Select(t => t.Id));
        
        Assert.Collection(result,
            newest =>
            {
                Assert.Equal(secondMovie.Id, newest.MovieId);
                Assert.Equal(secondMovie.Title, newest.Title);
                Assert.Equal(secondMovie.PosterPath, newest.PosterPath);
                Assert.Equal(newerTrip.CinemaName, newest.CinemaName);
                Assert.Equal(newerTrip.Format, newest.Format);
            },
            oldest =>
            {
                Assert.Equal(firstMovie.Id, oldest.MovieId);
                Assert.Equal(firstMovie.Title, oldest.Title);
                Assert.Equal(firstMovie.PosterPath, oldest.PosterPath);
                Assert.Equal(olderTrip.CinemaName, oldest.CinemaName);
                Assert.Equal(olderTrip.Format, oldest.Format);
            });
    }

    [Fact]
    public async Task MyTrips_ShouldReturnNoTrips_WhenUserHasNoTrips()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var movieId = Guid.NewGuid();

        var movie = new Movie { Id = movieId, TmdbId = Random.Shared.Next(), Title = "Test Movie Title" };
        var someoneElsesTrip = new Trip
        {
            Id = Guid.NewGuid(), UserId = otherUserId, MovieId = movieId, CinemaName = "Test Name 3",
            Format = Format.Standard, WatchedAt = DateTime.UtcNow
        };

        await using (var seedDb = _fixture.CreateDbContext())
        {
            seedDb.Users.AddRange(
                new User { Id = userId, UserName = $"user-{userId}", Email = $"{userId}@test.com" },
                new User { Id = otherUserId, UserName = $"user-{otherUserId}", Email = $"{otherUserId}@test.com" });
            seedDb.Movies.Add(movie);
            seedDb.Trips.Add(someoneElsesTrip);
            await seedDb.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var db = _fixture.CreateDbContext();
        var repository = new TripRepository(db);

        var result = await repository.MyTrips(userId);

        Assert.Empty(result);
    }
}