<template>
  <router-link v-if="path" :to="path" class="text-link">
    {{ text }}
  </router-link>
  <a
    v-else-if="href"
    :href="href"
    target="_blank"
    class="text-link"
  >
    {{ text }}
  </a>
</template>

<script lang="ts" setup>
// eslint-disable-next-line
defineProps<{
  text: string;
  path?: any;
  href?: string;
}>();
</script>
