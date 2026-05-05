import { SiteSettings } from '@/types/entities'

export function applyThemeSettings(settings: SiteSettings): void {
  const root = document.documentElement

  if (settings.primaryColor) {
    root.style.setProperty('--primary', settings.primaryColor)
    root.style.setProperty('--primary-foreground', getReadableTextColor(settings.primaryColor))
    root.style.setProperty('--primary-readable', getReadableAccentColor(settings.primaryColor))
  }

  if (settings.secondaryColor) {
    root.style.setProperty('--secondary', settings.secondaryColor)
  }
}

function getReadableTextColor(backgroundColor: string): string {
  const rgb = parseHexColor(backgroundColor)
  if (!rgb) return '#ffffff'

  const whiteContrast = getContrastRatio(rgb, [255, 255, 255])
  const blackContrast = getContrastRatio(rgb, [17, 24, 39])

  return whiteContrast >= blackContrast ? '#ffffff' : '#111827'
}

function getReadableAccentColor(color: string): string {
  const rgb = parseHexColor(color)
  if (!rgb) return color

  return getContrastRatio(rgb, [255, 255, 255]) >= 4.5 ? color : '#111827'
}

function parseHexColor(color: string): [number, number, number] | null {
  const normalized = color.trim().replace(/^#/, '')
  const hex = normalized.length === 3
    ? normalized.split('').map(value => value + value).join('')
    : normalized

  if (!/^[0-9a-fA-F]{6}$/.test(hex)) return null

  return [
    Number.parseInt(hex.slice(0, 2), 16),
    Number.parseInt(hex.slice(2, 4), 16),
    Number.parseInt(hex.slice(4, 6), 16),
  ]
}

function getContrastRatio(first: [number, number, number], second: [number, number, number]): number {
  const lighter = Math.max(getRelativeLuminance(first), getRelativeLuminance(second))
  const darker = Math.min(getRelativeLuminance(first), getRelativeLuminance(second))

  return (lighter + 0.05) / (darker + 0.05)
}

function getRelativeLuminance(rgb: [number, number, number]): number {
  const [red, green, blue] = rgb.map(value => {
    const channel = value / 255
    return channel <= 0.03928
      ? channel / 12.92
      : Math.pow((channel + 0.055) / 1.055, 2.4)
  })

  return 0.2126 * red + 0.7152 * green + 0.0722 * blue
}
