using FirstGIG.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstGIG.Identity.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id).ValueGeneratedNever();

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(rt => rt.Token).IsUnique();

        builder.Property(rt => rt.CreatedByIp)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(rt => rt.ExpiresAt).IsRequired();
    }
}
