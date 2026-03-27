<template>
  <div class="navbar">

    <!-- Top bar: user avatar + hamburger (mobile only) -->
    <div class="navbar__topbar" v-if="windowWidth < 1225">
      <UserAvatar />
      <button class="navbar__toggle" :class="{ 'navbar__toggle--open': isExpanded }" @click="toggleExpansion">
        <Menu v-if="!isExpanded" :size="22" />
        <X v-else :size="22" />
      </button>
    </div>

    <!-- Nav content -->
    <div
        v-show="!memberIsLoading && (isExpanded || windowWidth >= 1225 || !$router.currentRoute.value.name)"
        class="navbar__content"
    >
      <AdminNavbarItems v-if="userStore.hasRole(Role.Admin)"/>

      <!-- Logout inside drawer on mobile -->
      <div class="navbar__mobile-logout">
        <LogoutButton classes="btn"/>
      </div>
    </div>

    <!-- Logout pinned at bottom on desktop -->
    <div class="navbar__footer">
      <LogoutButton classes="btn btn--fullscreen"/>
    </div>

  </div>
</template>

<script lang="ts" setup>
import {ref, watch} from "vue";
import {Menu, X} from "lucide-vue-next";
import {useRouter} from "vue-router";
import {useUserStore} from "@/stores/userStore";
import {Role} from "@/types/enums";
import {useWindowSize} from "@/composables/useWindowSize";
import AdminNavbarItems from "@/components/navigation/AdminNavbarItems.vue";
import LogoutButton from "@/components/navigation/LogoutButton.vue";
import UserAvatar from "@/components/account/UserAvatar.vue";

const {width: windowWidth} = useWindowSize();

// eslint-disable-next-line
const props = defineProps<{
  memberIsLoading: boolean
}>()

const router = useRouter()
const userStore = useUserStore()

const isExpanded = ref<boolean>(false);

watch(() => router.currentRoute.value.fullPath, () => {
  isExpanded.value = false
})

function toggleExpansion() {
  isExpanded.value = !isExpanded.value;
}
</script>

<style lang="scss" scoped>
</style>
