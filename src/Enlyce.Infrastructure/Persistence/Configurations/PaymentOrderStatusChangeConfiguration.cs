using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class PaymentOrderStatusChangeConfiguration : IEntityTypeConfiguration<PaymentOrderStatusChange>
{
    public void Configure(EntityTypeBuilder<PaymentOrderStatusChange> builder)
    {
        builder.ToTable("PaymentOrderStatusChanges");
        builder.HasKey(change => change.Id);
        builder.Property(change => change.Id).ValueGeneratedNever();
        builder.Property(change => change.PreviousStatus).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(change => change.NewStatus).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(change => change.ProviderTransactionId).HasMaxLength(80).IsRequired();
        builder.Property(change => change.OccurredAt).IsRequired();
        builder.HasIndex(change => new { change.PaymentOrderId, change.OccurredAt });
    }
}
