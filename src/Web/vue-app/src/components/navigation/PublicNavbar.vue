<template>
  <nav class="public-navbar" :class="{ 'is-menu-open': isMenuOpen }">
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
          <li v-for="item in menuItems" :key="item.id" class="nav-item">

            <RouterLink
              v-if="!item.children || item.children.length === 0"
              :to="item.url || `/${item.pageSlug}`"
              class="public-navbar__link"
            >
              {{ item.label }}
            </RouterLink>

            <span v-else class="public-navbar__link nav-parent">
              {{ item.label }}
            </span>

            <ul v-if="item.children && item.children.length" class="submenu">
              <li v-for="child in item.children" :key="child.id">
                <RouterLink
                  :to="child.url || `/${child.pageSlug}`"
                  class="submenu-link"
                >
                  {{ child.label }}
                </RouterLink>
              </li>
            </ul>

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
          href="https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session"
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
import { ref, onMounted } from "vue";
import { useI18n } from "vue3-i18n";
import axios from "axios";
import LogoEdb from "@/assets/icons/logo__edb.svg";
import { NavigationMenuItem } from "@/types/entities";

const { t } = useI18n();
const isMenuOpen = ref(false);
const menuItems = ref<NavigationMenuItem[]>([]);

const fallbackLinks = [
  { key: "school" },
  { key: "recreational" },
  { key: "summerCamp" },
  { key: "troupes" },
  { key: "contact" },
];

async function loadMenu() {
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/menus/Primary`);
    menuItems.value = response.data?.menuItems || [];
  } catch {
    menuItems.value = [];
  }
}

onMounted(() => {
  loadMenu();
});
</script>

<style scoped>

.nav-item{
  position: relative;
}

.submenu{
  position: absolute;
  top: 100%;
  left: 0;
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: 0 6px 20px rgba(0,0,0,0.08);
  list-style: none;
  padding: 6px 0;
  min-width: 200px;
  z-index: 50;

  opacity: 0;
  visibility: hidden;
  transform: translateY(5px);
  transition: all 0.2s ease;
}

.nav-item:hover .submenu{
  opacity: 1;
  visibility: visible;
  transform: translateY(0);
}

.nav-item{
  padding-bottom: 10px;
}

.nav-parent{
  cursor: default;
}

.submenu-link{
  display: block;
  padding: 10px 16px;
  font-size: 0.9rem;
  text-decoration: none;
  color: inherit;
  transition: background 0.15s ease;
}

.submenu-link:hover{
  background: var(--color-muted);
}

</style>