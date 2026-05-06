<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>{{ t('routes.admin.children.auditLogs.name') }}</h1>
      <button class="btn" :disabled="isLoading" @click="reload">
        <RefreshCw :size="15" />
        {{ t('pages.auditLogs.refresh') }}
      </button>
    </div>

    <p class="audit-logs__hint">{{ t('pages.auditLogs.technicalHint') }}</p>

    <div class="audit-logs__filters">
      <label class="audit-logs__field">
        <span class="audit-logs__label">{{ t('pages.auditLogs.filters.userLabel') }}</span>
        <input
          v-model="filters.user"
          type="text"
          class="audit-logs__input"
          :placeholder="t('pages.auditLogs.filters.user')"
          @keyup.enter="applyFilters"
        />
      </label>

      <label class="audit-logs__field">
        <span class="audit-logs__label">{{ t('pages.auditLogs.filters.actionLabel') }}</span>
        <select v-model="filters.actionType" class="audit-logs__input" @change="applyFilters">
          <option value="">{{ t('pages.auditLogs.filters.allActions') }}</option>
          <option v-for="action in actionOptions" :key="action.value" :value="action.value">{{ action.label }}</option>
        </select>
      </label>

      <label class="audit-logs__field">
        <span class="audit-logs__label">{{ t('pages.auditLogs.filters.actionDateLabel') }}</span>
        <input
          v-model="filters.actionDate"
          type="date"
          class="audit-logs__input"
          :placeholder="t('pages.auditLogs.filters.actionDatePlaceholder')"
          @change="applyFilters"
        />
      </label>

      <button class="audit-logs__clear" type="button" @click="clearFilters">{{ t('pages.auditLogs.clearFilters') }}</button>
    </div>

    <Loader v-if="isLoading" />

    <div v-else-if="hasError" class="audit-logs__state">
      <p>{{ t('pages.auditLogs.error') }}</p>
    </div>

    <div v-else-if="!logs.length" class="audit-logs__state">
      <p>{{ t('pages.auditLogs.empty') }}</p>
    </div>

    <div v-else class="audit-logs">
      <div class="audit-logs__table-wrapper">
        <table class="audit-logs__table">
          <thead>
            <tr>
              <th>{{ t('pages.auditLogs.columns.date') }}</th>
              <th>{{ t('pages.auditLogs.columns.user') }}</th>
              <th>{{ t('pages.auditLogs.columns.action') }}</th>
              <th>{{ t('pages.auditLogs.columns.entity') }}</th>
              <th>{{ t('pages.auditLogs.columns.details') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="log in logs" :key="log.id">
              <td class="audit-logs__date">{{ formatDate(log.createdAt) }}</td>
              <td>
                <div class="audit-logs__user">{{ log.userDisplayName || '-' }}</div>
                <div v-if="log.userEmail" class="audit-logs__subtext">{{ log.userEmail }}</div>
              </td>
              <td>
                <span :class="['audit-logs__pill', getActionClass(log.actionType)]">{{ formatAction(log.actionType) }}</span>
              </td>
              <td>
                <div class="audit-logs__entity">{{ getEntityLabel(log) }}</div>
                <details v-if="log.entityId" class="audit-logs__technical">
                  <summary>{{ t('pages.auditLogs.technicalId') }}</summary>
                  <div class="audit-logs__subtext">{{ log.entityId }}</div>
                </details>
              </td>
              <td class="audit-logs__details">{{ formatDetails(log) }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="totalPages > 1" class="audit-logs__pagination">
        <button :disabled="pageIndex <= 1" @click="loadLogs(pageIndex - 1)">&#8592;</button>
        <span>{{ pageIndex }} / {{ totalPages }}</span>
        <button :disabled="pageIndex >= totalPages" @click="loadLogs(pageIndex + 1)">&#8594;</button>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {computed, onMounted, ref} from "vue"
import {useI18n} from "vue-i18n"
import {RefreshCw} from "lucide-vue-next"
import Loader from "@/components/layouts/items/Loader.vue"
import {useAuditLogService} from "@/serviceRegistry"
import {Tables} from "@/types/enums"
import {AuditLog} from "@/types/entities"
import {PaginatedResponse} from "@/types/responses"

const {t} = useI18n()
const auditLogService = useAuditLogService()

const pageSize = Tables.DefaultRowsPerPage
const pageIndex = ref(1)
const isLoading = ref(false)
const hasError = ref(false)
const logs = ref<AuditLog[]>([])
const response = ref<PaginatedResponse<AuditLog>>({totalItems: 0})
const filters = ref({
  user: "",
  actionType: "",
  actionDate: "",
})

const actionOptions = computed(() => [
  {value: "create", label: t('pages.auditLogs.actions.create')},
  {value: "update", label: t('pages.auditLogs.actions.update')},
  {value: "delete", label: t('pages.auditLogs.actions.delete')},
  {value: "login", label: t('pages.auditLogs.actions.login')},
  {value: "logout", label: t('pages.auditLogs.actions.logout')},
])

const totalPages = computed(() =>
  Math.max(1, Math.ceil((response.value.totalItems || 0) / pageSize))
)

onMounted(async () => {
  await loadLogs(1)
})

async function loadLogs(page: number) {
  isLoading.value = true
  hasError.value = false
  pageIndex.value = page

  try {
    const paginated = await auditLogService.getAll(page, pageSize, buildApiFilters())
    response.value = paginated
    logs.value = paginated.items || []
  } catch {
    logs.value = []
    hasError.value = true
  } finally {
    isLoading.value = false
  }
}

async function applyFilters() {
  await loadLogs(1)
}

async function clearFilters() {
  filters.value = {
    user: "",
    actionType: "",
    actionDate: "",
  }
  await loadLogs(1)
}

async function reload() {
  await loadLogs(pageIndex.value)
}

function formatDate(value?: string): string {
  if (!value) return '-'
  const date = parseAuditDate(value)
  if (isNaN(date.getTime())) return value
  return date.toLocaleString('fr-CA', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

function buildApiFilters() {
  const selectedDate = filters.value.actionDate

  return {
    user: filters.value.user,
    actionType: filters.value.actionType,
    fromDate: selectedDate || "",
    toDate: selectedDate || "",
  }
}

function parseAuditDate(value: string): Date {
  const utcMatch = value.match(/^(\d{4})-(\d{2})-(\d{2})[ T](\d{2}):(\d{2})(?::(\d{2}))?$/)
  if (utcMatch) {
    const [, year, month, day, hour, minute, second = "00"] = utcMatch
    return new Date(Date.UTC(
      Number(year),
      Number(month) - 1,
      Number(day),
      Number(hour),
      Number(minute),
      Number(second),
    ))
  }

  return new Date(value)
}

function formatAction(value?: string): string {
  if (!value) return '-'
  const label = String(t(`pages.auditLogs.actions.${value}`))
  return label.startsWith('pages.auditLogs.actions.') ? value : label
}

function formatEntityType(value?: string): string {
  if (!value) return '-'
  const label = String(t(`pages.auditLogs.entities.${value}`))
  return label.startsWith('pages.auditLogs.entities.') ? value : label
}

function getActionClass(value?: string): string {
  switch (value) {
    case 'create':
      return 'audit-logs__pill--create'
    case 'update':
      return 'audit-logs__pill--update'
    case 'delete':
      return 'audit-logs__pill--delete'
    case 'login':
      return 'audit-logs__pill--login'
    case 'logout':
      return 'audit-logs__pill--logout'
    default:
      return 'audit-logs__pill--neutral'
  }
}

function getEntityLabel(log: AuditLog): string {
  const entityType = formatEntityType(log.entityType)
  const entityName = extractEntityName(log)

  if (entityName) {
    return `${entityType} « ${entityName} »`
  }

  return entityType
}

function extractEntityName(log: AuditLog): string | null {
  if (log.entityType !== 'page' && log.entityType !== 'menu') {
    return null
  }

  const details = log.details ?? ''
  const quotedMatch = details.match(/['"]([^'"]+)['"]/)
  if (quotedMatch?.[1] && !isUuidLike(quotedMatch[1])) {
    return quotedMatch[1]
  }

  return null
}

function formatDetails(log: AuditLog): string {
  const details = (log.details ?? '').trim()
  if (!details) return '-'

  const mappedDetail = mapDetailToFrench(details)
  return sanitizeTechnicalValues(mappedDetail)
}

function mapDetailToFrench(details: string): string {
  const mappings: Array<[RegExp, string]> = [
    [/^Page '(.+)' created(?:.*)?\.$/i, "Page « $1 » créée."],
    [/^Page '(.+)' updated(?:.*)?\.$/i, "Page « $1 » modifiée."],
    [/^Page '(.+)' deleted(?:.*)?\.$/i, "Page « $1 » supprimée."],
    [/^Menu '(.+)' created(?:.*)?\.$/i, "Menu « $1 » créé."],
    [/^Menu '(.+)' updated(?:.*)?\.$/i, "Menu « $1 » modifié."],
    [/^Menu '(.+)' deleted(?:.*)?\.$/i, "Menu « $1 » supprimé."],
    [/^Menu item '(.+)' added to menu(?:.*)?\.$/i, "Item de menu « $1 » ajouté au menu."],
    [/^Menu item '(.+)' updated in menu(?:.*)?\.$/i, "Item de menu « $1 » modifié dans le menu."],
    [/^Menu item '(.+)' deleted from menu(?:.*)?\.$/i, "Item de menu « $1 » supprimé du menu."],
    [/^Menu '(.+)' items reordered\.$/i, "Ordre des items du menu mis à jour."],
    [/^Menu '[0-9a-f-]+' items reordered\.$/i, "Ordre des items du menu mis à jour."],
    [/^Administrator session started\.$/i, "Connexion admin réussie."],
    [/^Administrator session started with two-factor authentication\.$/i, "Connexion admin réussie avec authentification à deux facteurs."],
    [/^Administrator session ended\.$/i, "Déconnexion admin."],
  ]

  for (const [pattern, replacement] of mappings) {
    if (pattern.test(details)) {
      return details.replace(pattern, replacement)
    }
  }

  if (details.startsWith("Site settings updated:")) {
    return details.replace("Site settings updated:", "Réglages du site modifiés :")
  }

  return details
}

function sanitizeTechnicalValues(value: string): string {
  return value
    .replace(/\s*\(item:\s*[0-9a-f-]{36}\)\.?/gi, '')
    .replace(/\s*\(menu:\s*[0-9a-f-]{36}\)\.?/gi, '')
    .replace(/\b[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}\b/gi, t('pages.auditLogs.technicalReference'))
    .replace(/\s{2,}/g, ' ')
    .trim()
}

function isUuidLike(value: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value)
}
</script>

<style scoped>
.audit-logs__hint {
  margin: 0 0 1rem;
  color: #5c5c5c;
  font-size: 0.95rem;
}

.audit-logs__filters {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.75rem;
  align-items: end;
}

.audit-logs__field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.audit-logs__label {
  font-size: 0.875rem;
  font-weight: 600;
  color: #374151;
}

.audit-logs__input {
  width: 100%;
  padding: 0.625rem 0.75rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.375rem;
  background: white;
  font-size: 0.875rem;
}

.audit-logs__clear {
  border: none;
  background: none;
  color: #5c5c5c;
  text-align: left;
  cursor: pointer;
  padding: 0 0 0.25rem;
}

.audit-logs__state {
  color: #5c5c5c;
  padding: 1.5rem 0;
}

.audit-logs__table-wrapper {
  overflow-x: auto;
}

.audit-logs__table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.audit-logs__table th {
  text-align: left;
  padding: 0.75rem;
  border-bottom: 2px solid #e5e7eb;
}

.audit-logs__table td {
  padding: 0.75rem;
  border-bottom: 1px solid #f3f4f6;
  vertical-align: top;
}

.audit-logs__date {
  white-space: nowrap;
  font-family: monospace;
  color: #5c5c5c;
}

.audit-logs__user {
  font-weight: 600;
}

.audit-logs__subtext {
  color: #6b7280;
  font-size: 0.75rem;
  word-break: break-word;
}

.audit-logs__entity {
  font-weight: 600;
}

.audit-logs__technical {
  margin-top: 0.35rem;
}

.audit-logs__technical summary {
  cursor: pointer;
  color: #6b7280;
  font-size: 0.75rem;
}

.audit-logs__details {
  min-width: 320px;
  word-break: break-word;
}

.audit-logs__pill {
  display: inline-block;
  padding: 0.2rem 0.55rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.audit-logs__pill--create {
  background: rgba(34, 197, 94, 0.14);
  color: #166534;
}

.audit-logs__pill--update {
  background: rgba(245, 158, 11, 0.18);
  color: #9a3412;
}

.audit-logs__pill--delete {
  background: rgba(190, 30, 44, 0.12);
  color: #9f1239;
}

.audit-logs__pill--login {
  background: rgba(14, 165, 233, 0.14);
  color: #0369a1;
}

.audit-logs__pill--logout,
.audit-logs__pill--neutral {
  background: rgba(107, 114, 128, 0.14);
  color: #4b5563;
}

.audit-logs__pagination {
  margin-top: 1.25rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
}

.audit-logs__pagination button {
  padding: 0.5rem 1rem;
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
  background: white;
  cursor: pointer;
}

.audit-logs__pagination button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

@media (max-width: 900px) {
  .audit-logs__filters {
    grid-template-columns: 1fr 1fr;
  }
}

@media (max-width: 640px) {
  .audit-logs__filters {
    grid-template-columns: 1fr;
  }
}
</style>
