<template>
  <nav class="public-navbar" :class="{ 'is-scrolled': isScrolled, 'is-menu-open': isMenuOpen }">
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
          <li v-for="link in navLinks" :key="link.key">
            <a href="#" class="public-navbar__link" @click="isMenuOpen = false">
              {{ t(`public.nav.${link.key}`) }}
            </a>
          </li>
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
import { ref, onMounted, onUnmounted } from "vue";
import { useI18n } from "vue3-i18n";
import LogoEdb from "@/assets/icons/logo__edb.svg";

const { t } = useI18n();
const isMenuOpen = ref(false);
const isScrolled = ref(false);

const navLinks = [
  { key: "school" },
  { key: "recreational" },
  { key: "summerCamp" },
  { key: "troupes" },
  { key: "contact" },
];

function onScroll() {
  isScrolled.value = window.scrollY > 5;
}

onMounted(() => window.addEventListener("scroll", onScroll));
onUnmounted(() => window.removeEventListener("scroll", onScroll));
</script>
