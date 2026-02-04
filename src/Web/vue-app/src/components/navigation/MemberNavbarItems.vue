<template>
  <ul class="navbar__nav">
    <li>
      <RouterLink :to="t('routes.account.path')" class="navbar__navlink">
        {{ t('routes.account.name') }}
      </RouterLink>
    </li>
    <li>
      <RouterLink :to="t('routes.books.path')" class="navbar__navlink">
        {{ t('routes.books.name') }}
      </RouterLink>
    </li>
  </ul>
</template>

<script lang="ts" setup>
import { useI18n } from "vue3-i18n";

const {t} = useI18n()
</script>
