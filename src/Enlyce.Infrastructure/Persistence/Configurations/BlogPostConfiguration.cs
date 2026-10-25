using Enlyce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Enlyce.Infrastructure.Persistence.Configurations;

public sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");
        builder.HasKey(post => post.Id);
        builder.HasIndex(post => post.Slug).IsUnique();
        builder.HasIndex(post => new { post.IsPublished, post.PublishedAtUtc });
        builder.Property(post => post.Slug).HasMaxLength(120).IsRequired();
        builder.Property(post => post.Title).HasMaxLength(200).IsRequired();
        builder.Property(post => post.Summary).HasMaxLength(500).IsRequired();
        builder.Property(post => post.Body).HasMaxLength(20000).IsRequired();
        builder.Property(post => post.CoverImageUrl).HasMaxLength(500);
        builder.Property(post => post.SeoTitle).HasMaxLength(150).IsRequired();
        builder.Property(post => post.SeoDescription).HasMaxLength(300).IsRequired();
        builder.Property(post => post.AuthorUserId).IsRequired();
        builder.Property(post => post.CreatedAtUtc).IsRequired();
        builder.Property(post => post.UpdatedAtUtc).IsRequired();
    }
}
