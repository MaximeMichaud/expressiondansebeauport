<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>{{ t('routes.admin.children.siteHealth.name') }}</h1>
      <button class="btn" @click="loadHealth">
        <RefreshCw :size="15" />
        {{ t('pages.siteHealth.refresh') }}
      </button>
    </div>

    <Loader v-if="isLoading" />

    <div v-else-if="health" class="site-health">
      <div class="site-health__summary card">
        <component :is="statusIcon(health.overallStatus)" :size="32" :class="`site-health__icon--${health.overallStatus?.toLowerCase()}`" />
        <div>
          <p class="site-health__summary-label">{{ t('pages.siteHealth.overallStatus') }}</p>
          <p class="site-health__summary-value" :class="`site-health__text--${health.overallStatus?.toLowerCase()}`">
            {{ translateStatus(health.overallStatus) }}
          </p>
        </div>
      </div>

      <div class="site-health__checks">
        <div v-for="check in health.checks" :key="check.name" class="site-health__check card">
          <div class="site-health__check-left">
            <component :is="statusIcon(check.status)" :size="20" :class="`site-health__icon--${check.status?.toLowerCase()}`" />
            <div>
              <strong class="site-health__check-name">{{ check.name }}</strong>
              <p class="site-health__check-message">{{ check.message }}</p>
              <p v-if="check.details" class="site-health__check-details">{{ check.details }}</p>
            </div>
          </div>
          <span class="site-health__check-pill" :class="`site-health__pill--${check.status?.toLowerCase()}`">
            {{ translateStatus(check.status) }}
          </span>
        </div>
      </div>
    </div>

    <div v-else class="site-health">
      <p>{{ t('validation.errorOccured') }}</p>
      <button class="btn" @click="loadHealth" style="margin-top: 1rem;">
        <RefreshCw :size="15" />
        {{ t('pages.siteHealth.refresh') }}
      </button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {onMounted, ref} from "vue"
import {useSiteHealthService} from "@/inversify.config"
import {SiteHealth} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"
import { CheckCircle2, AlertTriangle, XCircle, RefreshCw } from "lucide-vue-next"

const {t} = useI18n()
const healthService = useSiteHealthService()

const isLoading = ref(false)
const health = ref<SiteHealth | null>(null)

onMounted(async () => {
  await loadHealth()
})

function translateStatus(status?: string): string {
  const map: Record<string, string> = {
    'Good': t('pages.siteHealth.status.good'),
    'Warning': t('pages.siteHealth.status.warning'),
    'Critical': t('pages.siteHealth.status.critical'),
  }
  return status ? (map[status] ?? status) : ''
}

function statusIcon(status?: string) {
  if (status === 'Warning') return AlertTriangle
  if (status === 'Critical') return XCircle
  return CheckCircle2
}

async function loadHealth() {
  isLoading.value = true
  try {
    health.value = await healthService.get()
  } catch {
    health.value = null
  }
  isLoading.value = false
}
</script>

<style scoped>
.site-health {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* Summary card */
.site-health__summary {
  display: flex;
  align-items: center;
  gap: 16px;
}

.site-health__summary-label {
  font-size: 0.8rem;
  color: #6b7280;
  margin-bottom: 2px;
}

.site-health__summary-value {
  font-size: 1.1rem;
  font-weight: 700;
}

/* Check rows */
.site-health__checks {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.site-health__check {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.site-health__check-left {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  flex: 1;
  min-width: 0;
}

.site-health__check-name {
  display: block;
  font-size: 0.9rem;
  margin-bottom: 2px;
}

.site-health__check-message {
  font-size: 0.85rem;
  color: #6b7280;
}

.site-health__check-details {
  margin-top: 2px;
  font-size: 0.78rem;
  color: #9ca3af;
  font-family: monospace;
}

/* Status pill */
.site-health__check-pill {
  flex-shrink: 0;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 3px 10px;
  border-radius: 100px;
}

/* Colors */
.site-health__icon--good   { color: #059669; }
.site-health__icon--warning { color: #d97706; }
.site-health__icon--critical { color: #dc2626; }

.site-health__text--good    { color: #059669; }
.site-health__text--warning { color: #d97706; }
.site-health__text--critical { color: #dc2626; }

.site-health__pill--good    { background: #d1fae5; color: #065f46; }
.site-health__pill--warning { background: #fef3c7; color: #92400e; }
.site-health__pill--critical { background: #fee2e2; color: #991b1b; }

@media (max-width: 767px) {
  .site-health__check {
    flex-wrap: wrap;
    align-items: flex-start;
  }

  .site-health__check-pill {
    margin-left: 32px;
  }
}
</style>
