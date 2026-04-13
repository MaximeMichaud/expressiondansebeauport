<template>
  <div :class="['soc', isDarkMode && 'soc--dark']" data-social>
    <!-- Theme toggle (fixed, top-right) -->
    <button @click="toggleTheme" class="soc-theme-toggle" :title="isDarkMode ? 'Mode clair' : 'Mode sombre'">
      <svg v-if="isDarkMode" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"/><line x1="12" y1="1" x2="12" y2="3"/><line x1="12" y1="21" x2="12" y2="23"/><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/><line x1="1" y1="12" x2="3" y2="12"/><line x1="21" y1="12" x2="23" y2="12"/><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/></svg>
      <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z"/></svg>
    </button>

    <!-- Header -->
    <header class="soc-header">
      <div class="soc-header__strip">
        <div class="soc-header__left">
          <router-link :to="{ name: 'socialImportant' }" class="soc-header__brand">
            <span class="soc-header__logo-circle">
              <LogoEdb class="soc-header__logo-svg" />
            </span>
            <span class="soc-header__wordmark">EDB <span class="soc-header__wordmark-accent">Social</span></span>
          </router-link>

          <!-- Nav tabs (desktop) -->
          <nav v-if="isAuthenticated" class="soc-header__nav">
            <router-link
              v-for="tab in tabs"
              :key="tab.name"
              :to="{ name: tab.name }"
              :class="['soc-header__nav-item', isActive(tab.name) && 'is-active']"
            >
              <component :is="tab.icon" class="soc-header__nav-icon" />
              <span>{{ tab.label }}</span>
            </router-link>
          </nav>
        </div>

        <div v-if="isAuthenticated" class="soc-header__right">
          <router-link :to="{ name: 'socialAccount' }" class="soc-header__profile-btn" title="Mon compte">
            <span class="soc-header__pfp" :style="{ background: userAvatarColor }">
              <img v-if="userPfp" :src="userPfp" alt="" class="soc-header__pfp-img" />
              <span v-else class="soc-header__pfp-initials">{{ userInitials }}</span>
            </span>
            <span class="soc-header__profile-name">{{ userName }}</span>
          </router-link>
          <router-link :to="{ name: 'socialMessages' }" class="soc-header__icon-btn" title="Messages">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/>
            </svg>
            <span v-if="unreadCount > 0" class="soc-header__notif">{{ unreadCount > 9 ? '9+' : unreadCount }}</span>
          </router-link>
          <button @click="handleLogout" class="soc-header__icon-btn soc-header__icon-btn--logout" title="Se déconnecter">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
              <path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/>
            </svg>
          </button>

          <!-- Hamburger (mobile) -->
          <button
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

      <!-- Mobile nav -->
      <nav v-if="isAuthenticated && isMenuOpen" class="soc-header__mobile-nav">
        <router-link
          v-for="tab in tabs"
          :key="tab.name"
          :to="{ name: tab.name }"
          :class="['soc-header__mobile-item', isActive(tab.name) && 'is-active']"
          @click="isMenuOpen = false"
        >
          <component :is="tab.icon" class="soc-header__nav-icon" />
          <span>{{ tab.label }}</span>
        </router-link>
        <button
          class="soc-header__mobile-item"
          @click="toggleTheme(); isMenuOpen = false"
        >
          <svg v-if="isDarkMode" class="soc-header__nav-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"/><line x1="12" y1="1" x2="12" y2="3"/><line x1="12" y1="21" x2="12" y2="23"/><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/><line x1="1" y1="12" x2="3" y2="12"/><line x1="21" y1="12" x2="23" y2="12"/><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/></svg>
          <svg v-else class="soc-header__nav-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z"/></svg>
          <span>{{ isDarkMode ? 'Mode clair' : 'Mode sombre' }}</span>
        </button>
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
      <div class="soc-footer__strip">
        <div class="soc-footer__left">
          <span class="soc-footer__brand">EDB Social</span>
          <span class="soc-footer__sub">Expression Danse de Beauport</span>
        </div>
        <div class="soc-footer__right">
          <a :href="mainSiteUrl" class="soc-footer__link">
            Site principal
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="7" y1="17" x2="17" y2="7"/><polyline points="7 7 17 7 17 17"/></svg>
          </a>
          <a href="tel:4186666158" class="soc-footer__link">418-666-6158</a>
        </div>
      </div>
    </footer>

    <!-- Toasts -->
    <Teleport to="body">
      <TransitionGroup name="soc-toast" tag="div" class="soc-toast-container">
        <div
          v-for="toast in toasts"
          :key="toast.id"
          :class="['soc-toast', `soc-toast--${toast.type}`]"
          @click="dismissToast(toast.id)"
        >
          <svg v-if="toast.type === 'success'" class="soc-toast__icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6L9 17l-5-5"/></svg>
          <svg v-else class="soc-toast__icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>
          <span>{{ toast.message }}</span>
        </div>
      </TransitionGroup>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, h, onUnmounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/userStore'
