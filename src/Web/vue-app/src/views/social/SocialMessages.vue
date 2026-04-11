<template>
  <div class="flex min-h-[400px] flex-col">
    <!-- Header -->
    <div class="flex items-center justify-between border-b border-gray-200 px-4 py-3">
      <h2 class="text-lg font-bold text-gray-900">Messages</h2>
      <button
        @click="showNewConvo = !showNewConvo"
        class="rounded-lg bg-[#1a1a1a] px-3 py-1.5 text-xs font-semibold text-white"
      >
        {{ showNewConvo ? 'Fermer' : '+ Nouveau' }}
      </button>
    </div>

    <!-- New conversation panel -->
    <div v-if="showNewConvo" class="border-b border-gray-200 bg-gray-50 p-4">
      <h3 class="mb-3 text-sm font-semibold text-gray-700">Choisir un membre</h3>

      <!-- Member search -->
      <input
        v-model="memberSearch"
        type="text"
        class="mb-3 w-full rounded-lg border border-gray-300 px-3 py-2 text-sm placeholder-gray-400 focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
        placeholder="Rechercher un membre..."
        @input="onMemberSearch"
      />

      <!-- Loading state -->
      <div v-if="loadingMembers" class="flex justify-center py-4">
        <div class="h-5 w-5 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
      </div>

      <!-- Search results -->
      <div v-else-if="searchResults.length" class="max-h-60 space-y-1 overflow-y-auto">
        <button
          v-for="member in searchResults"
          :key="member.id"
          @click="startNewConversation(member)"
          :disabled="startingConvo"
          class="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left transition hover:bg-white disabled:opacity-50"
        >
          <div class="flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full text-xs font-bold text-white" :style="{ background: member.avatarColor || getAvatarColor(member.fullName) }">
            {{ getInitials(member.fullName) }}
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 truncate">{{ member.fullName }}</p>
          </div>
        </button>
      </div>
      <p v-else class="py-4 text-center text-xs text-gray-500">
        {{ memberSearch ? 'Aucun membre trouvé.' : 'Aucun membre disponible.' }}
      </p>
    </div>

    <!-- Conversation list -->
    <div v-if="loading" class="flex flex-1 items-center justify-center">
      <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
    </div>
    <div v-else-if="!showNewConvo && conversations.length === 0" class="flex flex-col items-center justify-center gap-3 py-20 text-gray-400">
      <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/></svg>
      <span class="text-sm">Aucune conversation pour le moment.</span>
    </div>
    <div v-else-if="!showNewConvo" class="flex-1 overflow-y-auto">
      <router-link
        v-for="conv in conversations"
        :key="conv.id"
        :to="{ name: 'socialConversation', params: { conversationId: conv.id } }"
        class="flex items-center gap-3 border-b px-4 py-3 transition hover:bg-gray-50"
        style="border-color: var(--soc-divider, #f0f0f0);"
      >
        <div class="flex h-11 w-11 flex-shrink-0 items-center justify-center rounded-full text-sm font-bold text-white" :style="{ background: conv.otherMember.avatarColor || getAvatarColor(conv.otherMember.fullName) }">
          {{ getInitials(conv.otherMember.fullName) }}
        </div>
        <div class="min-w-0 flex-1">
          <span :class="['text-sm', conv.unreadCount > 0 ? 'font-bold text-gray-900' : 'font-medium text-gray-700']">
            {{ conv.otherMember.fullName }}
          </span>
          <p :class="['truncate text-xs', conv.unreadCount > 0 ? 'font-semibold text-gray-700' : 'text-gray-500']">
            {{ lastMessagePreview(conv.lastMessage) }}
          </p>
        </div>
        <div class="flex flex-col items-end gap-1 flex-shrink-0">
          <span class="text-[10px] text-gray-400">{{ formatTime(conv.lastMessage?.created) }}</span>
          <div v-if="conv.unreadCount > 0" class="flex h-5 min-w-5 items-center justify-center rounded-full bg-[#dc2626] px-1.5 text-[10px] font-bold text-white">
            {{ conv.unreadCount }}
          </div>
        </div>
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, onActivated } from 'vue'
import { useRouter } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useSignalR } from '@/composables/useSignalR'
import { useMemberStore } from '@/stores/memberStore'
import type { Conversation } from '@/types/entities'

