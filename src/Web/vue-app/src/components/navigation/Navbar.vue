<template>
  <div class="navbar">
    <template v-if="$windowWidth < 1225">
      <h2 class="navbar__title">{{ t("global.menu") }}</h2>

      <button
          v-if="$router.currentRoute.value.name"
          class="navbar__drawer-btn"
          @click="toggleExpansion"
      >
        {{ t(`routes.${rootRouteName}.name`) }}
        <IconChevron :class="{'icon--rotate-180' : !isExpanded}" class="icon icon--white"/>
      </button>
    </template>

    <Transition name="expand">
      <div
          v-show="!memberIsLoading && (isExpanded || $windowWidth >= 1225  || !$router.currentRoute.value.name)"
          ref="content"
          class="navbar__content"
      >
        <AdminNavbarItems v-if="userStore.hasRole(Role.Admin)"/>
        <MemberNavbarItems v-if="userStore.hasRole(Role.Member)"/>
      </div>
    </Transition>

    <div class="navbar__footer">
      <LogoutButton classes="navbar__logout"/>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {computed, onMounted, ref} from "vue";
import IconChevron from "@/assets/icons/icon__chevron.svg"
import {useI18n} from "vue3-i18n";
import {useRouter} from "vue-router";
import {useUserStore} from "@/stores/userStore";
import {Role} from "@/types/enums";
import AdminNavbarItems from "@/components/navigation/AdminNavbarItems.vue";
import MemberNavbarItems from "@/components/navigation/MemberNavbarItems.vue";
import LogoutButton from "@/components/navigation/LogoutButton.vue";

// eslint-disable-next-line
const props = defineProps<{
  memberIsLoading: boolean
}>()

const {t} = useI18n()

const userStore = useUserStore()

let isExpanded = ref<boolean>(true);
let content = ref<HTMLElement>();
let height = ref<string>();

const router = useRouter();
const currentRoute = ref(router.currentRoute);
let rootRouteName = computed(() => {
  let name = currentRoute.value.name;

  if (currentRoute.value.matched[0] != null) {
    name = currentRoute.value.matched[0].name
  }

  return name?.toString()
})


onMounted(() => {
  height.value = `${
      content.value != undefined ? content.value.scrollHeight : 0
  }px`;

  //close it once we have the scroll value
  toggleExpansion();
});

function toggleExpansion() {
  isExpanded.value = !isExpanded.value;
}
</script>

<style lang="scss" scoped>
.expand-leave-active,
.expand-enter-active {
  transition: max-height 0.2s cubic-bezier(0.69, 0.33, 0.16, 0.97);
  overflow: hidden;
}

.expand-enter-to,
.expand-leave-from {
  max-height: v-bind(height);
}

.expand-enter-from,
.expand-leave-to {
  max-height: 0;
}
</style>
