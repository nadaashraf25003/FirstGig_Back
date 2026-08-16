using FirstGIG.Identity.Domain.Entities;
using FirstGIG.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FirstGIG.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<int>();

        // Email owned value object
        builder.OwnsOne(u => u.Email, emailBuilder =>
        {
            emailBuilder.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(256);

            emailBuilder.HasIndex(e => e.Value)
                .IsUnique();
        });

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(u => u.RefreshTokens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(u => u.EmailVerificationToken).HasMaxLength(64);
        builder.Property(u => u.PasswordResetToken).HasMaxLength(64);

        builder.Property(u => u.CreatedAt).IsRequired();
    }
}
