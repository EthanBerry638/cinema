using cinema.Data;
using cinema.DTOs;
using Microsoft.EntityFrameworkCore;

namespace cinema.Repositories;

public class TripRepository (CinemaDbContext db) : ITripRepository
{
    private readonly CinemaDbContext _db = db;
    
    public async Task<List<TripSummary>> MyTrips(Guid userId, int limit, CancellationToken ct)
    {
        return await _db.Trips
            .Where(t => t.UserId == userId)
            .Join(_db.Movies,
                t => t.MovieId,
                m => m.Id,
                (t, m) => new { t, m })
            .OrderByDescending(tm => tm.t.WatchedAt)
            .ThenByDescending(tm => tm.t.CreatedAt)
            .Take(limit)
            .Select(tm => new TripSummary(
                tm.t.Id,
                tm.t.MovieId,
                tm.m.Title,
                tm.m.PosterPath,
                tm.t.WatchedAt,
                tm.t.CinemaName,
                tm.t.Format,
                tm.t.PricePaid,
                tm.t.Notes))
            .ToListAsync(cancellationToken: ct);
    }
}