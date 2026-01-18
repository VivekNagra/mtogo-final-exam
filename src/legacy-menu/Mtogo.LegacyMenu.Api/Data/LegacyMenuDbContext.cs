using Microsoft.EntityFrameworkCore;
using Mtogo.LegacyMenu.Api.Models;

namespace Mtogo.LegacyMenu.Api.Data;

public sealed class LegacyMenuDbContext : DbContext
{
  public LegacyMenuDbContext(DbContextOptions<LegacyMenuDbContext> options) : base(options) { }

  public DbSet<Restaurant> Restaurants => Set<Restaurant>();
  public DbSet<MenuItem> MenuItems => Set<MenuItem>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Restaurant>().HasKey(r => r.Id);
    modelBuilder.Entity<MenuItem>().HasKey(m => m.Id);

    modelBuilder.Entity<MenuItem>()
      .HasIndex(m => new { m.RestaurantId, m.Name });
  }
}
