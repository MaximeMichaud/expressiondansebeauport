<template>
  <nav class="public-navbar" :class="{ 'is-menu-open': isMenuOpen }">
    <div class="public-navbar__inner">
      <RouterLink :to="{ name: 'home' }" class="public-navbar__logo" @click="closeAll">
        <LogoEdb class="public-navbar__logo-icon" />
      </RouterLink>

      <button
        class="public-navbar__hamburger"
        :aria-label="t('global.menu')"
        :aria-expanded="isMenuOpen"
        @click="toggleMenu">
        <span class="public-navbar__hamburger-line" :class="{ 'is-open': isMenuOpen }" />
        <span class="public-navbar__hamburger-line" :class="{ 'is-open': isMenuOpen }" />
        <span class="public-navbar__hamburger-line" :class="{ 'is-open': isMenuOpen }" />
      </button>

      <div class="public-navbar__menu" :class="{ 'is-open': isMenuOpen }">
        <ul class="public-navbar__links">
          <li
            v-for="item in menuItems"
            :key="item.id"
            class="nav-item"
            :class="{
              'has-submenu': hasChildren(item),
              'is-open': isOpen(item.id),
              'is-active-parent': isParentActive(item),
            }"
          >
            <RouterLink
              v-if="!hasChildren(item)"
              :to="item.url || `/${item.pageSlug}`"
              class="public-navbar__link"
              @click="closeAll"
            >
              {{ item.label }}
            </RouterLink>

            <button
              v-else
              type="button"
              class="public-navbar__link nav-parent"
              :aria-expanded="isOpen(item.id)"
              :aria-controls="`submenu-${item.id}`"
              aria-haspopup="true"
              @click="toggleItem(item.id)"
            >
              <span>{{ item.label }}</span>
              <ChevronDown :size="10" class="nav-parent__chevron" aria-hidden="true" />
            </button>

            <ul
              v-if="hasChildren(item)"
              :id="`submenu-${item.id}`"
              class="submenu"
              :aria-hidden="!isOpen(item.id) && isMenuOpen"
            >
              <li v-for="child in item.children" :key="child.id">
                <RouterLink
                  :to="child.url || `/${child.pageSlug}`"
                  class="submenu-link"
                  @click="closeAll"
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
import { ref, watch, onMounted, onBeforeUnmount } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute } from "vue-router";
import axios from "axios";
import { ChevronDown } from "lucide-vue-next";
import LogoEdb from "@/assets/icons/logo__edb.svg";
import { NavigationMenuItem } from "@/types/entities";

const { t } = useI18n();
const route = useRoute();
const isMenuOpen = ref(false);
const openItemId = ref<string | null>(null);
const menuItems = ref<NavigationMenuItem[]>([]);

const fallbackLinks = [
  { key: "school" },
  { key: "recreational" },
  { key: "summerCamp" },
  { key: "troupes" },
  { key: "contact" },
];

function hasChildren(item: NavigationMenuItem): boolean {
  return !!(item.children && item.children.length > 0);
}

function isOpen(id: string | undefined): boolean {
  return !!id && openItemId.value === id;
}

function toggleItem(id: string | undefined) {
  if (!id) return;
  openItemId.value = openItemId.value === id ? null : id;
}

function toggleMenu() {
  isMenuOpen.value = !isMenuOpen.value;
  if (!isMenuOpen.value) openItemId.value = null;
}

function closeAll() {
  openItemId.value = null;
  isMenuOpen.value = false;
}

function isParentActive(item: NavigationMenuItem): boolean {
  if (!hasChildren(item)) return false;
  const currentPath = route.path;
  return (item.children ?? []).some(child => {
    const target = child.url || (child.pageSlug ? `/${child.pageSlug}` : '');
    if (!target) return false;
    return currentPath === target || currentPath.startsWith(`${target}/`);
  });
}

