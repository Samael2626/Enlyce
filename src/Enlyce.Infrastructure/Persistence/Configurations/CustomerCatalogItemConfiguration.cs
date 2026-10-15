using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class CustomerCatalogItemConfiguration : IEntityTypeConfiguration<CustomerCatalogItem>
{
    public void Configure(EntityTypeBuilder<CustomerCatalogItem> builder)
    {
        builder.ToTable("CustomerCatalogItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Catalog).HasMaxLength(30).IsRequired();
        builder.Property(item => item.Name).HasMaxLength(200).IsRequired();
        builder.Property(item => item.NormalizedName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.CreatedAtUtc).IsRequired();
        builder.HasIndex(item => new { item.Catalog, item.NormalizedName }).IsUnique();
    }
}