import { useMemberStore } from '@/stores/memberStore'
import { useSocialService, useAuthenticationService } from '@/inversify.config'
import { useSignalR } from '@/composables/useSignalR'
import { useSocialToast } from '@/composables/useSocialToast'
import LogoEdb from '@/assets/icons/logo__edb.svg'

const { toasts, dismiss: dismissToast, success: toastSuccess, error: toastError } = useSocialToast()

const router = useRouter()
const userStore = useUserStore()
const memberStore = useMemberStore()
const socialService = useSocialService()
const { start: startSignalR, onMessage } = useSignalR()

const authService = useAuthenticationService()

const isMenuOpen = ref(false)
const isDarkMode = ref(localStorage.getItem('soc-theme') === 'dark')

// Sync dark class on body so <Teleport to="body"> content (modals, toasts) inherits dark mode
function syncBodyDark(dark: boolean) {
  document.body.classList.toggle('soc--dark', dark)
}
syncBodyDark(isDarkMode.value)
watch(isDarkMode, syncBodyDark)

function toggleTheme() {
  isDarkMode.value = !isDarkMode.value
  localStorage.setItem('soc-theme', isDarkMode.value ? 'dark' : 'light')
}
const unreadCount = computed(() => memberStore.unreadMessageCount)
const userName = computed(() => {
  const m = memberStore.member
  if (m?.firstName) return m.firstName
  const email = userStore.user?.email
  return email ? email.split('@')[0] : ''
})
const userPfp = computed(() => memberStore.member?.profileImageUrl || '')
const userInitials = computed(() => {
  const m = memberStore.member
  const f = m?.firstName?.[0] || ''
  const l = m?.lastName?.[0] || ''
  return (f + l).toUpperCase() || '?'
})

