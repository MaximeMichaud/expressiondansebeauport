<template>
  <div class="tooltip"
       :class="{'tooltip--full':fullWith, 'tooltip--error':isError, 'tooltip--warning':isWarning, 'tooltip--success':isSuccess}">
    <IconCloseCircle v-if="isError"/>
    <IconAlert v-else-if="isWarning"/>
    <IconCheckCircle v-else-if="isSuccess"/>
    <IconInformationSlabCircle v-else/>

    <div class="rte rte--text-small">
      <slot></slot>
    </div>
  </div>
</template>

<script lang="ts" setup>
import IconInformationSlabCircle from 'vue-material-design-icons/InformationSlabCircle.vue';
import IconAlert from "vue-material-design-icons/Alert.vue";
import IconCheckCircle from "vue-material-design-icons/CheckCircle.vue";
import IconCloseCircle from "vue-material-design-icons/CloseCircle.vue";

// eslint-disable-next-line
defineProps<{
  isWarning?: boolean;
  isSuccess?: boolean;
  isError?: boolean;
  fullWith?: boolean;
}>();
</script>
