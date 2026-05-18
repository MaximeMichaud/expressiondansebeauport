<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>{{ t('routes.admin.children.version.name') }}</h1>
      <button class="btn" @click="loadVersion" :disabled="isLoading">
        <RefreshCw :size="15" />
        {{ t('pages.version.refresh') }}
      </button>
    </div>

    <Loader v-if="isLoading" />

    <div v-else-if="data" class="version">
      <div class="version__summary card" :class="summaryClass">
        <component :is="summaryIcon" :size="32" :class="summaryIconClass" />
        <div class="version__summary-text">
          <p class="version__summary-label">{{ summaryLabel }}</p>
          <p class="version__summary-value">{{ summaryMessage }}</p>
          <p v-if="data.updateError" class="version__summary-error">{{ data.updateError }}</p>
        </div>
        <a
          v-if="data.latestRelease && !data.isUpToDate"
          class="btn btn-primary"
          :href="data.latestRelease.htmlUrl"
          target="_blank"
          rel="noopener noreferrer"
        >
          <ExternalLink :size="15" />
          {{ t('pages.version.viewLatest') }}
        </a>
      </div>

      <section class="card version__section">
        <header class="version__section-header">
          <Package :size="20" />
          <h2>{{ t('pages.version.current.title') }}</h2>
        </header>
        <dl class="version__details">
          <div class="version__detail-row">
            <dt>{{ t('pages.version.current.tag') }}</dt>
            <dd class="version__mono">{{ data.current.semanticVersion || data.current.version }}</dd>
          </div>
          <div v-if="data.current.commitSha" class="version__detail-row">
            <dt>{{ t('pages.version.current.commit') }}</dt>
            <dd>
              <a
                v-if="commitUrl"
                :href="commitUrl"
                target="_blank"
                rel="noopener noreferrer"
                class="version__mono version__link"
              >
                {{ shortSha(data.current.commitSha) }}
              </a>
              <span v-else class="version__mono">{{ shortSha(data.current.commitSha) }}</span>
            </dd>
          </div>
          <div class="version__detail-row">
            <dt>{{ t('pages.version.current.builtAt') }}</dt>
            <dd>{{ formatDateTime(data.current.builtAt) }}</dd>
          </div>
          <div class="version__detail-row">
            <dt>{{ t('pages.version.current.repository') }}</dt>
            <dd>
              <a
                :href="data.repository.htmlUrl"
                target="_blank"
                rel="noopener noreferrer"
                class="version__link"
              >
                {{ data.repository.owner }}/{{ data.repository.name }}
                <ExternalLink :size="13" />
              </a>
            </dd>
          </div>
        </dl>
      </section>

      <section v-if="data.latestRelease" class="card version__section">
        <header class="version__section-header">
          <Star :size="20" />
          <h2>{{ t('pages.version.latest.title') }}</h2>
          <span v-if="data.latestRelease.isPrerelease" class="version__badge version__badge--warning">
            {{ t('pages.version.prerelease') }}
          </span>
        </header>
        <div class="version__release-meta">
          <a
            :href="data.latestRelease.htmlUrl"
            target="_blank"
            rel="noopener noreferrer"
            class="version__release-title"
          >
            {{ data.latestRelease.name }}
          </a>
          <p class="version__release-info">
            <span class="version__mono">{{ data.latestRelease.tagName }}</span>
            <span class="version__separator">•</span>
            <Calendar :size="13" />
            {{ formatDate(data.latestRelease.publishedAt) }}
            <template v-if="data.latestRelease.authorLogin">
              <span class="version__separator">•</span>
              <User :size="13" />
              {{ data.latestRelease.authorLogin }}
            </template>
          </p>
        </div>
        <div
          v-if="data.latestRelease.body"
          class="version__notes"
          v-html="renderMarkdown(data.latestRelease.body)"
        />
        <p v-else class="version__notes-empty">{{ t('pages.version.noNotes') }}</p>
      </section>

      <section v-if="otherReleases.length > 0" class="card version__section">
        <header class="version__section-header">
          <ScrollText :size="20" />
          <h2>{{ t('pages.version.history.title') }}</h2>
          <span class="version__badge">{{ otherReleases.length }}</span>
        </header>
        <ul class="version__history">
          <li v-for="release in otherReleases" :key="release.tagName" class="version__history-item">
            <div class="version__history-row">
              <button
                type="button"
                class="version__history-toggle"
                :aria-expanded="expanded.has(release.tagName)"
                @click="toggle(release.tagName)"
              >
                <ChevronRight :size="16" :class="{ 'version__chevron--expanded': expanded.has(release.tagName) }" />
                <span class="version__history-info">
                  <span class="version__history-name">{{ release.name }}</span>
                  <span class="version__history-meta">
                    <span class="version__mono">{{ release.tagName }}</span>
                    <span class="version__separator">•</span>
                    {{ formatDate(release.publishedAt) }}
                  </span>
                </span>
              </button>
              <span v-if="release.isPrerelease" class="version__badge version__badge--warning">
                {{ t('pages.version.prerelease') }}
              </span>
              <a
                :href="release.htmlUrl"
                target="_blank"
                rel="noopener noreferrer"
                class="version__history-link"
                :aria-label="t('pages.version.openOnGithub', { name: release.name })"
              >
                <ExternalLink :size="14" />
              </a>
            </div>
            <div v-if="expanded.has(release.tagName)" class="version__history-body">
              <div
                v-if="release.body"
                class="version__notes"
                v-html="renderMarkdown(release.body)"
              />
              <p v-else class="version__notes-empty">{{ t('pages.version.noNotes') }}</p>
            </div>
          </li>
        </ul>
      </section>

      <p class="version__fetched-at">
        {{ t('pages.version.fetchedAt', { date: formatDateTime(data.fetchedAt) }) }}
      </p>
    </div>

    <div v-else class="version version--error">
      <p>{{ t('validation.errorOccured') }}</p>
      <button class="btn" @click="loadVersion" :disabled="isLoading">
        <RefreshCw :size="15" />
        {{ t('pages.version.refresh') }}
      </button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {computed, onMounted, ref} from "vue"
