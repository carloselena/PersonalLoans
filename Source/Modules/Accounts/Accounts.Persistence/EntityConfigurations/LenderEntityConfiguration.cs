using Accounts.Domain.Lenders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounts.Persistence.EntityConfigurations;

public class LenderEntityConfiguration : IEntityTypeConfiguration<Lender>
{
    public void Configure(EntityTypeBuilder<Lender> builder)
    {
        builder.HasKey(l => l.Id);
        
        builder.HasIndex(l => l.UserId)
            .IsUnique();
        
        builder.Property(l => l.Id)
            .ValueGeneratedNever()
            .HasColumnOrder(0);

        builder.Property(l => l.UserId)
            .HasColumnOrder(1)
            .IsRequired();
    }
}