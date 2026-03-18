<template>
  <div class="p-4">
    <div v-if="loading" class="flex justify-center py-10">
      <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
    </div>
    <div v-else-if="announcements.length === 0" class="flex flex-col items-center justify-center gap-3 py-20 text-gray-400">
      <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M18 8A6 6 0 006 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 01-3.46 0"/></svg>
      <span class="text-sm">Aucune annonce pour le moment.</span>
    </div>
    <div v-else class="divide-y divide-gray-200">
      <div v-for="post in announcements" :key="post.id" class="py-4">
        <div class="flex gap-3">
          <div class="flex-1">
            <h3 class="text-base font-bold leading-tight text-gray-900">{{ post.content.slice(0, 80) }}</h3>
            <p class="mt-1 text-xs text-gray-500">{{ formatDate(post.created) }}</p>
            <div class="mt-2 flex gap-3 text-xs text-gray-400">
              <span>{{ post.likeCount }} j'aime</span>
              <span>{{ post.viewCount }} vues</span>
              <span>{{ post.commentCount }} commentaires</span>
            </div>
          </div>
          <div v-if="post.media && post.media.length" class="flex-shrink-0">
            <img :src="post.media[0].thumbnailUrl || post.media[0].mediaUrl" class="h-[72px] w-[72px] rounded-lg object-cover" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useSocialService } from '@/inversify.config'
import type { Post } from '@/types/entities'

const socialService = useSocialService()
const announcements = ref<Post[]>([])
const loading = ref(true)

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('fr-CA', { year: 'numeric', month: 'short', day: 'numeric' })
}

onMounted(async () => {
  try {
    announcements.value = await socialService.getAnnouncements()
  } catch (e) { /* */ }
  loading.value = false
})
</script>
