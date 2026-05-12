<template>
  <div class="dashboard">
    <Navbar :member-is-loading="userIsLoading"/>

    <main class="dashboard__content">
      <LogoutPopup/>

      <Notifications/>

      <div>
        <div class="dashboard__content-header" v-if="!isMobile">
          <UserAvatar/>
          <HelpButton v-if="isAdminRoute" class="dashboard__help-button"/>
        </div>

        <RouterView v-slot="{Component}">
          <template v-if="Component">
            <Suspense>
              <component :is="Component"/>
              <template #fallback>
                <Loader/>
              </template>
            </Suspense>
          </template>
        </RouterView>
      </div>
    </main>

    <HelpDrawer v-if="isAdminRoute"/>
  </div>
</template>
<script setup lang="ts">
import {onMounted, ref, computed} from "vue";
import {useRoute, useRouter} from "vue-router";
import {useI18n} from "vue-i18n";
import {useAdministratorService} from "@/serviceRegistry";
import Navbar from "@/components/navigation/Navbar.vue";
import LogoutPopup from "@/components/layouts/items/LogoutPopup.vue";
import Notifications from "@/components/layouts/items/Notifications.vue";
import Loader from "@/components/layouts/items/Loader.vue";
import {useWindowSize} from "@/composables/useWindowSize";
import UserAvatar from "@/components/account/UserAvatar.vue";
import HelpButton from "@/components/help/HelpButton.vue";
import HelpDrawer from "@/components/help/HelpDrawer.vue";
import {Administrator} from "@/types";
import {useAdministratorStore} from "@/stores/administratorStore";
import {usePersonStore} from "@/stores/personStore";

const {t} = useI18n()
const router = useRouter()
const route = useRoute()
const personStore = usePersonStore()
const administratorStore = useAdministratorStore()

const administratorService = useAdministratorService();

const userIsLoading = ref(true)

const {width} = useWindowSize();
const isMobile = computed(() => width.value < 1200);
const isAdminRoute = computed(() => {
  const name = route.name
  if (!name) return false
  const value = String(name)
  return value === 'admin' || value.startsWith('admin.children.')
})

onMounted(async () => {
  userIsLoading.value = true
  try {
    const administrator = await administratorService.getAuthenticated() as Administrator;
    personStore.setPerson(administrator)
    administratorStore.setAdministrator(administrator)
  } catch {
    await router.push(t("routes.login.path"))
  } finally {
    userIsLoading.value = false
  }
});
</script>