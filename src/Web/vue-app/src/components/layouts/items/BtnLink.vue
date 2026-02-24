<template>
  <router-link v-if="path" :to="path" class="btn">
    {{ name }}
  </router-link>
  <a
    v-else-if="href"
    :href="href"
    target="_blank"
    class="btn"
  >
    {{ name }}
  </a>
</template>

<script lang="ts" setup>
defineProps<{
  name: string;
  path?: any;
  href?: string;
}>();
</script>
