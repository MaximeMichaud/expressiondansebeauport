<template>
  <section v-if="isVisible" class="cookie-banner" aria-label="Préférences de cookies">
    <div class="cookie-banner__content">
      <p class="cookie-banner__title">Préférences de cookies</p>
      <p class="cookie-banner__text">
        Ce site utilise des cookies nécessaires à son fonctionnement ainsi que des cookies optionnels pour analyser l’utilisation du site et améliorer nos services.
        Vous pouvez accepter, refuser ou personnaliser vos préférences.
        <RouterLink to="/politique-confidentialite">Voir notre politique de confidentialité.</RouterLink>
      </p>

      <div v-if="showDetails" class="cookie-banner__choices">
        <label class="cookie-banner__choice cookie-banner__choice--disabled">
          <input type="checkbox" checked disabled>
          <span>
            <strong>Cookies nécessaires</strong>
            <small>Toujours actifs pour le bon fonctionnement du site.</small>
          </span>
        </label>

        <label class="cookie-banner__choice">
          <input v-model="analyticsEnabled" type="checkbox">
          <span>
            <strong>Cookies analytiques</strong>
            <small>Aident à comprendre l’utilisation du site.</small>
          </span>
        </label>

        <label class="cookie-banner__choice">
          <input v-model="marketingEnabled" type="checkbox">
          <span>
            <strong>Cookies marketing</strong>
            <small>Permettent d’adapter certains contenus ou campagnes.</small>
          </span>
        </label>
      </div>
    </div>

    <div class="cookie-banner__actions">
      <button class="cookie-banner__button cookie-banner__button--secondary" type="button" @click="rejectOptionalCookies">
        Refuser les optionnels
      </button>
      <button
        v-if="!showDetails"
        class="cookie-banner__button cookie-banner__button--secondary"
        type="button"
        @click="showDetails = true">
        Personnaliser
      </button>
      <button v-else class="cookie-banner__button" type="button" @click="saveCustomPreferences">
        Enregistrer mes préférences
      </button>
      <button class="cookie-banner__button" type="button" @click="acceptOptionalCookies">
        Accepter les optionnels
      </button>
    </div>
  </section>
</template>

<script setup lang="ts">
import {onBeforeUnmount, onMounted, ref} from "vue";
import {
  type CookiePreferences,
  getCookiePreferences,
  hasUserMadeChoice,
  setCookiePreferences
} from "@/services/cookiePreferencesService";
import {applyCookieControlledScripts} from "@/services/cookieControlledScripts";

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
.cookie-banner {
  position: fixed;
  right: 24px;
  bottom: 24px;
  left: 24px;
  z-index: 70000;
  display: flex;
  flex-direction: column;
  gap: 20px;
  align-items: stretch;
  max-width: 920px;
  margin: 0 auto;
  padding: 20px;
  color: #1f2933;
  background: #fff;
  border: 1px solid rgba(31, 41, 51, 0.12);
  border-radius: 12px;
  box-shadow: 0 18px 60px rgba(0, 0, 0, 0.18);
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
  border-top: 1px solid rgba(31, 41, 51, 0.12);
}

.cookie-banner__choice {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  padding: 12px;
  background: #f8fafc;
  border: 1px solid rgba(31, 41, 51, 0.08);
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
  color: #5f6b76;
}

.cookie-banner__choice--disabled {
  opacity: 0.75;
}

.cookie-banner__actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  padding-top: 16px;
  border-top: 1px solid rgba(31, 41, 51, 0.12);
}

.cookie-banner__button {
  padding: 10px 16px;
  font-weight: 700;
  color: #fff;
  cursor: pointer;
  background: #174ea6;
  border: 1px solid #174ea6;
  border-radius: 999px;
}

.cookie-banner__button--secondary {
  color: #174ea6;
  background: #fff;
}

@media (max-width: 720px) {
  .cookie-banner__actions {
    flex-direction: column-reverse;
  }
}
</style>
