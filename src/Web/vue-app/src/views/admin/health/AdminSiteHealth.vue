<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t('routes.admin.children.siteHealth.name') }}</h1>
    </div>
    <Loader v-if="isLoading" />
    <div v-else-if="health" class="site-health">
      <div class="site-health__status" :class="`site-health__status--${health.overallStatus?.toLowerCase()}`">
        <span class="site-health__badge">{{ health.overallStatus }}</span>
      </div>
      <div class="site-health__checks">
        <div v-for="check in health.checks" :key="check.name" class="site-health__check">
          <div class="site-health__check-header">
            <span class="site-health__check-status" :class="`site-health__check-status--${check.status?.toLowerCase()}`">
              {{ check.status === 'Good' ? '\u2713' : check.status === 'Warning' ? '\u26A0' : '\u2717' }}
            </span>
            <strong>{{ check.name }}</strong>
          </div>
          <p class="site-health__check-message">{{ check.message }}</p>
          <p v-if="check.details" class="site-health__check-details">{{ check.details }}</p>
        </div>
      </div>
      <button class="btn btn--secondary" @click="loadHealth" style="margin-top: 1rem;">
        {{ t('pages.siteHealth.refresh') }}
      </button>
    </div>
    <div v-else class="site-health">
      <p>{{ t('validation.errorOccured') }}</p>
      <button class="btn btn--secondary" @click="loadHealth" style="margin-top: 1rem;">
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

const {t} = useI18n()
const healthService = useSiteHealthService()

const isLoading = ref(false)
const health = ref<SiteHealth | null>(null)

onMounted(async () => {
  await loadHealth()
})

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
  margin-top: 1rem;
}

.site-health__status {
  padding: 1rem;
  border-radius: 0.5rem;
  margin-bottom: 1.5rem;
  text-align: center;
}

.site-health__status--good {
  background: #d1fae5;
  color: #065f46;
}

.site-health__status--warning {
  background: #fef3c7;
  color: #92400e;
}

.site-health__status--critical {
  background: #fee2e2;
  color: #991b1b;
}

.site-health__badge {
  font-size: 1.25rem;
  font-weight: 700;
}

.site-health__checks {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.site-health__check {
  padding: 1rem;
  border: 1px solid var(--color-gray-200, #e5e7eb);
  border-radius: 0.5rem;
}

.site-health__check-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.site-health__check-status {
  font-size: 1.25rem;
}

.site-health__check-status--good {
  color: #059669;
}

.site-health__check-status--warning {
  color: #d97706;
}

.site-health__check-status--critical {
  color: #dc2626;
}

.site-health__check-message {
  color: var(--color-gray-600, #4b5563);
  font-size: 0.875rem;
}

.site-health__check-details {
  margin-top: 0.5rem;
  color: var(--color-gray-500, #6b7280);
  font-size: 0.8rem;
  font-family: monospace;
}

.btn--secondary {
  background: var(--color-gray-200, #e5e7eb);
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 0.25rem;
  cursor: pointer;
}
</style>
