export class NavigationMenu {
  id?: string
  name?: string
  location?: string
  menuItems?: NavigationMenuItem[]
}

export class NavigationMenuItem {
  id?: string
  menuId?: string
  parentId?: string
  label?: string
  url?: string
  pageId?: string
  pageSlug?: string
  sortOrder?: number
  target?: string
  children?: NavigationMenuItem[]
}
