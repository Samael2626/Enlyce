using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class PaymentOrderConfiguration : IEntityTypeConfiguration<PaymentOrder>
{
    public void Configure(EntityTypeBuilder<PaymentOrder> builder)
    {
        builder.ToTable("PaymentOrders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedNever();
        builder.Property(order => order.Reference).HasMaxLength(64).IsRequired();
        builder.Property(order => order.CompanyName).HasMaxLength(160).IsRequired();
        builder.Property(order => order.TaxId).HasMaxLength(32).IsRequired();
        builder.Property(order => order.CustomerEmail).HasMaxLength(254).IsRequired();
        builder.Property(order => order.Currency).HasMaxLength(3).IsRequired();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(order => order.ProviderTransactionId).HasMaxLength(80);
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.UpdatedAt).IsRequired();
        builder.HasIndex(order => order.Reference).IsUnique();
        builder.HasIndex(order => order.ProviderTransactionId).IsUnique();
    }
}
