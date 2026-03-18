<template>
  <div class="flex h-[calc(100vh-60px)] flex-col">
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
      />

      <!-- Loading state -->
      <div v-if="loadingMembers" class="flex justify-center py-4">
        <div class="h-5 w-5 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
      </div>

      <!-- Members from groups -->
      <div v-else-if="filteredMembers.length" class="max-h-60 space-y-1 overflow-y-auto">
        <button
          v-for="member in filteredMembers"
          :key="member.memberId"
          @click="startNewConversation(member)"
          :disabled="startingConvo"
          class="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left transition hover:bg-white disabled:opacity-50"
        >
          <div class="flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full bg-gray-200 text-xs font-bold text-gray-600">
            {{ getInitials(member.fullName) }}
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 truncate">{{ member.fullName }}</p>
            <p v-if="member.groupName" class="text-xs text-gray-500 truncate">{{ member.groupName }}</p>
          </div>
        </button>
      </div>
      <p v-else class="py-4 text-center text-xs text-gray-500">
        {{ memberSearch ? 'Aucun membre trouvé.' : 'Chargement des membres de vos groupes...' }}
      </p>
    </div>

    <!-- Conversation list -->
    <div v-if="loading" class="flex flex-1 items-center justify-center">
      <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
    </div>
    <div v-else-if="!showNewConvo && conversations.length === 0" class="flex flex-1 items-center justify-center text-sm text-gray-500">
      Aucune conversation pour le moment.
    </div>
    <div v-else-if="!showNewConvo" class="flex-1 divide-y divide-gray-100 overflow-y-auto">
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
        <div v-if="conv.unreadCount > 0" class="flex h-5 w-5 flex-shrink-0 items-center justify-center rounded-full bg-[#1a1a1a] text-[10px] font-bold text-white">
          {{ conv.unreadCount }}
        </div>
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import type { Conversation, Group, GroupMember } from '@/types/entities'

interface MemberWithGroup extends GroupMember {
  groupName?: string
}

const router = useRouter()
const socialService = useSocialService()
const conversations = ref<Conversation[]>([])
const loading = ref(true)

// New conversation state
const showNewConvo = ref(false)
const memberSearch = ref('')
const allGroupMembers = ref<MemberWithGroup[]>([])
const loadingMembers = ref(false)
const startingConvo = ref(false)

const filteredMembers = computed(() => {
  const search = memberSearch.value.toLowerCase().trim()
  if (!search) return allGroupMembers.value
  return allGroupMembers.value.filter(m =>
    m.fullName.toLowerCase().includes(search)
  )
})

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

async function loadGroupMembers() {
  loadingMembers.value = true
  try {
    const groups: Group[] = await socialService.getMyGroups()
    const seen = new Set<string>()
    const members: MemberWithGroup[] = []

    for (const group of groups) {
      try {
        const groupMembers = await socialService.getGroupMembers(group.id)
        for (const gm of groupMembers) {
          if (!seen.has(gm.memberId)) {
            seen.add(gm.memberId)
            members.push({ ...gm, groupName: group.name })
          }
        }
      } catch { /* skip group on error */ }
    }

    allGroupMembers.value = members
  } catch { /* */ }
  loadingMembers.value = false
}

async function startNewConversation(member: MemberWithGroup) {
  startingConvo.value = true
  try {
    const conversation = await socialService.startConversation(member.memberId)
    showNewConvo.value = false
    if (conversation?.id) {
      router.push({ name: 'socialConversation', params: { conversationId: conversation.id } })
    }
  } catch { /* */ }
  startingConvo.value = false
}

// Load group members when the new conversation panel opens
watch(showNewConvo, (val) => {
  if (val && allGroupMembers.value.length === 0) {
    loadGroupMembers()
  }
})

onMounted(async () => {
  try {
    conversations.value = await socialService.getConversations()
  } catch (e) { /* */ }
  loading.value = false
})
</script>
