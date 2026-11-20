using System.ComponentModel.DataAnnotations;

namespace cinema.Settings;

public class TripRateLimitSettings
{
    public const string SectionName = "TripRateLimit";
    
    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; }
    
    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan Window { get; set; }
    
    [Range(1, int.MaxValue)]
    public int SegmentsPerWindow { get; set; }
}