import type { SocialLink } from "./socialLink"
import type { FooterPartner } from "./footerPartner"

export class SiteSettings {
  id?: string
  siteTitle?: string
  tagline?: string
  primaryColor?: string
  secondaryColor?: string
  logoMediaFileId?: string
  logoUrl?: string
  faviconMediaFileId?: string
  faviconUrl?: string
  headingFont?: string
  bodyFont?: string
  footerDescription?: string
  footerAddress?: string
  footerCity?: string
  footerPhone?: string
  footerEmail?: string
  facebookUrl?: string
  instagramUrl?: string
  copyrightText?: string
  isMaintenanceMode?: boolean
  maintenanceMessage?: string
  maintenanceRetryAfter?: number
  socialLinks?: SocialLink[]
  footerPartners?: FooterPartner[]
}
