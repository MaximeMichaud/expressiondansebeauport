<template>
  <div class="public-page" v-if="page">
    <section class="public-page__hero">
      <h1 class="public-page__title">{{ page.title }}</h1>
    </section>
    <div class="public-page__content">
      <div v-for="section in page.sections" :key="section.id" class="public-page__section">
        <h2 v-if="section.title" class="public-page__section-title">{{ section.title }}</h2>
        <img v-if="section.imageUrl" :src="section.imageUrl" :alt="section.title" class="public-page__section-image" />
        <div v-if="section.htmlContent" class="public-page__section-content" v-html="section.htmlContent"></div>
      </div>
    </div>
  </div>
  <Loader v-else />
</template>

<script lang="ts" setup>
import {ref, watch} from "vue";
import {useRoute} from "vue-router";
import {usePageService} from "@/inversify.config";
import {Page} from "@/types/entities";
import Loader from "@/components/layouts/items/Loader.vue";

const route = useRoute();
const pageService = usePageService();

const page = ref<Page | null>(null);

async function loadPage(slug: string) {
  page.value = null;
  page.value = await pageService.getPageBySlug(slug);
}

await loadPage(route.params.slug as string);

watch(() => route.params.slug, async (newSlug) => {
  if (newSlug) {
    await loadPage(newSlug as string);
  }
});
</script>
