<template>
  <div v-if="routeKey" class="subnav" :class="{ 'subnav--expand': isExpanded }">
    <button class="subnav__title" @click="toggleExpansion">
      {{ t(`routes.${routeKey}.name`) }}
      <IconChevron class="icon icon--white" :class="{'icon--rotate-180 icon--black' : !isExpanded}"/>
    </button>

    <Transition name="expand">
      <ul
        class="subnav__content"
        ref="content"
        v-show="isExpanded"
        v-if="directChildRoutes != null && directChildRoutes.length > 0"
      >
          <li v-for="child in directChildRoutes" :key="child.name">
            <RouterLink :to="getChildPath(routeKey, child.name?.toString())" class="subnav__navlink">
              {{ t(`routes.${child.name?.toString()}.name`) }}
            </RouterLink>
          </li>
      </ul>
    </Transition>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, computed } from "vue";
import IconChevron from "@/assets/icons/icon__chevron.svg";
import { useI18n } from "vue3-i18n";
import { useRouter } from "vue-router";
import { getChildPath } from "@/router/helpers";

// eslint-disable-next-line
const props = defineProps<{
  routeKey: string
}>();

const { t } = useI18n()
const router = useRouter();

const directChildRoutes = computed(() => {
  const routes = router.getRoutes();

  return routes.filter(r => r.path == t(`routes.${props.routeKey}.path`))[0].children;
})

let isExpanded = ref<boolean>(true);
let content = ref<HTMLElement>();
let height = ref<string>();

onMounted(() => {
  height.value = `${
    content.value != undefined ? content.value.scrollHeight : 0
    }px`;

    //close it once we have the scroll value
    toggleExpansion();
});

function toggleExpansion() {
  isExpanded.value = !isExpanded.value;
}
</script>

<style scoped lang="scss">
.expand-leave-active,
.expand-enter-active {
  transition: max-height 0.2s cubic-bezier(0.69, 0.33, 0.16, 0.97);
  overflow: hidden;
}

.expand-enter-to,
.expand-leave-from {
  max-height: v-bind(height);
}

.expand-enter-from,
.expand-leave-to {
  max-height: 0;
}
</style>
