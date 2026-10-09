using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class CustomerDemandConfiguration : IEntityTypeConfiguration<CustomerDemand>
{
    public void Configure(EntityTypeBuilder<CustomerDemand> builder)
    {
        builder.ToTable("CustomerDemands");
        builder.HasKey(demand => demand.Id);
        builder.Property(demand => demand.Id).ValueGeneratedNever();
        builder.Property(demand => demand.Operation).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(demand => demand.PropertyType).HasConversion<string>().HasMaxLength(20);
        builder.Property(demand => demand.City).HasMaxLength(100).IsRequired();
        builder.Property(demand => demand.Neighborhood).HasMaxLength(100);
        builder.Property(demand => demand.MinimumPrice).HasPrecision(18, 2);
        builder.Property(demand => demand.MaximumPrice).HasPrecision(18, 2);
        builder.Property(demand => demand.Notes).HasMaxLength(2_000);
        builder.Property(demand => demand.CreatedAt).IsRequired();
        builder.Property(demand => demand.UpdatedAt).IsRequired();
        builder.Property(demand => demand.Active).IsRequired();
        builder.HasOne<Contact>().WithMany().HasForeignKey(demand => demand.ContactId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Lead>().WithMany().HasForeignKey(demand => demand.LeadId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(demand => new { demand.ContactId, demand.Active });
        builder.HasIndex(demand => new { demand.City, demand.PropertyType, demand.Operation });
    }
}

public sealed class DemandPropertyLinkConfiguration : IEntityTypeConfiguration<DemandPropertyLink>
{
    public void Configure(EntityTypeBuilder<DemandPropertyLink> builder)
    {
        builder.ToTable("DemandPropertyLinks");
        builder.HasKey(link => new { link.DemandId, link.PropertyId });
        builder.Property(link => link.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(link => link.LinkedAt).IsRequired();
        builder.Property(link => link.UpdatedAt).IsRequired();
        builder.HasOne<CustomerDemand>().WithMany().HasForeignKey(link => link.DemandId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Inmueble>().WithMany().HasForeignKey(link => link.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(link => link.PropertyId);
    }
}
