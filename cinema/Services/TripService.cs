using cinema.Models;
using cinema.Repositories;

namespace cinema.Services;

public class TripService (ITripRepository tripRepository)
{
    private readonly ITripRepository _tripRepository = tripRepository;
    
    public async Task<List<Trip>> MyTrips(Guid userId)
    {
        return null;
    }
}