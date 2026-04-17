<template>
  <div class="revision-history">
    <button class="btn btn--outline btn--sm" @click="togglePanel">
      <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
      Historique
    </button>

    <div v-if="isOpen" class="revision-panel">
      <div class="revision-panel__header">
        <h4>Historique des révisions</h4>
        <button class="revision-panel__close" @click="isOpen = false">&times;</button>
      </div>

      <div v-if="loading" class="revision-panel__loading">Chargement...</div>

      <div v-else-if="revisions.length === 0" class="revision-panel__empty">
        Aucune révision disponible.
      </div>

      <template v-else>
        <!-- Liste des révisions -->
        <div v-if="!selectedRevision" class="revision-list">
          <button
            v-for="rev in revisions"
            :key="rev.id"
            class="revision-item"
            @click="selectRevision(rev.id)"
          >
            <span class="revision-item__number">#{{ rev.revisionNumber }}</span>
            <span class="revision-item__title">{{ rev.title }}</span>
            <span class="revision-item__meta">
              {{ formatDate(rev.createdAt) }}
              <span v-if="rev.createdBy"> - {{ rev.createdBy }}</span>
            </span>
          </button>
        </div>

        <!-- Détail d'une révision -->
        <div v-else class="revision-detail">
          <button class="btn btn--outline btn--sm revision-detail__back" @click="selectedRevision = null">
            &larr; Retour
          </button>
          <div class="revision-detail__header">
            <strong>Révision #{{ selectedRevision.revisionNumber }}</strong>
            <span>{{ formatDate(selectedRevision.createdAt) }}</span>
          </div>
          <div class="revision-detail__content">
            <div class="revision-diff" v-html="diffHtml"></div>
          </div>
          <div class="revision-detail__actions">
            <button
              class="btn btn--sm"
              :disabled="restoring"
              @click="confirmRestore"
            >
              {{ restoring ? 'Restauration...' : 'Restaurer cette version' }}
            </button>
          </div>
        </div>
      </template>
    </div>

    <!-- Modal de confirmation -->
    <div v-if="showConfirm" class="revision-confirm-overlay" @click.self="showConfirm = false">
      <div class="revision-confirm">
        <p>Restaurer la révision #{{ selectedRevision?.revisionNumber }} ?</p>
        <p class="revision-confirm__sub">L'état actuel sera sauvegardé dans l'historique avant la restauration.</p>
        <div class="revision-confirm__actions">
          <button class="btn btn--outline btn--sm" @click="showConfirm = false">Annuler</button>
          <button class="btn btn--sm" @click="doRestore">Confirmer</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {ref} from 'vue'
import type {PageRevision, PageRevisionListItem, Page} from '@/types/entities'
import {usePageService} from '@/serviceRegistry'

const props = defineProps<{
  pageId: string
  currentPage: Page
}>()

const emit = defineEmits<{
  restored: [page: Page]
}>()

const pageService = usePageService()
const isOpen = ref(false)
const loading = ref(false)
const revisions = ref<PageRevisionListItem[]>([])
const selectedRevision = ref<PageRevision | null>(null)
const diffHtml = ref('')
const restoring = ref(false)
const showConfirm = ref(false)

async function togglePanel() {
  isOpen.value = !isOpen.value
  if (isOpen.value) {
    await loadRevisions()
  }
}

async function loadRevisions() {
  loading.value = true
  revisions.value = await pageService.getRevisions(props.pageId)
  loading.value = false
}

async function selectRevision(revisionId: string) {
  const rev = await pageService.getRevision(props.pageId, revisionId)
  selectedRevision.value = rev
  diffHtml.value = buildDiff(rev)
}

function buildDiff(revision: PageRevision): string {
  const fields = [
    {label: 'Titre', old: props.currentPage.title, new: revision.title},
    {label: 'Mode', old: props.currentPage.contentMode, new: revision.contentMode},
    {label: 'Meta description', old: props.currentPage.metaDescription, new: revision.metaDescription}
  ]

  let html = ''
  for (const f of fields) {
    if (f.old !== f.new) {
      html += `<div class="diff-field"><strong>${f.label}</strong><div class="diff-old">${escapeHtml(f.old ?? '')}</div><div class="diff-new">${escapeHtml(f.new ?? '')}</div></div>`
    }
  }

  // Comparer le contenu principal
  if (revision.contentMode === 'blocks') {
    if (props.currentPage.blocks !== revision.blocks) {
      html += `<div class="diff-field"><strong>Blocs</strong><div class="diff-changed">Le contenu des blocs a été modifié</div></div>`
    }
  } else {
    if (props.currentPage.content !== revision.content) {
      html += `<div class="diff-field"><strong>Contenu</strong><div class="diff-changed">Le contenu HTML a été modifié</div></div>`
    }
  }

  if (props.currentPage.customCss !== revision.customCss) {
    html += `<div class="diff-field"><strong>CSS personnalisé</strong><div class="diff-changed">Le CSS a été modifié</div></div>`
  }

  if (!html) {
    html = '<div class="diff-same">Aucune différence avec la version actuelle.</div>'
  }

  return html
}

function escapeHtml(str: string): string {
  return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
}

