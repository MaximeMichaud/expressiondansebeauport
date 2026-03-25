<template>
  <!-- Social subdomain layouts -->
  <SocialAuthLayout v-if="isSocial && isSocialAuthPath"/>
  <SocialLayout v-else-if="isSocial"/>

  <!-- Main site layouts (wrapped for scoped SCSS reset) -->
  <div v-else class="main-site">
    <PublicLayout v-if="isPublicPath"/>
    <AuthenticationLayout v-else-if="!userStore.user.email || isAuthenticationPath"/>
    <DashboardLayout v-else/>
  </div>
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
import {useUserService, useSiteSettingsService} from "@/inversify.config";
import {isSocialRoute} from "@/router";
import i18n from "@/i18n";
import {applyThemeSettings} from "@/theme";

const router = useRouter();
const userStore = useUserStore();
const userService = useUserService();
const siteSettingsService = useSiteSettingsService();

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
    }
  }

  if (isPublicPath.value)
    return

  if (isSocial.value && isSocialAuthPath.value)
    return

  if (!userStore.user.email) {
    const user = await userService.getCurrentUser().catch(() => null)
    if (user) {
      userStore.setUser(user)
    } else if (!isAuthenticationPath.value && !isSocialAuthPath.value) {
      if (isSocial.value) {
        await router.push({ name: 'socialLogin' })
      } else {
        await router.push(i18n.t("routes.login.path"))
      }
    }
  }
});

</script>

<style lang="scss">
@use "./sass/index.scss";
</style>
