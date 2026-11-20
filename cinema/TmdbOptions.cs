using System.ComponentModel.DataAnnotations;

namespace cinema;

public class TmdbOptions
{
    public const string SectionName = "Tmdb";
    
    [Required]
    [Url]
    public string PosterBaseUrl { get; set; } = string.Empty;
}