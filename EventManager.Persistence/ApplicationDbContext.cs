using EventManager.Domain.Constants;
using EventManager.Domain.Models;
using EventManager.Persistence.Configurations;
using EventManager.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public new DbSet<User> Users { get; set; }
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<ParticipantEntity> Participants { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<ImageEntity> Images { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new EventEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ParticipantEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ImageConfiguration());

        modelBuilder.Entity<IdentityRole<Guid>>()
            .HasData(new List<IdentityRole<Guid>>
            {
                new IdentityRole<Guid>()
                {
                    Id = IdentityRoleConstants.AdminRoleGuid,
                    Name = IdentityRoleConstants.Admin,
                    NormalizedName = IdentityRoleConstants.Admin.ToUpper()
                },
                new IdentityRole<Guid>()
                {
                    Id = IdentityRoleConstants.UserGuid,
                    Name = IdentityRoleConstants.User,
                    NormalizedName = IdentityRoleConstants.User.ToUpper()
                }
            });

        // Создание начального админа
        var adminUser = CreateInitialAdmin();

        modelBuilder.Entity<User>().HasData(adminUser);

        modelBuilder.Entity<IdentityUserRole<Guid>>().HasData(
            new IdentityUserRole<Guid>
            {
                RoleId = IdentityRoleConstants.AdminRoleGuid,
                UserId = adminUser.Id
            }
        );
    }

    private User CreateInitialAdmin()
    {
        var adminUser = new User
        {
            Id = Guid.Parse("ab4456be-2623-4808-9426-1a1f09ae956f"),
            UserName = "admin@example.com",
            NormalizedUserName = "ADMIN@EXAMPLE.COM",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "Admin",
            DateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            //Пароль StrongPassword123!
            PasswordHash = "AQAAAAIAAYagAAAAEKRnSu3AUnjUGydDa4WyUFwhI9T4W36QwBl7MlqqdDasp+6zKhjkEh3DIvHrHFBTgQ==",
            SecurityStamp = "fixed-security-stamp",
            ConcurrencyStamp = "fixed-user-concurrency-stamp"
        };
        return adminUser;
    }

}
