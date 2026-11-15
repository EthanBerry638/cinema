using cinema.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace cinema.Data.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => m.TmdbId).IsUnique();
        builder.Property(m => m.Title).IsRequired().HasMaxLength(300);
        builder.Property(m => m.PosterPath).HasMaxLength(200);
        builder.Property(m => m.TmdbVoteAverage).HasPrecision(3,1);
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.LastSyncedAt).IsRequired();
    }
}