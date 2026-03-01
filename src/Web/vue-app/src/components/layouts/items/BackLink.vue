<template>
  <router-link v-if="path" :to="path" class="back-link">
    <IconChevronLeft class="icon icon--black" />
  </router-link>
  <button v-else @click="goBack()" class="back-link">
    <IconChevronLeft class="icon icon--black" />
  </button>
</template>

<script lang="ts" setup>
import IconChevronLeft from 'vue-material-design-icons/ChevronLeft.vue';
import { useRouter } from "vue-router";

// eslint-disable-next-line
const props = defineProps<{
  path?: any;
}>();

const router = useRouter();

function goBack() {
  router.back();
}
</script>
