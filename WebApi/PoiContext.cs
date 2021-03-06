using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi;

public class PoiContext : DbContext
{
    public PoiContext(DbContextOptions<PoiContext> options) : base(options)
    {
    }

    public DbSet<User>? Users { get; set; }
    public DbSet<Poi>? Pois { get; set; }
    public DbSet<Checkin>? Checkins { get; set; }
    public DbSet<IdentityUserClaim<string>>? Claims { get; set; }
    public DbSet<Category>? Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Poi>().ToTable("Pois");
        modelBuilder.Entity<Checkin>().ToTable("Checkins");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        modelBuilder.Entity<Category>().ToTable("Categories");
    }
}
