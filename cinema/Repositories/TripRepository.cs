using cinema.Data;
using cinema.Models;

namespace cinema.Repositories;

public class TripRepository (CinemaDbContext db) : ITripRepository
{
    private readonly CinemaDbContext _db = db;
    
    public async Task<List<Trip>> MyTrips(Guid userId)
    {
        return null;
    }
}