using System.Text.RegularExpressions;

namespace Enlyce.Domain.Entities;

public sealed class BlogPost
{
    private static readonly Regex SlugPattern = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public Guid Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? CoverImageUrl { get; private set; }
    public string SeoTitle { get; private set; } = string.Empty;
    public string SeoDescription { get; private set; } = string.Empty;
    public bool IsPublished { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid AuthorUserId { get; private set; }

    private BlogPost() { }

    public static BlogPost Create(string slug, string title, string summary, string body,
        string? coverImageUrl, string? seoTitle, string? seoDescription, bool isPublished,
        Guid authorUserId, DateTime nowUtc)
    {
        if (authorUserId == Guid.Empty) throw new ArgumentException("Author is required.", nameof(authorUserId));
        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            AuthorUserId = authorUserId,
            CreatedAtUtc = nowUtc
        };
        post.Update(slug, title, summary, body, coverImageUrl, seoTitle, seoDescription, isPublished, nowUtc);
        return post;
    }

    public void Update(string slug, string title, string summary, string body,
        string? coverImageUrl, string? seoTitle, string? seoDescription, bool isPublished, DateTime nowUtc)
    {
        slug = slug.Trim().ToLowerInvariant();
        title = title.Trim();
        summary = summary.Trim();
        body = body.Trim();
        seoTitle = string.IsNullOrWhiteSpace(seoTitle) ? title : seoTitle.Trim();
        seoDescription = string.IsNullOrWhiteSpace(seoDescription) ? summary : seoDescription.Trim();
        coverImageUrl = string.IsNullOrWhiteSpace(coverImageUrl) ? null : coverImageUrl.Trim();

        if (slug.Length is < 3 or > 120 || !SlugPattern.IsMatch(slug))
            throw new ArgumentException("Slug must contain lowercase letters, numbers and hyphens.", nameof(slug));
        if (TitleInvalid(title) || summary.Length is < 20 or > 500 || body.Length is < 50 or > 20000 ||
            seoTitle.Length > 150 || seoDescription.Length > 300 || coverImageUrl?.Length > 500)
            throw new ArgumentException("Blog content has invalid length.");
        if (coverImageUrl is not null)
        {
            if (!Uri.TryCreate(coverImageUrl, UriKind.RelativeOrAbsolute, out var coverUri))
                throw new ArgumentException("Cover URL is invalid.", nameof(coverImageUrl));
            if (coverUri.IsAbsoluteUri && coverUri.Scheme != Uri.UriSchemeHttps)
                throw new ArgumentException("Cover URL must use HTTPS.", nameof(coverImageUrl));
            if (!coverUri.IsAbsoluteUri && !coverImageUrl.StartsWith('/'))
                throw new ArgumentException("Cover URL must be an absolute path.", nameof(coverImageUrl));
        }

        Slug = slug;
        Title = title;
        Summary = summary;
        Body = body;
        CoverImageUrl = coverImageUrl;
        SeoTitle = seoTitle;
        SeoDescription = seoDescription;
        IsPublished = isPublished;
        PublishedAtUtc = isPublished ? PublishedAtUtc ?? nowUtc : null;
        UpdatedAtUtc = nowUtc;
    }

    private static bool TitleInvalid(string title) => title.Length is < 5 or > 200;
}
