<template>
  <div class="flex h-[calc(100vh-60px)] flex-col">
    <!-- Header -->
    <div class="flex items-center justify-between border-b border-gray-200 px-4 py-3">
      <h2 class="text-lg font-bold text-gray-900">Messages</h2>
      <button class="rounded-lg bg-[#be1e2c] px-3 py-1.5 text-xs font-semibold text-white">
        + Nouveau
      </button>
    </div>

    <!-- Conversation list -->
    <div v-if="loading" class="flex flex-1 items-center justify-center">
      <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#be1e2c] border-t-transparent"></div>
    </div>
    <div v-else-if="conversations.length === 0" class="flex flex-1 items-center justify-center text-sm text-gray-500">
      Aucune conversation pour le moment.
    </div>
    <div v-else class="flex-1 divide-y divide-gray-100 overflow-y-auto">
      <router-link
        v-for="conv in conversations"
        :key="conv.id"
        :to="{ name: 'socialConversation', params: { conversationId: conv.id } }"
        :class="[
          'flex items-center gap-3 px-4 py-3 transition hover:bg-gray-50',
          conv.unreadCount > 0 ? 'bg-red-50/50' : ''
        ]"
      >
        <div class="flex h-11 w-11 flex-shrink-0 items-center justify-center rounded-full bg-gray-300 text-sm font-bold text-white">
          {{ getInitials(conv.otherMember.fullName) }}
        </div>
        <div class="min-w-0 flex-1">
          <div class="flex items-center justify-between">
            <span :class="['text-sm', conv.unreadCount > 0 ? 'font-bold text-gray-900' : 'font-medium text-gray-700']">
              {{ conv.otherMember.fullName }}
            </span>
            <span class="text-xs text-gray-400">{{ formatTime(conv.lastMessage?.created) }}</span>
          </div>
          <p :class="['truncate text-xs', conv.unreadCount > 0 ? 'font-semibold text-gray-700' : 'text-gray-500']">
            {{ conv.lastMessage?.content || 'Aucun message' }}
          </p>
        </div>
        <div v-if="conv.unreadCount > 0" class="flex h-5 w-5 flex-shrink-0 items-center justify-center rounded-full bg-[#be1e2c] text-[10px] font-bold text-white">
          {{ conv.unreadCount }}
        </div>
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useSocialService } from '@/inversify.config'
import type { Conversation } from '@/types/entities'

const socialService = useSocialService()
const conversations = ref<Conversation[]>([])
const loading = ref(true)

function getInitials(name: string) {
  return name?.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2) || '?'
}

function formatTime(dateStr?: string) {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  const now = new Date()
  const diffH = Math.floor((now.getTime() - date.getTime()) / 3600000)
  if (diffH < 24) return date.toLocaleTimeString('fr-CA', { hour: '2-digit', minute: '2-digit' })
  if (diffH < 168) return date.toLocaleDateString('fr-CA', { weekday: 'short' })
  return date.toLocaleDateString('fr-CA', { month: 'short', day: 'numeric' })
}

onMounted(async () => {
  try {
    conversations.value = await socialService.getConversations()
  } catch (e) { /* */ }
  loading.value = false
})
</script>
