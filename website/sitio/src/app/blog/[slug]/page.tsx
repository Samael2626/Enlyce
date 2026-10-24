import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { getBlogPost } from "@/lib/api/blog";

export const dynamic = "force-dynamic";

type Props = { params: Promise<{ slug: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug } = await params;
  const post = await getBlogPost(slug);
  if (!post) return { title: "Artículo no encontrado" };
  return {
    title: post.seoTitle,
    description: post.seoDescription,
    alternates: { canonical: `/blog/${post.slug}` },
    openGraph: {
      type: "article", title: post.seoTitle, description: post.seoDescription,
      url: `/blog/${post.slug}`,
      images: post.coverImageUrl ? [{ url: post.coverImageUrl, alt: post.title }] : undefined,
      publishedTime: post.publishedAtUtc ?? undefined,
    },
  };
}

export default async function BlogPostPage({ params }: Props) {
  const { slug } = await params;
  const post = await getBlogPost(slug);
  if (!post) notFound();

  return <main className="editorial-blog editorial-blog-article">
    <Link href="/blog">← Volver al blog</Link>
    <article>
      <header>
        <p className="section-kicker">Blog inmobiliario</p>
        <h1>{post.title}</h1>
        <p>{post.summary}</p>
        {post.publishedAtUtc && <time dateTime={post.publishedAtUtc}>
          {new Intl.DateTimeFormat("es-CO", { dateStyle: "long" }).format(new Date(post.publishedAtUtc))}
        </time>}
      </header>
      {post.coverImageUrl && <img className="editorial-blog-cover" src={post.coverImageUrl} alt="" />}
      <div className="editorial-blog-body">{post.body}</div>
    </article>
  </main>;
}