async function loadMenu() {
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/menus/Primary`);
    menuItems.value = response.data?.menuItems || [];
  } catch {
    menuItems.value = [];
  }
}

function onDocumentClick(event: MouseEvent) {
  const target = event.target as HTMLElement | null;
  if (target && !target.closest('.public-navbar')) {
    openItemId.value = null;
  }
}

function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') {
    openItemId.value = null;
    isMenuOpen.value = false;
  }
}

watch(() => route.path, () => {
  closeAll();
});

watch(isMenuOpen, (open) => {
  if (typeof document !== 'undefined') {
    document.body.style.overflow = open ? 'hidden' : '';
  }
});

onMounted(() => {
  loadMenu();
  document.addEventListener('click', onDocumentClick);
  document.addEventListener('keydown', onKeydown);
});

onBeforeUnmount(() => {
  document.removeEventListener('click', onDocumentClick);
  document.removeEventListener('keydown', onKeydown);
  document.body.style.overflow = '';
});
</script>

<style scoped>
.nav-item {
  position: relative;
}

.nav-parent {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  cursor: pointer;
  border: none;
  font: inherit;
  text-align: left;
  background: transparent;
  position: relative;
}

.nav-parent__chevron {
  transition: transform 200ms cubic-bezier(0.4, 0, 0.2, 1), opacity 180ms ease;
  opacity: 0.85;
}

.submenu {
  list-style: none;
  margin: 0;
}

.submenu-link {
  display: block;
  font-size: 0.9rem;
  text-decoration: none;
  transition: background 0.15s ease, color 0.15s ease;
}

@media (max-width: 47.99em) {
  .nav-item {
    padding: 0;
  }

  .nav-parent {
    width: 100%;
    justify-content: space-between;
    min-height: 44px;
  }

  .nav-item.is-open .nav-parent__chevron {
    transform: rotate(180deg);
  }

  .submenu {
    max-height: 0;
    overflow: hidden;
    padding: 0 0 0 16px;
    transition: max-height 220ms cubic-bezier(0.4, 0, 0.2, 1);
  }

  .nav-item.is-open .submenu {
    max-height: 600px;
  }

  .submenu-link {
    padding: 10px 12px;
    color: rgba(255, 255, 255, 0.75);
    min-height: 44px;
    display: flex;
    align-items: center;
    border-radius: 6px;
  }

  .submenu-link:hover,
  .submenu-link.router-link-active {
    background: rgba(255, 255, 255, 0.12);
    color: #ffffff;
  }

  .nav-item.is-active-parent > .nav-parent {
    color: #ffffff;
    background-color: rgba(255, 255, 255, 0.08);
  }
}

@media (min-width: 48em) {
  .nav-parent__chevron {
    position: absolute;
    right: 1px;
    top: 50%;
    transform: translateY(-50%);
    opacity: 0.75;
    pointer-events: none;
  }

  .nav-item:hover .nav-parent__chevron,
  .nav-item.is-open .nav-parent__chevron,
  .nav-parent:focus-visible .nav-parent__chevron {
    opacity: 1;
    transform: translateY(-50%) rotate(180deg);
  }

  .submenu {
    position: absolute;
    top: calc(100% - 4px);
    left: 0;
    background: #ffffff;
    border: 1px solid #e5e7eb;
    border-radius: 10px;
    box-shadow: 0 10px 32px rgba(0, 0, 0, 0.12), 0 2px 6px rgba(0, 0, 0, 0.04);
    padding: 6px 0;
    min-width: 220px;
    z-index: 50;

    opacity: 0;
    visibility: hidden;
    transform: translateY(6px);
    pointer-events: none;
    transition: opacity 180ms ease, visibility 180ms ease, transform 180ms ease;
  }

  .nav-item:hover .submenu,
  .nav-item.is-open .submenu {
    opacity: 1;
    visibility: visible;
    transform: translateY(0);
    pointer-events: auto;
  }

  .submenu-link {
    padding: 10px 16px;
    color: #1a1a1a;
  }

  .submenu-link:hover,
  .submenu-link.router-link-active {
    background: #f4f6f8;
    color: #000000;
  }

  .nav-item.is-active-parent > .nav-parent {
    color: #ffffff;
  }

  .nav-item.is-active-parent > .nav-parent::after {
    content: "";
    position: absolute;
    bottom: 2px;
    left: 12px;
    right: 12px;
    height: 2px;
    border-radius: 1px;
    background-color: rgba(255, 255, 255, 0.9);
  }
}
</style>
