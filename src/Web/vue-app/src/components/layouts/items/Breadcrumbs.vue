<template>
  <ul class="breadcrumbs">
    <template v-for="(matched, i) in $route.matched" :key="i">
      <li class="breadcrumbs__breadcrumb" v-if="i+1 < $route.matched.length && matched.path != $route.path">
        <router-link v-if="!matched.meta.noLinkInBreadcrumbs" :to="matched.path">
          {{ t(`routes.${matched.name?.toString()}.name`) }}
        </router-link>
        <p v-else>{{ t(`routes.${matched.name?.toString()}.name`) }}</p>
      </li>
    </template>
    <li class="breadcrumbs__active">
      {{ title }}
    </li>
  </ul>
</template>

<script lang="ts" setup>
import { useI18n } from "vue3-i18n";

const {t} = useI18n()

// eslint-disable-next-line
defineProps<{
  title: string;
}>();
</script>
