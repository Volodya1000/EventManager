using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace EventManager.Persistence.Configurations;

public class EventEntityConfiguration : IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(e => e.Name).IsUnique(); //имя уникально в рамках всех событий

        builder.Property(e => e.Location)
            .IsRequired();

        builder.HasMany(e => e.Participants)
            .WithOne(p => p.Event)
            .HasForeignKey(p => p.EventId);

        builder.HasMany(e => e.Images)
       .WithOne(i => i.Event)          
       .HasForeignKey(i => i.EventId)  
       .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Category)
           .WithMany()
           .HasForeignKey(e => e.CategoryId);
    }
}
