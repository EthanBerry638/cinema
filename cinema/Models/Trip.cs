namespace cinema.Models;

public class Trip
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid MovieId { get; set; }
    public DateTime WatchedAt { get; set; } = DateTime.UtcNow;
    public required string CinemaName { get; set; }
    public Format Format { get; set; }
    public decimal? PricePaid { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}