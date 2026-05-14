<template>
  <section class="home-reviews" aria-labelledby="home-reviews-title">
    <div class="home-reviews__inner">
      <div class="home-reviews__header">
        <p class="home-reviews__eyebrow">{{ sectionEyebrow }}</p>
        <h2 id="home-reviews-title" class="home-reviews__title">{{ sectionTitle }}</h2>
        <p class="home-reviews__subtitle">{{ sectionSubtitle }}</p>
      </div>

      <Loader v-if="isLoading" />

      <div v-else-if="hasError" class="home-reviews__state" role="status">
        <p>{{ t('public.home.reviews.error') }}</p>
        <button type="button" class="btn" @click="loadReviews">{{ t('public.page.retry') }}</button>
      </div>

      <div v-else-if="reviews.length === 0" class="home-reviews__state" role="status">
        <p>{{ t('public.home.reviews.empty') }}</p>
      </div>

      <div v-else-if="isAutoMode" class="home-reviews__carousel-shell home-reviews__carousel-shell--animated" tabindex="0" :aria-label="t('public.home.reviews.carouselAriaLabel')">
        <div class="home-reviews__track home-reviews__track--animated">
          <div
            v-for="(group, groupIndex) in animatedGroups"
            :key="`group-${groupIndex}`"
            class="home-reviews__group"
            :aria-hidden="groupIndex > 0 ? 'true' : undefined">
            <article
              v-for="review in group"
              :key="`${groupIndex}-${review.id}`"
              class="home-reviews__card">
              <div class="home-reviews__stars" :aria-label="t('public.home.reviews.ratingAriaLabel', { rating: review.rating ?? 0 })">
                <Star
                  v-for="index in 5"
                  :key="index"
                  :size="18"
                  :class="['home-reviews__star', {'home-reviews__star--filled': index <= (review.rating ?? 0)}]" />
              </div>
              <h3 class="home-reviews__card-title">{{ review.title }}</h3>
              <p class="home-reviews__comment">{{ review.comment }}</p>
              <p class="home-reviews__author">{{ review.author }}</p>
            </article>
          </div>
        </div>
      </div>

      <div v-else class="home-reviews__carousel-shell home-reviews__carousel-shell--manual" :aria-label="t('public.home.reviews.carouselAriaLabel')">
        <div class="home-reviews__manual-track">
          <article
            v-for="review in reviews"
            :key="review.id"
            class="home-reviews__card">
            <div class="home-reviews__stars" :aria-label="t('public.home.reviews.ratingAriaLabel', { rating: review.rating ?? 0 })">
              <Star
                v-for="index in 5"
                :key="index"
                :size="18"
                :class="['home-reviews__star', {'home-reviews__star--filled': index <= (review.rating ?? 0)}]" />
            </div>
            <h3 class="home-reviews__card-title">{{ review.title }}</h3>
            <p class="home-reviews__comment">{{ review.comment }}</p>
            <p class="home-reviews__author">{{ review.author }}</p>
          </article>
        </div>
      </div>
    </div>
  </section>
</template>

<script lang="ts" setup>
import Loader from "@/components/layouts/items/Loader.vue"
import {useSiteSettingsService} from "@/serviceRegistry"
import {Review} from "@/types/entities"
import {Star} from "lucide-vue-next"
import {computed, onBeforeUnmount, onMounted, ref} from "vue"
import {useI18n} from "vue-i18n"

const {t} = useI18n()
const siteSettingsService = useSiteSettingsService()

const reviews = ref<Review[]>([])
const isLoading = ref(true)
const hasError = ref(false)
const prefersReducedMotion = ref(false)
const canHoverAnimate = ref(false)
const sectionEyebrow = ref(t("public.home.reviews.eyebrow"))
const sectionTitle = ref(t("public.home.reviews.title"))
const sectionSubtitle = ref(t("public.home.reviews.subtitle"))

let reduceMotionQuery: MediaQueryList | null = null
let hoverAnimationQuery: MediaQueryList | null = null

const isAutoMode = computed(() =>
  reviews.value.length > 1 &&
  canHoverAnimate.value &&
  !prefersReducedMotion.value
)

