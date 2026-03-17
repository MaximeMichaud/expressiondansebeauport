<template>
  <div class="soc">
    <!-- Header -->
    <header class="soc-header">
      <div class="soc-header__bar">
        <!-- Left: Logo + wordmark -->
        <router-link to="/" class="soc-header__brand">
          <span class="soc-header__logo-circle">
            <LogoEdb class="soc-header__logo-svg" />
          </span>
          <span class="soc-header__wordmark">EDB <span class="soc-header__wordmark-accent">Social</span></span>
        </router-link>

        <!-- Right: Icons (desktop) + Hamburger (mobile) -->
        <div class="soc-header__right">
          <template v-if="isAuthenticated">
            <router-link :to="{ name: 'socialMessages' }" class="soc-header__icon-btn" title="Messages">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                <path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/>
              </svg>
              <span v-if="unreadCount > 0" class="soc-header__notif">{{ unreadCount > 9 ? '9+' : unreadCount }}</span>
            </router-link>
            <router-link :to="{ name: 'socialAccount' }" class="soc-header__icon-btn" title="Mon compte">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="8" r="4"/><path d="M20 21a8 8 0 00-16 0"/>
              </svg>
            </router-link>
          </template>
          <button
            v-if="isAuthenticated"
            class="soc-header__hamburger"
            aria-label="Menu"
            @click="isMenuOpen = !isMenuOpen"
          >
            <span :class="['soc-header__ham-line', isMenuOpen && 'is-open']" />
            <span :class="['soc-header__ham-line', isMenuOpen && 'is-open']" />
            <span :class="['soc-header__ham-line', isMenuOpen && 'is-open']" />
          </button>
        </div>
      </div>

      <!-- Nav tabs -->
      <nav v-if="isAuthenticated" class="soc-header__nav" :class="{ 'is-open': isMenuOpen }">
        <router-link
          v-for="tab in tabs"
          :key="tab.name"
          :to="{ name: tab.name }"
          :class="['soc-header__nav-item', isActive(tab.name) && 'is-active']"
          @click="isMenuOpen = false"
        >
          <component :is="tab.icon" class="soc-header__nav-icon" />
          <span>{{ tab.label }}</span>
        </router-link>
      </nav>
    </header>

    <!-- Content -->
    <main class="soc-main">
      <RouterView v-slot="{ Component }">
        <template v-if="Component">
          <Suspense>
            <component :is="Component" />
            <template #fallback>
              <div class="soc-loader">
                <div class="soc-loader__ring" />
              </div>
            </template>
          </Suspense>
        </template>
      </RouterView>
    </main>

    <!-- Footer -->
    <footer v-if="isAuthenticated" class="soc-footer">
      <div class="soc-footer__inner">
        <span class="soc-footer__brand">Expression Danse de Beauport</span>
        <span class="soc-footer__sep">&middot;</span>
        <span>788 Av. de l'Éducation, Québec</span>
        <span class="soc-footer__sep">&middot;</span>
        <span>418-666-6158</span>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, h, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/userStore'
import { useMemberStore } from '@/stores/memberStore'
import { useSocialService } from '@/inversify.config'
import { useSignalR } from '@/composables/useSignalR'
import LogoEdb from '@/assets/icons/logo__edb.svg'

const router = useRouter()
const userStore = useUserStore()
const memberStore = useMemberStore()
const socialService = useSocialService()
const { start: startSignalR, onMessage } = useSignalR()

const isMenuOpen = ref(false)
const unreadCount = computed(() => memberStore.unreadMessageCount)
const isAuthenticated = computed(() => !!userStore.user.email)

// Load unread count and start SignalR when authenticated
watch(isAuthenticated, async (val) => {
  if (val) {
    try {
      const count = await socialService.getUnreadCount()
      memberStore.setUnreadCount(count)
    } catch { /* */ }
    try {
      await startSignalR()
      onMessage(() => { memberStore.incrementUnreadCount() })
    } catch { /* */ }
  }
}, { immediate: true })

const isActive = (name: string) => router.currentRoute.value.name === name

