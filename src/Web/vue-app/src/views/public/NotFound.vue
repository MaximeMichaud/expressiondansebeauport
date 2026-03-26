<template>
  <div class="not-found">
    <div class="not-found__container">
      <h1 class="not-found__code">404</h1>
      <p class="not-found__message">{{ t('public.page.notFound') }}</p>
      <RouterLink :to="{ name: 'home' }" class="btn btn--primary">{{ t('public.page.backHome') }}</RouterLink>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { useI18n } from "vue-i18n"
const { t } = useI18n()
</script>

<style scoped>
.not-found {
  min-height: 60vh;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 140px 1rem 3rem;
}

.not-found__container {
  max-width: 400px;
}

.not-found__code {
  font-size: 6rem;
  font-weight: 800;
  color: var(--color-primary, #be1e2c);
  line-height: 1;
  margin-bottom: 1rem;
}

.not-found__message {
  font-size: 1.25rem;
  margin-bottom: 2rem;
  color: var(--color-text-muted, #666);
}
</style>
