<template>
  <p class="navbar__section-title">{{ t('routes.admin.name') }}</p>
  <ul class="navbar__nav">
    <li v-for="child in adminChildRoutes" :key="child.name">
      <RouterLink :to="getChildPath('admin', child.name?.toString())" class="navbar__navlink">
        <component :is="iconMap[child.name?.toString() ?? '']" :size="16" />
        {{ t(`routes.${child.name?.toString()}.name`) }}
      </RouterLink>
    </li>
    <li>
      <RouterLink :to="t('routes.account.path')" class="navbar__navlink">
        <UserCircle :size="16" />
        {{ t('routes.account.name') }}
      </RouterLink>
    </li>
  </ul>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { useI18n } from "vue3-i18n";
import { useRouter } from "vue-router";
import { getChildPath } from "@/router/helpers";
import { Images, FileText, LayoutList, Palette, Activity, ArrowLeftRight, UserCircle, Users, UsersRound, CalendarDays } from "lucide-vue-next";

const { t } = useI18n();
const router = useRouter();

const iconMap: Record<string, unknown> = {
  'admin.children.media': Images,
  'admin.children.pages': FileText,
  'admin.children.menus': LayoutList,
  'admin.children.customizer': Palette,
  'admin.children.siteHealth': Activity,
  'admin.children.importExport': ArrowLeftRight,
  'admin.children.groups': Users,
  'admin.children.members': UsersRound,
  'admin.children.sessions': CalendarDays,
};

const adminChildRoutes = computed(() => {
  const routes = router.getRoutes();
  const match = routes.find(r => r.path === t('routes.admin.path'));
  return match?.children ?? [];
});
</script>