import {useI18n} from "vue-i18n"
import {marked} from "marked"
import DOMPurify from "dompurify"
import {useAppVersionService} from "@/serviceRegistry"
import {AppVersion} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"
import {Calendar, CheckCircle2, ChevronRight, ExternalLink, Package, RefreshCw, ScrollText, Star, AlertTriangle, User} from "lucide-vue-next"

const {t} = useI18n()
const service = useAppVersionService()

const isLoading = ref(false)
const data = ref<AppVersion | null>(null)
const expanded = ref<Set<string>>(new Set())

onMounted(loadVersion)

async function loadVersion() {
  isLoading.value = true
  try {
    data.value = await service.get()
  } catch {
    data.value = null
  } finally {
    isLoading.value = false
  }
}

const otherReleases = computed(() => {
  if (!data.value) return []
  const latestTag = data.value.latestRelease?.tagName
  return data.value.releases.filter(r => r.tagName !== latestTag)
})

const summaryIcon = computed(() => {
  if (data.value?.updateError) return AlertTriangle
  return data.value?.isUpToDate ? CheckCircle2 : AlertTriangle
})

const summaryIconClass = computed(() => {
  if (data.value?.updateError) return 'version__icon--warning'
  return data.value?.isUpToDate ? 'version__icon--good' : 'version__icon--warning'
})

const summaryClass = computed(() => {
  if (data.value?.updateError) return 'version__summary--warning'
  return data.value?.isUpToDate ? 'version__summary--good' : 'version__summary--warning'
})

const summaryLabel = computed(() => {
  if (!data.value) return ''
  if (data.value.updateError) return t('pages.version.status.unknown')
  return data.value.isUpToDate ? t('pages.version.status.upToDate') : t('pages.version.status.updateAvailable')
})

const summaryMessage = computed(() => {
  if (!data.value) return ''
  const current = data.value.current.semanticVersion || data.value.current.version
  const latest = data.value.latestRelease?.tagName
  if (data.value.isUpToDate) {
    return t('pages.version.status.upToDateDetail', {version: current})
  }
  if (latest) {
    return t('pages.version.status.updateDetail', {current, latest})
  }
  return current
})

const commitUrl = computed(() => {
  if (!data.value?.current.commitSha) return null
  return `${data.value.repository.htmlUrl}/commit/${data.value.current.commitSha}`
})

function toggle(tag: string) {
  if (expanded.value.has(tag)) {
    expanded.value.delete(tag)
  } else {
    expanded.value.add(tag)
  }
  expanded.value = new Set(expanded.value)
}

function shortSha(sha?: string | null): string {
  if (!sha) return '-'
  return sha.slice(0, 7)
}

function formatDate(unix: number): string {
  if (!unix) return '-'
  return new Date(unix * 1000).toLocaleDateString('fr-CA', {
    year: 'numeric', month: 'long', day: 'numeric'
  })
}

function formatDateTime(unix: number): string {
  if (!unix) return '-'
  return new Date(unix * 1000).toLocaleString('fr-CA', {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit'
  })
}

function renderMarkdown(raw: string): string {
  const html = marked.parse(raw, {gfm: true, breaks: true, async: false}) as string
  const clean = DOMPurify.sanitize(html, {FORBID_TAGS: ['style', 'script', 'iframe']})
  const template = document.createElement('template')
  template.innerHTML = clean
  template.content.querySelectorAll('a').forEach(a => {
    a.setAttribute('target', '_blank')
    a.setAttribute('rel', 'noopener noreferrer')
  })
  return template.innerHTML
}
</script>

