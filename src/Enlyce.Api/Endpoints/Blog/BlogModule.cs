using System.Security.Claims;
using Enlyce.Domain.Entities;
using Enlyce.Domain.Ports;

namespace Enlyce.Api.Endpoints.Blog;

public static class BlogModule
{
    public static void MapBlog(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/blog", async (IBlogPostRepository repository, CancellationToken ct) =>
            Results.Ok((await repository.ListAsync(false, ct)).Select(ToResponse)))
            .AllowAnonymous().WithName("ListPublicBlogPosts");

        app.MapGet("/api/public/blog/{slug}", async (string slug, IBlogPostRepository repository, CancellationToken ct) =>
        {
            var post = await repository.GetBySlugAsync(slug, false, ct);
            return post is null ? Results.NotFound() : Results.Ok(ToResponse(post));
        }).AllowAnonymous().WithName("GetPublicBlogPost");

        var admin = app.MapGroup("/api/configuracion/blog").RequireAuthorization("Administrador");

        admin.MapGet("/", async (IBlogPostRepository repository, CancellationToken ct) =>
            Results.Ok((await repository.ListAsync(true, ct)).Select(ToResponse)));

        admin.MapPost("/", async (BlogPostRequest request, ClaimsPrincipal user,
            IBlogPostRepository repository, CancellationToken ct) =>
        {
            if (!TryCreate(request, user, out var post, out var error)) return Results.BadRequest(new { error });
            if (await repository.SlugExistsAsync(post!.Slug, null, ct))
                return Results.Conflict(new { error = "Ya existe un artículo con esa URL." });
            await repository.SaveAsync(post, ct);
            return Results.Created($"/api/configuracion/blog/{post.Id}", ToResponse(post));
        });

        admin.MapPut("/{id:guid}", async (Guid id, BlogPostRequest request,
            IBlogPostRepository repository, CancellationToken ct) =>
        {
            var post = await repository.GetByIdAsync(id, ct);
            if (post is null) return Results.NotFound();
            try
            {
                post.Update(request.Slug, request.Title, request.Summary, request.Body,
                    request.CoverImageUrl, request.SeoTitle, request.SeoDescription,
                    request.IsPublished, DateTime.UtcNow);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
            if (await repository.SlugExistsAsync(post.Slug, id, ct))
                return Results.Conflict(new { error = "Ya existe un artículo con esa URL." });
            await repository.SaveAsync(post, ct);
            return Results.Ok(ToResponse(post));
        });
    }

    private static bool TryCreate(BlogPostRequest request, ClaimsPrincipal user,
        out BlogPost? post, out string? error)
    {
        post = null;
        error = null;
        if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            error = "Usuario no identificado.";
            return false;
        }
        try
        {
            post = BlogPost.Create(request.Slug, request.Title, request.Summary, request.Body,
                request.CoverImageUrl, request.SeoTitle, request.SeoDescription,
                request.IsPublished, userId, DateTime.UtcNow);
            return true;
        }
        catch (ArgumentException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static BlogPostResponse ToResponse(BlogPost post) => new(post.Id, post.Slug, post.Title,
        post.Summary, post.Body, post.CoverImageUrl, post.SeoTitle, post.SeoDescription,
        post.IsPublished, post.PublishedAtUtc, post.CreatedAtUtc, post.UpdatedAtUtc);
}

public sealed record BlogPostRequest(string Slug, string Title, string Summary, string Body,
    string? CoverImageUrl, string? SeoTitle, string? SeoDescription, bool IsPublished);

public sealed record BlogPostResponse(Guid Id, string Slug, string Title, string Summary, string Body,
    string? CoverImageUrl, string SeoTitle, string SeoDescription, bool IsPublished,
    DateTime? PublishedAtUtc, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
