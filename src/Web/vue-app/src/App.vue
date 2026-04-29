<template>
  <!-- Social subdomain layouts -->
  <SocialAuthLayout v-if="isSocial && isSocialAuthPath"/>
  <SocialLayout v-else-if="isSocial"/>

  <!-- Main site layouts -->
  <PublicLayout v-else-if="isPublicPath"/>
  <AuthenticationLayout v-else-if="!userStore.user.email || isAuthenticationPath"/>
  <DashboardLayout v-else/>

  <UpdateToast/>
</template>

<script lang="ts" setup>
import {computed, onMounted, defineAsyncComponent} from "vue";
import {useRouter} from "vue-router";
import {useUserStore} from "@/stores/userStore";
import PublicLayout from "@/components/layouts/PublicLayout.vue";
import AuthenticationLayout from "@/components/layouts/AuthenticationLayout.vue";
import UpdateToast from "@/components/UpdateToast.vue";
import {useUserService, useSiteSettingsService} from "@/serviceRegistry";
import {isSocialRoute} from "@/router";
import i18n from "@/i18n";
import {applyThemeSettings} from "@/theme";
import {useSiteSettingsStore} from "@/stores/siteSettingsStore";
import {useHead} from "@unhead/vue";
import Cookies from "universal-cookie";

const DashboardLayout = defineAsyncComponent(() => import("@/components/layouts/DashboardLayout.vue"));
const SocialLayout = defineAsyncComponent(() => import("@/components/layouts/SocialLayout.vue"));
const SocialAuthLayout = defineAsyncComponent(() => import("@/components/layouts/SocialAuthLayout.vue"));

const router = useRouter();
const userStore = useUserStore();
const userService = useUserService();
const siteSettingsService = useSiteSettingsService();
const siteSettingsStore = useSiteSettingsStore();

const pageTitle = computed(() => {
  const titleKey = [...router.currentRoute.value.matched].reverse().find(r => r.meta.title)?.meta.title as string | undefined
  return titleKey ? i18n.global.t(titleKey) : ''
})

useHead({
  title: pageTitle,
  titleTemplate: (title) => {
    if (isSocialRoute(router.currentRoute.value)) {
      return title ? `EDB Social - ${title}` : 'EDB Social'
    }
    const siteName = siteSettingsStore.siteTitle
    if (!title) return siteName || ''
    return siteName ? `${title} | ${siteName}` : title
  }
})

const publicRoutes = ['home', 'publicPage', 'notFound']
const isPublicPath = computed(() => {
  return publicRoutes.includes(router.currentRoute.value.name as string)
    || router.currentRoute.value.meta?.public === true
});

const authenticationRoutes = ['login', 'twoFactor', 'forgotPassword', 'resetPassword']
const isAuthenticationPath = computed(() => {
  return authenticationRoutes.includes(router.currentRoute.value.name as string)
});

const isSocial = computed(() => isSocialRoute(router.currentRoute.value))

const isSocialAuthPath = computed(() => {
  return router.currentRoute.value.meta?.socialAuth === true
});

onMounted(async () => {
  if (!isSocial.value) {
    const siteSettings = await siteSettingsService.getPublic().catch(() => null)
    if (siteSettings) {
      applyThemeSettings(siteSettings)
      if (siteSettings.siteTitle) {
        siteSettingsStore.setSiteTitle(siteSettings.siteTitle)
      }
    }
  }

  if (isSocial.value && isSocialAuthPath.value)
    return

  // Visiteur sans accessToken lisible côté JS : inutile d'interroger /users/me, ce qui
  // produirait un 401 attendu (et un 403 en cascade depuis l'intercepteur sur /refresh-token).
  // Le refreshToken est httpOnly donc pas lisible ici, on s'appuie sur l'accessToken seul.
  // TODO sécurité : passer accessToken en httpOnly + axios withCredentials + lecture
  // du cookie côté JwtBearer ASP.NET. Tant que l'accessToken reste lisible en JS pour
  // alimenter le header Bearer, on s'appuie sur sa présence pour gater l'appel.
  const hasAccessToken = !!new Cookies().get("accessToken")
  const user = hasAccessToken
    ? await userService.getCurrentUser().catch(() => null)
    : null
  if (user) {
    userStore.setUser(user)
  } else if (!isAuthenticationPath.value && !isSocialAuthPath.value && !isPublicPath.value) {
    if (isSocial.value) {
      await router.push({ name: 'socialLogin' })
    } else {
      await router.push(i18n.global.t("routes.login.path"))
    }
  }
});

</script>

<style lang="scss">
@use "./sass/index.scss";
</style>
