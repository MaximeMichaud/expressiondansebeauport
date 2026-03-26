import {defineStore} from 'pinia'

interface SiteSettingsState {
  siteTitle: string
}

export const useSiteSettingsStore = defineStore('siteSettings', {
  state: (): SiteSettingsState => ({
    siteTitle: ''
  }),

  actions: {
    setSiteTitle(title: string) {
      this.siteTitle = title
    }
  }
})
