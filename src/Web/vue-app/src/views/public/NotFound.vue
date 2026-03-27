<template>
  <div class="error-page">
    <div class="error-page__container">
      <p class="error-page__code">404</p>
      <h1 class="error-page__title">{{ t('public.page.notFound') }}</h1>
      <p class="error-page__message">{{ t('public.page.notFoundMessage') }}</p>
      <div class="error-page__actions">
        <RouterLink :to="{ name: 'home' }" class="error-page__btn">
          {{ t('public.page.backHome') }}
        </RouterLink>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { useI18n } from "vue-i18n"
const { t } = useI18n()
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
  margin-bottom: 2.5rem;
}

.error-page__actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
}

.error-page__btn {
  display: inline-block;
  background: #be1e2c;
  color: #fff;
  padding: 13px 32px;
  border-radius: 10px;
  font-weight: 700;
  font-size: 0.95rem;
  text-decoration: none;
  transition: background 0.2s ease;
}

.error-page__btn:hover {
  background: #9e1824;
}
</style>
