<template>
  <router-link v-if="path" :to="path" class="back-link">
    <IconChevronLeft class="icon icon--black" />
    <span class="btn btn--link">{{ t("global.back")}}</span>
  </router-link>
  <button v-else @click="goBack()" class="back-link">
    <IconChevronLeft class="icon icon--black" />
    <span class="btn btn--link">{{ t("global.back")}}</span>
  </button>
</template>

<script lang="ts" setup>
import IconChevronLeft from 'vue-material-design-icons/ChevronLeft.vue';
import { useRouter } from "vue-router";
import { useI18n } from "vue3-i18n";

// eslint-disable-next-line
const props = defineProps<{
  path?: any;
}>();

const router = useRouter();

const { t } = useI18n()

function goBack() {
  router.back();
}
</script>
