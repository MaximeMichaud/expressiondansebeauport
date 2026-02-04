<template>
  <div class="tooltip tooltip--form">
    <div class="tooltip__cta">
      <IconHelp class="icon icon--blue" />
      <div class="rte rte--small">
        <slot></slot>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import IconHelp from "@/assets/icons/icon__help.svg";
</script>
