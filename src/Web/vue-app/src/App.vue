<template>
  <AuthenticationLayout v-if="!userStore.user.email || isAuthenticationPath"/>
  <DashboardLayout v-else/>
</template>

<script lang="ts" setup>
import {computed, onMounted} from "vue";
import {useRouter} from "vue-router";
import {useUserStore} from "@/stores/userStore";
import AuthenticationLayout from "@/components/layouts/AuthenticationLayout.vue";
import DashboardLayout from "@/components/layouts/DashboardLayout.vue";
import {useUserService} from "@/inversify.config";

const router = useRouter();
const userStore = useUserStore();
const userService = useUserService();

const authenticationRoutes = ['login', 'twoFactor', 'forgotPassword', 'resetPassword']
let isAuthenticationPath = computed(() => {
  return authenticationRoutes.includes(router.currentRoute.value.name as string)
});

onMounted(async () => {
  if (!userStore.user.email)
    userStore.setUser(await userService.getCurrentUser())
});

</script>

<style lang="scss">
@use "./sass/index.scss";
</style>

