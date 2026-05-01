<template>
  <div class="error-page">
    <div class="error-page__container">
      <Breadcrumbs :items="breadcrumbs" class="error-page__breadcrumbs" />
      <p class="error-page__code">503</p>
      <h1 class="error-page__title">{{ t('public.maintenance.title') }}</h1>
      <p class="error-page__message">{{ message }}</p>
      <p class="error-page__retry-hint">{{ t('public.maintenance.retryHint') }}</p>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed, ref, onMounted } from "vue"
import { useI18n } from "vue-i18n"
import axios from "axios"
import Breadcrumbs from "@/components/layouts/items/Breadcrumbs.vue"
import type {BreadcrumbItem} from "@/types/entities"

const { t } = useI18n()
const message = ref(t('public.maintenance.defaultMessage'))
const breadcrumbs = computed<BreadcrumbItem[]>(() => [
  { label: t('routes.home.name'), url: '/', isCurrent: false },
  { label: t('public.maintenance.title'), isCurrent: true }
])

onMounted(async () => {
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/maintenance-status`)
    if (response.data?.maintenanceMessage) {
      message.value = response.data.maintenanceMessage
    }
  } catch {
    // Si l'API est indisponible, on affiche le message par défaut.
  }
})
</script>

<style scoped>
.error-page {
  min-height: 80vh;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 140px 1.5rem 4rem;
}

.error-page__container {
  max-width: 480px;
}

.error-page__breadcrumbs {
  margin-bottom: 1.5rem;
}

.error-page__breadcrumbs :deep(.breadcrumbs__list) {
  justify-content: center;
}

.error-page__code {
  font-size: 8rem;
  font-weight: 800;
  line-height: 1;
  color: #be1e2c;
  margin-bottom: 0.25rem;
  letter-spacing: -4px;
}

.error-page__title {
  font-size: 1.75rem;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 1rem;
}

.error-page__message {
  font-size: 1.05rem;
  color: #666;
  line-height: 1.7;
  margin-bottom: 1rem;
}

.error-page__retry-hint {
  font-size: 0.9rem;
  color: #999;
  margin: 0;
}
</style>
