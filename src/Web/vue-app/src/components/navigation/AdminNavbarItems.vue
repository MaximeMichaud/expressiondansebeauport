<template>
  <ul class="navbar__nav">
    <li>
      <RouterLink :to="t('routes.account.path')" class="navbar__navlink">
        {{ t('routes.account.name') }}
      </RouterLink>
    </li>
    <li>
      <Subnav :routeKey="'admin'"/>
    </li>
  </ul>
</template>

<script lang="ts" setup>
import { useI18n } from "vue3-i18n";
import Subnav from "./Subnav.vue";

const {t} = useI18n()
</script>
