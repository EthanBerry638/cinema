using cinema.Data;
using cinema.Models;
using Microsoft.EntityFrameworkCore;

namespace cinema.Repositories;

public class TripRepository (CinemaDbContext db) : ITripRepository
{
    private readonly CinemaDbContext _db = db;
    
    public async Task<List<Trip>> MyTrips(Guid userId)
    {
        return await _db.Trips.Where(t => t.UserId == userId).ToListAsync();
    }
}