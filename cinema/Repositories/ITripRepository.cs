using cinema.DTOs;

namespace cinema.Repositories;

public interface ITripRepository
{
    Task<List<TripSummary>> MyTrips(Guid userId);
}