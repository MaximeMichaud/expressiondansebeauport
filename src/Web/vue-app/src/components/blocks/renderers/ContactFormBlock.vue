<template>
  <PublicContactForm
    v-if="resolvedData.enabled !== false"
    :block-id="block.id"
    :page-slug="pageSlug"
    :title="resolvedData.title"
    :intro-text="resolvedData.introText"
    :submit-label="resolvedData.submitLabel"
    :success-message="resolvedData.successMessage"
    :recipient-email="resolvedData.recipientEmail"
    :enabled="resolvedData.enabled"
  />
</template>

<script lang="ts" setup>
import {computed} from "vue"
import {useRoute} from "vue-router"
import PublicContactForm from "@/components/contact/PublicContactForm.vue"
import {normalizeContactFormConfig} from "@/types/entities/pageBlock"
import type {ContactFormBlockData, PageBlock} from "@/types/entities/pageBlock"

const route = useRoute()

const pageSlug = computed(() => typeof route.params.slug === 'string' ? route.params.slug : undefined)
const props = defineProps<{
  block: PageBlock
  data: ContactFormBlockData
}>()

const resolvedData = computed(() => normalizeContactFormConfig(props.data))
</script>
