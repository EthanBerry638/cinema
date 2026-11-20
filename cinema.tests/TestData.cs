using cinema.Models;

namespace cinema.tests;

public static class TestData
{
    public static User NewUser()
    {
        var id = Guid.NewGuid();
        return new User { Id = id, UserName = $"user-{id}", Email = $"user-email-{id}@test.com" };
    }
    
    public static Movie NewMovie(string title, string? posterPath = null) => new()
    {
        Id = Guid.NewGuid(), TmdbId = Random.Shared.Next(), Title = title, PosterPath = posterPath
    };

    public static Trip NewTrip(Guid userId, Guid movieId, 
        DateTime watchedAt, Format format = Format.Standard, DateTime? createdAt = null) => new()
    {
        Id = Guid.NewGuid(), UserId = userId, MovieId = movieId,
        CinemaName = $"Cinema {Guid.NewGuid():N}", Format = format, WatchedAt = watchedAt,
        CreatedAt = createdAt ?? DateTime.UtcNow
    };
}