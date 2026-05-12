<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t('pages.help.index.title') }}</h1>
      <div v-if="canEdit" class="hidden md:block">
        <RouterLink
          :to="{ name: 'admin.children.help.add' }"
          class="btn"
        >
          <PlusIcon :size="16" class="help-index__btn-icon" />
          {{ t('pages.help.index.addButton') }}
        </RouterLink>
      </div>
    </div>
    <p class="help-index__subtitle">{{ t('pages.help.index.subtitle') }}</p>

    <div v-if="!canEdit" class="help-index__readonly-banner">
      <InfoIcon :size="16" class="help-index__readonly-icon" />
      {{ t('pages.help.permissions.readOnlyBanner') }}
    </div>

    <div v-if="canEdit" class="md:hidden">
      <RouterLink
        :to="{ name: 'admin.children.help.add' }"
        class="btn mt-10"
      >
        <PlusIcon :size="16" class="help-index__btn-icon" />
        {{ t('pages.help.index.addButton') }}
      </RouterLink>
    </div>

    <Loader v-if="isLoading" />

    <template v-else>
      <!-- État vide global -->
      <div v-if="articles.length === 0" class="help-index__global-empty">
        <BookOpenIcon :size="40" class="help-index__empty-icon" />
        <p class="help-index__empty-title">{{ t('pages.help.index.emptyGlobal') }}</p>
        <p class="help-index__empty-sub">{{ t('pages.help.index.emptyGlobalSub') }}</p>
        <RouterLink :to="{ name: 'admin.children.help.add' }" class="btn help-index__empty-btn">
          <PlusIcon :size="16" class="help-index__btn-icon" />
          {{ t('pages.help.index.emptyGlobalCta') }}
        </RouterLink>
      </div>

      <div v-else class="help-index">
        <section
          v-for="category in nonEmptyCategories"
          :key="category"
          class="help-index__category"
        >
          <button
            type="button"
            class="help-index__category-header"
            @click="toggleCategory(category)"
            :aria-expanded="!collapsed[category]"
          >
            <span class="help-index__category-title">
              {{ t(`pages.help.categories.${category}`) }}
            </span>
            <span class="help-index__category-count">
              {{ articlesByCategory[category]?.length ?? 0 }}
            </span>
            <ChevronRightIcon
              :size="16"
              class="help-index__chevron"
              :class="{ 'help-index__chevron--open': !collapsed[category] }"
            />
          </button>

          <div v-if="!collapsed[category]" class="help-index__articles">
            <ul class="help-index__list">
              <li
                v-for="article in articlesByCategory[category]"
                :key="article.id"
                class="help-index__item"
              >
                <div class="help-index__item-main">
                  <span class="help-index__item-title">{{ article.title }}</span>
                  <span class="help-index__item-slug">/{{ article.slug }}</span>
                </div>
                <div class="help-index__badges">
                  <span v-if="!article.isPublished" class="help-index__badge help-index__badge--draft">
                    {{ t('pages.help.index.draftBadge') }}
                  </span>
                  <span v-if="isRecentlyUpdated(article.updatedAt)" class="help-index__badge help-index__badge--updated">
                    {{ t('pages.help.index.updatedBadge') }}
                  </span>
                </div>
                <div v-if="canEdit" class="help-index__actions">
                  <RouterLink
                    :to="{ name: 'admin.children.help.edit', params: { id: article.id } }"
                    class="help-index__action-btn"
                    :title="t('global.update')"
                  >
                    <PencilIcon :size="15" />
                  </RouterLink>
                  <button
                    type="button"
                    class="help-index__action-btn help-index__action-btn--danger"
                    :disabled="preventMultipleSubmit"
                    :title="t('global.delete')"
                    @click="onDelete(article)"
                  >
                    <Trash2Icon :size="15" />
                  </button>
                </div>
              </li>
            </ul>
          </div>
        </section>
      </div>
    </template>

    <ConfirmModal
      :open="confirmModal.open"
      :title="t('pages.help.index.deleteModalTitle')"
      :message="confirmModal.message"
      :confirm-label="t('global.delete')"
      :cancel-label="t('global.cancel')"
      :danger="true"
      @confirm="confirmDelete"
      @cancel="cancelDelete"
    />
  </div>
