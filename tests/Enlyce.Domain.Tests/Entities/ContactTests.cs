using Enlyce.Domain.Entities;
using Enlyce.Domain.Errors;
using Enlyce.Domain.ValueObjects;

namespace Enlyce.Domain.Tests.Entities;

public sealed class ContactTests
{
    [Fact]
    public void Create_NormalizesEmailAndTrimsName()
    {
        var contact = Contact.Create("  Ana Pérez  ", Email.Create(" ANA@EXAMPLE.COM "));

        Assert.Equal("Ana Pérez", contact.Name);
        Assert.Equal("ana@example.com", contact.Email);
        Assert.True(contact.Active);
    }

    [Fact]
    public void Update_RejectsBlankName()
    {
        var contact = Contact.Create("Ana", Email.Create("ana@example.com"));

        Assert.Throws<DomainError>(() => contact.Update(" ", null));
    }
}
