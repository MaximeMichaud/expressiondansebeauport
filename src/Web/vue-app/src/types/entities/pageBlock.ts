export interface PageBlock {
  id: string
  type: BlockType
  data: Record<string, any>
}

export type BlockType = 'rich-text' | 'google-map' | 'image-gallery' | 'hero' | 'faq' | 'cta-button'

export interface RichTextBlockData {
  html: string
}

export interface GoogleMapBlockData {
  embedUrl: string
  address?: string
  height?: number
}

export interface ImageGalleryBlockData {
  images: { url: string; alt: string }[]
  columns?: number
}

export interface HeroBlockData {
  title: string
  subtitle?: string
  backgroundImageUrl?: string
  ctaLabel?: string
  ctaUrl?: string
  overlayOpacity?: number
}

export interface FaqBlockData {
  items: { question: string; answer: string }[]
}

export interface CtaButtonBlockData {
  label: string
  url: string
  style?: 'primary' | 'secondary'
  alignment?: 'left' | 'center' | 'right'
  openInNewTab?: boolean
}

export const BLOCK_LABELS: Record<BlockType, string> = {
  'rich-text': 'Texte riche',
  'google-map': 'Carte Google Maps',
  'image-gallery': 'Galerie d\'images',
  'hero': 'Section hero',
  'faq': 'FAQ (Accordion)',
  'cta-button': 'Bouton d\'action'
}
