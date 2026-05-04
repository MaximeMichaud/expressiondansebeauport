import { defineStore } from 'pinia'

interface AvatarRegistryState {
  byMemberId: Record<string, string | null>
}

export const useAvatarRegistryStore = defineStore('avatarRegistry', {
  state: (): AvatarRegistryState => ({
    byMemberId: {}
  }),

  actions: {
    setAvatar(memberId: string, url: string | null | undefined) {
      if (!memberId) return
      this.byMemberId[memberId] = url ?? null
    },

    clearAvatar(memberId: string) {
      if (!memberId) return
      this.byMemberId[memberId] = null
    },

    populateFromList<T extends Record<string, any>>(
      items: T[] | null | undefined,
      idKey: keyof T,
      urlKey: keyof T
    ) {
      if (!items) return
      for (const item of items) {
        const id = item?.[idKey] as string | undefined
        if (!id) continue
        this.byMemberId[id] = (item[urlKey] ?? null) as string | null
      }
    },

    populateOne<T extends Record<string, any>>(
      item: T | null | undefined,
      idKey: keyof T,
      urlKey: keyof T
    ) {
      if (!item) return
      const id = item[idKey] as string | undefined
      if (!id) return
      this.byMemberId[id] = (item[urlKey] ?? null) as string | null
    }
  },

  getters: {
    getAvatar: (state) => (memberId: string | undefined | null, fallback?: string | null): string | null => {
      if (!memberId) return fallback ?? null
      if (memberId in state.byMemberId) return state.byMemberId[memberId]
      return fallback ?? null
    }
  }
})
