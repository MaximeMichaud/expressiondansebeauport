<template>
  <Teleport to="body">
    <Transition name="help-drawer-backdrop">
      <div
        v-if="helpDrawer.isOpen"
        class="help-drawer__backdrop"
        @click.self="onBackdropClick"
      />
    </Transition>
    <Transition name="help-drawer-panel">
      <aside
        v-if="helpDrawer.isOpen"
        ref="drawerRef"
        class="help-drawer"
        role="dialog"
        aria-modal="true"
        :aria-labelledby="headerId"
      >
        <header class="help-drawer__header">
          <h2 :id="headerId" class="help-drawer__title">{{ t('components.helpDrawer.title') }}</h2>
          <button
            type="button"
            class="help-drawer__close"
            :aria-label="t('components.helpDrawer.close')"
            @click="helpDrawer.close()"
          >
            <X :size="20" aria-hidden="true" />
          </button>
        </header>

        <div class="help-drawer__search">
          <Search :size="16" class="help-drawer__search-icon" aria-hidden="true" />
          <input
            ref="searchInputRef"
            v-model="searchInput"
            type="search"
            class="help-drawer__search-input"
            :placeholder="t('components.helpDrawer.search.placeholder')"
            :aria-label="t('components.helpDrawer.search.placeholder')"
          />
        </div>

        <div class="help-drawer__body">
          <!-- Squelette de chargement -->
          <div v-if="helpDrawer.isLoading" class="help-drawer__skeleton" aria-busy="true" aria-label="Chargement...">
            <div class="help-drawer__skeleton-line help-drawer__skeleton-line--title" />
            <div class="help-drawer__skeleton-line" />
            <div class="help-drawer__skeleton-line help-drawer__skeleton-line--short" />
            <div class="help-drawer__skeleton-line help-drawer__skeleton-line--title" style="margin-top: 1.5rem" />
            <div class="help-drawer__skeleton-line" />
            <div class="help-drawer__skeleton-line help-drawer__skeleton-line--short" />
          </div>

          <template v-else>
            <!-- Vue recherche -->
            <section v-if="trimmedQuery" class="help-drawer__section">
              <p
                v-if="filteredArticles.length === 0"
                class="help-drawer__empty"
              >
                {{ t('components.helpDrawer.searchEmpty', { query: searchInput.trim() }) }}
              </p>
              <ul v-else class="help-drawer__list">
                <li
                  v-for="article in filteredArticles"
                  :key="article.id ?? article.slug"
                  class="help-drawer__item"
                >
                  <button
                    type="button"
                    class="help-drawer__item-button"
                    @click="onSelectArticle(article)"
                  >
                    <span class="help-drawer__item-title">{{ article.title }}</span>
                    <span
                      v-if="article.category"
                      class="help-drawer__item-category"
                    >{{ t('pages.help.categories.' + article.category) }}</span>
                  </button>
                </li>
              </ul>
            </section>

            <!-- Vue article (contextuel ou sélectionné) -->
            <article
              v-else-if="helpDrawer.currentArticle"
              class="help-drawer__article"
            >
              <button
                type="button"
                class="help-drawer__back"
                @click="onBackToList"
              >
                <ChevronLeft :size="14" aria-hidden="true" />
                {{ t('components.helpDrawer.browseAll') }}
              </button>
              <p v-if="helpDrawer.isCurrentArticleContextual" class="help-drawer__article-context">
                {{ t('components.helpDrawer.contextual') }}
              </p>
              <h3 class="help-drawer__article-title">
                {{ helpDrawer.currentArticle.title }}
              </h3>
              <div
                v-if="helpDrawer.currentArticle.content"
                class="help-drawer__article-content"
                v-html="helpDrawer.currentArticle.content"
              />
            </article>

            <!-- Vue liste par catégorie -->
            <section v-else class="help-drawer__section">
              <p
                v-if="helpDrawer.allArticles.length === 0"
                class="help-drawer__empty"
              >
                {{ t('components.helpDrawer.noArticles') }}
              </p>
              <template v-else>
                <div
                  v-for="group in groupedArticles"
                  :key="group.category"
                  class="help-drawer__group"
                >
                  <h4 class="help-drawer__group-title">
                    {{ t('pages.help.categories.' + group.category) }}
                  </h4>
                  <ul class="help-drawer__list">
                    <li
                      v-for="article in group.articles"
                      :key="article.id ?? article.slug"
                      class="help-drawer__item"
                    >
                      <button
                        type="button"
                        class="help-drawer__item-button"
                        @click="onSelectArticle(article)"
                      >
                        <span class="help-drawer__item-title">{{ article.title }}</span>
                      </button>
                    </li>
                  </ul>
                </div>
              </template>
            </section>
          </template>
        </div>

      </aside>
    </Transition>
  </Teleport>