const animatedGroups = computed<Review[][]>(() => {
  return [reviews.value, reviews.value]
})

onMounted(async () => {
  setupMotionPreferences()
  await loadReviews()
})

onBeforeUnmount(() => {
  reduceMotionQuery?.removeEventListener("change", handleReducedMotionChange)
  hoverAnimationQuery?.removeEventListener("change", handleHoverAnimationChange)
})

async function loadReviews() {
  isLoading.value = true
  hasError.value = false

  try {
    const settings = await siteSettingsService.getPublic()
    reviews.value = settings.reviews || []
    sectionEyebrow.value = resolveSectionText(settings.reviewsSectionEyebrow, "public.home.reviews.eyebrow")
    sectionTitle.value = resolveSectionText(settings.reviewsSectionTitle, "public.home.reviews.title")
    sectionSubtitle.value = resolveSectionText(settings.reviewsSectionSubtitle, "public.home.reviews.subtitle")
  } catch {
    hasError.value = true
    reviews.value = []
    sectionEyebrow.value = t("public.home.reviews.eyebrow")
    sectionTitle.value = t("public.home.reviews.title")
    sectionSubtitle.value = t("public.home.reviews.subtitle")
  } finally {
    isLoading.value = false
  }
}

function setupMotionPreferences() {
  reduceMotionQuery = window.matchMedia("(prefers-reduced-motion: reduce)")
  hoverAnimationQuery = window.matchMedia("(hover: hover) and (pointer: fine)")

  prefersReducedMotion.value = reduceMotionQuery.matches
  canHoverAnimate.value = hoverAnimationQuery.matches

  reduceMotionQuery.addEventListener("change", handleReducedMotionChange)
  hoverAnimationQuery.addEventListener("change", handleHoverAnimationChange)
}

function handleReducedMotionChange(event: MediaQueryListEvent) {
  prefersReducedMotion.value = event.matches
}

function handleHoverAnimationChange(event: MediaQueryListEvent) {
  canHoverAnimate.value = event.matches
}

function resolveSectionText(value: string | undefined, fallbackKey: string) {
  return value?.trim() || t(fallbackKey)
}
</script>

<style scoped>
.home-reviews {
  padding: 4rem 1.5rem 5rem;
  background:
    radial-gradient(circle at top left, rgba(190, 30, 44, 0.08), transparent 38%),
    linear-gradient(180deg, #fffaf8 0%, #ffffff 100%);
}

.home-reviews__inner {
  max-width: 1200px;
  margin: 0 auto;
}

.home-reviews__header {
  max-width: 680px;
  margin: 0 auto 2rem;
  text-align: center;
}

.home-reviews__eyebrow {
  margin: 0 0 0.75rem;
  font-size: 0.9rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--primary);
}

.home-reviews__title {
  margin: 0;
  font-size: clamp(2rem, 4vw, 3rem);
  line-height: 1.18;
}

.home-reviews__subtitle {
  margin: 1rem 0 0;
  color: #57534e;
  font-size: 1.05rem;
  line-height: 1.7;
}

.home-reviews__state {
  display: grid;
  justify-items: center;
  gap: 1rem;
  padding: 2rem;
  border: 1px solid rgba(190, 30, 44, 0.12);
  border-radius: 1.5rem;
  background: rgba(255, 255, 255, 0.9);
}

.home-reviews__carousel-shell {
  position: relative;
  overflow-x: auto;
  overflow-y: visible;
  padding: 0.5rem 0.5rem 1.25rem;
  scrollbar-width: none;
  -ms-overflow-style: none;
  touch-action: pan-x;
  -webkit-overflow-scrolling: touch;
}

.home-reviews__carousel-shell::-webkit-scrollbar {
  display: none;
}

.home-reviews__carousel-shell::before,
.home-reviews__carousel-shell::after {
  content: "";
  position: absolute;
  top: 0;
  bottom: 0;
  width: clamp(2.5rem, 4vw, 4.5rem);
  pointer-events: none;
  z-index: 2;
}

.home-reviews__carousel-shell::before {
  left: 0;
  background: linear-gradient(90deg, rgba(255, 250, 248, 0.96) 0%, rgba(255, 250, 248, 0.82) 30%, rgba(255, 250, 248, 0) 100%);
}

