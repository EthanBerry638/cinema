using cinema.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace cinema.Data;

public class CinemaDbContext(DbContextOptions<CinemaDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); 

        builder.ApplyConfigurationsFromAssembly(typeof(CinemaDbContext).Assembly);
    }
}