</template>

<script lang="ts" setup>
import {computed, nextTick, onBeforeUnmount, onMounted, ref, watch} from 'vue'
import {useI18n} from 'vue-i18n'
import {useRoute} from 'vue-router'
import {ChevronLeft, Search, X} from 'lucide-vue-next'

import {useHelpDrawerStore} from '@/stores/helpDrawerStore'
import {HELP_CATEGORIES} from '@/types/entities/helpArticle'
import type {HelpArticle} from '@/types/entities'

const {t} = useI18n()
const route = useRoute()
const helpDrawer = useHelpDrawerStore()

const headerId = 'help-drawer-title'
const drawerRef = ref<HTMLElement | null>(null)
const searchInputRef = ref<HTMLInputElement | null>(null)

const searchInput = ref(helpDrawer.searchQuery ?? '')

watch(searchInput, (value) => {
  helpDrawer.setSearchQuery(value)
})

watch(
  () => helpDrawer.searchQuery,
  (value) => {
    if (value !== searchInput.value) {
      searchInput.value = value
    }
  }
)

const trimmedQuery = computed(() => searchInput.value.trim().toLowerCase())

const filteredArticles = computed<HelpArticle[]>(() => {
  const query = trimmedQuery.value
  if (!query) return []
  return helpDrawer.allArticles.filter((article) => {
    const haystack = `${article.title ?? ''} ${article.content ?? ''}`.toLowerCase()
    return haystack.includes(query)
  })
})

const groupedArticles = computed(() => {
  const groups = new Map<string, HelpArticle[]>()
  for (const article of helpDrawer.allArticles) {
    const key = article.category ?? '—'
    if (!groups.has(key)) groups.set(key, [])
    groups.get(key)!.push(article)
  }

  // Ordre selon HELP_CATEGORIES, catégories vides ignorées
  return HELP_CATEGORIES
    .filter(cat => groups.has(cat))
    .map(cat => ({
      category: cat,
      articles: [...(groups.get(cat) ?? [])].sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0))
    }))
})

function onBackdropClick() {
  helpDrawer.close()
}

function onSelectArticle(article: HelpArticle) {
  helpDrawer.setCurrentArticle(article)
  searchInput.value = ''
}

function onBackToList() {
  helpDrawer.setCurrentArticle(null)
}

function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape' && helpDrawer.isOpen) {
    helpDrawer.close()
  }
}

// Focus initial sur le champ de recherche à l'ouverture
watch(
  () => helpDrawer.isOpen,
  async (open) => {
    if (open) {
      const tasks: Promise<unknown>[] = []
      if (!helpDrawer.hasLoadedAll) {
        tasks.push(helpDrawer.loadAll())
      }
      const routeName = route.name ? String(route.name) : null
      if (routeName && helpDrawer.lastLoadedRouteName !== routeName) {
        tasks.push(helpDrawer.loadForRoute(routeName))
      }
      if (tasks.length > 0) {
        await Promise.all(tasks)
      }
      await nextTick()
      searchInputRef.value?.focus()
    }
  }
)

watch(
  () => route.name,
  async (newName) => {
    if (!helpDrawer.isOpen) return
    const routeName = newName ? String(newName) : null
    if (routeName && helpDrawer.lastLoadedRouteName !== routeName) {
      await helpDrawer.loadForRoute(routeName)
    }
  }
)

onMounted(() => {
  window.addEventListener('keydown', onKeydown)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKeydown)
})
</script>

