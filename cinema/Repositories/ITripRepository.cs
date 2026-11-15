using cinema.Models;

namespace cinema.Repositories;

public interface ITripRepository
{
    Task<List<Trip>> MyTrips(Guid userId);
}