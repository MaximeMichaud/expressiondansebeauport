<template>
  <div class="public-layout">
    <a class="skip-link" href="#main-content" @click="focusMainContent">
      Passer au contenu principal
    </a>
    <NewsBanner />
    <PublicNavbar />
    <main
      id="main-content"
      ref="mainContent"
      class="public-layout__content"
      tabindex="-1"
    >
      <RouterView v-slot="{ Component }">
        <template v-if="Component">
          <Suspense>
            <component :is="Component" />
            <template #fallback />
          </Suspense>
        </template>
      </RouterView>
    </main>
    <PublicFooter />
    <CookieBanner />
  </div>
</template>

<script lang="ts" setup>
import {nextTick, ref, watch} from "vue";
import {useRoute} from "vue-router";
import PublicNavbar from "@/components/navigation/PublicNavbar.vue";
import PublicFooter from "@/components/layouts/items/PublicFooter.vue";
import CookieBanner from "@/components/layouts/items/CookieBanner.vue";
import NewsBanner from "@/components/layouts/items/NewsBanner.vue";

const route = useRoute();
const mainContent = ref<HTMLElement | null>(null);

async function focusMainContent() {
  await nextTick();
  mainContent.value?.focus({preventScroll: true});
}

watch(() => route.fullPath, () => {
  focusMainContent();
});
</script>
