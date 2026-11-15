namespace cinema.Models;

public class WatchlistItem
{
    public Guid UserId { get; set; }
    public Guid MovieId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReleaseNotifiedAt { get; set; }
}