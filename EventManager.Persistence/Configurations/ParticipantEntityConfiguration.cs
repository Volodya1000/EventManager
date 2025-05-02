using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManager.Persistence.Configurations;

public class ParticipantEntityConfiguration : IEntityTypeConfiguration<ParticipantEntity>
{
    public void Configure(EntityTypeBuilder<ParticipantEntity> builder)
    {
        builder.ToTable("Participants");

        //builder.HasKey(p => p.Id);
        //builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.HasKey(p => new { p.UserId, p.EventId });
        // Уникальный индекс для предотвращения дублирования регистраций
        builder.HasIndex(p => new { p.UserId, p.EventId }).IsUnique();

        builder.Property(p => p.RegistrationDate)
            .IsRequired();
    }
}
