using cinema.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace cinema.Data.Configurations;

public class WatchlistItemConfiguration : IEntityTypeConfiguration<WatchlistItem>
{
    public void Configure(EntityTypeBuilder<WatchlistItem> builder)
    {
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Movie>()
            .WithMany()
            .HasForeignKey(w => w.MovieId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasKey(w => new { w.UserId, w.MovieId });
        builder.Property(w => w.AddedAt).IsRequired();
    }
}