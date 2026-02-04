<template>
  <div :class="`notifications ${isInPageFlow ? 'notifications--in-page-flow' : ''}`">
    <notifications position="top right">
      <template #body="props">
        <div :class="`tooltip tooltip--${props.item.type} tooltip--notification`">
          <IconCloseCircle v-if="props.item.type === 'error'"/>
          <IconCheckCircle v-else-if="props.item.type === 'success'"/>
          <IconInformationSlabCircle v-else/>

          <div v-html="props.item.text"></div>
        </div>
      </template>
    </notifications>
  </div>
</template>

<script lang="ts" setup>
import IconInformationSlabCircle from "vue-material-design-icons/InformationSlabCircle.vue";
import IconCheckCircle from "vue-material-design-icons/CheckCircle.vue";
import IconCloseCircle from "vue-material-design-icons/CloseCircle.vue";

// eslint-disable-next-line
const props = defineProps<{
  isInPageFlow?:boolean;
}>();
</script>
