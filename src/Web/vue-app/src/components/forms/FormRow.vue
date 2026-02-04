<template>
  <div :class="display">
    <slot></slot>
  </div>
</template>

<script lang="ts" setup>
import {computed} from "vue";

// eslint-disable-next-line
const props = defineProps<{
  centerFields?: boolean;
  withThreeColumns?: boolean;
  withFourColumns?: boolean;
}>();

const display = computed(() => {
  let classes = "form__row";

  if (props.centerFields) classes += ' align-center'
  if (props.withFourColumns) classes += ' quadruple'
  if (props.withThreeColumns) classes += ' triple'

  return classes
})
</script>
