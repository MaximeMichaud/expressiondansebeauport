<template>
  <p class="navbar__section-title">{{ t('routes.admin.name') }}</p>
  <ul class="navbar__nav">
    <li v-for="route in mainRoutes" :key="route.name">
      <RouterLink :to="route.path" class="navbar__navlink">
        <component :is="route.meta.navIcon" :size="16" />
        {{ t(`routes.${String(route.name)}.name`) }}
      </RouterLink>
    </li>

    <li class="navbar__navgroup">
      <button
        class="navbar__navlink navbar__navgroup-toggle"
        :class="{
          'navbar__navgroup-toggle--open': toolsOpen,
          'navbar__navgroup-toggle--active': isToolsActive,
        }"
        @click="toolsOpen = !toolsOpen"
      >
        <Wrench :size="16" />
        {{ t('nav.tools') }}
        <ChevronDown :size="14" class="navbar__navgroup-chevron" />
      </button>
      <ul v-show="toolsOpen" class="navbar__subnav">
        <li v-for="route in toolRoutes" :key="route.name">
          <RouterLink :to="route.path" class="navbar__navlink navbar__navlink--sub">
            <component :is="route.meta.navIcon" :size="14" />
            {{ t(`routes.${String(route.name)}.name`) }}
          </RouterLink>
        </li>
      </ul>
    </li>
  </ul>

  <ul v-if="accountRoute" class="navbar__nav navbar__nav--account">
    <li>
      <RouterLink :to="accountRoute.path" class="navbar__navlink">
        <component :is="accountRoute.meta.navIcon" :size="16" />
        {{ t('routes.account.name') }}
      </RouterLink>
    </li>
  </ul>
</template>

<script lang="ts" setup>
import { computed, onMounted, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { useRoute, useRouter } from "vue-router";
import { Wrench, ChevronDown } from "lucide-vue-next";
import { useBackupService } from "@/serviceRegistry";

const { t } = useI18n();
const router = useRouter();
const route = useRoute();
const backupService = useBackupService();
const backupAvailable = ref(true);
const toolsOpen = ref(false);

const TOOL_NAMES = new Set([
  'admin.children.siteHealth',
  'admin.children.importExport',
  'admin.children.backup',
  'admin.children.errorLogs',
]);

const allAdminRoutes = computed(() =>
  router.getRoutes().filter(r =>
    r.meta.navIcon &&
    String(r.name ?? '').startsWith('admin.children.') &&
    (r.name !== 'admin.children.backup' || backupAvailable.value)
  )
);

const mainRoutes = computed(() =>
  allAdminRoutes.value.filter(r => !TOOL_NAMES.has(String(r.name)))
);

const toolRoutes = computed(() =>
  allAdminRoutes.value.filter(r => TOOL_NAMES.has(String(r.name)))
);

const accountRoute = computed(() =>
  router.getRoutes().find(r => r.name === 'account')
);

const isToolsActive = computed(() =>
  TOOL_NAMES.has(String(route.name))
);

watch(isToolsActive, (active) => {
  if (active) toolsOpen.value = true;
}, { immediate: true });

onMounted(async () => {
  backupAvailable.value = await backupService.checkStatus();
});
</script>
