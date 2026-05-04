<template>
  <!-- Bandeau de prévisualisation -->
  <div class="preview-banner">
    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
    Aperçu - cette page n'est pas encore publiée
  </div>

  <article
    v-if="page"
    class="public-page public-page--preview">
    <component :is="'style'" v-if="page.customCss">{{ page.customCss }}</component>
    <div class="public-page__container">
      <h1 class="public-page__title">{{ page.title }}</h1>
      <template v-if="page.contentMode === 'blocks'">
        <PageBlocksRenderer :blocks="parsedBlocks" />
      </template>
      <div v-else class="public-page__content" v-html="page.content"></div>
    </div>
  </article>
  <div v-else-if="isLoading" class="public-page public-page--loading">
    <Loader />
  </div>
  <div v-else class="public-page public-page--not-found">
    <div class="public-page__container">
      <h1>Aperçu non disponible</h1>
      <p v-if="errorMessage">{{ errorMessage }}</p>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {computed, ref, watch} from "vue"
import {useRoute} from "vue-router"
import {useHead} from "@unhead/vue"
import axios from "axios"
import {Page} from "@/types/entities"
import type {PageBlock} from "@/types/entities/pageBlock"
import Loader from "@/components/layouts/items/Loader.vue"
import PageBlocksRenderer from "@/components/blocks/PageBlocksRenderer.vue"

const route = useRoute()
const pageTitle = ref('Aperçu')
useHead({title: pageTitle})

const page = ref<Page | null>(null)
const isLoading = ref(true)
const errorMessage = ref('')

const parsedBlocks = computed<PageBlock[]>(() => {
  if (!page.value?.blocks) return []
  try { return JSON.parse(page.value.blocks) } catch { return [] }
})

async function loadPreview(slug: string, token: string) {
  isLoading.value = true
  errorMessage.value = ''
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/pages/preview/${slug}?token=${token}`)
    page.value = response.data
    pageTitle.value = `Aperçu - ${page.value!.title ?? ''}`
  } catch (error: any) {
    page.value = null
    if (error.response?.status === 403) {
      errorMessage.value = "Le lien de prévisualisation a expiré ou n'est pas valide."
    } else {
      errorMessage.value = "Impossible de charger l'aperçu."
    }
  }
  isLoading.value = false
}

watch(() => [route.params.slug, route.query.token], ([slug, token]) => {
  const tokenStr = typeof token === 'string' ? token : Array.isArray(token) ? token[0] : undefined
  if (slug && tokenStr) {
    loadPreview(slug as string, tokenStr)
  } else {
    isLoading.value = false
    errorMessage.value = "Paramètre de prévisualisation manquant."
  }
}, {immediate: true})
</script>

<style>
.preview-banner {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.625rem 1rem;
  background: #fbbf24;
  color: #78350f;
  font-weight: 600;
  font-size: 0.875rem;
  box-shadow: 0 2px 8px rgb(0 0 0 / 0.1);
}

.public-page--preview {
  padding-top: 180px !important;
}
</style>