<style scoped>
.help-drawer__backdrop {
  position: fixed;
  inset: 0;
  z-index: 9998;
  background: rgba(0, 0, 0, 0.45);
  backdrop-filter: blur(2px);
  -webkit-backdrop-filter: blur(2px);
}

.help-drawer {
  position: fixed;
  top: 0;
  right: 0;
  bottom: 0;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  width: min(420px, 100vw);
  height: 100vh;
  background: #ffffff;
  color: #1c1917;
  box-shadow: -10px 0 30px -10px rgba(0, 0, 0, 0.3);
}

/* Écrans larges ≥ 1600px */
@media (min-width: 1600px) {
  .help-drawer {
    width: 480px;
  }
}

/* Mobile */
@media (max-width: 639px) {
  .help-drawer {
    width: 100vw;
    padding-top: env(safe-area-inset-top, 0);
  }
}

.help-drawer__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem 1.25rem;
  border-bottom: 1px solid #e5e7eb;
}

.help-drawer__title {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 700;
  color: #111827;
}

.help-drawer__close {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border: 0;
  border-radius: 9999px;
  background: transparent;
  color: #6b7280;
  cursor: pointer;
  transition: background-color 0.15s, color 0.15s;
  flex-shrink: 0;
}

.help-drawer__close:hover {
  background: #f3f4f6;
  color: #111827;
}

.help-drawer__close:focus-visible {
  outline: 2px solid #b91c1c;
  outline-offset: 2px;
}

.help-drawer__search {
  position: relative;
  padding: 0.75rem 1.25rem;
  border-bottom: 1px solid #f3f4f6;
}

.help-drawer__search-icon {
  position: absolute;
  top: 50%;
  left: 1.75rem;
  transform: translateY(-50%);
  color: #6b7280;
  pointer-events: none;
}

.help-drawer__search-input {
  width: 100%;
  padding: 0.5rem 0.75rem 0.5rem 2.25rem;
  border-radius: 0.5rem;
  border: 1px solid #e5e7eb;
  background: #f9fafb;
  font-size: 0.875rem;
  color: inherit;
  outline: none;
  transition: border-color 0.15s, background-color 0.15s, box-shadow 0.15s;
}

.help-drawer__search-input:focus {
  border-color: #b91c1c;
  background: #ffffff;
  box-shadow: 0 0 0 3px rgba(185, 28, 28, 0.1);
}

.help-drawer__body {
  flex: 1;
  overflow-y: auto;
  padding: 1rem 1.25rem;
  scroll-behavior: smooth;
}

/* Squelette de chargement */
.help-drawer__skeleton {
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
  padding: 0.5rem 0;
}

