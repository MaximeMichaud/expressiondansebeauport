<template>
  <section class="home-hero">
    <div class="home-hero__content">
      <p class="home-hero__eyebrow">{{ t('public.home.eyebrow') }}</p>
      <h1 class="home-hero__title">Expression Danse<br>Beauport</h1>
      <p class="home-hero__subtitle">{{ t('public.home.subtitle') }}</p>
      <a
        href="https://www.qidigo.com/u/Expression-danse-de-Beauport/activities/session"
        target="_blank"
        rel="noopener noreferrer"
        class="home-hero__cta">
        {{ t('public.home.cta') }}
      </a>
    </div>
  </section>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
const {t} = useI18n()
</script>
