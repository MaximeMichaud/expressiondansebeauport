<template>
  <div v-if="isVisible" class="news-banner">
    <RouterLink :to="bannerUrl" class="news-banner__link">
      <span class="news-banner__text">{{ bannerText }}</span>
      <span class="news-banner__arrow" aria-hidden="true">→</span>
    </RouterLink>
    <button class="news-banner__close" :aria-label="t('global.close')" @click="dismiss">
      <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
        <line x1="18" y1="6" x2="6" y2="18" />
        <line x1="6" y1="6" x2="18" y2="18" />
      </svg>
    </button>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import axios from "axios";

const { t } = useI18n();

const STORAGE_KEY = "news-banner-dismissed";

const isVisible = ref(false);
const bannerText = ref("");
const bannerUrl = ref("/actualites");

async function loadBannerSettings() {
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/site-settings`);
    const data = response.data;
    if (data?.isBannerEnabled) {
      bannerText.value = data.bannerText || "";
      bannerUrl.value = data.bannerUrl || "/actualites";
      const dismissed = sessionStorage.getItem(STORAGE_KEY);
      if (!dismissed) {
        isVisible.value = true;
        setBannerHeightVar("40px");
      }
    }
  } catch {
    isVisible.value = false;
  }
}

function setBannerHeightVar(height: string) {
  document.documentElement.style.setProperty("--banner-height", height);
}

function dismiss() {
  isVisible.value = false;
  sessionStorage.setItem(STORAGE_KEY, "1");
  setBannerHeightVar("0px");
}

onMounted(() => {
  loadBannerSettings();
});
</script>

<style scoped>
.news-banner {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 200;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 10px 48px 10px 16px;
  background-color: var(--primary);
  color: #fff;
  font-size: 0.875rem;
  font-weight: 500;
}

.news-banner__link {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #fff;
  text-decoration: none;
  flex: 1;
  justify-content: center;

  &:hover .news-banner__text {
    text-decoration: underline;
  }
}

.news-banner__arrow {
  font-size: 1rem;
  transition: transform 0.2s ease;
}

.news-banner__link:hover .news-banner__arrow {
  transform: translateX(4px);
}

.news-banner__close {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  background: rgba(255, 255, 255, 0.15);
  border: none;
  border-radius: 50%;
  color: #fff;
  cursor: pointer;
  transition: background 0.2s ease;

  &:hover {
    background: rgba(255, 255, 255, 0.3);
  }
}
</style>
