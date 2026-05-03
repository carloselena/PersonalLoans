using Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.EntityConfigurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .ValueGeneratedNever()
            .HasColumnOrder(0);

        builder.Property(t => t.UserId)
            .HasColumnOrder(1)
            .IsRequired();
        
        builder.Property(t => t.TokenHash)
            .HasMaxLength(64)
            .HasColumnOrder(2)
            .IsRequired();

        builder.Property(t => t.ExpiresAt)
            .HasColumnOrder(3)
            .IsRequired();

        builder.Property(t => t.RevokedAt)
            .HasColumnOrder(4)
            .IsRequired(false);

        builder.Property(t => t.CreatedAt)
            .HasColumnOrder(5)
            .IsRequired();

        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.UserId);

        builder.HasOne<User>()
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}