.help-drawer__skeleton-line {
  height: 0.875rem;
  border-radius: 0.375rem;
  background: linear-gradient(90deg, #f3f4f6 25%, #e5e7eb 50%, #f3f4f6 75%);
  background-size: 200% 100%;
  animation: help-drawer-shimmer 1.4s ease infinite;
}

.help-drawer__skeleton-line--title {
  height: 1rem;
  width: 55%;
  background: linear-gradient(90deg, #e5e7eb 25%, #d1d5db 50%, #e5e7eb 75%);
  background-size: 200% 100%;
  animation: help-drawer-shimmer 1.4s ease infinite;
}

.help-drawer__skeleton-line--short {
  width: 70%;
}

@keyframes help-drawer-shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

.help-drawer__empty {
  font-size: 0.875rem;
  color: #6b7280;
  text-align: center;
  padding: 2rem 1rem;
  margin: 0;
  line-height: 1.5;
}

.help-drawer__group + .help-drawer__group {
  margin-top: 1.25rem;
}

.help-drawer__group-title {
  margin: 0 0 0.375rem;
  font-size: 0.6875rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: #9ca3af;
}

.help-drawer__list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.help-drawer__item + .help-drawer__item {
  margin-top: 0.125rem;
}

.help-drawer__item-button {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 0;
  border-radius: 0.5rem;
  background: transparent;
  color: inherit;
  text-align: left;
  cursor: pointer;
  transition: background-color 0.15s;
}

.help-drawer__item-button:hover {
  background: #f3f4f6;
}

.help-drawer__item-button:focus-visible {
  outline: 2px solid #b91c1c;
  outline-offset: -2px;
}

.help-drawer__item-title {
  font-size: 0.875rem;
  font-weight: 500;
  color: #111827;
}

.help-drawer__item-category {
  font-size: 0.75rem;
  color: #9ca3af;
  margin-top: 0.125rem;
}

.help-drawer__article {
  display: flex;
  flex-direction: column;
}

.help-drawer__back {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  align-self: flex-start;
  margin-bottom: 1rem;
  padding: 0.25rem 0.5rem;
  border: 0;
  border-radius: 0.375rem;
  background: transparent;
  color: #6b7280;
  font-size: 0.75rem;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.15s, color 0.15s;
}

.help-drawer__back:hover {
  background: #f3f4f6;
  color: #111827;
}

.help-drawer__back:focus-visible {
  outline: 2px solid #b91c1c;
  outline-offset: 2px;
}

.help-drawer__article-context {
  margin: 0 0 0.25rem;
  font-size: 0.6875rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: #b91c1c;
}

.help-drawer__article-title {
  margin: 0 0 1rem;
  font-size: 1.0625rem;
  font-weight: 700;
  color: #111827;
  line-height: 1.35;
}

/* Contenu HTML de l'article (TipTap) */
.help-drawer__article-content {
  font-size: 0.9375rem;
  line-height: 1.6;
  color: #374151;
}

.help-drawer__article-content :deep(h1):first-child,
.help-drawer__article-content :deep(h2):first-child,
.help-drawer__article-content :deep(h3):first-child,
.help-drawer__article-content :deep(p):first-child {
  margin-top: 0;
}

.help-drawer__article-content :deep(h2) {
  font-size: 1.125rem;
  font-weight: 700;
  margin-top: 1.25rem;
  margin-bottom: 0.5rem;
  color: #111827;
  line-height: 1.3;
}

.help-drawer__article-content :deep(h3) {
  font-size: 1rem;
  font-weight: 700;
  margin-top: 1rem;
  margin-bottom: 0.375rem;
  color: #111827;
  line-height: 1.3;
}

.help-drawer__article-content :deep(p) {
  margin: 0 0 0.75rem;
  line-height: 1.6;
  color: #374151;
}

.help-drawer__article-content :deep(ul),
.help-drawer__article-content :deep(ol) {
  padding-left: 1.25rem;
  margin: 0 0 0.75rem;
}

.help-drawer__article-content :deep(li) {
  margin-bottom: 0.25rem;
  line-height: 1.5;
}

.help-drawer__article-content :deep(a) {
  color: #b91c1c;
  text-decoration: none;
}

.help-drawer__article-content :deep(a:hover) {
  text-decoration: underline;
}

.help-drawer__article-content :deep(strong) {
  font-weight: 600;
}

.help-drawer__article-content :deep(code) {
  background: #f3f4f6;
  border: 1px solid #e5e7eb;
  border-radius: 0.25rem;
  padding: 0.05rem 0.3rem;
  font-size: 0.875em;
  color: #111827;
}

.help-drawer__article-content :deep(pre) {
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  padding: 0.75rem 1rem;
  overflow-x: auto;
  margin-bottom: 0.75rem;
  font-size: 0.875em;
}

.help-drawer__article-content :deep(blockquote) {
  border-left: 3px solid #e5e7eb;
  padding-left: 1rem;
  color: #6b7280;
  margin: 0 0 0.75rem;
}

/* Transitions */
.help-drawer-backdrop-enter-active,
.help-drawer-backdrop-leave-active {
  transition: opacity 0.25s cubic-bezier(0.25, 0.8, 0.25, 1);
}
.help-drawer-backdrop-enter-from,
.help-drawer-backdrop-leave-to {
  opacity: 0;
}

.help-drawer-panel-enter-active,
.help-drawer-panel-leave-active {
  transition: transform 0.25s cubic-bezier(0.25, 0.8, 0.25, 1);
}
.help-drawer-panel-enter-from,
.help-drawer-panel-leave-to {
  transform: translateX(100%);
}
</style>
