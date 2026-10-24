import type { Metadata } from "next";
import Link from "next/link";
import { getBlogPosts } from "@/lib/api/blog";

export const dynamic = "force-dynamic";

export const metadata: Metadata = {
  title: "Blog inmobiliario | L&C Propiedad Raíz",
  description: "Guías y consejos para comprar, arrendar y vender inmuebles en el Valle de Aburrá.",
  alternates: { canonical: "/blog" },
};

export default async function BlogPage() {
  const posts = await getBlogPosts();
  return <main className="editorial-blog">
    <header className="editorial-blog-header">
      <p className="section-kicker">Conocimiento inmobiliario</p>
      <h1>Decisiones con criterio.</h1>
      <p>Guías de nuestro equipo para entender el mercado y elegir mejor.</p>
    </header>
    {posts.length === 0 ? <p>Aún no hay artículos publicados.</p> :
      <div className="editorial-blog-grid">{posts.map((post) =>
        <article key={post.id} className="editorial-blog-card">
          {post.coverImageUrl && <img src={post.coverImageUrl} alt="" />}
          <div>
            <time dateTime={post.publishedAtUtc ?? undefined}>{post.publishedAtUtc ?
              new Intl.DateTimeFormat("es-CO", { dateStyle: "long" }).format(new Date(post.publishedAtUtc)) : ""}</time>
            <h2><Link href={`/blog/${post.slug}`}>{post.title}</Link></h2>
            <p>{post.summary}</p>
            <Link href={`/blog/${post.slug}`}>Leer artículo</Link>
          </div>
        </article>)}</div>}
  </main>;
}
