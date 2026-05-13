export const HELP_CATEGORIES = [
  'PremiersPas',
  'Pages',
  'Menus',
  'Medias',
  'Membres',
  'Groupes',
  'Sauvegardes',
  'Parametres'
] as const

export type HelpCategory = typeof HELP_CATEGORIES[number]

export class HelpArticle {
  id?: string
  title: string = ''
  slug: string = ''
  category: HelpCategory = 'PremiersPas'
  content: string | null = null
  contentMode: string = 'html'
  sortOrder: number = 0
  isPublished: boolean = false
  routeHint: string | null = null
  createdAt?: string
  updatedAt?: string
}
