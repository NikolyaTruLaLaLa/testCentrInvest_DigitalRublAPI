using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Data.Configurations
{
    internal class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Id)
                .ValueGeneratedNever();

            builder.Property(w => w.ClientId)
                .IsRequired();

            builder.Property(w => w.Code)
                .IsRequired()
                .HasMaxLength(255);
            builder.HasIndex(w => w.Code)
                .IsUnique();

            builder.Property(w => w.Status)
                .IsRequired()
                .HasMaxLength(4)
                .HasConversion<string>();

            builder.Property(w => w.AccountNumber)
               .HasMaxLength(50);
            builder.HasIndex(w => w.AccountNumber)
                .IsUnique()
                .HasFilter("\"AccountNumber\" IS NOT NULL");

            builder.Ignore("_isInitialized");
            builder.Ignore(w => w.IsActive);

            builder.HasOne(w => w.Client)
                .WithMany(c => c.Wallets)
                .HasForeignKey(w => w.ClientId)
                .OnDelete(DeleteBehavior.Cascade);



        }
    }
}
