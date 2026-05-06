export interface PageBlock {
  id: string
  type: BlockType
  data: Record<string, any>
  width?: BlockWidth
}

export type BlockType = 'rich-text' | 'google-map' | 'image-gallery' | 'hero' | 'faq' | 'cta-button' | 'contact-form'

export type BlockWidth = 'full' | 'half'

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
  backgroundColor?: string
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

export interface ContactFormBlockData {
  title: string
  introText?: string
  submitLabel?: string
  successMessage?: string
  recipientEmail?: string
  enabled?: boolean
}

export function createDefaultContactFormBlockData(): ContactFormBlockData {
  return {
    title: 'Contactez-nous',
    introText: 'Vous avez une question? Envoyez-nous un message et nous vous répondrons dès que possible.',
    submitLabel: 'Envoyer',
    successMessage: 'Votre message a été envoyé.',
    recipientEmail: '',
    enabled: true,
  }
}

export function normalizeContactFormConfig(config: unknown): ContactFormBlockData {
  const defaults = createDefaultContactFormBlockData()

  if (config == null) {
    return defaults
  }

  if (typeof config === 'string') {
    const trimmed = config.trim()
    if (!trimmed) {
      return defaults
    }

    try {
      const parsed = JSON.parse(trimmed)
      return normalizeContactFormConfig(parsed)
    } catch {
      return defaults
    }
  }

  if (typeof config !== 'object' || Array.isArray(config)) {
    return defaults
  }

  const source = config as Record<string, unknown>

  return {
    title: typeof source.title === 'string' && source.title.trim() ? source.title : defaults.title,
    introText: typeof source.introText === 'string' ? source.introText : defaults.introText,
    submitLabel: typeof source.submitLabel === 'string' && source.submitLabel.trim() ? source.submitLabel : defaults.submitLabel,
    successMessage: typeof source.successMessage === 'string' && source.successMessage.trim() ? source.successMessage : defaults.successMessage,
    recipientEmail: typeof source.recipientEmail === 'string' ? source.recipientEmail : defaults.recipientEmail,
    enabled: typeof source.enabled === 'boolean' ? source.enabled : defaults.enabled,
  }
}

export const BLOCK_LABELS: Record<BlockType, string> = {
  'rich-text': 'Texte riche',
  'google-map': 'Carte Google Maps',
  'image-gallery': 'Galerie d\'images',
  'hero': 'Section hero',
  'faq': 'FAQ (Accordion)',
  'cta-button': 'Bouton d\'action',
  'contact-form': 'Formulaire de contact'
}
