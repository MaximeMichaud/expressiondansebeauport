<template>
  <article
    v-if="page"
    class="public-page">
    <component :is="'style'" v-if="page.customCss">{{ page.customCss }}</component>
    <div class="public-page__container">
      <h1 class="public-page__title">{{ page.title }}</h1>
      <div class="public-page__content" v-html="page.content"></div>
      <section class="public-page__map-section">
        <h2>Carte</h2>
        <p class="public-page__map-address">
          <a
            :href="googleMapsLink"
            target="_blank"
            rel="noopener noreferrer">
            788 avenue du Cénacle, 788 Av. de l'Éducation, Québec, QC G1E 5J4
          </a>
        </p>
        <div class="public-page__map-frame">
          <iframe
            title="Carte de l'école Expression Danse de Beauport"
            :src="googleMapsEmbedUrl"
            loading="lazy"
            referrerpolicy="no-referrer-when-downgrade"
            allowfullscreen>
          </iframe>
        </div>
      </section>
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
import {onMounted, ref} from "vue"
import {useI18n} from "vue3-i18n"
import axios from "axios"
import {Page} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"

const {t} = useI18n()
const contactPageSlug = "nous-joindre"
const googleMapsLink = "https://www.google.com/maps/search/?api=1&query=788+avenue+du+C%C3%A9nacle%2C+788+Av.+de+l%27%C3%89ducation%2C+Qu%C3%A9bec%2C+QC+G1E+5J4"
const googleMapsEmbedUrl = "https://www.google.com/maps?q=788+avenue+du+C%C3%A9nacle%2C+788+Av.+de+l%27%C3%89ducation%2C+Qu%C3%A9bec%2C+QC+G1E+5J4&z=15&output=embed"

const page = ref<Page | null>(null)
const isLoading = ref(true)

async function loadPage() {
  isLoading.value = true
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/pages/${contactPageSlug}`)
    page.value = response.data
    document.title = `${page.value!.title} - EDB`
  } catch {
    page.value = null
  }
  isLoading.value = false
}

onMounted(() => {
  loadPage()
})
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

.public-page--not-found h1 {
  margin-bottom: 1.5rem;
}

.public-page--not-found .btn {
  margin-top: 2.5rem;
  display: inline-block;
}

.public-page__container {
  max-width: 800px;
  margin: 0 auto;
}

.public-page__title {
  font-size: 2.5rem;
  margin-bottom: 2rem;
  color: var(--color-primary, #be1e2c);
  text-align: center;
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

.public-page__map-section {
  margin-top: 2.5rem;
}

.public-page__map-address {
  margin-bottom: 1rem;
}

.public-page__map-frame {
  overflow: hidden;
  border-radius: 16px;
  border: 1px solid rgba(190, 30, 44, 0.12);
  box-shadow: 0 14px 32px rgba(0, 0, 0, 0.08);
  aspect-ratio: 16 / 9;
  background: #f4f6f8;
}

.public-page__map-frame iframe {
  width: 100%;
  height: 100%;
  border: 0;
  display: block;
}

@media (max-width: 640px) {
  .public-page__map-frame {
    aspect-ratio: 4 / 3;
  }
}
</style>
