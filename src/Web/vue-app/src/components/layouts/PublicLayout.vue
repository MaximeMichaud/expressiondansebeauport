<template>
  <div class="public-layout">
    <NewsBanner />
    <PublicNavbar />
    <main class="public-layout__content">
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
import PublicNavbar from "@/components/navigation/PublicNavbar.vue";
import PublicFooter from "@/components/layouts/items/PublicFooter.vue";
import CookieBanner from "@/components/layouts/items/CookieBanner.vue";
import NewsBanner from "@/components/layouts/items/NewsBanner.vue";
</script>
