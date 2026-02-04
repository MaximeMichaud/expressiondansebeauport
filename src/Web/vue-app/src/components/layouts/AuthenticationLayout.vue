<template>
  <div class="authentication-page">
    <div class="container container--maxwidth-lg">
      <div class="authentication-page__container">
        <LogoGarneau class="authentication-page__logo icon icon--white" />

        <LangSwitcher />

        <div class="grid__row grid__row--jcc">
          <div class="grid__col grid__col--sm-8 grid__col--md-6">
            <div class="authentication-page__content">
              <Notifications :is-in-page-flow="true" />

              <RouterView v-slot="{Component}">
                <template v-if="Component">
                  <Suspense>
                    <component :is="Component"/>
                    <template #fallback>
                      <Loader/>
                    </template>
                  </Suspense>
                </template>
              </RouterView>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import Loader from "@/components/layouts/items/Loader.vue";
import Notifications from "@/components/layouts/items/Notifications.vue";
import LangSwitcher from "@/components/layouts/items/LangSwitcher.vue";
import LogoGarneau from "@/assets/icons/logo__garneau.svg";
</script>