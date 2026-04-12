import { ref, onMounted, onUnmounted, type Ref, nextTick } from 'vue'

interface UseInfiniteScrollOptions<T> {
  fetchFn: (page: number) => Promise<{ items: T[]; hasMore: boolean }>
  scrollContainer: Ref<HTMLElement | null>
  direction?: 'down' | 'up'
  threshold?: number
}

export function useInfiniteScroll<T>(options: UseInfiniteScrollOptions<T>) {
  const {
    fetchFn,
    scrollContainer,
    direction = 'down',
    threshold = 200,
  } = options

  const items = ref<T[]>([]) as Ref<T[]>
  const loading = ref(false)
  const loadingMore = ref(false)
  const hasMore = ref(true)
  const page = ref(1)

  let scrollHandler: (() => void) | null = null
  let attachedEl: HTMLElement | null = null

  async function load() {
    loading.value = true
    page.value = 1
    hasMore.value = true
    try {
      const result = await fetchFn(1)
      items.value = result.items
      hasMore.value = result.hasMore
    } catch { /* */ }
    loading.value = false
  }

  async function loadMore() {
    if (loadingMore.value || !hasMore.value) return
    loadingMore.value = true
    const nextPage = page.value + 1
    try {
      const result = await fetchFn(nextPage)
      if (direction === 'up') {
        // Save scroll position before prepending
        const el = scrollContainer.value
        const prevScrollHeight = el?.scrollHeight ?? 0

        items.value = [...result.items, ...items.value]

        // Restore scroll position after DOM update
        await nextTick()
        if (el) {
          const newScrollHeight = el.scrollHeight
          el.scrollTop = newScrollHeight - prevScrollHeight
        }
      } else {
        items.value = [...items.value, ...result.items]
      }
      hasMore.value = result.hasMore
      page.value = nextPage
    } catch { /* */ }
    loadingMore.value = false
  }

  function onScroll() {
    const el = scrollContainer.value
    if (!el || loadingMore.value || !hasMore.value) return

    if (direction === 'down') {
      const distanceFromBottom = el.scrollHeight - el.scrollTop - el.clientHeight
      if (distanceFromBottom < threshold) {
        loadMore()
      }
    } else {
      // direction === 'up': trigger when near the top
      if (el.scrollTop < threshold) {
        loadMore()
      }
    }
  }

  function attachScroll() {
    const el = scrollContainer.value
    if (!el || scrollHandler) return
    scrollHandler = onScroll
    attachedEl = el
    el.addEventListener('scroll', scrollHandler, { passive: true })
  }

  function detachScroll() {
    if (attachedEl && scrollHandler) {
      attachedEl.removeEventListener('scroll', scrollHandler)
      scrollHandler = null
      attachedEl = null
    }
  }

  // Refresh page 1 for polling — updates existing items in place and prepends/appends new ones
  async function refreshFirst() {
    try {
      const result = await fetchFn(1)
      if (items.value.length === 0) {
        items.value = result.items
        hasMore.value = result.hasMore
        return result.items
      }

      // Find new items not already in the list (by id)
      const existingIds = new Set((items.value as any[]).map((i: any) => i.id))
      const newItems = result.items.filter((i: any) => !existingIds.has(i.id))

      if (direction === 'up') {
        // For messages: new items go at the end (most recent)
        items.value = [...items.value, ...newItems]
      } else {
        // For feeds: new items go at the beginning
        items.value = [...newItems, ...items.value]
      }

      // Update existing items in place (likes, read status, etc.)
      const freshMap = new Map(result.items.map((i: any) => [i.id, i]))
      items.value = items.value.map((item: any) => {
        const fresh = freshMap.get(item.id)
        return fresh ? { ...item, ...fresh } : item
      }) as T[]

      return result.items
    } catch {
      return []
    }
  }

  async function reset() {
    detachScroll()
    await load()
    await nextTick()
    attachScroll()
  }

  onMounted(() => {
    nextTick(() => attachScroll())
  })

  onUnmounted(() => {
    detachScroll()
  })

  return {
    items,
    loading,
    loadingMore,
    hasMore,
    page,
    load,
    loadMore,
    refreshFirst,
    reset,
    attachScroll,
    detachScroll,
  }
}
