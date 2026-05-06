<template>
  <Transition name="back-to-top">
    <button
      v-if="isVisible"
      type="button"
      class="back-to-top"
      :aria-label="t('global.backToTop')"
      :title="t('global.backToTop')"
      @click="scrollToTop"
    >
      <ChevronUp :size="22" :stroke-width="2.5" aria-hidden="true" />
    </button>
  </Transition>
</template>

<script lang="ts" setup>
import { ref, onMounted, onUnmounted, watch, nextTick } from "vue";
import { useRoute } from "vue-router";
import { useI18n } from "vue-i18n";
import { ChevronUp } from "lucide-vue-next";

const SCROLL_THRESHOLD = 300;
const MIN_SCROLLABLE_PIXELS = 400;
const RECHECK_DELAY_MS = 120;

const { t } = useI18n();
const route = useRoute();

const isVisible = ref(false);
const isPageLong = ref(false);
let ticking = false;
let resizeObserver: ResizeObserver | null = null;

function checkPageLength() {
  isPageLong.value =
    document.documentElement.scrollHeight - window.innerHeight >
    MIN_SCROLLABLE_PIXELS;
  if (!isPageLong.value) isVisible.value = false;
  else updateVisibility();
}

function updateVisibility() {
  isVisible.value = isPageLong.value && window.scrollY > SCROLL_THRESHOLD;
}

function onScroll() {
  if (ticking) return;
  ticking = true;
  requestAnimationFrame(() => {
    updateVisibility();
    ticking = false;
  });
}

function scrollToTop() {
  const reduceMotion = window.matchMedia(
    "(prefers-reduced-motion: reduce)",
  ).matches;
  window.scrollTo({ top: 0, behavior: reduceMotion ? "auto" : "smooth" });
}

onMounted(() => {
  checkPageLength();
  window.addEventListener("scroll", onScroll, { passive: true });
  window.addEventListener("resize", checkPageLength, { passive: true });
  if (typeof ResizeObserver !== "undefined") {
    resizeObserver = new ResizeObserver(() => checkPageLength());
    resizeObserver.observe(document.body);
  }
});

onUnmounted(() => {
  window.removeEventListener("scroll", onScroll);
  window.removeEventListener("resize", checkPageLength);
  resizeObserver?.disconnect();
  resizeObserver = null;
});

watch(
  () => route.fullPath,
  async () => {
    isVisible.value = false;
    await nextTick();
    checkPageLength();
    setTimeout(checkPageLength, RECHECK_DELAY_MS);
  },
);
</script>