function confirmRestore() {
  showConfirm.value = true
}

async function doRestore() {
  if (!selectedRevision.value) return
  restoring.value = true
  showConfirm.value = false

  const restored = await pageService.restoreRevision(props.pageId, selectedRevision.value.id)
  if (restored) {
    emit('restored', restored)
    selectedRevision.value = null
    await loadRevisions()
  }
  restoring.value = false
}

function formatDate(dateStr?: string): string {
  if (!dateStr) return ''
  try {
    const date = new Date(dateStr)
    if (isNaN(date.getTime())) return dateStr
    const now = new Date()
    const diffMs = now.getTime() - date.getTime()
    const diffMin = Math.floor(diffMs / 60000)
    if (diffMin < 1) return "à l'instant"
    if (diffMin < 60) return `il y a ${diffMin}min`
    const diffH = Math.floor(diffMin / 60)
    if (diffH < 24) return `il y a ${diffH}h`
    return date.toLocaleDateString('fr-CA', {day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit'})
  } catch {
    return dateStr
  }
}
</script>

<style scoped>
.revision-history {
  position: relative;
}


.btn--sm {
  font-size: 0.8125rem;
  padding: 0.375rem 0.75rem;
}

.revision-panel {
  position: absolute;
  top: 100%;
  right: 0;
  z-index: 50;
  width: 380px;
  max-height: 500px;
  overflow-y: auto;
  background: white;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.5rem;
  box-shadow: 0 10px 25px rgb(0 0 0 / 0.1);
  margin-top: 0.5rem;
}

.revision-panel__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem 1rem;
  border-bottom: 1px solid var(--color-gray-200, #e5e7eb);
}

.revision-panel__header h4 {
  margin: 0;
  font-size: 0.875rem;
}

.revision-panel__close {
  background: none;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  color: var(--color-gray-500, #6b7280);
}

.revision-panel__loading,
.revision-panel__empty {
  padding: 1.5rem;
  text-align: center;
  color: var(--color-gray-500, #6b7280);
  font-size: 0.875rem;
}

.revision-list {
  display: flex;
  flex-direction: column;
}

.revision-item {
  display: flex;
  flex-direction: column;
  padding: 0.75rem 1rem;
  border: none;
  border-bottom: 1px solid var(--color-gray-100, #f3f4f6);
  background: none;
  text-align: left;
  cursor: pointer;
  transition: background 0.15s;
}

.revision-item:hover {
  background: var(--color-gray-50, #f9fafb);
}

.revision-item__number {
  font-weight: 600;
  font-size: 0.8125rem;
  color: var(--color-primary, #2563eb);
}

.revision-item__title {
  font-size: 0.875rem;
  margin-top: 0.125rem;
}

.revision-item__meta {
  font-size: 0.75rem;
  color: var(--color-gray-500, #6b7280);
  margin-top: 0.25rem;
}

.revision-detail {
  padding: 1rem;
}

.revision-detail__back {
  margin-bottom: 0.75rem;
}

.revision-detail__header {
  display: flex;
  justify-content: space-between;
  font-size: 0.875rem;
  margin-bottom: 0.75rem;
}

.revision-detail__content {
  max-height: 250px;
  overflow-y: auto;
  font-size: 0.8125rem;
}

.revision-detail__actions {
  margin-top: 1rem;
  padding-top: 0.75rem;
  border-top: 1px solid var(--color-gray-200, #e5e7eb);
}

:deep(.diff-field) {
  margin-bottom: 0.75rem;
}

:deep(.diff-field strong) {
  display: block;
  font-size: 0.75rem;
  color: var(--color-gray-600, #4b5563);
  margin-bottom: 0.25rem;
}

:deep(.diff-old) {
  background: #fef2f2;
  color: #991b1b;
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
  font-size: 0.8125rem;
  margin-bottom: 0.25rem;
  word-break: break-word;
}

:deep(.diff-new) {
  background: #f0fdf4;
  color: #166534;
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
  font-size: 0.8125rem;
  word-break: break-word;
}

:deep(.diff-changed) {
  background: #fffbeb;
  color: #92400e;
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
  font-size: 0.8125rem;
}

:deep(.diff-same) {
  text-align: center;
  color: var(--color-gray-500, #6b7280);
  padding: 1rem;
}

.revision-confirm-overlay {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgb(0 0 0 / 0.3);
}

.revision-confirm {
  background: white;
  border-radius: 0.5rem;
  padding: 1.5rem;
  max-width: 400px;
  box-shadow: 0 20px 40px rgb(0 0 0 / 0.15);
}

.revision-confirm p {
  margin: 0 0 0.5rem;
}

.revision-confirm__sub {
  font-size: 0.8125rem;
  color: var(--color-gray-500, #6b7280);
}

.revision-confirm__actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
  margin-top: 1rem;
}
</style>

<style>
.revision-history .btn.btn--outline,
.revision-history .btn.btn--sm {
  background: transparent;
  border: 1px solid #d1d5db;
  color: #374151;
}

.revision-history .btn.btn--outline:hover,
.revision-history .btn.btn--sm:hover {
  background: #f9fafb;
}
</style>
