<template>
  <Transition name="fade">
    <div class="popup popup--logout" v-if="needToLogout">
      <span class="popup__bg"></span>
      <div class="popup__container">
        <div class="popup__header">
          <p class="popup__title h2-like">{{ t('global.logout.timeout') }}</p>
        </div>
        <div class="popup__content">
          <div class="popup__block">
            <ProgressRing
              :label="timeLeftBeforeReturnToHome"
              :progress="percentageLeftBeforeReturnHome"
              :radius="24"
              :stroke="3"
            />
            <div v-html="t('global.logout.redirectMessage')"></div>
          </div>
          <button class="popup__btn btn" type="button" @click="logoutAndRedirectToLogin">{{ t('global.logout.returnHome') }}</button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script lang="ts" setup>
import { useApiStore } from "@/stores/apiStore";
import { computed, watch, ref, onBeforeUnmount } from "vue";
import { useI18n } from "vue3-i18n";
import ProgressRing from "./ProgressRing.vue";

const {t} = useI18n()
const apiStore = useApiStore();
const needToLogout = computed(() => apiStore.needToLogout);

watch(needToLogout, () => {
  interval.value = setInterval(() => {
    timeLeftBeforeReturnToHome.value--;

    if (timeLeftBeforeReturnToHome.value == 0) {
      logoutAndRedirectToLogin();
      clearInterval(interval.value);
    }
  }, 1000);
});

let interval = ref();
const timeLeftBeforeReturnToHome = ref<number>(10);
const percentageLeftBeforeReturnHome = computed(
  () => (timeLeftBeforeReturnToHome.value * 100) / 10
);

function logoutAndRedirectToLogin() {
  const logoutButton = document.getElementById("js-logout-btn") as HTMLElement;

  if (logoutButton)
    logoutButton.click();

  setTimeout(() => window.location.replace("/authentication/login"), 1000);
}

onBeforeUnmount(() => {
  if (interval.value != null) {
    clearInterval(interval.value);
  }
});
</script>

<style scoped lang="scss">
.fade-leave-active,
.fade-enter-active {
  transition: opacity 0.2s cubic-bezier(0.69, 0.33, 0.16, 0.97);
  overflow: hidden;
}

.fade-enter-to,
.fade-leave-from {
  opacity: 1;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
