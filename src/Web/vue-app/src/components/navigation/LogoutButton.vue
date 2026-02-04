<template>
  <button :class="classes" @click="logout">
    {{ t('global.logout') }}
  </button>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n";
import {useRouter} from "vue-router";
import {useUserStore} from "@/stores/userStore";
import {useMemberStore} from "@/stores/memberStore";
import {useAuthenticationService} from "@/inversify.config";
import {useAdministratorStore} from "@/stores/administratorStore";
import {usePersonStore} from "@/stores/personStore";

// eslint-disable-next-line
const props = defineProps<{
  classes?: string
}>();

const {t} = useI18n()
const router = useRouter()
const userStore = useUserStore();
const personStore = usePersonStore();
const memberStore = useMemberStore();
const administratorStore = useAdministratorStore();
const authenticationService = useAuthenticationService()

async function logout() {
  let succeededOrNotResponse = await authenticationService.logout()
  if (succeededOrNotResponse.succeeded) {
    userStore.reset()
    personStore.reset()
    memberStore.reset()
    administratorStore.reset()
    await router.push(t("routes.login.path"))
  }
}
</script>