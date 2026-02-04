<template>
  <RouterView v-slot="{Component}">
    <template v-if="Component">
      <Suspense>
        <component :is="Component" />
        <template #fallback>
          <Loader />
        </template>
      </Suspense>
    </template>
  </RouterView>
</template>
<script setup lang="ts">
import Loader from "@/components/layouts/items/Loader.vue";
</script>