<template>
  <legend class="form__legend">
      <h3>
          {{ title }}
      </h3>
  </legend>
</template>

<script lang="ts" setup>
// eslint-disable-next-line
defineProps<{
    title: string
}>();
</script>
