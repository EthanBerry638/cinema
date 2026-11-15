namespace cinema.Models;

public class Movie
{
    public Guid Id { get; set; }
    public int TmdbId { get; set; }
    public required string Title { get; set; }
    public string? Overview { get; set; }
    public string? PosterPath { get; set; }
    public DateOnly? ReleaseDateUk { get; set; }
    public int? RuntimeMinutes { get; set; }
    public decimal? TmdbVoteAverage { get; set; }
    public int? TmdbVoteCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
}