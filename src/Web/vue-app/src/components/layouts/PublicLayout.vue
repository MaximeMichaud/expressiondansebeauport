<template>
  <div class="public-layout">
    <PublicNavbar />
    <main class="public-layout__content">
      <RouterView v-slot="{ Component }">
        <template v-if="Component">
          <Suspense>
            <component :is="Component" />
            <template #fallback>
              <Loader />
            </template>
          </Suspense>
        </template>
      </RouterView>
    </main>
    <PublicFooter />
  </div>
</template>

<script lang="ts" setup>
import PublicNavbar from "@/components/navigation/PublicNavbar.vue";
import PublicFooter from "@/components/layouts/items/PublicFooter.vue";
import Loader from "@/components/layouts/items/Loader.vue";
</script>
