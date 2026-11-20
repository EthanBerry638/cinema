using cinema.DTOs;
using cinema.Repositories;
using cinema.Settings;
using Microsoft.Extensions.Options;

namespace cinema.Services;

public class TripService (ITripRepository tripRepository, IOptions<TmdbOptions> tmdbOptions)
{
    private readonly ITripRepository _tripRepository = tripRepository;
    private readonly string _posterBaseUrl = tmdbOptions.Value.PosterBaseUrl;
    
    public async Task<List<TripSummaryResponse>> MyTrips(Guid userId, CancellationToken ct = default)
    {
        var trips = await _tripRepository.MyTrips(userId, ct);

        return trips.Select(t => new TripSummaryResponse(
            t.Id, t.MovieId, t.Title, BuildPosterUrl(t.PosterPath), t.WatchedAt, t.CinemaName,
            t.Format, t.PricePaid, t.Notes)).ToList();
    }

    private string? BuildPosterUrl(string? posterPath) =>
        string.IsNullOrWhiteSpace(posterPath)
            ? null
            : $"{_posterBaseUrl}/{posterPath.TrimStart('/')}";
}