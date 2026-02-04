<template>
  <div class="form-errors-list">
    <p class="form-errors-list__title" v-if="title">
      <IconWarning class="icon" />
      {{ title }}
    </p>
    <ul class="form-errors-list__list">
      <li class="form-errors-list__item" v-for="error in errorsList" :key="error">{{ error }}</li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import IconWarning from "@/assets/icons/icon__warning.svg"

// eslint-disable-next-line
const props = defineProps<{
  title?: string;
  errorsList: string[];
}>();
// eslint-disable-next-line
</script>