<style scoped>
.version {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.version__summary {
  display: flex;
  align-items: center;
  gap: 16px;
  border-left: 4px solid transparent;
}

.version__summary--good { border-left-color: #059669; }
.version__summary--warning { border-left-color: #d97706; }

.version__summary-text { flex: 1; }

.version__summary-label {
  font-size: 0.8rem;
  color: #6b7280;
  margin-bottom: 2px;
}

.version__summary-value {
  font-size: 1.05rem;
  font-weight: 600;
}

.version__summary-error {
  margin-top: 4px;
  font-size: 0.82rem;
  color: #b45309;
}

.version__section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.version__section-header {
  display: flex;
  align-items: center;
  gap: 8px;
}

.version__section-header h2 {
  margin: 0;
  font-size: 1rem;
  flex: 1;
}

.version__details {
  display: grid;
  grid-template-columns: minmax(140px, max-content) 1fr;
  gap: 6px 16px;
  margin: 0;
}

.version__detail-row {
  display: contents;
}

.version__detail-row dt {
  font-size: 0.85rem;
  color: #6b7280;
}

.version__detail-row dd {
  margin: 0;
  font-size: 0.9rem;
}

.version__mono {
  font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
  font-size: 0.85rem;
}

.version__link {
  color: #2563eb;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.version__link:hover { text-decoration: underline; }

.version__release-meta {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.version__release-title {
  font-size: 1.05rem;
  font-weight: 600;
  color: #1f2937;
  text-decoration: none;
}

.version__release-title:hover { text-decoration: underline; }

.version__release-info {
  font-size: 0.82rem;
  color: #6b7280;
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 4px;
}

.version__separator { margin: 0 4px; color: #d1d5db; }

.version__notes {
  font-size: 0.9rem;
  line-height: 1.55;
  color: #1f2937;
}

.version__notes :deep(h1),
.version__notes :deep(h2),
.version__notes :deep(h3) {
  margin-top: 12px;
  margin-bottom: 6px;
  font-size: 0.95rem;
  font-weight: 600;
}

.version__notes :deep(ul),
.version__notes :deep(ol) {
  padding-left: 1.2rem;
  margin: 6px 0;
}

.version__notes :deep(li) { margin: 2px 0; }

.version__notes :deep(code) {
  background: #f3f4f6;
  padding: 1px 5px;
  border-radius: 4px;
  font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
  font-size: 0.82rem;
}

.version__notes :deep(pre) {
  background: #f3f4f6;
  padding: 10px;
  border-radius: 6px;
  overflow-x: auto;
}

.version__notes :deep(a) { color: #2563eb; }

.version__notes-empty {
  font-style: italic;
  color: #9ca3af;
  font-size: 0.88rem;
}

.version__badge {
  display: inline-flex;
  align-items: center;
  font-size: 0.72rem;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 100px;
  background: #e5e7eb;
  color: #374151;
}

.version__badge--warning {
  background: #fef3c7;
  color: #92400e;
}

.version__history {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.version__history-item {
  border-top: 1px solid #f3f4f6;
}

.version__history-item:first-child { border-top: none; }

.version__history-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.version__history-toggle {
  flex: 1;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 4px;
  background: none;
  border: none;
  cursor: pointer;
  text-align: left;
  color: inherit;
  min-width: 0;
}

.version__history-toggle:hover { background: #f9fafb; }
.version__history-toggle:focus-visible { outline: 2px solid #2563eb; outline-offset: -2px; border-radius: 4px; }

.version__chevron--expanded { transform: rotate(90deg); transition: transform 0.15s; }
.version__history-toggle :deep(svg) { transition: transform 0.15s; }

.version__history-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.version__history-name { font-weight: 500; font-size: 0.9rem; }

.version__history-meta {
  font-size: 0.78rem;
  color: #6b7280;
  display: flex;
  align-items: center;
}

.version__history-link {
  color: #6b7280;
  display: inline-flex;
  padding: 4px;
}

.version__history-link:hover { color: #2563eb; }

.version__history-body {
  padding: 4px 4px 14px 30px;
}

.version__icon--good { color: #059669; }
.version__icon--warning { color: #d97706; }

.version__fetched-at {
  font-size: 0.78rem;
  color: #9ca3af;
  text-align: right;
  margin: 8px 4px 0;
}

.version--error {
  align-items: flex-start;
}

@media (max-width: 640px) {
  .version__details {
    grid-template-columns: 1fr;
    gap: 2px 0;
  }
  .version__detail-row dt { margin-top: 6px; }
  .version__summary { flex-wrap: wrap; }
}
</style>
