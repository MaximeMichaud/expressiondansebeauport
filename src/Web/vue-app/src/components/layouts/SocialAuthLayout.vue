<template>
  <div class="soc-auth">
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
  </div>
</template>

<script setup lang="ts">
import LogoEdb from '@/assets/icons/logo__edb.svg'
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
  display: flex;
  align-items: center;
  justify-content: center;
  background: $soc-warm-gray;
  padding: 24px 16px;

  &__card {
    width: 100%;
    max-width: 420px;
    background: white;
    border-radius: 16px;
    box-shadow: 0 1px 4px rgba(0,0,0,0.04), 0 0 0 1px rgba(0,0,0,0.03);
    padding: 32px 28px;
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
  }

  &__title-accent {
    font-weight: 500;
    color: #78716c;
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
}

@keyframes soc-auth-spin { to { transform: rotate(360deg); } }
</style>
