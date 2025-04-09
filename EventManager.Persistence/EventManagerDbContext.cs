using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Persistence;

public class EventManagerDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<UserEntity> EventEntity { get; set; }
    public DbSet<UserEntity> ParticipantEntity { get; set; }
    public DbSet<UserEntity> CategoryEntity { get; set; }

    public EventManagerDbContext(DbContextOptions<EventManagerDbContext> options)
          : base(options)
    {
        Database.EnsureCreated();
    }

}
