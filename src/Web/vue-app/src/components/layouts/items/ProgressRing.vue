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

 
const props = defineProps<{
  label: number
  radius: number
  progress: number
  stroke: number
}>();

const normalizedRadius = computed(() => props.radius - props.stroke * 2);
const circumference = computed(() => normalizedRadius.value * 2 * Math.PI);
const strokeDashoffset = computed(() => circumference.value - props.progress / 100 * circumference.value);
const dimension = computed(() => props.radius * 2);
const styles = computed(() => {
  return {
    height: dimension.value - 8 + 'px',
    width: dimension.value - 8 + 'px',
  }
})
</script>
