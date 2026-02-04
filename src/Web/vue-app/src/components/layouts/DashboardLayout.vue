<template>
  <div class="dashboard">
    <Navbar :member-is-loading="userIsLoading"/>

    <main class="dashboard__content">
      <LogoutPopup/>

      <Notifications/>

      <div>
        <div class="dashboard__content-header" v-if="!isMobile">
          <UserAvatar/>
          <LangSwitcher/>
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
  </div>
</template>
<script setup lang="ts">
import {onMounted, ref, computed} from "vue";
import {useMemberStore} from "@/stores/memberStore";
import {useAdministratorService, useMemberService} from "@/inversify.config";
import Navbar from "@/components/navigation/Navbar.vue";
import LogoutPopup from "@/components/layouts/items/LogoutPopup.vue";
import Notifications from "@/components/layouts/items/Notifications.vue";
import Loader from "@/components/layouts/items/Loader.vue";
import {useWindowSize} from "vue-window-size";
import UserAvatar from "@/components/account/UserAvatar.vue";
import LangSwitcher from "@/components/layouts/items/LangSwitcher.vue";
import {Administrator, Member} from "@/types";
import {Role} from "@/types/enums";
import {useAdministratorStore} from "@/stores/administratorStore";
import {usePersonStore} from "@/stores/personStore";
import {useUserStore} from "@/stores/userStore";

const userStore = useUserStore()
const personStore = usePersonStore()
const memberStore = useMemberStore()
const administratorStore = useAdministratorStore()

const memberService = useMemberService();
const administratorService = useAdministratorService();

const userIsLoading = ref(true)

const {width} = useWindowSize();
const isMobile = computed(() => width.value < 1200);

onMounted(async () => {
  userIsLoading.value = true
  if (userStore.hasRole(Role.Member)) {
    let member = await memberService.getAuthenticated() as Member;
    personStore.setPerson(member)
    memberStore.setMember(member)
  } else {
    let administrator = await administratorService.getAuthenticated() as Administrator;
    personStore.setPerson(administrator)
    administratorStore.setAdministrator(administrator)
  }
  userIsLoading.value = false
});
</script>