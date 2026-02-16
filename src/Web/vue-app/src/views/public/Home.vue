<template>
  <section class="home-hero">
    <div class="home-hero__content">
      <h1 class="home-hero__title">Expression Danse de Beauport</h1>
      <p class="home-hero__subtitle">La danse pour tous, à tout âge</p>
    </div>
  </section>
  <div class="public-page" v-if="page && page.sections?.length">
    <div class="public-page__content">
      <div v-for="section in page.sections" :key="section.id" class="public-page__section">
        <h2 v-if="section.title && section.title !== 'Bienvenue'" class="public-page__section-title">{{ section.title }}</h2>
        <img v-if="section.imageUrl" :src="section.imageUrl" :alt="section.title" class="public-page__section-image" />
        <div v-if="section.htmlContent" class="public-page__section-content" v-html="section.htmlContent"></div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import {ref} from "vue";
import {usePageService} from "@/inversify.config";
import {Page} from "@/types/entities";

const pageService = usePageService();
const page = ref<Page | null>(null);

try {
  page.value = await pageService.getPageBySlug('accueil');
} catch {
  // Page not seeded yet, show static content only
}
</script>
