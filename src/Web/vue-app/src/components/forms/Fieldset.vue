<template>
  <fieldset class="form__fieldset">
    <legend class="form__legend" v-if="title">
      <h3>
        {{ title }}
      </h3>
    </legend>
    <slot></slot>
  </fieldset>
</template>

<script lang="ts" setup>
// eslint-disable-next-line
defineProps<{
  title?: string
}>();
</script>
