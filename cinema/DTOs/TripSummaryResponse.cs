using cinema.Models;

namespace cinema.DTOs;

public record TripSummaryResponse(
    Guid Id,
    Guid MovieId,
    string Title,
    string? PosterUrl,
    DateTime WatchedAt,
    string CinemaName,
    Format Format,
    decimal? PricePaid,
    string? Notes);