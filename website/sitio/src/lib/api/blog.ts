import { apiGet, apiGetOrNull } from "./client";

export interface BlogPost {
  id: string;
  slug: string;
  title: string;
  summary: string;
  body: string;
  coverImageUrl: string | null;
  seoTitle: string;
  seoDescription: string;
  publishedAtUtc: string | null;
}

export function getBlogPosts(): Promise<BlogPost[]> {
  return apiGet<BlogPost[]>("/api/public/blog", { revalidate: 60 });
}

export function getBlogPost(slug: string): Promise<BlogPost | null> {
  return apiGetOrNull<BlogPost>(`/api/public/blog/${encodeURIComponent(slug)}`, { revalidate: 60 });
}