const avatarColors = ['#e53e3e', '#dd6b20', '#d69e2e', '#38a169', '#319795', '#3182ce', '#5a67d8', '#805ad5', '#d53f8c', '#e53e3e']
function getAvatarColor(name: string) {
  let hash = 0
  for (let i = 0; i < (name?.length || 0); i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return avatarColors[Math.abs(hash) % avatarColors.length]
}
const userAvatarColor = computed(() => {
  const m = memberStore.member
  if (m?.avatarColor) return m.avatarColor
  const name = m?.firstName ? `${m.firstName} ${m.lastName || ''}` : userName.value
  return getAvatarColor(name)
})

async function handleLogout() {
  await authService.logout()
  userStore.reset()
  memberStore.reset()
  await router.push({ name: 'socialLogin' })
}
const isAuthenticated = computed(() => !!userStore.user.email)

// Load member profile, unread count, and start SignalR when authenticated
watch(isAuthenticated, async (val) => {
  if (val) {
    try {
      const profile = await socialService.getMyProfile()
      memberStore.setMember(profile)
    } catch { /* */ }
    try {
      const result = await socialService.getUnreadCount()
      memberStore.setUnreadCount(result.count)
      if (result.joinRequestNotifications && result.joinRequestNotifications.length > 0) {
        for (const n of result.joinRequestNotifications) {
          if (n.status === 'Accepted') {
            toastSuccess(`Votre demande pour ${n.groupName} a été acceptée!`, 6000)
          } else if (n.status === 'Rejected') {
            toastError(`Votre demande pour ${n.groupName} a été refusée.`, 6000)
          }
        }
      }
    } catch { /* */ }
    try {
      await startSignalR()
      onMessage(() => { memberStore.incrementUnreadCount() })
    } catch { /* */ }
  }
}, { immediate: true })

// Poll unread count globally every 3 seconds
let unreadPoll: ReturnType<typeof setInterval> | null = null
watch(isAuthenticated, (val) => {
  if (val) {
    unreadPoll = setInterval(async () => {
      try {
        const result = await socialService.getUnreadCount()
        memberStore.setUnreadCount(result.count)
        if (result.joinRequestNotifications && result.joinRequestNotifications.length > 0) {
          for (const n of result.joinRequestNotifications) {
            if (n.status === 'Accepted') {
              toastSuccess(`Votre demande pour ${n.groupName} a été acceptée!`, 6000)
            } else if (n.status === 'Rejected') {
              toastError(`Votre demande pour ${n.groupName} a été refusée.`, 6000)
            }
          }
        }
      } catch { /* */ }
    }, 3000)
  } else if (unreadPoll) {
    clearInterval(unreadPoll)
    unreadPoll = null
  }
}, { immediate: true })
onUnmounted(() => {
  if (unreadPoll) clearInterval(unreadPoll)
})

const isActive = (name: string) => router.currentRoute.value.name === name

const mainSiteUrl = computed(() => '/')

const IconBell = { render: () => h('svg', { width: 18, height: 18, viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '1.8', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [h('path', { d: 'M18 8A6 6 0 006 8c0 7-3 9-3 9h18s-3-2-3-9' }), h('path', { d: 'M13.73 21a2 2 0 01-3.46 0' })]) }
const IconGrid = { render: () => h('svg', { width: 18, height: 18, viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '1.8', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [h('rect', { x: '3', y: '3', width: '7', height: '7', rx: '1' }), h('rect', { x: '14', y: '3', width: '7', height: '7', rx: '1' }), h('rect', { x: '3', y: '14', width: '7', height: '7', rx: '1' }), h('rect', { x: '14', y: '14', width: '7', height: '7', rx: '1' })]) }
const IconUsers = { render: () => h('svg', { width: 18, height: 18, viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '1.8', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [h('path', { d: 'M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2' }), h('circle', { cx: '9', cy: '7', r: '4' }), h('path', { d: 'M23 21v-2a4 4 0 00-3-3.87' }), h('path', { d: 'M16 3.13a4 4 0 010 7.75' })]) }

const tabs = [
  { name: 'socialImportant', label: 'Annonces', icon: IconBell },
  { name: 'socialPortal', label: 'Groupes', icon: IconGrid },
  { name: 'socialMembers', label: 'Membres', icon: IconUsers },
]
</script>

<style lang="scss">
.soc-theme-toggle {
  position: fixed;
  top: 16px;
  right: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: 12px;
  color: #78716c;
  background: rgba(0,0,0,0.05);
  cursor: pointer;
  transition: color 0.15s, background 0.15s;
  z-index: 999;
  &:hover { color: #1a1a1a; background: rgba(0,0,0,0.08); }

  .soc--dark & {
    color: rgba(255,255,255,0.4);
    background: rgba(255,255,255,0.06);
    &:hover { color: white; background: rgba(255,255,255,0.1); }
  }
}

$soc-black: #1a1a1a;
$soc-black-light: #f0f0f0;
$soc-black-subtle: #f7f7f7;
$soc-dark: #1c1917;
$soc-warm-gray: #f5f3f0;
$soc-border: #e7e0da;
$soc-text: #292524;
$soc-text-muted: #78716c;
$soc-font-display: 'Montserrat', sans-serif;
$soc-font-body: 'Karla', sans-serif;

.soc {
  // Light mode (default)
  --soc-page-bg: #{$soc-warm-gray};
  --soc-content-bg: white;
  --soc-content-border: rgba(0,0,0,0.04);
  --soc-text: #{$soc-text};
  --soc-text-muted: #{$soc-text-muted};
  --soc-border: #{$soc-border};
  --soc-bar-bg: white;
  --soc-bar-text: #{$soc-text-muted};
  --soc-bar-text-strong: #{$soc-dark};
  --soc-bar-hover: #{$soc-warm-gray};
  --soc-bar-active: #{$soc-warm-gray};
  --soc-logo-bg: #{$soc-black};
  --soc-logo-fill: white;
  --soc-notif-bg: #dc2626;
  --soc-notif-text: white;
  --soc-notif-ring: white;
  --soc-avatar-bg: #{$soc-black};
  --soc-avatar-text: white;
  --soc-ham-color: #{$soc-text};
  --soc-footer-sub: #{$soc-text-muted};
  --soc-input-bg: #faf9f7;
  --soc-input-border: #{$soc-border};
  --soc-card-bg: white;
  --soc-card-border: #ece9e4;
  --soc-divider: #f0f0f0;

  // Dark mode
  &--dark {
    --soc-page-bg: #0a0a09;
    --soc-content-bg: #181716;
    --soc-content-border: transparent;
    --soc-text: #e7e5e4;
    --soc-text-muted: #a8a29e;
    --soc-border: rgba(255,255,255,0.12);
    --soc-bar-bg: #181716;
    --soc-bar-text: rgba(255,255,255,0.5);
    --soc-bar-text-strong: white;
    --soc-bar-hover: rgba(255,255,255,0.08);
    --soc-bar-active: rgba(255,255,255,0.12);
    --soc-logo-bg: rgba(255,255,255,0.12);
    --soc-logo-fill: white;
    --soc-notif-bg: #dc2626;
    --soc-notif-text: white;
    --soc-notif-ring: #181716;
    --soc-avatar-bg: #3a3836;
    --soc-avatar-text: #e7e5e4;
    --soc-ham-color: rgba(255,255,255,0.6);
    --soc-footer-sub: rgba(255,255,255,0.35);
    --soc-input-bg: rgba(255,255,255,0.06);
    --soc-input-border: rgba(255,255,255,0.12);
    --soc-card-bg: #222120;
    --soc-card-border: rgba(255,255,255,0.1);
    --soc-divider: rgba(255,255,255,0.08);

    --primary: #e7e5e4;
    --ring: oklch(0.9 0 0);

    color: var(--soc-text);
  }

  --primary: #1a1a1a;
  --ring: oklch(0.145 0 0);
  min-height: 100vh;

  *, *::before, *::after {
    outline-color: oklch(0.145 0 0 / 0.5);
  }

  a, button, [role="button"], input[type="submit"], input[type="button"] {
    cursor: pointer;
  }
  display: flex;
  flex-direction: column;
  background: var(--soc-page-bg);
  font-family: $soc-font-body;
  color: var(--soc-text);
  transition: background 0.3s, color 0.3s;
}

.soc-header {
  position: sticky;
  top: 0;
  z-index: 900;

  &__strip {
    display: flex;
    align-items: center;
    justify-content: space-between;
    max-width: 960px;
    margin: 0 auto;
    padding: 0 20px;
    height: 56px;
    background: var(--soc-bar-bg);
    border-radius: 0 0 14px 14px;
    color: var(--soc-bar-text);
    box-shadow: 0 1px 3px rgba(0,0,0,0.04);
    transition: background 0.25s, color 0.25s;
  }

  &__left {
    display: flex;
    align-items: center;
    gap: 20px;
  }

  &__brand {
    display: flex;
    align-items: center;
    gap: 10px;
    text-decoration: none;
    flex-shrink: 0;
  }

  &__logo-circle {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    border-radius: 9px;
    background: var(--soc-logo-bg);
    flex-shrink: 0;
    transition: background 0.25s;
  }

  &__logo-svg {
    width: 20px;
    height: 20px;
    * { fill: var(--soc-logo-fill) !important; transition: fill 0.25s; }
  }

  &__wordmark {
    font-family: $soc-font-display;
    font-weight: 700;
    font-size: 0.95rem;
    letter-spacing: -0.02em;
    color: var(--soc-bar-text-strong);
    transition: color 0.25s;
  }

  &__wordmark-accent {
    font-weight: 500;
    color: var(--soc-bar-text);
    transition: color 0.25s;
  }

  &__nav {
    display: none;
  }

  &__nav-item {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 6px 14px;
    font-family: $soc-font-display;
    font-size: 0.78rem;
    font-weight: 600;
    color: var(--soc-bar-text);
    text-decoration: none;
    border-radius: 8px;
    cursor: pointer;
    transition: color 0.15s, background 0.15s;
    white-space: nowrap;
    &:hover { color: var(--soc-bar-text-strong); background: var(--soc-bar-hover); }
    &.is-active { color: var(--soc-bar-text-strong); background: var(--soc-bar-active); }
  }

  &__nav-icon {
    flex-shrink: 0;
    opacity: 0.5;
    .is-active & { opacity: 1; }
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
    width: 36px;
    height: 36px;
    border-radius: 10px;
    color: var(--soc-bar-text);
    text-decoration: none;
    cursor: pointer;
    transition: color 0.15s, background 0.15s;
    &:hover { color: var(--soc-bar-text-strong); background: var(--soc-bar-hover); }
    &--logout:hover { color: #dc2626; background: rgba(#dc2626, 0.08); }
  }

  &__pfp {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 26px;
    height: 26px;
    border-radius: 50%;
    background: var(--soc-avatar-bg);
    overflow: hidden;
    flex-shrink: 0;
  }

  &__pfp-img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  &__pfp-initials {
    font-family: $soc-font-display;
    font-weight: 700;
    font-size: 0.55rem;
    color: var(--soc-avatar-text);
  }

  &__profile-btn {
    display: flex;
    align-items: center;
    gap: 7px;
    padding: 4px 12px 4px 4px;
    border-radius: 10px;
    color: var(--soc-bar-text);
    text-decoration: none;
    cursor: pointer;
    transition: color 0.15s, background 0.15s;
    &:hover { color: var(--soc-bar-text-strong); background: var(--soc-bar-hover); }
  }

  &__profile-name {
    font-family: $soc-font-display;
    font-size: 0.78rem;
    font-weight: 600;
    white-space: nowrap;
    max-width: 120px;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  &__notif {
    position: absolute;
    top: 3px;
    right: 3px;
    min-width: 15px;
    height: 15px;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0 4px;
    border-radius: 99px;
    background: var(--soc-notif-bg);
    color: var(--soc-notif-text);
    font-family: $soc-font-display;
    font-size: 0.55rem;
    font-weight: 700;
    line-height: 1;
    box-shadow: 0 0 0 2px var(--soc-notif-ring);
    transition: background 0.25s, color 0.25s;
  }

  &__hamburger {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    gap: 4px;
    width: 36px;
    height: 36px;
    border-radius: 10px;
    cursor: pointer;
    transition: background 0.15s;
    &:hover { background: var(--soc-bar-hover); }
  }

  &__ham-line {
    display: block;
    width: 16px;
    height: 2px;
    background: var(--soc-ham-color);
    border-radius: 2px;
    transition: transform 0.25s ease, opacity 0.25s ease, background 0.25s;
    &.is-open:nth-child(1) { transform: translateY(6px) rotate(45deg); }
    &.is-open:nth-child(2) { opacity: 0; }
    &.is-open:nth-child(3) { transform: translateY(-6px) rotate(-45deg); }
  }

  // Mobile nav dropdown
  &__mobile-nav {
    display: flex;
    flex-direction: column;
    gap: 2px;
    max-width: 720px;
    margin: 0 auto;
    padding: 8px 20px 12px;
    background: var(--soc-bar-bg);
    transition: background 0.25s;
  }

  &__mobile-item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 10px 14px;
    font-family: $soc-font-display;
    font-size: 0.85rem;
    font-weight: 600;
    color: var(--soc-bar-text);
    text-decoration: none;
    border-radius: 10px;
    cursor: pointer;
    transition: color 0.15s, background 0.15s;
    &:hover { color: var(--soc-bar-text-strong); background: var(--soc-bar-hover); }
    &.is-active { color: var(--soc-bar-text-strong); background: var(--soc-bar-active); }
  }
}

@media (min-width: 48em) {
  .soc-header {
    &__strip {
      height: 52px;
      padding: 0 28px;
    }
    &__hamburger { display: none; }
    &__profile-btn, &__icon-btn--logout { display: flex; }
    &__nav {
      display: flex;
      align-items: center;
      gap: 4px;
    }
  }
}

.soc-main {
  flex: 1 1 0;
  min-height: 0;
  width: 100%;
  max-width: 720px;
  margin: 16px auto 0;
  background: var(--soc-content-bg);
  border-radius: 12px;
  transition: background 0.3s;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  overflow-x: hidden;
}

@media (min-width: 48em) {
  .soc-main { border-left: 1px solid var(--soc-content-border); border-right: 1px solid var(--soc-content-border); }
}

.soc-loader {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 80px 0;
  &__ring {
    width: 28px;
    height: 28px;
    border: 2.5px solid var(--soc-border);
    border-top-color: var(--soc-bar-text-strong);
    border-radius: 50%;
    animation: soc-spin 0.7s linear infinite;
  }
}

@keyframes soc-spin { to { transform: rotate(360deg); } }

.soc-footer {
  margin-top: auto;

  &__strip {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    max-width: 960px;
    margin: 16px auto 0;
    padding: 16px 24px;
    background: var(--soc-bar-bg);
    border-radius: 14px 14px 0 0;
    color: var(--soc-bar-text);
    font-size: 0.72rem;
    box-shadow: 0 -1px 3px rgba(0,0,0,0.03);
    transition: background 0.25s, color 0.25s;
  }

  &__left {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  &__brand {
    font-family: $soc-font-display;
    font-weight: 700;
    font-size: 0.8rem;
    color: var(--soc-bar-text-strong);
    letter-spacing: -0.01em;
    transition: color 0.25s;
  }

  &__sub {
    color: var(--soc-footer-sub);
    transition: color 0.25s;
  }

  &__right {
    display: flex;
    align-items: center;
    gap: 16px;
  }

  &__link {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    color: var(--soc-bar-text);
    text-decoration: none;
    font-weight: 500;
    cursor: pointer;
    transition: color 0.15s;
    white-space: nowrap;
    &:hover { color: var(--soc-bar-text-strong); }
  }
}

@media (max-width: 47.99em) {
  .soc-footer__strip {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
    border-radius: 0;
  }
  .soc-footer__right { gap: 12px; }
  .soc-theme-toggle { display: none; }
}

// Toast system
.soc-toast-container {
  position: fixed;
  bottom: 20px;
  right: 20px;
  z-index: 9999;
  display: flex;
  flex-direction: column-reverse;
  gap: 8px;
  pointer-events: none;
}

.soc-toast {
  pointer-events: auto;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 18px;
  border-radius: 12px;
  font-family: $soc-font-body;
  font-size: 0.82rem;
  font-weight: 500;
  cursor: pointer;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.12), 0 0 0 1px rgba(0, 0, 0, 0.04);
  backdrop-filter: blur(8px);
  max-width: 360px;

  &--success {
    background: rgba(21, 128, 61, 0.06);
    color: #15803d;
    border: 1px solid rgba(21, 128, 61, 0.15);
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08), 0 0 0 1px rgba(21, 128, 61, 0.1);
  }

  &--error {
    background: rgba(220, 38, 38, 0.06);
    color: #dc2626;
    border: 1px solid rgba(220, 38, 38, 0.15);
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08), 0 0 0 1px rgba(220, 38, 38, 0.1);
  }

  &__icon { flex-shrink: 0; }
}

// Transitions
.soc-toast-enter-active {
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
.soc-toast-leave-active {
  transition: all 0.2s ease-in;
}
.soc-toast-enter-from {
  opacity: 0;
  transform: translateX(30px) scale(0.95);
}
.soc-toast-leave-to {
  opacity: 0;
  transform: translateX(10px) scale(0.98);
}

@media (max-width: 47.99em) {
  .soc-toast-container {
    left: 16px;
    right: 16px;
    bottom: 16px;
  }
  .soc-toast { max-width: 100%; }
}

// ============================================
// Dark mode: global overrides for child content
// ============================================
.soc--dark {
  // Tailwind text colors
  .text-gray-900 { color: #e7e5e4 !important; }
  .text-gray-800 { color: #d6d3d1 !important; }
  .text-gray-700 { color: #a8a29e !important; }
  .text-gray-600 { color: #a8a29e !important; }
  .text-gray-500 { color: #78716c !important; }
  .text-gray-400 { color: #57534e !important; }

  // Backgrounds
  .bg-white { background-color: var(--soc-content-bg) !important; }
  .bg-gray-50 { background-color: rgba(255,255,255,0.03) !important; }
  .bg-gray-100 { background-color: rgba(255,255,255,0.06) !important; }
  .bg-gray-200 { background-color: var(--soc-avatar-bg) !important; color: var(--soc-avatar-text) !important; }
  .bg-gray-300 { background-color: var(--soc-avatar-bg) !important; color: var(--soc-avatar-text) !important; }

  // Borders
  .border-gray-100 { border-color: var(--soc-divider) !important; }
  .border-gray-200 { border-color: var(--soc-border) !important; }
  .border-gray-300 { border-color: var(--soc-border) !important; }
  .border-gray-900 { border-color: #e7e5e4 !important; }
  .divide-gray-100 > * + * { border-color: var(--soc-divider) !important; }
  .divide-gray-200 > * + * { border-color: var(--soc-border) !important; }

  // Inputs
  input, textarea, select {
    background-color: var(--soc-input-bg) !important;
    border-color: var(--soc-input-border) !important;
    color: var(--soc-text) !important;
    &::placeholder { color: #57534e !important; }
  }

  // Card shadow/outline removal
  .soc-main { box-shadow: none; outline: none; border: none; }

  // Group banner
  .group-banner { background: var(--soc-content-bg) !important; border-color: var(--soc-divider) !important; button:not(.soc-header__icon-btn--logout) { color: rgba(255,255,255,0.6) !important; } .soc-header__icon-btn--logout:hover { color: #dc2626 !important; } h1 { color: white !important; } }
  .group-header-avatar div { background: white !important; color: #1a1a1a !important; }

  // Group logo
  .group-logo.bg-\[\#1a1a1a\] { background-color: white !important; span { color: #1a1a1a !important; } }

  // Publish button
  .btn-publish.bg-\[\#1a1a1a\] { background-color: #e7e5e4 !important; color: #1c1917 !important; &:hover { background-color: white !important; } }

  // Buttons with bg-[#1a1a1a]
  .bg-\[\#1a1a1a\] { background-color: #e7e5e4 !important; color: #1c1917 !important; }
  .hover\:bg-\[\#000000\]:hover { background-color: white !important; color: #1c1917 !important; }

  // Spinners
  .border-\[\#1a1a1a\] { border-color: #e7e5e4 !important; }

  // Alert backgrounds
  .bg-red-50 { background-color: rgba(220, 38, 38, 0.1) !important; }
  .text-red-700, .text-red-600, .text-red-500 { color: #dc2626 !important; }
  .bg-green-50 { background-color: rgba(34, 197, 94, 0.1) !important; }
  .text-green-700 { color: #86efac !important; }

  // Password validation
  .text-emerald-700 { color: #6ee7b7 !important; }
  .bg-emerald-600 { background-color: #059669 !important; }

  // Group header dark bg stays dark
  .bg-\[\#1a1a1a\] { background-color: #0f0f0e !important; color: white !important; }

  // Hover states
  .hover\:bg-gray-50:hover { background-color: rgba(255,255,255,0.04) !important; }
  .hover\:bg-white:hover { background-color: rgba(255,255,255,0.06) !important; }

  // Border-b with gray
  .border-b { border-color: var(--soc-divider); }

  // Card styles from SocialAccount
  .soc-account {
    &__card { background: var(--soc-card-bg) !important; border-color: var(--soc-card-border) !important; }
    &__card-header { border-color: var(--soc-divider) !important; color: var(--soc-text-muted); h3 { color: var(--soc-text-muted) !important; } }
    &__input { background: var(--soc-input-bg) !important; border-color: var(--soc-input-border) !important; color: var(--soc-text) !important; &::placeholder { color: #57534e !important; } }
    &__avatar { color: white !important; }
    &__btn-primary { background: #e7e5e4 !important; color: #1c1917 !important; &:hover { background: white !important; } }
    &__header { border-color: var(--soc-divider) !important; }
    &__back { color: var(--soc-text-muted) !important; &:hover { color: var(--soc-text) !important; background: var(--soc-bar-hover) !important; } }
    &__title { color: var(--soc-text) !important; }
    &__label { color: var(--soc-text-muted) !important; }
    &__btn-logout { color: var(--soc-text-muted) !important; border-color: var(--soc-card-border) !important; &:hover { color: #fca5a5 !important; background: rgba(220,38,38,0.08) !important; } }
  }

  // Conversation
  .soc-convo__bubble--mine { background: #e7e5e4 !important; color: #1c1917 !important; }
  .soc-convo__bubble--other { background: #292524 !important; color: #d6d3d1 !important; }
  .soc-convo__bubble--pending { background: #57534e !important; }
  .soc-convo__send { background: #e7e5e4 !important; color: #1c1917 !important; }
}

// Dark mode for teleported content (modals, toasts)
// <Teleport to="body"> escapes .soc--dark wrapper, so we duplicate vars on body
body.soc--dark {
  --soc-page-bg: #0a0a09;
  --soc-content-bg: #181716;
  --soc-text: #e7e5e4;
  --soc-text-muted: #a8a29e;
  --soc-border: rgba(255,255,255,0.12);
  --soc-bar-bg: #181716;
  --soc-bar-text: rgba(255,255,255,0.5);
  --soc-bar-text-strong: white;
  --soc-bar-hover: rgba(255,255,255,0.08);
  --soc-bar-active: rgba(255,255,255,0.12);
  --soc-input-bg: rgba(255,255,255,0.06);
  --soc-input-border: rgba(255,255,255,0.12);
  --soc-card-bg: #222120;
  --soc-card-border: rgba(255,255,255,0.1);
  --soc-divider: rgba(255,255,255,0.08);

  // Toasts
  .soc-toast {
    &--success {
      background: rgba(21, 128, 61, 0.15);
      color: #86efac;
      border-color: rgba(21, 128, 61, 0.3);
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3), 0 0 0 1px rgba(21, 128, 61, 0.2);
    }
    &--error {
      background: rgba(220, 38, 38, 0.15);
      color: #fca5a5;
      border-color: rgba(220, 38, 38, 0.3);
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3), 0 0 0 1px rgba(220, 38, 38, 0.2);
    }
  }

  // Portal modal icon needs light stroke in dark mode
  .portal-modal__icon-ring svg { stroke: white; }

  // Delete/confirm modals — dark mode overrides for teleported content
  .portal-modal__card,
  .ann-modal__card,
  .ann-d-modal__card,
  .convo-modal__card,
  .mp-modal__card {
    background: #222120 !important;
    color: #e7e5e4;
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.7);
  }

  .portal-modal__title,
  .ann-modal__title,
  .ann-d-modal__title,
  .convo-modal__title,
  .mp-modal__title {
    color: #e7e5e4 !important;
  }

  .portal-modal__text,
  .ann-modal__text,
  .ann-d-modal__text,
  .convo-modal__text,
  .mp-modal__text {
    color: #a8a29e !important;
  }

  .ann-modal__icon-ring,
  .ann-d-modal__icon-ring,
  .convo-modal__icon-ring,
  .mp-modal__icon-ring {
    background: rgba(220, 38, 38, 0.15) !important;
  }

  // portal-modal icon-ring: only override when used for destructive actions (has inline red bg)
  // The join modal icon-ring keeps its neutral var(--soc-bar-hover) color
  .portal-modal__icon-ring {
    background: var(--soc-bar-hover) !important;
  }
  .portal-modal__icon-ring[style*="rgba(220"] {
    background: rgba(220, 38, 38, 0.15) !important;
  }

  .portal-modal__btn--cancel,
  .ann-modal__btn--cancel,
  .ann-d-modal__btn--cancel,
  .convo-modal__btn--cancel,
  .mp-modal__btn--cancel {
    background: rgba(255, 255, 255, 0.08) !important;
    color: #e7e5e4 !important;
    &:hover { background: rgba(255, 255, 255, 0.14) !important; }
  }

  .portal-modal__input,
  .ann-modal__input,
  .mp-modal__input {
    background: rgba(255, 255, 255, 0.06) !important;
    border-color: rgba(255, 255, 255, 0.12) !important;
    color: #e7e5e4 !important;
  }
}
</style>
