<template>
  <section v-if="isVisible" class="cookie-banner" :aria-label="t('public.cookieBanner.title')">
    <div class="cookie-banner__content">
      <p class="cookie-banner__title">{{ t('public.cookieBanner.title') }}</p>
      <p class="cookie-banner__text">
        {{ t('public.cookieBanner.description') }}
        <RouterLink to="/politique-confidentialite">{{ t('public.cookieBanner.privacyLink') }}</RouterLink>
      </p>

      <div v-if="showDetails" class="cookie-banner__choices">
        <label class="cookie-banner__choice cookie-banner__choice--disabled">
          <input type="checkbox" checked disabled>
          <span>
            <strong>{{ t('public.cookieBanner.necessary.title') }}</strong>
            <small>{{ t('public.cookieBanner.necessary.description') }}</small>
          </span>
        </label>

        <label class="cookie-banner__choice">
          <input v-model="analyticsEnabled" type="checkbox">
          <span>
            <strong>{{ t('public.cookieBanner.analytics.title') }}</strong>
            <small>{{ t('public.cookieBanner.analytics.description') }}</small>
          </span>
        </label>

        <label class="cookie-banner__choice">
          <input v-model="marketingEnabled" type="checkbox">
          <span>
            <strong>{{ t('public.cookieBanner.marketing.title') }}</strong>
            <small>{{ t('public.cookieBanner.marketing.description') }}</small>
          </span>
        </label>
      </div>
    </div>

    <div class="cookie-banner__actions">
      <button class="cookie-banner__button cookie-banner__button--secondary" type="button" @click="rejectOptionalCookies">
        {{ t('public.cookieBanner.actions.reject') }}
      </button>
      <button
        v-if="!showDetails"
        class="cookie-banner__button cookie-banner__button--secondary"
        type="button"
        @click="showDetails = true">
        {{ t('public.cookieBanner.actions.customize') }}
      </button>
      <button v-else class="cookie-banner__button" type="button" @click="saveCustomPreferences">
        {{ t('public.cookieBanner.actions.save') }}
      </button>
      <button class="cookie-banner__button" type="button" @click="acceptOptionalCookies">
        {{ t('public.cookieBanner.actions.accept') }}
      </button>
    </div>
  </section>
</template>

<script setup lang="ts">
import {onBeforeUnmount, onMounted, ref} from "vue";
import {useI18n} from "vue-i18n";
import {
  type CookiePreferences,
  getCookiePreferences,
  hasUserMadeChoice,
  setCookiePreferences
} from "@/services/cookiePreferencesService";
import {applyCookieControlledScripts} from "@/services/cookieControlledScripts";

const {t} = useI18n();

const isVisible = ref(!hasUserMadeChoice());
const showDetails = ref(false);
const currentPreferences = getCookiePreferences();
const analyticsEnabled = ref(currentPreferences?.analytics ?? false);
const marketingEnabled = ref(currentPreferences?.marketing ?? false);

function closeWithPreferences(preferences: Partial<CookiePreferences>) {
  const savedPreferences = setCookiePreferences(preferences);
  analyticsEnabled.value = savedPreferences.analytics;
  marketingEnabled.value = savedPreferences.marketing;
  applyCookieControlledScripts();
  isVisible.value = false;
  showDetails.value = false;
}

function acceptOptionalCookies() {
  closeWithPreferences({
    analytics: true,
    marketing: true
  });
}

function rejectOptionalCookies() {
  closeWithPreferences({
    analytics: false,
    marketing: false
  });
}

function saveCustomPreferences() {
  closeWithPreferences({
    analytics: analyticsEnabled.value,
    marketing: marketingEnabled.value
  });
}

function openCookiePreferences() {
  const preferences = getCookiePreferences();
  analyticsEnabled.value = preferences?.analytics ?? false;
  marketingEnabled.value = preferences?.marketing ?? false;
  showDetails.value = true;
  isVisible.value = true;
}

onMounted(() => {
  applyCookieControlledScripts();
  window.addEventListener('open-cookie-preferences', openCookiePreferences);
});
onBeforeUnmount(() => window.removeEventListener('open-cookie-preferences', openCookiePreferences));
</script>

<style scoped lang="scss">
@use "@/sass/tools" as *;

.cookie-banner {
  position: fixed;
  right: 24px;
  bottom: 24px;
  left: 24px;
  z-index: $zindex-popup;
  display: flex;
  flex-direction: column;
  gap: 20px;
  align-items: stretch;
  max-width: 920px;
  margin: 0 auto;
  padding: 20px;
  color: $color-text;
  background: $color-white;
  border: 1px solid $color-border;
  border-radius: 12px;
  box-shadow: 0 18px 60px rgba($color-black, 0.18);
}

.cookie-banner__content {
  display: grid;
  gap: 8px;
}

.cookie-banner__title {
  margin: 0;
  font-weight: 700;
}

.cookie-banner__text {
  margin: 0;
  line-height: 1.45;
}

.cookie-banner a {
  font-weight: 500;
  text-decoration: underline;
}

.cookie-banner__choices {
  display: grid;
  gap: 14px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid $color-border;
}

.cookie-banner__choice {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  padding: 12px;
  background: $color-grey-lighter;
  border: 1px solid rgba($color-black, 0.08);
  border-radius: 10px;
}

.cookie-banner__choice input {
  margin-top: 4px;
}

.cookie-banner__choice strong,
.cookie-banner__choice small {
  display: block;
}

.cookie-banner__choice small {
  margin-top: 2px;
  color: $color-grey-medium;
}

.cookie-banner__choice--disabled {
  opacity: 0.75;
}

.cookie-banner__actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  padding-top: 16px;
  border-top: 1px solid $color-border;
}

.cookie-banner__button {
  padding: 10px 16px;
  font-weight: 700;
  color: $color-white;
  cursor: pointer;
  background: var(--primary);
  border: 1px solid var(--primary);
  border-radius: 999px;
}

.cookie-banner__button--secondary {
  color: var(--primary);
  background: $color-white;
}

@media (max-width: 720px) {
  .cookie-banner__actions {
    flex-direction: column-reverse;
  }
}
</style>
