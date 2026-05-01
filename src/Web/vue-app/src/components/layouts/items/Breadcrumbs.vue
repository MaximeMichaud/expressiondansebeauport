<template>
  <nav v-if="breadcrumbs.length > 1" class="breadcrumbs" aria-label="Fil d'Ariane">
    <ol class="breadcrumbs__list">
      <li
        v-for="(item, index) in breadcrumbs"
        :key="`${item.label}-${index}`"
        class="breadcrumbs__item">
        <a
          v-if="shouldUseAnchor(item)"
          :href="item.url"
          class="breadcrumbs__link">
          {{ item.label }}
        </a>
        <RouterLink
          v-else-if="shouldUseRouterLink(item)"
          :to="item.url!"
          class="breadcrumbs__link">
          {{ item.label }}
        </RouterLink>
        <span
          v-else
          class="breadcrumbs__label"
          :aria-current="item.isCurrent ? 'page' : undefined">
          {{ item.label }}
        </span>
      </li>
    </ol>
  </nav>
</template>

<script lang="ts" setup>
import {computed} from "vue";
import { useI18n } from "vue-i18n";
import { useRoute } from "vue-router";
import type {BreadcrumbItem} from "@/types/entities";

const {t} = useI18n()
const route = useRoute()

const props = withDefaults(defineProps<{
  items?: BreadcrumbItem[];
  title?: string;
}>(), {
  items: () => []
});

const breadcrumbs = computed<BreadcrumbItem[]>(() => {
  if (props.items.length > 0)
    return props.items;

  const routeBreadcrumbs = route.matched
    .filter(matched => matched.name && matched.path !== route.path)
    .map(matched => ({
      label: t(`routes.${matched.name?.toString()}.name`),
      url: matched.meta.noLinkInBreadcrumbs ? undefined : matched.path,
      isCurrent: false
    }));

  return [
    ...routeBreadcrumbs,
    {
      label: props.title || '',
      isCurrent: true
    }
  ].filter(item => item.label.length > 0);
});

function shouldUseRouterLink(item: BreadcrumbItem): boolean {
  return !item.isCurrent && !!item.url && item.url.startsWith('/');
}

function shouldUseAnchor(item: BreadcrumbItem): boolean {
  return !item.isCurrent && !!item.url && !item.url.startsWith('/');
}
</script>
