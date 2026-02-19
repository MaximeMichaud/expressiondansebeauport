export class Page {
  id?: string
  title?: string
  slug?: string
  isHomePage?: boolean
  sections?: PageSection[]
}

export class PageSection {
  id?: string
  title?: string
  htmlContent?: string
  imageUrl?: string
  sortOrder?: number
}
