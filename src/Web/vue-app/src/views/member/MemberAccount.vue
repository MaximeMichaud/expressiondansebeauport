<template>
  <div class="content-grid">
    <div class="content-grid__header">
      <h1>{{ t(`routes.account.name`) }}</h1>
    </div>

    <div class="content-grid content-grid--double">
      <div class="content-grid__header">
        <h2>{{t('pages.account.loginInfos')}}</h2>
      </div>

      <p>TODO: Add change password form</p>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { useI18n } from "vue3-i18n";

const {t} = useI18n();
</script>
