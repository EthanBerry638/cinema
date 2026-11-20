using cinema.Models;

namespace cinema.DTOs;

public record TripSummary(
    Guid Id,
    Guid MovieId,
    string Title,
    string? PosterPath,
    DateTime WatchedAt,
    string CinemaName,
    Format Format,
    decimal? PricePaid,
    string? Notes);