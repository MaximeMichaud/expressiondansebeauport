<template>
  <!-- Social subdomain layouts -->
  <SocialAuthLayout v-if="isSocial && isSocialAuthPath"/>
  <SocialLayout v-else-if="isSocial"/>

  <!-- Main site layouts -->
  <PublicLayout v-else-if="isPublicPath"/>
  <AuthenticationLayout v-else-if="!userStore.user.email || isAuthenticationPath"/>
  <DashboardLayout v-else/>

  <CookieBanner/>
  <UpdateToast/>
</template>

<script lang="ts" setup>
import {computed, onMounted} from "vue";
import {useRouter} from "vue-router";
import {useUserStore} from "@/stores/userStore";
import PublicLayout from "@/components/layouts/PublicLayout.vue";
import AuthenticationLayout from "@/components/layouts/AuthenticationLayout.vue";
import DashboardLayout from "@/components/layouts/DashboardLayout.vue";
import SocialLayout from "@/components/layouts/SocialLayout.vue";
import SocialAuthLayout from "@/components/layouts/SocialAuthLayout.vue";
import CookieBanner from "@/components/layouts/items/CookieBanner.vue";
import UpdateToast from "@/components/UpdateToast.vue";
import {useUserService, useSiteSettingsService} from "@/serviceRegistry";
import {isSocialRoute} from "@/router";
import i18n from "@/i18n";
import {applyThemeSettings} from "@/theme";
import {useSiteSettingsStore} from "@/stores/siteSettingsStore";
import {useHead} from "@unhead/vue";

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

  const user = await userService.getCurrentUser().catch(() => null)
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
