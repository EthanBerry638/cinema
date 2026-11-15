using cinema.Models;
using cinema.Repositories;

namespace cinema.tests.Integration_Tests.Repository_Tests;

public class TripRepositoryTests(PostgresFixture fixture)
{
    private readonly PostgresFixture _fixture = fixture;

    [Fact]
    public async Task MyTrips_ShouldReturnTrips_WhenUserHasTrips()
    {
        var userId = Guid.NewGuid();       
        var otherUserId = Guid.NewGuid();
        var movieId = Guid.NewGuid();

        var sharedMovie = new Movie { Id = movieId, TmdbId = Random.Shared.Next(), Title = "Test Movie Title" };
        var myTrips = new List<Trip>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, MovieId = movieId, CinemaName = "Test Name 1", 
                Format = Format.Dolby },
            new() { Id = Guid.NewGuid(), UserId = userId, MovieId = movieId, CinemaName = "Test Name 2",
                Format = Format.FourDx }
        };
        var someoneElsesTrip = new Trip
        {
            Id = Guid.NewGuid(), UserId = otherUserId, MovieId = movieId, CinemaName = "Test Name 3", 
                Format = Format.Standard
        };

        await using (var seedDb = _fixture.CreateDbContext())
        {
            seedDb.Users.AddRange(
                new User { Id = userId, UserName = $"user-{userId}", Email = $"{userId}@test.com" },
                new User { Id = otherUserId, UserName = $"user-{otherUserId}", Email = $"{otherUserId}@test.com" });
            seedDb.Movies.Add(sharedMovie);
            seedDb.Trips.AddRange(myTrips);
            seedDb.Trips.Add(someoneElsesTrip);
            await seedDb.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        
        await using var db = _fixture.CreateDbContext();
        var repository = new TripRepository(db);

        var result = await repository.MyTrips(userId);
        
        Assert.Equal(
            myTrips.Select(t => t.Id).Order(),
            result.Select(t => t.Id).Order());
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
            Format = Format.Standard
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