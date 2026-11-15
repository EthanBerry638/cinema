using cinema.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace cinema.Data.Configurations;

public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Movie>()
            .WithMany()
            .HasForeignKey(t => t.MovieId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => new { t.UserId, t.WatchedAt });
        builder.Property(t => t.WatchedAt).IsRequired();
        builder.Property(t => t.CinemaName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Format).HasConversion<string>().IsRequired();
        builder.Property(t => t.PricePaid).HasPrecision(6, 2);
        builder.Property(t => t.Notes).HasMaxLength(1000);
        builder.Property(t => t.CreatedAt).IsRequired();
    }
}