.home-reviews__carousel-shell::after {
  right: 0;
  background: linear-gradient(270deg, rgba(255, 255, 255, 0.96) 0%, rgba(255, 255, 255, 0.82) 30%, rgba(255, 255, 255, 0) 100%);
}

.home-reviews__carousel-shell--animated {
  overflow: hidden;
}

.home-reviews__carousel-shell--manual::before,
.home-reviews__carousel-shell--manual::after {
  display: none;
}

.home-reviews__track {
  display: flex;
  width: max-content;
}

.home-reviews__track--animated {
  animation: home-reviews-marquee 38s linear infinite;
  will-change: transform;
}

.home-reviews__carousel-shell--animated:hover .home-reviews__track--animated,
.home-reviews__carousel-shell--animated:focus-visible .home-reviews__track--animated {
  animation-play-state: paused;
}

.home-reviews__group {
  display: grid;
  grid-auto-flow: column;
  grid-auto-columns: minmax(280px, 32rem);
  gap: 1.5rem;
  width: max-content;
  padding-right: 1.5rem;
}

.home-reviews__manual-track {
  display: grid;
  grid-auto-flow: column;
  grid-auto-columns: minmax(280px, 32rem);
  gap: 1.5rem;
  width: max-content;
}

.home-reviews__card {
  display: grid;
  gap: 1rem;
  min-height: 100%;
  padding: 2rem;
  border: 1px solid rgba(190, 30, 44, 0.1);
  border-radius: 1.75rem;
  background: linear-gradient(180deg, #ffffff 0%, #fff7f5 100%);
  box-shadow: 0 16px 36px rgba(15, 23, 42, 0.08);
  transform: translateY(0) scale(1) rotateX(0deg);
  transition: transform 220ms ease, box-shadow 220ms ease, border-color 220ms ease;
  transform-origin: center center;
}

.home-reviews__card:hover,
.home-reviews__card:focus-visible {
  transform: translateY(-6px) scale(1.015) rotateX(1deg);
  box-shadow: 0 24px 50px rgba(15, 23, 42, 0.14);
  border-color: rgba(190, 30, 44, 0.18);
  outline: none;
}

.home-reviews__stars {
  display: flex;
  gap: 0.25rem;
  color: #d6d3d1;
}

.home-reviews__star--filled {
  color: #f59e0b;
  fill: currentColor;
}

.home-reviews__card-title {
  margin: 0;
  font-size: 1.35rem;
  line-height: 1.3;
}

.home-reviews__comment {
  margin: 0;
  color: #44403c;
  line-height: 1.75;
}

.home-reviews__author {
  margin: auto 0 0;
  font-weight: 700;
  color: #1f2937;
}

@media (prefers-reduced-motion: reduce) {
  .home-reviews__track--animated {
    animation: none;
  }

  .home-reviews__card {
    transition: none;
  }
}

@media (max-width: 767px) {
  .home-reviews {
    padding: 3rem 1rem 4rem;
  }

  .home-reviews__title {
    line-height: normal;
  }

  .home-reviews__carousel-shell::before,
  .home-reviews__carousel-shell::after {
    width: 2rem;
  }

  .home-reviews__manual-track {
    grid-auto-columns: minmax(16rem, 82vw);
    gap: 1rem;
  }

  .home-reviews__card {
    gap: 0.85rem;
    padding: 1.2rem;
  }

  .home-reviews__card-title {
    font-size: 1.1rem;
  }

  .home-reviews__comment {
    font-size: 0.97rem;
    line-height: 1.65;
  }
}

@media (min-width: 768px) and (max-width: 1024px) {
  .home-reviews__title {
    line-height: normal;
  }

  .home-reviews__manual-track {
    grid-auto-columns: minmax(17.5rem, 68vw);
    gap: 1.15rem;
  }

  .home-reviews__card {
    gap: 0.95rem;
    padding: 1.5rem;
  }

  .home-reviews__card-title {
    font-size: 1.2rem;
  }
}

@keyframes home-reviews-marquee {
  from {
    transform: translateX(0);
  }

  to {
    transform: translateX(calc(-50%));
  }
}
</style>
