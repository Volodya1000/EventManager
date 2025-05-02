using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Persistence.Configurations;

public class ImageEntityConfiguration : IEntityTypeConfiguration<ImageEntity>
{
    public void Configure(EntityTypeBuilder<ImageEntity> builder)
    {
        builder.ToTable("Images");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
       .ValueGeneratedOnAdd();

        builder.Property(i => i.Url);

        builder.HasOne(i => i.Event)
       .WithMany(e => e.Images)
       .HasForeignKey(i => i.EventId)
       .OnDelete(DeleteBehavior.Cascade);

    }
}