</template>

<script lang="ts" setup>
import {computed, onMounted, reactive, ref} from "vue"
import {useI18n} from "vue-i18n"
import {RouterLink} from "vue-router"
import {useHelpArticleService} from "@/serviceRegistry"
import {HELP_CATEGORIES, HelpArticle, type HelpCategory} from "@/types/entities/helpArticle"
import {useHelpDrawerStore} from "@/stores/helpDrawerStore"
import Loader from "@/components/layouts/items/Loader.vue"
import ConfirmModal from "@/components/social/ConfirmModal.vue"
import {PlusIcon, PencilIcon, Trash2Icon, ChevronRightIcon, BookOpenIcon, InfoIcon} from "lucide-vue-next"
import {notifySuccess, notifyError} from "@/notify"

const {t} = useI18n()
const helpArticleService = useHelpArticleService()
const helpDrawerStore = useHelpDrawerStore()
const canEdit = computed(() => helpDrawerStore.canEdit)

const isLoading = ref(false)
const preventMultipleSubmit = ref(false)
const articles = ref<HelpArticle[]>([])

const STORAGE_KEY = 'help-categories-expanded'

function loadExpandedState(): Record<HelpCategory, boolean> {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (raw) return JSON.parse(raw)
  } catch {
    // ignore
  }
  return {} as Record<HelpCategory, boolean>
}

function saveExpandedState() {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(collapsed))
  } catch {
    // ignore
  }
}

const savedState = loadExpandedState()
const collapsed = reactive<Record<HelpCategory, boolean>>(
  HELP_CATEGORIES.reduce((acc, cat) => {
    // Par défaut ouvert (collapsed = false), sauf si l'utilisateur a choisi autrement
    acc[cat] = savedState[cat] ?? false
    return acc
  }, {} as Record<HelpCategory, boolean>)
)

const articlesByCategory = computed<Record<HelpCategory, HelpArticle[]>>(() => {
  const groups = HELP_CATEGORIES.reduce((acc, cat) => {
    acc[cat] = []
    return acc
  }, {} as Record<HelpCategory, HelpArticle[]>)

  for (const article of articles.value) {
    const cat = article.category as HelpCategory
    if (groups[cat]) {
      groups[cat].push(article)
    }
  }

  for (const cat of HELP_CATEGORIES) {
    groups[cat].sort((a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0))
  }

  return groups
})

// Afficher uniquement les catégories non vides
const nonEmptyCategories = computed(() =>
  HELP_CATEGORIES.filter(cat => (articlesByCategory.value[cat]?.length ?? 0) > 0)
)

// Modal de confirmation suppression
const confirmModal = reactive({
  open: false,
  message: '',
  article: null as HelpArticle | null
})

onMounted(async () => {
  await helpDrawerStore.loadPermissions()
  await loadArticles()
})

async function loadArticles() {
  isLoading.value = true
  try {
    articles.value = await helpArticleService.getAll()
  } finally {
    isLoading.value = false
  }
}

function toggleCategory(category: HelpCategory) {
  collapsed[category] = !collapsed[category]
  saveExpandedState()
}

function isRecentlyUpdated(updatedAt?: string): boolean {
  if (!updatedAt) return false
  const updated = new Date(updatedAt).getTime()
  if (Number.isNaN(updated)) return false
  const thirtyDaysMs = 30 * 24 * 60 * 60 * 1000
  return Date.now() - updated < thirtyDaysMs
}

function onDelete(article: HelpArticle) {
  if (preventMultipleSubmit.value || !article.id) return
  confirmModal.article = article
  confirmModal.message = t('pages.help.index.deleteModalMessage', { title: article.title })
  confirmModal.open = true
}

function cancelDelete() {
  confirmModal.open = false
  confirmModal.article = null
}

