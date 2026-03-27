<template>
  <article
    v-if="page"
    class="public-page">
    <component :is="'style'" v-if="page.customCss">{{ page.customCss }}</component>
    <div class="public-page__container public-contact__container">
      <h1 class="public-page__title">{{ page.title }}</h1>
      <div class="public-page__content" v-html="page.content"></div>
      <section class="public-contact__map-section">
        <h2>Carte</h2>
        <p class="public-contact__map-address">
          <a
            :href="googleMapsLink"
            target="_blank"
            rel="noopener noreferrer">
            788 avenue du Cénacle, 788 Av. de l'Éducation, Québec, QC G1E 5J4
          </a>
        </p>
        <div class="public-contact__map-frame">
          <iframe
            title="Carte de l'école Expression Danse de Beauport"
            :src="googleMapsEmbedUrl"
            loading="lazy"
            referrerpolicy="no-referrer-when-downgrade"
            allowfullscreen>
          </iframe>
        </div>
      </section>
      <section class="public-contact__directions-section">
        <div class="public-contact__directions-intro">
          <p class="public-contact__directions-eyebrow">Accès au studio</p>
          <h2>Comment se rendre au studio</h2>
          <p>
            L'accès au stationnement et au local se fait par l'Avenue de l'Éducation. Google Maps peut
            parfois manquer de précision sur ce point, donc voici les repères visuels à suivre.
          </p>
          <p class="public-contact__directions-address">
            788 avenue du Cénacle / 788 avenue de l'Éducation, Québec, QC G1E 5J4
          </p>
        </div>

        <div class="public-contact__directions-grid">
          <article
            v-for="step in directionsSteps"
            :key="step.title"
            class="public-contact__direction-card"
            :class="{ 'public-contact__direction-card--stacked': step.layout === 'stacked' }">
            <button
              type="button"
              class="public-contact__direction-image-button"
              @click="openLightbox(step.image, step.alt)">
              <img
                :src="step.image"
                :alt="step.alt"
                class="public-contact__direction-image">
            </button>
            <div class="public-contact__direction-content">
              <p class="public-contact__direction-step">{{ step.step }}</p>
              <h3>{{ step.title }}</h3>
              <p>{{ step.description }}</p>
            </div>
          </article>
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
  <div
    v-if="lightboxImage"
    class="public-contact__lightbox"
    @click.self="closeLightbox">
    <div class="public-contact__lightbox-dialog">
      <button
        type="button"
        class="public-contact__lightbox-close"
        aria-label="Fermer l'image agrandie"
        @click="closeLightbox">
        ×
      </button>
      <img
        :src="lightboxImage"
        :alt="lightboxAlt"
        class="public-contact__lightbox-image">
    </div>
  </div>
</template>

<script lang="ts" setup>
import {onMounted, onUnmounted, ref, watch} from "vue"
import {useI18n} from "vue3-i18n"
import axios from "axios"
import {Page} from "@/types/entities"
import Loader from "@/components/layouts/items/Loader.vue"
import imageDevantStudio from "@/assets/images/directions/image-devant-studio.jpg"
import vueDeRueEducation from "@/assets/images/directions/vue-de-rue-education.jpg"
import directionsSurMap from "@/assets/images/directions/directions-sur-map.jpg"

const {t} = useI18n()
const contactPageSlug = "nous-joindre"
const googleMapsLink = "https://www.google.com/maps/search/?api=1&query=788+avenue+du+C%C3%A9nacle%2C+788+Av.+de+l%27%C3%89ducation%2C+Qu%C3%A9bec%2C+QC+G1E+5J4"
const googleMapsEmbedUrl = "https://www.google.com/maps?q=788+avenue+du+C%C3%A9nacle%2C+788+Av.+de+l%27%C3%89ducation%2C+Qu%C3%A9bec%2C+QC+G1E+5J4&z=15&output=embed"
const directionsSteps = [
  {
    step: "Étape 1",
    title: "Repérez le Centre des loisirs",
    description: "Le studio se trouve dans ce secteur. C'est le premier bon repère pour confirmer que vous êtes au bon endroit.",
    image: imageDevantStudio,
    alt: "Vue du centre des loisirs où se situe le studio de danse"
  },
  {
    step: "Étape 2",
    title: "Passez par l'Avenue de l'Éducation",
    description: "Repérez l'église et l'école, puis prenez l'entrée à droite pour accéder au bon stationnement et à l'entrée du local.",
    image: vueDeRueEducation,
    alt: "Repère visuel montrant l'église, l'école et l'entrée à droite par l'Avenue de l'Éducation",
    layout: "stacked"
  },
  {
    step: "Étape 3",
    title: "Suivez le trajet indiqué",
    description: "Cette vue aérienne montre le chemin à emprunter pour arriver directement du bon côté du bâtiment.",
    image: directionsSurMap,
    alt: "Vue aérienne annotée montrant le chemin à suivre vers le studio"
  }
]

const page = ref<Page | null>(null)
const isLoading = ref(true)
const lightboxImage = ref<string | null>(null)
const lightboxAlt = ref("")

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

function openLightbox(image: string, alt: string) {
  lightboxImage.value = image
  lightboxAlt.value = alt
}

