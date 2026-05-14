import {defineStore} from 'pinia'
import {ref} from 'vue'

import {useHelpArticleService} from '@/serviceRegistry'
import type {HelpArticle} from '@/types/entities'

export const useHelpDrawerStore = defineStore('helpDrawer', () => {
  const isOpen = ref(false)
  const currentArticle = ref<HelpArticle | null>(null)
  const isCurrentArticleContextual = ref(false)
  const isLoading = ref(false)
  const allArticles = ref<HelpArticle[]>([])
  const searchQuery = ref('')
  const hasLoadedAll = ref(false)
  const lastLoadedRouteName = ref<string | null>(null)
  const canEdit = ref(false)
  const permissionsLoaded = ref(false)
  let _routeLoadSeq = 0

  function open() {
    isOpen.value = true
  }

  function close() {
    isOpen.value = false
  }

  function toggle() {
    isOpen.value = !isOpen.value
  }

  function setSearchQuery(value: string) {
    searchQuery.value = value
  }

  function setCurrentArticle(article: HelpArticle | null) {
    currentArticle.value = article
    isCurrentArticleContextual.value = false
    lastLoadedRouteName.value = null
  }

  function invalidate() {
    hasLoadedAll.value = false
    lastLoadedRouteName.value = null
    currentArticle.value = null
    isCurrentArticleContextual.value = false
  }

  async function loadPermissions() {
    if (permissionsLoaded.value) return
    try {
      const service = useHelpArticleService()
      const permissions = await service.getPermissions()
      canEdit.value = permissions.canEdit
    } catch {
      canEdit.value = false
    } finally {
      permissionsLoaded.value = true
    }
  }

  let _loadAllInFlight: Promise<void> | null = null

  async function loadAll(force = false) {
    if (hasLoadedAll.value && !force) return
    if (_loadAllInFlight) return _loadAllInFlight
    isLoading.value = true
    _loadAllInFlight = (async () => {
      try {
        const service = useHelpArticleService()
        const articles = await service.getAll(undefined, true)
        allArticles.value = articles ?? []
        hasLoadedAll.value = true
      } catch {
        allArticles.value = []
      } finally {
        isLoading.value = false
        _loadAllInFlight = null
      }
    })()
    return _loadAllInFlight
  }

  async function loadForRoute(routeName: string | null | undefined) {
    if (!routeName) {
      currentArticle.value = null
      isCurrentArticleContextual.value = false
      lastLoadedRouteName.value = null
      return
    }
    const seq = ++_routeLoadSeq
    isLoading.value = true
    try {
      const service = useHelpArticleService()
      const article = await service.getByRoute(routeName)
      if (seq !== _routeLoadSeq) return
      currentArticle.value = article
      isCurrentArticleContextual.value = article !== null
      lastLoadedRouteName.value = routeName
    } catch {
      if (seq === _routeLoadSeq) {
        currentArticle.value = null
        isCurrentArticleContextual.value = false
      }
    } finally {
      if (seq === _routeLoadSeq) isLoading.value = false
    }
  }

  async function loadById(id: string) {
    if (!id) return
    isLoading.value = true
    try {
      const service = useHelpArticleService()
      const article = await service.getById(id)
      currentArticle.value = article
      isCurrentArticleContextual.value = false
    } catch {
      currentArticle.value = null
      isCurrentArticleContextual.value = false
    } finally {
      isLoading.value = false
    }
  }

  function reset() {
    _routeLoadSeq++
    isOpen.value = false
    currentArticle.value = null
    isCurrentArticleContextual.value = false
    isLoading.value = false
    allArticles.value = []
    searchQuery.value = ''
    hasLoadedAll.value = false
    lastLoadedRouteName.value = null
    canEdit.value = false
    permissionsLoaded.value = false
  }

  return {
    isOpen,
    currentArticle,
    isCurrentArticleContextual,
    isLoading,
    allArticles,
    searchQuery,
    hasLoadedAll,
    lastLoadedRouteName,
    canEdit,
    open,
    close,
    toggle,
    setSearchQuery,
    setCurrentArticle,
    invalidate,
    loadAll,
    loadForRoute,
    loadById,
    loadPermissions,
    reset
  }
})
