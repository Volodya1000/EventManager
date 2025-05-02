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
        modelBuilder.ApplyConfiguration(new ImageEntityConfiguration());

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

        SeedInitialCategories(modelBuilder);
    }

    private void SeedInitialCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoryEntity>().HasData(
            new CategoryEntity { Id = Guid.Parse("7e345e1a-9b5a-4c5e-8d2a-3e7f4b5c6d7e"), Name = "Conferences" },
            new CategoryEntity { Id = Guid.Parse("8f456f2b-9c6b-5d6f-9e3b-4f8a5c6d7e8f"), Name = "Workshops" },
            new CategoryEntity { Id = Guid.Parse("9a567f3c-ad7c-6e7f-0f4c-5d9b8a7c6d5e"), Name = "Festivals" },
            new CategoryEntity { Id = Guid.Parse("bcd89e4d-be8d-7f8a-1e5d-6e0c9b8a7f6e"), Name = "Meetups" },
            new CategoryEntity { Id = Guid.Parse("cde9af5e-cf9e-8a9b-2b6e-7f1dac9b8e7f"), Name = "Exhibitions" },
            new CategoryEntity { Id = Guid.Parse("def0b06f-d0af-9bac-3c7f-8a2ebdac9f8a"), Name = "Concerts" },
            new CategoryEntity { Id = Guid.Parse("ef1a0170-e1b0-acbd-4d80-9b3fcebda0a9"), Name = "Sports" },
            new CategoryEntity { Id = Guid.Parse("f02b1281-f2c1-bdce-5e91-ac40dfceb1ba"), Name = "Seminar" },
            new CategoryEntity { Id = Guid.Parse("013c2392-03d2-cedf-6fa2-bd51e0cfd2cb"), Name = "Weddings" },
            new CategoryEntity { Id = Guid.Parse("124d34a3-14e3-dfe0-70b3-ce62f1d0e3dc"), Name = "Charity" }
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

    public override async Task<int> SaveChangesAsync(
       bool acceptAllChangesOnSuccess,
       CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        ChangeTracker.Clear();
        return result;
    }

}
