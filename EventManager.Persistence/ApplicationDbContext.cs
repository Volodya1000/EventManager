using EventManager.Domain.Models;
using EventManager.Persistence.Configurations;
using EventManager.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public DbSet<User> Users { get; set; }
    public DbSet<EventEntity> EventEntity { get; set; }
    public DbSet<ParticipantEntity> ParticipantEntity { get; set; }
    public DbSet<CategoryEntity> CategoryEntity { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new EventEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ParticipantEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryEntityConfiguration());

        base.OnModelCreating(modelBuilder);
    }

}
