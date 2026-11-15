using cinema.DTOs;
using cinema.Repositories;

namespace cinema.Services;

public class TripService (ITripRepository tripRepository)
{
    private readonly ITripRepository _tripRepository = tripRepository;
    
    public async Task<List<TripSummaryResponse>> MyTrips(Guid userId)
    {
        return null;
    }
}