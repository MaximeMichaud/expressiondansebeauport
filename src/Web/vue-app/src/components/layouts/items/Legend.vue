<template>
  <legend class="form__legend">
      <h3>
          {{ title }}
      </h3>
  </legend>
</template>

<script lang="ts" setup>
 
defineProps<{
    title: string
}>();
</script>
