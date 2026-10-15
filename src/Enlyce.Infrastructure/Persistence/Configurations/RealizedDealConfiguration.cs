using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class RealizedDealConfiguration : IEntityTypeConfiguration<RealizedDeal>
{
    public void Configure(EntityTypeBuilder<RealizedDeal> builder)
    {
        builder.ToTable("RealizedDeals", table =>
        {
            table.HasCheckConstraint("CK_RealizedDeals_GrossAmount", "\"GrossAmount\" > 0");
            table.HasCheckConstraint("CK_RealizedDeals_CommissionAmount", "\"CommissionAmount\" >= 0");
            table.HasCheckConstraint("CK_RealizedDeals_Currency", "char_length(\"Currency\") = 3");
        });
        builder.HasKey(deal => deal.Id);
        builder.Property(deal => deal.Id).ValueGeneratedNever();
        builder.Property(deal => deal.Operation).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(deal => deal.GrossAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(deal => deal.CommissionAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(deal => deal.Currency).HasMaxLength(3).IsRequired();
        builder.Property(deal => deal.ClosedOn).IsRequired();
        builder.Property(deal => deal.RecordedAtUtc).IsRequired();

        builder.HasOne<Inmueble>().WithMany().HasForeignKey(deal => deal.PropertyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Contact>().WithMany().HasForeignKey(deal => deal.ContactId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Lead>().WithMany().HasForeignKey(deal => deal.LeadId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Asesor>().WithMany().HasForeignKey(deal => deal.AdvisorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(deal => deal.ClosedOn);
        builder.HasIndex(deal => deal.AdvisorId);
        builder.HasIndex(deal => deal.PropertyId);
    }
}
