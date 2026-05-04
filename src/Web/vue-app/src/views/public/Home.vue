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
import heroDesktop from "@/assets/images/hero-desktop.avif"
import heroMobile from "@/assets/images/hero-mobile.avif"
import {useHead} from "@unhead/vue"
import {useI18n} from "vue-i18n"

const {t} = useI18n()

useHead({
  link: [
    {
      rel: "preload",
      as: "image",
      href: heroMobile,
      type: "image/avif",
      media: "(max-width: 767px)",
      fetchpriority: "high"
    },
    {
      rel: "preload",
      as: "image",
      href: heroDesktop,
      type: "image/avif",
      media: "(min-width: 768px)",
      fetchpriority: "high"
    }
  ]
})
</script>
