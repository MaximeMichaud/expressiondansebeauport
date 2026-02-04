<template>
  <section class="card" :class="{ 'card--expand': isExpanded }">
    <div v-if="title" class="card__expand-trigger">
      <div class="card__title-container">
        <div class="card__title-subcontainer">
          <h2 class="card__title" >
            {{ title }}
          </h2>
          <slot name="on-title-right"></slot>
        </div>
        <BtnLink v-if="linkText && linkPath" :name="linkText" :path="linkPath" />
        <button v-else-if="downloadLabel" class="btn btn--blue" @click="handleDownload()">
          <IconDownload class="icon icon--black"/>
          {{ downloadLabel }}
        </button>
      </div>

      <button
        type="button"
        class="plus-sign plus-sign--minus"
        :class="{ 'plus-sign--plus': isExpanded }"
        v-if="$windowWidth < 768"
        @click="toggleExpansion"
      ></button>
    </div>

    <Transition name="expand">
      <div
        class="card__content"
        ref="content"
        v-show="isExpanded || $windowWidth >= 768 || title == null"
      >
        <slot></slot>
      </div>
    </Transition>
  </section>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import BtnLink from "@/components/layouts/items/BtnLink.vue";
import IconDownload from "@/assets/icons/icon__download.svg";

// eslint-disable-next-line
defineProps<{
  title?: string;
  linkText?: string;
  linkPath?: string;
  downloadLabel?: string;
}>();

let isExpanded = ref<boolean>(true);
let content = ref<HTMLElement>();
let height = ref<string>();

// eslint-disable-next-line
const emit = defineEmits<{
  (event: "download"): void
}>()

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

function handleDownload() {
  emit("download")
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