async function confirmDelete() {
  const article = confirmModal.article
  confirmModal.open = false
  if (!article?.id) return

  preventMultipleSubmit.value = true
  try {
    const response = await helpArticleService.delete(article.id)
    if (response && response.succeeded) {
      helpDrawerStore.invalidate()
      notifySuccess(t('pages.help.index.deleteSuccess'))
      await loadArticles()
    } else {
      notifyError(t('pages.help.index.deleteError'))
    }
  } finally {
    preventMultipleSubmit.value = false
    confirmModal.article = null
  }
}
</script>

<style scoped>
.help-index__subtitle {
  color: var(--color-gray-500, #6b7280);
  margin: 0.5rem 0 1.5rem;
}

.help-index__readonly-banner {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: #eff6ff;
  color: #1e40af;
  border: 1px solid #bfdbfe;
  border-radius: 0.375rem;
  padding: 0.625rem 0.875rem;
  font-size: 0.875rem;
  margin-bottom: 1rem;
}

.help-index__readonly-icon {
  flex-shrink: 0;
}

.help-index__btn-icon {
  vertical-align: middle;
  margin-right: 0.25rem;
  margin-top: -2px;
}

/* État vide global */
.help-index__global-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  text-align: center;
  gap: 0.5rem;
}

.help-index__empty-icon {
  color: var(--color-gray-300, #d1d5db);
  margin-bottom: 0.5rem;
}

.help-index__empty-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--color-gray-700, #374151);
  margin: 0;
}

.help-index__empty-sub {
  font-size: 0.875rem;
  color: var(--color-gray-500, #6b7280);
  margin: 0 0 1rem;
}

.help-index__empty-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
}

/* Liste */
.help-index {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.help-index__category {
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.5rem;
  overflow: hidden;
  background: #fff;
}

.help-index__category-header {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.875rem 1rem;
  background: var(--color-gray-50, #f9fafb);
  border: none;
  cursor: pointer;
  font: inherit;
  text-align: left;
}

.help-index__category-header:hover {
  background: var(--color-gray-100, #f3f4f6);
}

.help-index__category-title {
  font-weight: 600;
  flex: 1;
}

.help-index__category-count {
  font-size: 0.8125rem;
  color: var(--color-gray-500, #6b7280);
  background: #fff;
  padding: 0.125rem 0.5rem;
  border-radius: 999px;
  border: 1px solid var(--color-gray-200, #e5e7eb);
}

.help-index__chevron {
  display: inline-block;
  color: var(--color-gray-400, #9ca3af);
  transition: transform 0.15s ease;
}

.help-index__chevron--open {
  transform: rotate(90deg);
}

.help-index__articles {
  padding: 0.5rem 1rem 1rem;
}

.help-index__list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.help-index__item {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.75rem;
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.375rem;
  transition: background 0.1s;
}

.help-index__item:hover {
  background: var(--color-gray-50, #f9fafb);
}

.help-index__item-main {
  flex: 1;
  min-width: 180px;
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
}

.help-index__item-title {
  font-weight: 500;
}

.help-index__item-slug {
  color: var(--color-gray-400, #9ca3af);
  font-size: 0.75rem;
  font-family: monospace;
}

.help-index__badges {
  display: flex;
  gap: 0.375rem;
  flex-wrap: wrap;
}

.help-index__badge {
  display: inline-block;
  padding: 0.125rem 0.5rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 500;
}

.help-index__badge--draft {
  background: #fef3c7;
  color: #92400e;
}

.help-index__badge--updated {
  background: #dbeafe;
  color: #1e40af;
}

.help-index__actions {
  display: flex;
  gap: 0.25rem;
  flex-shrink: 0;
}

.help-index__action-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.375rem;
  background: #fff;
  color: var(--color-gray-600, #4b5563);
  cursor: pointer;
  text-decoration: none;
  transition: background 0.12s, border-color 0.12s, color 0.12s;
}

.help-index__action-btn:hover {
  background: var(--color-gray-100, #f3f4f6);
  border-color: var(--color-gray-300, #d1d5db);
}

.help-index__action-btn--danger:hover {
  background: #fee2e2;
  border-color: #fca5a5;
  color: #b91c1c;
}

.help-index__action-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}
</style>
