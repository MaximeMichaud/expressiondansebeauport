<template>
  <div class="progress-ring" :style="styles">
    <span class="progress-ring__label">{{label}}</span>
    <svg class="progress-ring__line" :height="dimension" :width="dimension">
      <circle
        :stroke-dasharray="circumference + ' ' + circumference"
        :style="{ strokeDashoffset }"
        :stroke-width="stroke"
        :r="normalizedRadius"
        :cx="radius"
        :cy="radius"
      />
    </svg>
  </div>
</template>

<script lang="ts" setup>
import { computed } from "vue";

// eslint-disable-next-line
const props = defineProps<{
  label: number
  radius: number
  progress: number
  stroke: number
}>();

let normalizedRadius = computed(() => props.radius - props.stroke * 2);
let circumference = computed(() => normalizedRadius.value * 2 * Math.PI);
let strokeDashoffset = computed(() => circumference.value - props.progress / 100 * circumference.value);
let dimension = computed(() => props.radius * 2);
let styles = computed(() => {
  return {
    height: dimension.value - 8 + 'px',
    width: dimension.value - 8 + 'px',
  }
})
</script>
