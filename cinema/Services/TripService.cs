using cinema.DTOs;
using cinema.Repositories;

namespace cinema.Services;

public class TripService (ITripRepository tripRepository)
{
    private readonly ITripRepository _tripRepository = tripRepository;
    
    private const string PosterBaseUrl = "https://image.tmdb.org/t/p/w500";
    
    public async Task<List<TripSummaryResponse>> MyTrips(Guid userId)
    {
        var trips = await _tripRepository.MyTrips(userId);

        return trips.Select(t => new TripSummaryResponse(
            t.Id, t.MovieId, t.Title, BuildPosterUrl(t.PosterPath), t.WatchedAt, t.CinemaName,
            t.Format, t.PricePaid, t.Notes)).ToList();
    }

    private string? BuildPosterUrl(string? posterPath) =>
        string.IsNullOrWhiteSpace(posterPath)
            ? null
            : $"{PosterBaseUrl}/{posterPath.TrimStart('/')}";
}