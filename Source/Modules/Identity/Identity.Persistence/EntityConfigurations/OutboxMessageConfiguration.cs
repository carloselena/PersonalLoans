using Identity.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.EntityConfigurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(om => om.Id);
        
        builder.HasIndex(om => om.ProcessedAt);
        
        builder.Property(om => om.Type)
            .HasMaxLength(256)
            .IsRequired();
        
        builder.Property(om => om.Payload)
            .IsRequired();
        
        builder.Property(om => om.Error)
            .HasMaxLength(2048)
            .IsRequired(false);
    }
}