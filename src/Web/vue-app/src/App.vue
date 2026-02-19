<template>
  <PublicLayout v-if="isPublicPath"/>
  <AuthenticationLayout v-else-if="!userStore.user.email || isAuthenticationPath"/>
  <DashboardLayout v-else/>
</template>

<script lang="ts" setup>
import {computed, onMounted} from "vue";
import {useRouter} from "vue-router";
import {useUserStore} from "@/stores/userStore";
import PublicLayout from "@/components/layouts/PublicLayout.vue";
import AuthenticationLayout from "@/components/layouts/AuthenticationLayout.vue";
import DashboardLayout from "@/components/layouts/DashboardLayout.vue";
import {useUserService} from "@/inversify.config";

const router = useRouter();
const userStore = useUserStore();
const userService = useUserService();

const publicRoutes = ['home', 'publicPage']
const isPublicPath = computed(() => {
  return publicRoutes.includes(router.currentRoute.value.name as string)
});

const authenticationRoutes = ['login', 'twoFactor', 'forgotPassword', 'resetPassword']
const isAuthenticationPath = computed(() => {
  return authenticationRoutes.includes(router.currentRoute.value.name as string)
});

onMounted(async () => {
  if (isPublicPath.value)
    return

  if (!userStore.user.email) {
    const user = await userService.getCurrentUser().catch(() => null)
    if (user)
      userStore.setUser(user)
  }
});

</script>

<style lang="scss">
@use "./sass/index.scss";
</style>

