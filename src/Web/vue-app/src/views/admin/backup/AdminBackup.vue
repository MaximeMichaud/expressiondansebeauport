<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t('routes.admin.children.backup.name') }}</h1>
      <button v-if="isAvailable" class="btn" :disabled="isCreating" @click="onCreateBackup">
        <HardDriveDownload :size="15" />
        {{ isCreating ? t('pages.backup.creating') : t('pages.backup.createBackup') }}
      </button>
    </div>

    <div v-if="!isAvailable && !isLoading" class="backup__unavailable">
      <p>{{ t('pages.backup.unavailable') }}</p>
    </div>

    <template v-else>
    <p class="backup__description">{{ t('pages.backup.description') }}</p>

    <Loader v-if="isLoading" />

    <p v-else-if="!backups.length" class="backup__empty">{{ t('pages.backup.noBackups') }}</p>

    <table v-else class="backup__table">
      <thead>
        <tr>
          <th>{{ t('pages.backup.fileName') }}</th>
          <th>{{ t('pages.backup.size') }}</th>
          <th>{{ t('pages.backup.date') }}</th>
          <th>{{ t('pages.backup.type') }}</th>
          <th>{{ t('pages.backup.status') }}</th>
          <th>{{ t('global.table.actions') }}</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="backup in backups" :key="backup.id">
          <td class="backup__filename">{{ backup.fileName }}</td>
          <td>{{ formatSize(backup.sizeInBytes) }}</td>
          <td>{{ formatDate(backup.createdAt) }}</td>
          <td>
            <span class="backup__pill" :class="backup.type === 'Manual' ? 'backup__pill--manual' : 'backup__pill--scheduled'">
              {{ backup.type === 'Manual' ? t('pages.backup.typeManual') : t('pages.backup.typeScheduled') }}
            </span>
          </td>
          <td>
            <span class="backup__pill" :class="statusClass(backup.status)">
              {{ statusLabel(backup.status) }}
            </span>
          </td>
          <td class="backup__actions">
            <template v-if="backup.status === 'Completed'">
              <button class="backup__action-btn" :title="t('global.download')" @click="onDownload(backup)">
                <Download :size="16" />
              </button>
              <button class="backup__action-btn" :title="t('pages.backup.restore')" @click="onRestore(backup)">
                <RotateCcw :size="16" />
              </button>
            </template>
            <button class="backup__action-btn backup__action-btn--danger" :title="t('global.delete')" @click="onDelete(backup)">
              <Trash2 :size="16" />
            </button>
          </td>
        </tr>
      </tbody>
    </table>
    </template>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {onMounted, ref} from "vue"
import {useBackupService} from "@/inversify.config"
import {BackupRecord} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"
import {HardDriveDownload, Download, RotateCcw, Trash2} from "lucide-vue-next"

const {t} = useI18n()
const backupService = useBackupService()

const isLoading = ref(false)
const isCreating = ref(false)
const isAvailable = ref(true)
const backups = ref<BackupRecord[]>([])

onMounted(async () => {
  isLoading.value = true
  isAvailable.value = await backupService.checkStatus()
  if (isAvailable.value) {
    await loadBackups()
  }
  isLoading.value = false
})

async function loadBackups() {
  isLoading.value = true
  try {
    backups.value = await backupService.getAll()
  } finally {
    isLoading.value = false
  }
}

async function onCreateBackup() {
  isCreating.value = true
  try {
    const result = await backupService.create()
    if (result) {
      await loadBackups()
    }
  } finally {
    isCreating.value = false
  }
}

async function onDownload(backup: BackupRecord) {
  const blob = await backupService.download(backup.fileName)
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = backup.fileName
  a.style.display = 'none'
  document.body.appendChild(a)
  a.click()
  setTimeout(() => {
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
  }, 1000)
}

async function onRestore(backup: BackupRecord) {
  const confirmed = confirm(t('pages.backup.restoreConfirmation'))
  if (!confirmed) return

  const result = await backupService.restore(backup.fileName)
  if (result.succeeded) {
    alert(t('pages.backup.restoreSuccess'))
  }
}

async function onDelete(backup: BackupRecord) {
  const confirmed = confirm(t('pages.backup.deleteConfirmation'))
  if (!confirmed) return

  const result = await backupService.deleteBackup(backup.id)
  if (result.succeeded) {
    backups.value = backups.value.filter(b => b.id !== backup.id)
  }
}

function formatSize(bytes: number): string {
  if (!bytes) return "-"
  if (bytes < 1024) return `${bytes} o`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} Ko`
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(1)} Mo`
  return `${(bytes / 1024 / 1024 / 1024).toFixed(1)} Go`
}

function formatDate(dateStr: string): string {
  if (!dateStr) return "-"
  try {
    const date = new Date(dateStr)
    return date.toLocaleDateString('fr-CA') + ' ' + date.toLocaleTimeString('fr-CA', {hour: '2-digit', minute: '2-digit'})
  } catch {
    return dateStr
  }
}

function statusLabel(status: string): string {
  const map: Record<string, string> = {
    'Completed': t('pages.backup.statusCompleted'),
    'InProgress': t('pages.backup.statusInProgress'),
    'Failed': t('pages.backup.statusFailed'),
  }
  return map[status] ?? status
}

function statusClass(status: string): string {
  const map: Record<string, string> = {
    'Completed': 'backup__pill--completed',
    'InProgress': 'backup__pill--progress',
    'Failed': 'backup__pill--failed',
  }
  return map[status] ?? ''
}
</script>

<style scoped>
.backup__unavailable {
  background: #fef3c7;
  border: 1px solid #f59e0b;
  border-radius: 8px;
  padding: 1.5rem;
  color: #92400e;
  font-size: 0.875rem;
  line-height: 1.5;
}

.backup__description {
  color: #6b7280;
  font-size: 0.875rem;
  margin-bottom: 1rem;
}

.backup__empty {
  color: #5c5c5c;
  font-size: 0.875rem;
  padding: 16px 0;
}

.backup__table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.backup__table th {
  text-align: left;
  padding: 0.75rem;
  border-bottom: 2px solid var(--color-gray-200, #e5e7eb);
  font-weight: 600;
  color: #374151;
}

.backup__table td {
  padding: 0.75rem;
  border-bottom: 1px solid var(--color-gray-100, #f3f4f6);
}

.backup__filename {
  font-family: monospace;
  font-size: 0.8rem;
}

.backup__pill {
  display: inline-block;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 100px;
}

.backup__pill--manual { background: #dbeafe; color: #1e40af; }
.backup__pill--scheduled { background: #e0e7ff; color: #3730a3; }
.backup__pill--completed { background: #d1fae5; color: #065f46; }
.backup__pill--progress { background: #fef3c7; color: #92400e; }
.backup__pill--failed { background: #fee2e2; color: #991b1b; }

.backup__actions {
  display: flex;
  gap: 0.5rem;
}

.backup__action-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
  background: white;
  cursor: pointer;
  color: #374151;
  transition: background-color 0.15s;
}

.backup__action-btn:hover {
  background: var(--color-gray-100, #f3f4f6);
}

.backup__action-btn--danger {
  color: #dc2626;
  border-color: #fca5a5;
}

.backup__action-btn--danger:hover {
  background: #fee2e2;
}

@media (max-width: 767px) {
  .backup__table {
    font-size: 0.8rem;
  }

  .backup__table th:nth-child(3),
  .backup__table td:nth-child(3),
  .backup__table th:nth-child(4),
  .backup__table td:nth-child(4) {
    display: none;
  }
}
</style>
