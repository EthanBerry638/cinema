using cinema.Models;
using cinema.Repositories;

namespace cinema.tests.Integration_Tests.Repository_Tests;

public class TripRepositoryTests(PostgresFixture fixture)
{
    private readonly PostgresFixture _fixture = fixture;

    [Fact]
    public async Task MyTrips_ShouldReturnTripsNewestFirst_WhenUserHasTrips()
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

        await using var db = _fixture.CreateDbContext();
        var repository = new TripRepository(db);

        var result = await repository.MyTrips(user.Id, TestContext.Current.CancellationToken);

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
        var user = TestData.NewUser();
        var otherUser = TestData.NewUser();
        var movie = TestData.NewMovie("Test Movie Title");
        var someoneElsesTrip = TestData.NewTrip(otherUser.Id, movie.Id, DateTime.UtcNow);
        await _fixture.SeedAsync(
            users: [user, otherUser],
            movies: [movie],
            trips: [someoneElsesTrip]);

        await using var db = _fixture.CreateDbContext();
        var repository = new TripRepository(db);

        var result = await repository.MyTrips(user.Id, TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task MyTrips_ShouldThrowOperationCanceled_WhenCancellationTokenIsCancelled()
    {
        var ct = new CancellationToken(canceled: true);
        await using var db = _fixture.CreateDbContext();
        var repository = new TripRepository(db);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => repository.MyTrips(Guid.NewGuid(), ct));
    }

    [Fact]
    public async Task MyTrips_ShouldOrderByCreatedAtDescending_WhenWatchedAtIsEqual()
    {
        var watchedAt = DateTime.UtcNow.AddDays(-1);
        var user = TestData.NewUser();
        var movie = TestData.NewMovie("Test Movie");
        var yesterdaysTrip1 = TestData.NewTrip(user.Id, movie.Id, watchedAt,
            createdAt: DateTime.UtcNow);
        var yesterdaysTrip2 = TestData.NewTrip(user.Id, movie.Id, watchedAt,
            createdAt: DateTime.UtcNow.AddDays(-1));
        var yesterdaysTrip3 = TestData.NewTrip(user.Id, movie.Id, watchedAt,
            createdAt: DateTime.UtcNow.AddDays(-3));
        await _fixture.SeedAsync(
            users: [user],
            movies: [movie]);
        await _fixture.SeedAsync(trips: [yesterdaysTrip1]);
        await _fixture.SeedAsync(trips: [yesterdaysTrip2]);
        await _fixture.SeedAsync(trips: [yesterdaysTrip3]);
        
        await using var db = _fixture.CreateDbContext();
        var repository = new TripRepository(db);
        
        var result = await repository.MyTrips(user.Id, TestContext.Current.CancellationToken);
        
        Assert.Equal([yesterdaysTrip1.Id, yesterdaysTrip2.Id, yesterdaysTrip3.Id], result.Select(t => t.Id));
    }
}
