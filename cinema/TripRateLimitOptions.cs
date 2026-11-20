using System.ComponentModel.DataAnnotations;

namespace cinema;

public class TripRateLimitOptions
{
    public const string SectionName = "TripRateLimit";
    
    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; }
    public TimeSpan Window { get; set; }
    [Range(1, int.MaxValue)]
    public int SegmentsPerWindow { get; set; }
    [Range(1, int.MaxValue)]
    public int QueueLimit { get; set; }
}