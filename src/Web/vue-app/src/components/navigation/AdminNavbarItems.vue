<template>
  <p class="navbar__section-title">{{ t('routes.admin.name') }}</p>
  <ul class="navbar__nav">
    <li v-for="route in adminNavRoutes" :key="route.name">
      <RouterLink :to="route.path" class="navbar__navlink">
        <component :is="route.meta.navIcon" :size="16" />
        {{ t(`routes.${String(route.name)}.name`) }}
      </RouterLink>
    </li>
    <li v-if="accountRoute">
      <RouterLink :to="accountRoute.path" class="navbar__navlink">
        <component :is="accountRoute.meta.navIcon" :size="16" />
        {{ t('routes.account.name') }}
      </RouterLink>
    </li>
  </ul>
</template>

<script lang="ts" setup>
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useBackupService } from "@/serviceRegistry";

const { t } = useI18n();
const router = useRouter();
const backupService = useBackupService();
const backupAvailable = ref(true);

const adminNavRoutes = computed(() =>
  router.getRoutes().filter(r =>
    r.meta.navIcon &&
    String(r.name ?? '').startsWith('admin.children.') &&
    (r.name !== 'admin.children.backup' || backupAvailable.value)
  )
);

const accountRoute = computed(() =>
  router.getRoutes().find(r => r.name === 'account')
);

onMounted(async () => {
  backupAvailable.value = await backupService.checkStatus();
});
</script>
