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
        builder.Property(contact => contact.Email).HasMaxLength(320).IsRequired();
        builder.ComplexProperty(contact => contact.Phone, phone =>
            phone.Property(value => value.Value).HasColumnName("Phone").HasMaxLength(20));
        builder.Property(contact => contact.CreatedAt).IsRequired();
        builder.Property(contact => contact.UpdatedAt).IsRequired();
        builder.Property(contact => contact.Active).IsRequired();
        builder.HasIndex(contact => contact.Email).IsUnique();
    }
}