const router = useRouter()
const socialService = useSocialService()
const { onMessage, offMessage } = useSignalR()
const memberStore = useMemberStore()
const conversations = ref<Conversation[]>([])
const loading = ref(true)

// New conversation state
const showNewConvo = ref(false)
const memberSearch = ref('')
const searchResults = ref<any[]>([])
const loadingMembers = ref(false)
const startingConvo = ref(false)
let searchTimeout: ReturnType<typeof setTimeout> | null = null

function getInitials(name: string) {
  if (!name || !name.trim()) return '??'
  return name.split(' ').filter(n => n.length > 0).map(n => n[0]).join('').toUpperCase().slice(0, 2)
}

function lastMessagePreview(lastMessage: any): string {
  if (!lastMessage) return 'Aucun message'
  const isMine = lastMessage.isMine === true

  if (lastMessage.content && lastMessage.content.trim()) {
    return isMine ? `Vous: ${lastMessage.content}` : lastMessage.content
  }

  const mediaCount = lastMessage.mediaCount ?? 0
  const hasLegacy = lastMessage.hasLegacyMedia === true
  if (mediaCount === 0 && !hasLegacy) return 'Aucun message'

  const hasVideo = lastMessage.hasVideo === true
  const hasImage = lastMessage.hasImage === true || hasLegacy
  const count = Math.max(mediaCount, hasLegacy ? 1 : 0)

  let label: string
  if (hasVideo && hasImage) label = count > 1 ? `${count} fichiers` : 'un fichier'
  else if (hasVideo) label = count > 1 ? `${count} vidéos` : 'une vidéo'
  else if (hasImage) label = count > 1 ? `${count} photos` : 'une photo'
  else return 'Aucun message'

  return isMine ? `Vous avez envoyé ${label}` : `Vous a envoyé ${label}`
}

const avatarColors = ['#e53e3e', '#dd6b20', '#d69e2e', '#38a169', '#319795', '#3182ce', '#5a67d8', '#805ad5', '#d53f8c', '#e53e3e']
function getAvatarColor(name: string) {
  let hash = 0
  for (let i = 0; i < (name?.length || 0); i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return avatarColors[Math.abs(hash) % avatarColors.length]
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

async function searchMembers(query: string) {
  loadingMembers.value = true
  try {
    searchResults.value = await socialService.searchMembers(query)
  } catch { searchResults.value = [] }
  loadingMembers.value = false
}

function onMemberSearch() {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => searchMembers(memberSearch.value), 300)
}

async function startNewConversation(member: any) {
  startingConvo.value = true
  try {
    const memberId = member.memberId || member.id
    const conversation = await socialService.startConversation(memberId)
    showNewConvo.value = false
    if (conversation?.id) {
      router.push({ name: 'socialConversation', params: { conversationId: conversation.id } })
    }
  } catch { /* */ }
  startingConvo.value = false
}

// Load all members when the new conversation panel opens
watch(showNewConvo, (val) => {
  if (val && searchResults.value.length === 0) {
    searchMembers('')
  }
})

async function loadConversations() {
  try {
    const all = await socialService.getConversations()
    conversations.value = all.filter((c: any) => c.lastMessage)
  } catch { /* */ }
  loading.value = false
}

let pollInterval: ReturnType<typeof setInterval> | null = null

// Refresh list instantly when a new DM arrives via SignalR
const onNewMessage = () => loadConversations()

// Refresh list when unread count changes (syncs with nav badge)
watch(() => memberStore.unreadMessageCount, () => loadConversations())

onMounted(() => {
  loadConversations()
  onMessage(onNewMessage)
  pollInterval = setInterval(async () => {
    try {
      const all = await socialService.getConversations()
      conversations.value = all.filter((c: any) => c.lastMessage)
    } catch { /* */ }
  }, 3000)
})

onActivated(loadConversations)

onUnmounted(() => {
  offMessage(onNewMessage)
  if (pollInterval) clearInterval(pollInterval)
})
</script>
