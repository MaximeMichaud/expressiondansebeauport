<template>
  <router-link v-if="path" :to="path" class="btn btn--icon-translate-hover">
    {{ name }}
    <IconArrow class="icon" />
  </router-link>
  <a
    v-else-if="href"
    :href="href"
    target="_blank"
    class="btn btn--icon-translate-hover"
  >
    {{ name }}
    <IconArrow class="icon" />
  </a>
</template>

<script lang="ts" setup>
import IconArrow from "@/assets/icons/icon__arrow.svg";

// eslint-disable-next-line
defineProps<{
  name: string;
  path?: any;
  href?: string;
}>();
</script>
