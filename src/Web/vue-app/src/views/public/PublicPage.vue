<template>
  <article v-if="page" class="public-page">
    <div class="public-page__container">
      <h1 class="public-page__title">{{ page.title }}</h1>
      <div class="public-page__content" v-html="page.content"></div>
    </div>
  </article>
  <div v-else-if="isLoading" class="public-page public-page--loading">
    <Loader />
  </div>
  <div v-else class="public-page public-page--not-found">
    <div class="public-page__container">
      <h1>{{ t('public.page.notFound') }}</h1>
      <p>{{ t('public.page.notFoundMessage') }}</p>
      <RouterLink :to="{ name: 'home' }" class="btn btn--primary">{{ t('public.page.backHome') }}</RouterLink>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {ref, watch} from "vue"
import {useRoute} from "vue-router"
import {useI18n} from "vue3-i18n"
import axios from "axios"
import {Page} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"

const {t} = useI18n()
const route = useRoute()

const page = ref<Page | null>(null)
const isLoading = ref(true)

async function loadPage(slug: string) {
  isLoading.value = true
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/pages/${slug}`)
    page.value = response.data
  } catch {
    page.value = null
  }
  isLoading.value = false
}

watch(() => route.params.slug, (newSlug) => {
  if (newSlug) loadPage(newSlug as string)
}, {immediate: true})
</script>

<style scoped>
.public-page {
  min-height: 60vh;
  padding: 6rem 1rem 3rem;
}

.public-page--loading {
  display: flex;
  align-items: center;
  justify-content: center;
}

.public-page--not-found {
  text-align: center;
  padding-top: 8rem;
}

.public-page__container {
  max-width: 800px;
  margin: 0 auto;
}

.public-page__title {
  font-size: 2.5rem;
  margin-bottom: 2rem;
  color: var(--color-primary, #be1e2c);
}

.public-page__content :deep(h2) {
  font-size: 1.75rem;
  margin: 2rem 0 1rem;
}

.public-page__content :deep(h3) {
  font-size: 1.25rem;
  margin: 1.5rem 0 0.75rem;
}

.public-page__content :deep(p) {
  margin-bottom: 1rem;
  line-height: 1.7;
}

.public-page__content :deep(ul) {
  margin-bottom: 1rem;
  padding-left: 1.5rem;
}

.public-page__content :deep(li) {
  margin-bottom: 0.5rem;
  line-height: 1.5;
}

.public-page__content :deep(a) {
  color: var(--color-primary, #be1e2c);
}
</style>
