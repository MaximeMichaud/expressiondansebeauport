<template>
  <div class="lang-switcher"
       :class="{ 'lang-switcher--green-light': isMobile, 'lang-switcher--white lang-switcher--authentication-page': isAuthenticationPath }"
       v-if="LOCALES.length > 1">
    <template
      v-for="locale in LOCALES"
      :key="locale.value">
      <button class="lang-switcher__option"
                   :aria-label="t('global.changeLanguage')"
                   v-if="locale.value != currentLocale"
                   @click="switchLanguage(locale.value)">
        <IconWeb :class="`icon ${isAuthenticationPath ? 'icon--white' : 'icon--green'}`" />
        <span class="lang-switcher__option-text">{{ locale.value }}</span>
      </button>
    </template>
  </div>
</template>

<script lang="ts" setup>
import { useI18n } from "vue3-i18n";
import { computed } from "vue";
import { LOCALES } from "@/locales";
import IconWeb from "vue-material-design-icons/Web.vue";
import { useWindowSize } from "vue-window-size";
import { useRouter } from "vue-router";

const { width } = useWindowSize();
const isMobile = computed(() => width.value < 768);

const { t, getLocale, setLocale } = useI18n();

const currentLocale = computed(() => getLocale());

const router = useRouter();

const authenticationRoutes = ['login', 'twoFactor', 'forgotPassword', 'resetPassword']
let isAuthenticationPath = computed(() => {
  return authenticationRoutes.includes(router.currentRoute.value.name as string)
});

async function switchLanguage(locale: string) {
  setLocale(locale);
  document.documentElement.lang = locale;
  document.cookie = "lang=" + locale + ";path=/";
}

</script>
