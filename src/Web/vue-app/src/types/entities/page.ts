export class Page {
  id?: string
  title?: string
  slug?: string
  content?: string
  customCss?: string
  status?: string
  featuredImageId?: string
  featuredImageUrl?: string
  metaDescription?: string
  sortOrder?: number
  contentMode?: string
  blocks?: string  // JSON string, parsed côté composant
}
