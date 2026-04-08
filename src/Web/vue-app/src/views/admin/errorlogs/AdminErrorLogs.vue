<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>{{ t('routes.admin.children.errorLogs.name') }}</h1>
      <div class="error-logs__actions">
        <input v-model="searchQuery" type="text" class="error-logs__search" :placeholder="t('pages.errorLogs.search')" />
        <select v-model="selectedLevel" class="error-logs__filter">
          <option value="">{{ t('pages.errorLogs.allLevels') }}</option>
          <option value="Warning">Warning</option>
          <option value="Error">Error</option>
          <option value="Fatal">Fatal</option>
        </select>
        <select v-model="selectedSource" class="error-logs__filter">
          <option value="">{{ t('pages.errorLogs.allSources') }}</option>
          <option v-for="source in availableSources" :key="source" :value="source">{{ source }}</option>
        </select>
        <button class="btn" @click="loadLogs">
          <RefreshCw :size="15" />
          {{ t('pages.errorLogs.refresh') }}
        </button>
      </div>
    </div>

    <Loader v-if="isLoading" />

    <div v-else-if="filteredLogs.length" class="error-logs">
      <div class="error-logs__table-wrapper">
        <table class="error-logs__table">
          <thead>
            <tr>
              <th>{{ t('pages.errorLogs.date') }}</th>
              <th>{{ t('pages.errorLogs.level') }}</th>
              <th>{{ t('pages.errorLogs.message') }}</th>
              <th>{{ t('pages.errorLogs.source') }}</th>
            </tr>
          </thead>
          <tbody>
            <template v-for="(log, index) in filteredLogs" :key="index">
              <tr tabindex="0" role="button" :aria-expanded="expandedIndex === index" @click="toggleExpand(index)" @keydown.enter="toggleExpand(index)" @keydown.space.prevent="toggleExpand(index)">
                <td class="error-logs__cell--date">{{ formatDate(log.timestamp) }}</td>
                <td>
                  <span class="error-logs__pill" :class="`error-logs__pill--${log.level?.toLowerCase()}`">
                    {{ log.level }}
                  </span>
                </td>
                <td class="error-logs__cell--message">{{ log.message }}</td>
                <td class="error-logs__cell--source">{{ log.sourceContext ?? '-' }}</td>
              </tr>
              <tr v-if="expandedIndex === index && log.exception" class="error-logs__expanded">
                <td colspan="4">
                  <pre class="error-logs__exception">{{ log.exception }}</pre>
                </td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>
    </div>

    <div v-else class="error-logs error-logs--empty">
      <p>{{ t('pages.errorLogs.noLogs') }}</p>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue-i18n"
import {computed, onMounted, ref, watch} from "vue"
import {useErrorLogsService} from "@/inversify.config"
import {ErrorLog} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"
import {RefreshCw} from "lucide-vue-next"

const {t} = useI18n()
const errorLogsService = useErrorLogsService()

const isLoading = ref(false)
const logs = ref<ErrorLog[]>([])
const selectedLevel = ref("")
const selectedSource = ref("")
const searchQuery = ref("")
const expandedIndex = ref<number | null>(null)

const availableSources = computed(() => {
  const sources = new Set(logs.value.map(l => l.sourceContext).filter(Boolean))
  return [...sources].sort()
})

const filteredLogs = computed(() => {
  let result = logs.value
  if (selectedLevel.value) {
    result = result.filter(l => l.level === selectedLevel.value)
  }
  if (selectedSource.value) {
    result = result.filter(l => l.sourceContext === selectedSource.value)
  }
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    result = result.filter(l =>
      l.message?.toLowerCase().includes(q) ||
      l.exception?.toLowerCase().includes(q) ||
      l.sourceContext?.toLowerCase().includes(q)
    )
  }
  return result
})

watch([selectedLevel, selectedSource, searchQuery], () => {
  expandedIndex.value = null
})

onMounted(async () => {
  await loadLogs()
})

async function loadLogs() {
  isLoading.value = true
  expandedIndex.value = null
  try {
    logs.value = await errorLogsService.getAll()
  } catch {
    logs.value = []
  }
  isLoading.value = false
}

function toggleExpand(index: number) {
  expandedIndex.value = expandedIndex.value === index ? null : index
}

function formatDate(timestamp?: string): string {
  if (!timestamp) return '-'
  const date = new Date(timestamp)
  if (isNaN(date.getTime())) return timestamp
  return date.toLocaleString('fr-CA', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
}
</script>

<style scoped>
.error-logs__actions {
  display: flex;
  gap: 8px;
  align-items: center;
}

.error-logs__search {
  padding: 6px 10px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.85rem;
  min-width: 200px;
}

.error-logs__filter {
  padding: 6px 10px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.85rem;
  background: white;
}

.error-logs__table-wrapper {
  overflow-x: auto;
}

.error-logs__table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
}

.error-logs__table th {
  text-align: left;
  padding: 10px 12px;
  border-bottom: 2px solid #e5e7eb;
  font-weight: 600;
  color: #374151;
  white-space: nowrap;
}

.error-logs__table td {
  padding: 8px 12px;
  border-bottom: 1px solid #f3f4f6;
  vertical-align: top;
}

.error-logs__table tbody tr:hover {
  background: #f9fafb;
  cursor: pointer;
}

.error-logs__cell--date {
  white-space: nowrap;
  color: #6b7280;
  font-family: monospace;
  font-size: 0.8rem;
}

.error-logs__cell--message {
  word-break: break-word;
  white-space: normal;
}

.error-logs__cell--source {
  color: #9ca3af;
  font-size: 0.8rem;
  word-break: break-word;
  white-space: normal;
}

.error-logs__pill {
  display: inline-block;
  font-size: 0.72rem;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 100px;
  text-transform: uppercase;
}

.error-logs__pill--warning { background: #fef3c7; color: #92400e; }
.error-logs__pill--error   { background: #fee2e2; color: #991b1b; }
.error-logs__pill--fatal   { background: #581c87; color: #f5f3ff; }

.error-logs__expanded td {
  padding: 0 12px 12px;
  background: #f9fafb;
}

.error-logs__exception {
  margin: 0;
  padding: 12px;
  background: #1f2937;
  color: #f9fafb;
  border-radius: 6px;
  font-size: 0.78rem;
  overflow-x: auto;
  white-space: pre-wrap;
  word-break: break-all;
}

.error-logs--empty {
  text-align: center;
  color: #6b7280;
  padding: 2rem 0;
}

@media (max-width: 767px) {
  .content-grid__header {
    flex-direction: column;
    gap: 8px;
  }

  .error-logs__actions {
    width: 100%;
  }

  .error-logs__filter {
    flex: 1;
  }
}
</style>