const IconHome = { render: () => h('svg', { width: 18, height: 18, viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '1.8', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [h('path', { d: 'M3 9l9-7 9 7v11a2 2 0 01-2 2H5a2 2 0 01-2-2z' }), h('polyline', { points: '9 22 9 12 15 12 15 22' })]) }
const IconBell = { render: () => h('svg', { width: 18, height: 18, viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '1.8', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [h('path', { d: 'M18 8A6 6 0 006 8c0 7-3 9-3 9h18s-3-2-3-9' }), h('path', { d: 'M13.73 21a2 2 0 01-3.46 0' })]) }
const IconGrid = { render: () => h('svg', { width: 18, height: 18, viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '1.8', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [h('rect', { x: '3', y: '3', width: '7', height: '7', rx: '1' }), h('rect', { x: '14', y: '3', width: '7', height: '7', rx: '1' }), h('rect', { x: '3', y: '14', width: '7', height: '7', rx: '1' }), h('rect', { x: '14', y: '14', width: '7', height: '7', rx: '1' })]) }

const tabs = [
  { name: 'socialHome', label: 'Accueil', icon: IconHome },
  { name: 'socialImportant', label: 'Annonces', icon: IconBell },
  { name: 'socialPortal', label: 'Groupes', icon: IconGrid },
]
</script>

<style lang="scss">
$soc-red: #be1e2c;
$soc-red-light: #f9e8ea;
$soc-red-subtle: #fdf4f5;
$soc-dark: #1c1917;
$soc-warm-gray: #f5f3f0;
$soc-border: #e7e0da;
$soc-text: #292524;
$soc-text-muted: #78716c;
$soc-font-display: 'Montserrat', sans-serif;
$soc-font-body: 'Karla', sans-serif;

.soc {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: $soc-warm-gray;
  font-family: $soc-font-body;
  color: $soc-text;
}

.soc-header {
  position: sticky;
  top: 0;
  z-index: 900;
  background: white;
  border-bottom: 1px solid $soc-border;
  box-shadow: 0 1px 3px rgba(0,0,0,0.04);

  &__bar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    max-width: 960px;
    margin: 0 auto;
    padding: 0 20px;
    height: 56px;
  }

  &__brand {
    display: flex;
    align-items: center;
    gap: 10px;
    text-decoration: none;
    color: $soc-dark;
  }

  &__logo-circle {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 34px;
    height: 34px;
    border-radius: 10px;
    background: $soc-red;
    flex-shrink: 0;
    box-shadow: 0 2px 8px rgba($soc-red, 0.25);
  }

  &__logo-svg {
    width: 22px;
    height: 22px;
    * { fill: white !important; }
  }

  &__wordmark {
    font-family: $soc-font-display;
    font-weight: 700;
    font-size: 1.05rem;
    letter-spacing: -0.02em;
    color: $soc-dark;
  }

  &__wordmark-accent {
    font-weight: 500;
    color: $soc-text-muted;
  }

  &__right {
    display: flex;
    align-items: center;
    gap: 2px;
  }

  &__icon-btn {
    position: relative;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 38px;
    height: 38px;
    border-radius: 10px;
    color: $soc-text-muted;
    text-decoration: none;
    transition: color 0.15s, background 0.15s;
    &:hover { color: $soc-red; background: $soc-red-subtle; }
  }

  &__notif {
    position: absolute;
    top: 4px;
    right: 4px;
    min-width: 16px;
    height: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0 4px;
    border-radius: 99px;
    background: $soc-red;
    color: white;
    font-family: $soc-font-display;
    font-size: 0.6rem;
    font-weight: 700;
    line-height: 1;
    box-shadow: 0 0 0 2px white;
  }

  &__hamburger {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    gap: 4px;
    width: 38px;
    height: 38px;
    border-radius: 10px;
    cursor: pointer;
    transition: background 0.15s;
    &:hover { background: $soc-warm-gray; }
  }

  &__ham-line {
    display: block;
    width: 18px;
    height: 2px;
    background: $soc-text;
    border-radius: 2px;
    transition: transform 0.25s ease, opacity 0.25s ease;
    &.is-open:nth-child(1) { transform: translateY(6px) rotate(45deg); }
    &.is-open:nth-child(2) { opacity: 0; }
    &.is-open:nth-child(3) { transform: translateY(-6px) rotate(-45deg); }
  }

  &__nav {
    display: none;
    max-width: 960px;
    margin: 0 auto;
    padding: 0 20px;
    &.is-open {
      display: flex;
      flex-direction: column;
      gap: 2px;
      padding-bottom: 12px;
    }
  }

  &__nav-item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 10px 14px;
    font-family: $soc-font-display;
    font-size: 0.85rem;
    font-weight: 600;
    color: $soc-text-muted;
    text-decoration: none;
    border-radius: 10px;
    transition: color 0.15s, background 0.15s;
    &:hover { color: $soc-text; background: $soc-warm-gray; }
    &.is-active { color: $soc-red; background: $soc-red-light; }
  }

  &__nav-icon {
    flex-shrink: 0;
    opacity: 0.65;
    .is-active & { opacity: 1; }
  }
}

@media (min-width: 48em) {
  .soc-header {
    &__bar { height: 60px; padding: 0 32px; }
    &__hamburger { display: none; }
    &__nav {
      display: flex;
      flex-direction: row;
      align-items: stretch;
      gap: 0;
      padding: 0 32px;
      border-top: 1px solid $soc-border;
      &.is-open { flex-direction: row; padding-bottom: 0; }
    }
    &__nav-item {
      padding: 12px 18px;
      border-radius: 0;
      border-bottom: 2px solid transparent;
      margin-bottom: -1px;
      font-size: 0.8rem;
      letter-spacing: 0.01em;
      background: none !important;
      &:hover { color: $soc-text; border-bottom-color: $soc-border; background: none !important; }
      &.is-active { color: $soc-red; border-bottom-color: $soc-red; background: none !important; }
    }
  }
}

.soc-main {
  flex: 1;
  width: 100%;
  max-width: 720px;
  margin: 16px auto;
  background: white;
  border-radius: 12px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.04), 0 0 0 1px rgba(0,0,0,0.03);
  overflow: hidden;
}

@media (max-width: 47.99em) {
  .soc-main { margin: 0; border-radius: 0; box-shadow: none; }
}

.soc-loader {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 80px 0;
  &__ring {
    width: 28px;
    height: 28px;
    border: 2.5px solid $soc-border;
    border-top-color: $soc-red;
    border-radius: 50%;
    animation: soc-spin 0.7s linear infinite;
  }
}

@keyframes soc-spin { to { transform: rotate(360deg); } }

.soc-footer {
  margin-top: auto;
  padding: 24px 20px;
  text-align: center;
  font-size: 0.78rem;
  color: $soc-text-muted;
  &__inner { display: flex; flex-wrap: wrap; justify-content: center; align-items: center; gap: 6px; }
  &__brand { font-family: $soc-font-display; font-weight: 600; color: $soc-text; }
  &__sep { color: $soc-border; }
}
</style>
