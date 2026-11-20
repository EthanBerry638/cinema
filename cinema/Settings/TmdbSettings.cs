using System.ComponentModel.DataAnnotations;

namespace cinema.Settings;

public class TmdbSettings
{
    public const string SectionName = "Tmdb";
    
    [Required]
    [Url]
    public string PosterBaseUrl { get; set; } = string.Empty;
}