<template>
  <article
    v-if="page"
    :class="['public-page', pageClass]">
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
import { computed } from "vue"

const pageClass = computed(() => {
  if (!route.params.slug) return ""
  return "public-page--" +
    (route.params.slug as string)
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "")
      .replace(/[^a-zA-Z0-9-]/g, "")
})

const {t} = useI18n()
const route = useRoute()

const page = ref<Page | null>(null)
const isLoading = ref(true)

async function loadPage(slug: string) {
  isLoading.value = true
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/pages/${slug}`)
    page.value = response.data
    document.title = `${page.value!.title} | EDB`
  } catch {
    page.value = null
  }
  isLoading.value = false
}

watch(() => route.params.slug, (newSlug) => {
  if (newSlug) {
    loadPage(newSlug as string)
  } else {
    isLoading.value = false
    page.value = null
  }
}, {immediate: true})
</script>

<style>
.public-page {
  min-height: 60vh;
  padding: 140px 1rem 3rem;
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

.public-page--camp-d-ete .public-page__container {
  max-width: 1100px;
}

.public-page--camp-d-ete .public-page__title {
  text-align: center;
  font-size: 3rem;
  margin-bottom: 3rem;
}

.public-page--camp-d-ete .public-page__content {
  display: flex;
  flex-direction: column;
  gap: 4rem;
}

.public-page--camp-d-ete .camp-hero {
  height: 400px;
  background: linear-gradient(135deg, #be1e2c, #ff6b6b);
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  text-align: center;
  padding: 2rem;
}

.public-page--camp-d-ete .camp-hero h2 {
  font-size: 2.5rem;
  margin-bottom: 1rem;
}

.public-page--camp-d-ete .camp-cards {
  display: flex;
  gap: 2rem;
  justify-content: center;
  flex-wrap: wrap;
}

.public-page--camp-d-ete .camp-card {
  background: white;
  padding: 2rem;
  width: 300px;
  border-radius: 16px;
  box-shadow: 0 15px 35px rgba(0,0,0,0.08);
  transition: all 0.3s ease;
  text-align: center;
}

.public-page--camp-d-ete .camp-card:hover {
  transform: scale(1.05);
  box-shadow: 0 20px 40px rgba(0,0,0,0.15);
}

.public-page--camp-d-ete .camp-info {
  background: #f8f8f8;
  padding: 3rem;
  border-radius: 16px;
  text-align: center;
}

.public-page--camp-d-ete .camp-info li {
  font-size: 1.1rem;
  margin: 0.8rem 0;
}

.public-page--camp-d-ete .btn-camp {
  display: inline-block;
  background: #be1e2c;
  color: white;
  padding: 14px 28px;
  border-radius: 10px;
  font-weight: bold;
  text-decoration: none;
  margin-top: 1.5rem;
  transition: 0.3s;
}

.public-page--camp-d-ete .btn-camp:hover {
  background: #9e1824;
}

.public-page--camp-d-ete .camp-highlight {
  background: #f4f6f8;
  padding: 2.5rem;
  border-radius: 16px;
}

.public-page--camp-d-ete .camp-highlight ul {
  list-style: none;
  padding: 0;
}

.public-page--camp-d-ete .camp-highlight li {
  margin: 0.8rem 0;
  font-size: 1.1rem;
}

.public-page--camp-d-ete strong {
  color: #be1e2c;
}

.public-page--camp-d-ete section {
  padding-bottom: 1rem;
}

.public-page--camp-d-ete .camp-cards {
  margin-top: 3rem;
}
</style>
