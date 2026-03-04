import { SiteSettings } from '@/types/entities'

export function applyThemeSettings(settings: SiteSettings): void {
  const root = document.documentElement

  if (settings.primaryColor) {
    root.style.setProperty('--primary', settings.primaryColor)
  }

  if (settings.secondaryColor) {
    root.style.setProperty('--secondary', settings.secondaryColor)
  }
}
