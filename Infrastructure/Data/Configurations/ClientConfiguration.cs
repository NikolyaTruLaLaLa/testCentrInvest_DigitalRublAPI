using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Data.Configurations
{
    public class ClientConfiguration: IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder) {
            builder.ToTable("Client");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Mid)
                .IsRequired()
                .HasMaxLength(255);
            builder.HasIndex(c => c.Mid)
                .IsUnique();

            builder.Property(c => c.LastName).IsRequired().HasMaxLength(200);
            builder.Property(c => c.FirstName).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Patronymic).HasMaxLength(200);

            builder.Property(c => c.ParticipantDRId).HasMaxLength(255);
            builder.HasIndex(c => c.ParticipantDRId)
            .IsUnique()
            .HasFilter("\"ParticipantDRId\" IS NOT NULL");

            builder.HasMany(c => c.Wallets)
                .WithOne(w => w.Client)
                .HasForeignKey(w => w.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
