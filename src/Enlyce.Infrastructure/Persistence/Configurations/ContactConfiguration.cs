using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");
        builder.HasKey(contact => contact.Id);
        builder.Property(contact => contact.Id).ValueGeneratedNever();
        builder.Property(contact => contact.Name).HasMaxLength(200).IsRequired();
        builder.Property(contact => contact.Surname).HasMaxLength(200);
        builder.Property(contact => contact.CustomerType).HasMaxLength(30);
        builder.Property(contact => contact.CustomerStatus).HasMaxLength(20).IsRequired();
        builder.Property(contact => contact.CustomerFunnel).HasMaxLength(100).HasDefaultValue("Clientes").IsRequired();
        builder.Property(contact => contact.Email).HasMaxLength(320);
        builder.Property(contact => contact.Address).HasMaxLength(500);
        builder.Property(contact => contact.Country).HasMaxLength(100).IsRequired();
        builder.Property(contact => contact.Department).HasMaxLength(100);
        builder.Property(contact => contact.City).HasMaxLength(100);
        builder.ComplexProperty(contact => contact.Phone, phone =>
            phone.Property(value => value.Value).HasColumnName("Phone").HasMaxLength(20));
        builder.ComplexProperty(contact => contact.Landline, phone =>
            phone.Property(value => value.Value).HasColumnName("Landline").HasMaxLength(20));
        builder.Property(contact => contact.DateOfBirth);
        builder.Property(contact => contact.IdentificationNumber).HasMaxLength(50);
        builder.Property(contact => contact.ReferredBy).HasMaxLength(200);
        builder.Property(contact => contact.AcquisitionMedium).HasMaxLength(200);
        builder.Property(contact => contact.AdditionalData).HasMaxLength(4000);
        builder.Property(contact => contact.CreatedAt).IsRequired();
        builder.Property(contact => contact.UpdatedAt).IsRequired();
        builder.Property(contact => contact.Active).IsRequired();
        builder.Property(contact => contact.CanReceiveAutomatedEmail).IsRequired();
        builder.HasIndex(contact => contact.Email).HasFilter("\"Email\" IS NOT NULL");
        builder.HasOne<Asesor>().WithMany().HasForeignKey(contact => contact.OwnerAdvisorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
