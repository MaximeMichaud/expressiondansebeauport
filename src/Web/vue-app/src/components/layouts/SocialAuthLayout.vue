<template>
  <div :class="['soc-auth', isDarkMode && 'soc-auth--dark']">
    <!-- Theme toggle -->
    <button @click="toggleTheme" class="soc-auth__theme-toggle" :title="isDarkMode ? 'Mode clair' : 'Mode sombre'">
      <svg v-if="isDarkMode" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"/><line x1="12" y1="1" x2="12" y2="3"/><line x1="12" y1="21" x2="12" y2="23"/><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/><line x1="1" y1="12" x2="3" y2="12"/><line x1="21" y1="12" x2="23" y2="12"/><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/></svg>
      <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z"/></svg>
    </button>

    <div class="soc-auth__card">
      <div class="soc-auth__header">
        <span class="soc-auth__logo-circle">
          <LogoEdb class="soc-auth__logo-svg" />
        </span>
        <span class="soc-auth__title">EDB <span class="soc-auth__title-accent">Social</span></span>
      </div>
      <RouterView v-slot="{ Component }">
        <template v-if="Component">
          <Suspense>
            <component :is="Component" />
            <template #fallback>
              <div class="soc-auth__loader">
                <div class="soc-auth__loader-ring" />
              </div>
            </template>
          </Suspense>
        </template>
      </RouterView>
    </div>

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
import { ref, watch } from 'vue'
import LogoEdb from '@/assets/icons/logo__edb.svg'
import { useSocialToast } from '@/composables/useSocialToast'

const { toasts, dismiss: dismissToast } = useSocialToast()
const isDarkMode = ref(localStorage.getItem('soc-theme') === 'dark')

function syncBodyDark(dark: boolean) {
  document.body.classList.toggle('soc--dark', dark)
}
syncBodyDark(isDarkMode.value)
watch(isDarkMode, syncBodyDark)

function toggleTheme() {
  isDarkMode.value = !isDarkMode.value
  localStorage.setItem('soc-theme', isDarkMode.value ? 'dark' : 'light')
}
</script>

<style lang="scss">
$soc-black: #1a1a1a;
$soc-warm-gray: #f5f3f0;
$soc-border: #e7e0da;
$soc-font-display: 'Montserrat', sans-serif;

.soc-auth {
  --primary: #1a1a1a;
  --ring: oklch(0.145 0 0);
  min-height: 100vh;

  *, *::before, *::after {
    outline-color: oklch(0.145 0 0 / 0.5);
  }

  a, button, [role="button"], input[type="submit"] {
    cursor: pointer;
  }

  display: flex;
  align-items: center;
  justify-content: center;
  background: $soc-warm-gray;
  padding: 24px 16px;
  position: relative;
  transition: background 0.3s, color 0.3s;

  &__theme-toggle {
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
    transition: color 0.15s, background 0.15s;
    z-index: 10;
    &:hover { color: $soc-black; background: rgba(0,0,0,0.08); }
  }

  &__card {
    width: 100%;
    max-width: 420px;
    background: white;
    border-radius: 16px;
    box-shadow: 0 1px 4px rgba(0,0,0,0.04), 0 0 0 1px rgba(0,0,0,0.03);
    padding: 32px 28px;
    transition: background 0.3s, box-shadow 0.3s;
  }

  &__header {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
    margin-bottom: 28px;
  }

  &__logo-circle {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 38px;
    height: 38px;
    border-radius: 10px;
    background: $soc-black;
    box-shadow: 0 2px 8px rgba($soc-black, 0.25);
    transition: background 0.3s;
  }

  &__logo-svg {
    width: 24px;
    height: 24px;
    * { fill: white !important; }
  }

  &__title {
    font-family: $soc-font-display;
    font-weight: 700;
    font-size: 1.2rem;
    letter-spacing: -0.02em;
    color: #1c1917;
    transition: color 0.3s;
  }

  &__title-accent {
    font-weight: 500;
    color: #78716c;
    transition: color 0.3s;
  }

  &__loader {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 48px 0;
  }

  &__loader-ring {
    width: 28px;
    height: 28px;
    border: 2.5px solid $soc-border;
    border-top-color: $soc-black;
    border-radius: 50%;
    animation: soc-auth-spin 0.7s linear infinite;
  }

  // Dark mode
  &--dark {
    background: #0a0a09;

    .soc-auth__theme-toggle {
      color: rgba(255,255,255,0.4);
      background: rgba(255,255,255,0.06);
      &:hover { color: white; background: rgba(255,255,255,0.1); }
    }

    .soc-auth__card {
      background: #1e1d1c;
      box-shadow: 0 1px 4px rgba(0,0,0,0.2), 0 0 0 1px rgba(255,255,255,0.06);
    }

    .soc-auth__logo-circle {
      background: rgba(255,255,255,0.12);
    }

    .soc-auth__title { color: #e7e5e4; }
    .soc-auth__title-accent { color: rgba(255,255,255,0.4); }
    .soc-auth__loader-ring { border-color: rgba(255,255,255,0.1); border-top-color: white; }

    // Form text
    .text-gray-900 { color: #e7e5e4 !important; }
    .text-gray-700 { color: #a8a29e !important; }
    .text-gray-500 { color: #78716c !important; }
    .text-gray-400 { color: #57534e !important; }

    // Inputs
    input, textarea {
      background: rgba(255,255,255,0.06) !important;
      border-color: rgba(255,255,255,0.12) !important;
      color: #e7e5e4 !important;
      &::placeholder { color: #57534e !important; }
    }

    // Primary buttons
    .bg-\[\#1a1a1a\] { background-color: #e7e5e4 !important; color: #1c1917 !important; }
    .hover\:bg-\[\#000000\]:hover { background-color: white !important; color: #1c1917 !important; }

    // Links
    .text-\[\#1a1a1a\] { color: #e7e5e4 !important; }

    // Password validation
    .border-gray-300 { border-color: rgba(255,255,255,0.15) !important; }
    .bg-emerald-600 { background-color: #059669 !important; }
    .text-emerald-700 { color: #6ee7b7 !important; }

    // Error text
    .text-red-500 { color: #fca5a5 !important; }
  }
}

@keyframes soc-auth-spin { to { transform: rotate(360deg); } }
</style>
