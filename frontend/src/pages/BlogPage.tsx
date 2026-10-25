import { useState, type FormEvent } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { BlogPost, BlogPostInput } from "@/lib/types"
import { useAuthStore } from "@/stores/authStore"

const emptyPost: BlogPostInput = {
  slug: "", title: "", summary: "", body: "", coverImageUrl: null,
  seoTitle: "", seoDescription: "", isPublished: false,
}

export function BlogPage() {
  const user = useAuthStore((state) => state.user)
  const queryClient = useQueryClient()
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [draft, setDraft] = useState<BlogPostInput>(emptyPost)
  const posts = useQuery({ queryKey: ["blog-posts"], queryFn: () => api.getBlogPosts(), enabled: user?.rol === "Administrador" })
  const save = useMutation({
    mutationFn: () => selectedId ? api.updateBlogPost(selectedId, draft) : api.createBlogPost(draft),
    onSuccess: async (saved) => {
      setSelectedId(saved.id)
      await queryClient.invalidateQueries({ queryKey: ["blog-posts"] })
    },
  })

  if (user?.rol !== "Administrador") return <p className="p-8">No tienes permiso para editar el blog.</p>

  const select = (post: BlogPost) => {
    setSelectedId(post.id)
    setDraft({ slug: post.slug, title: post.title, summary: post.summary, body: post.body,
      coverImageUrl: post.coverImageUrl, seoTitle: post.seoTitle, seoDescription: post.seoDescription,
      isPublished: post.isPublished })
  }
  const update = <K extends keyof BlogPostInput>(key: K, value: BlogPostInput[K]) =>
    setDraft((current) => ({ ...current, [key]: value }))
  const submit = (event: FormEvent) => { event.preventDefault(); save.mutate() }

  return <main className="space-y-6 p-6">
    <header><p className="crm-eyebrow">Sitio público</p><h1 className="font-display text-3xl">Blog editorial</h1>
      <p className="text-sm text-muted-foreground">Redacta en texto plano. Los borradores solo son visibles en este panel.</p></header>
    <div className="grid gap-6 xl:grid-cols-[18rem_1fr]">
      <aside className="crm-panel space-y-2 p-4" aria-label="Artículos">
        <button type="button" className="w-full rounded border p-2 text-left" onClick={() => { setSelectedId(null); setDraft(emptyPost) }}>Nuevo artículo</button>
        {posts.isLoading && <p>Cargando artículos...</p>}
        {posts.isError && <p role="alert">No se pudieron cargar los artículos.</p>}
        {posts.data?.map((post) => <button key={post.id} type="button" onClick={() => select(post)}
          className="block w-full rounded border p-3 text-left" aria-current={selectedId === post.id ? "true" : undefined}>
          <strong className="block">{post.title}</strong><small>{post.isPublished ? "Publicado" : "Borrador"}</small>
        </button>)}
      </aside>
      <form onSubmit={submit} className="crm-panel grid gap-4 p-6">
        <label>Título <input className="w-full rounded border p-2" required minLength={5} maxLength={200} value={draft.title}
          onChange={(event) => update("title", event.target.value)} /></label>
        <label>URL del artículo <input className="w-full rounded border p-2" required minLength={3} maxLength={120}
          pattern="[a-z0-9]+(-[a-z0-9]+)*" value={draft.slug} onChange={(event) => update("slug", event.target.value)} /></label>
        <label>Resumen <textarea className="w-full rounded border p-2" required minLength={20} maxLength={500} rows={3}
          value={draft.summary} onChange={(event) => update("summary", event.target.value)} /></label>
        <label>Cuerpo del artículo <textarea className="w-full rounded border p-2" required minLength={50} maxLength={20000} rows={14}
          value={draft.body} onChange={(event) => update("body", event.target.value)} /></label>
        <label>URL de portada HTTPS o ruta local <input className="w-full rounded border p-2" maxLength={500}
          value={draft.coverImageUrl ?? ""} onChange={(event) => update("coverImageUrl", event.target.value || null)} /></label>
        <label>Título SEO <input className="w-full rounded border p-2" maxLength={150} value={draft.seoTitle}
          onChange={(event) => update("seoTitle", event.target.value)} /></label>
        <label>Descripción SEO <textarea className="w-full rounded border p-2" maxLength={300} rows={2}
          value={draft.seoDescription} onChange={(event) => update("seoDescription", event.target.value)} /></label>
        <label className="flex gap-2"><input type="checkbox" checked={draft.isPublished}
          onChange={(event) => update("isPublished", event.target.checked)} /> Publicar en el sitio</label>
        <button type="submit" disabled={save.isPending} className="rounded bg-primary px-4 py-2 text-primary-foreground">
          {save.isPending ? "Guardando..." : "Guardar artículo"}
        </button>
        {save.isError && <p role="alert">{save.error instanceof Error ? save.error.message : "No se pudo guardar."}</p>}
        {save.isSuccess && <p role="status">Artículo guardado.</p>}
      </form>
    </div>
  </main>
}