function closeLightbox() {
  lightboxImage.value = null
  lightboxAlt.value = ""
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === "Escape" && lightboxImage.value) {
    closeLightbox()
  }
}

watch(lightboxImage, (val) => {
  document.body.style.overflow = val ? "hidden" : ""
})

onMounted(() => {
  loadPage()
  document.addEventListener("keydown", onKeydown)
})

onUnmounted(() => {
  document.removeEventListener("keydown", onKeydown)
  document.body.style.overflow = ""
})
</script>

<style scoped>
.public-contact__container {
  max-width: 800px;
}

.public-contact__map-section {
  margin-top: 2.5rem;
}

.public-contact__map-address {
  margin-bottom: 1rem;
}

.public-contact__map-frame {
  overflow: hidden;
  border-radius: 16px;
  border: 1px solid rgba(190, 30, 44, 0.12);
  box-shadow: 0 14px 32px rgba(0, 0, 0, 0.08);
  aspect-ratio: 16 / 9;
  background: #f4f6f8;
}

.public-contact__map-frame iframe {
  width: 100%;
  height: 100%;
  border: 0;
  display: block;
}

.public-contact__directions-section {
  margin-top: 3rem;
}

.public-contact__directions-intro {
  padding: 1.5rem;
  border-radius: 16px;
  background: #f4f6f8;
  border: 1px solid rgba(190, 30, 44, 0.12);
}

.public-contact__directions-eyebrow {
  margin-bottom: 0.5rem;
  font-size: 0.85rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-primary, #be1e2c);
}

.public-contact__directions-intro h2 {
  margin-bottom: 0.75rem;
}

.public-contact__directions-address {
  margin-top: 1rem;
  margin-bottom: 0;
  font-weight: 700;
}

.public-contact__directions-grid {
  display: grid;
  gap: 1.5rem;
  margin-top: 1.5rem;
}

.public-contact__direction-card {
  overflow: hidden;
  border-radius: 16px;
  background: #ffffff;
  border: 1px solid rgba(190, 30, 44, 0.12);
  box-shadow: 0 14px 32px rgba(0, 0, 0, 0.08);
}

.public-contact__direction-image-button {
  display: block;
  width: 100%;
  padding: 0;
  border: 0;
  background: transparent;
  cursor: zoom-in;
}

.public-contact__direction-image {
  display: block;
  width: 100%;
  aspect-ratio: 16 / 10;
  object-fit: cover;
  background: #f4f6f8;
}

.public-contact__direction-content {
  padding: 1.25rem;
}

.public-contact__direction-step {
  margin-bottom: 0.4rem;
  font-size: 0.85rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--color-primary, #be1e2c);
}

.public-contact__direction-content h3 {
  margin-bottom: 0.6rem;
}

.public-contact__direction-content p:last-child {
  margin-bottom: 0;
}

.public-contact__lightbox {
  position: fixed;
  inset: 0;
  z-index: 4000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background: rgba(0, 0, 0, 0.8);
}

.public-contact__lightbox-dialog {
  position: relative;
  width: min(1100px, 100%);
  padding-top: 3rem;
}

.public-contact__lightbox-close {
  position: absolute;
  top: 0;
  right: 0;
  z-index: 2;
  width: 2.5rem;
  height: 2.5rem;
  border: 0;
  border-radius: 999px;
  background: #ffffff;
  color: #111111;
  font-size: 1.75rem;
  line-height: 1;
  cursor: pointer;
  box-shadow: 0 10px 24px rgba(0, 0, 0, 0.25);
}

.public-contact__lightbox-image {
  display: block;
  width: 100%;
  max-height: 85vh;
  object-fit: contain;
  border-radius: 18px;
  background: #ffffff;
}

@media (max-width: 640px) {
  .public-contact__map-frame {
    aspect-ratio: 4 / 3;
  }
}

@media (min-width: 768px) {
  .public-contact__directions-grid {
    grid-template-columns: 1fr;
  }

  .public-contact__direction-card {
    display: grid;
    grid-template-columns: minmax(0, 1.35fr) minmax(320px, 0.9fr);
    align-items: stretch;
  }

  .public-contact__direction-image {
    aspect-ratio: auto;
    height: 100%;
    min-height: 340px;
    object-fit: contain;
  }

  .public-contact__direction-content {
    display: flex;
    flex-direction: column;
    justify-content: center;
    padding: 1.5rem;
  }

  .public-contact__direction-card--stacked {
    display: block;
  }

  .public-contact__direction-card--stacked .public-contact__direction-image {
    width: 100%;
    height: auto;
    min-height: 0;
    aspect-ratio: auto;
    object-fit: contain;
  }

  .public-contact__direction-card--stacked .public-contact__direction-content {
    display: block;
  }
}

@media (min-width: 1100px) {
  .public-contact__container {
    max-width: 1080px;
  }

  .public-contact__direction-card {
    grid-template-columns: minmax(0, 1.55fr) minmax(340px, 0.85fr);
  }

  .public-contact__direction-image {
    min-height: 420px;
  }

  .public-contact__direction-card--stacked .public-contact__direction-image {
    min-height: 0;
  }
}
</style>
