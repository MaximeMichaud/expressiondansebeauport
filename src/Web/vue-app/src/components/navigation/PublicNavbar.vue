<template>
  <nav class="public-navbar" :class="{ 'is-scrolled': isScrolled, 'is-menu-open': isMenuOpen, 'is-solid': !isHome }">
    <div class="public-navbar__inner">
      <RouterLink :to="{ name: 'home' }" class="public-navbar__logo">
        <LogoEdb class="public-navbar__logo-icon" />
      </RouterLink>

      <button
        class="public-navbar__hamburger"
        :aria-label="t('global.menu')"
        @click="isMenuOpen = !isMenuOpen">
        <span class="public-navbar__hamburger-line" :class="{ 'is-open': isMenuOpen }" />
        <span class="public-navbar__hamburger-line" :class="{ 'is-open': isMenuOpen }" />
        <span class="public-navbar__hamburger-line" :class="{ 'is-open': isMenuOpen }" />
      </button>

      <div class="public-navbar__menu" :class="{ 'is-open': isMenuOpen }">
        <ul class="public-navbar__links">
          <li v-for="item in menuItems" :key="item.id">
            <RouterLink
              :to="item.url || `/${item.pageSlug}`"
              class="public-navbar__link"
              @click="isMenuOpen = false">
              {{ item.label }}
            </RouterLink>
          </li>
          <template v-if="menuItems.length === 0">
            <li v-for="link in fallbackLinks" :key="link.key">
              <a href="#" class="public-navbar__link" @click="isMenuOpen = false">
                {{ t(`public.nav.${link.key}`) }}
              </a>
            </li>
          </template>
        </ul>

        <a
          href="https://www.qidigo.com/u/Expression-Danse-de-Beauport"
          target="_blank"
          rel="noopener noreferrer"
          class="public-navbar__register-btn">
          {{ t('public.nav.register') }}
        </a>
      </div>
    </div>
  </nav>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useI18n } from "vue3-i18n";
import { useRoute } from "vue-router";
import axios from "axios";
import LogoEdb from "@/assets/icons/logo__edb.svg";
import { NavigationMenuItem } from "@/types/entities";

const { t } = useI18n();
const route = useRoute();
const isMenuOpen = ref(false);
const isScrolled = ref(false);
const isHome = computed(() => route.name === 'home');
const menuItems = ref<NavigationMenuItem[]>([]);

const fallbackLinks = [
  { key: "school" },
  { key: "recreational" },
  { key: "summerCamp" },
  { key: "troupes" },
  { key: "contact" },
];

function onScroll() {
  isScrolled.value = window.scrollY > 5;
}

async function loadMenu() {
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/menus/Primary`);
    menuItems.value = response.data?.menuItems || [];
  } catch {
    menuItems.value = [];
  }
}

onMounted(() => {
  window.addEventListener("scroll", onScroll);
  loadMenu();
});
onUnmounted(() => window.removeEventListener("scroll", onScroll));
</script>
