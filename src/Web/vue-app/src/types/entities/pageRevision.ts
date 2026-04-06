export interface PageRevision {
  id: string
  pageId: string
  title: string
  content?: string
  customCss?: string
  contentMode: string
  blocks?: string
  metaDescription?: string
  status: string
  revisionNumber: number
  revisionType: string
  createdAt?: string
  createdBy?: string
}

export interface PageRevisionListItem {
  id: string
  revisionNumber: number
  revisionType: string
  createdAt?: string
  createdBy?: string
  title: string
}
