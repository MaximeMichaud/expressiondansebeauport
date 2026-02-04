<template>
  <button @click="goBack()" class="back-link-title">
    <IconBack class="icon" />
    <h1>{{ title }}</h1>
  </button>
</template>

<script lang="ts" setup>
import IconBack from "@/assets/icons/icon__back.svg";
import { useRouter } from "vue-router";

// eslint-disable-next-line
defineProps<{
  title: string;
}>();

const router = useRouter();

function goBack() {
  router.back();
}
</script>
