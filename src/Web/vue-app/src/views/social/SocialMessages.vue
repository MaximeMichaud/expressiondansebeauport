<template>
  <div class="flex min-h-[400px] flex-col">
    <!-- Tab bar (admin only) -->
    <div v-if="isAdmin" class="flex border-b border-gray-200">
      <button
        @click="activeTab = 'mine'"
        :class="['flex-1 py-3 text-center text-sm font-semibold transition', activeTab === 'mine' ? 'border-b-2 border-[#1a1a1a] text-gray-900' : 'text-gray-400 hover:text-gray-600']"
      >
        Mes messages
      </button>
      <button
        @click="activeTab = 'admin'"
        :class="['flex-1 py-3 text-center text-sm font-semibold transition', activeTab === 'admin' ? 'border-b-2 border-[#1a1a1a] text-gray-900' : 'text-gray-400 hover:text-gray-600']"
      >
        Admin
      </button>
    </div>

    <!-- Header (non-admin sees this, admin sees it only on "mine" tab) -->
    <div v-if="!isAdmin || activeTab === 'mine'" class="flex items-center justify-between border-b border-gray-200 px-4 py-3">
      <h2 class="text-lg font-bold text-gray-900">Messages</h2>
      <button
        @click="showNewConvo = !showNewConvo"
        class="rounded-lg border border-[rgba(21,128,61,0.15)] bg-[rgba(21,128,61,0.06)] px-3 py-1.5 text-xs font-semibold text-[#15803d] transition hover:bg-[rgba(21,128,61,0.12)] cursor-pointer"
      >
        {{ showNewConvo ? 'Fermer' : '+ Nouvelle conversation' }}
      </button>
    </div>

    <!-- New conversation panel -->
    <div v-if="showNewConvo && (!isAdmin || activeTab === 'mine')" class="border-b border-gray-200 bg-gray-50 p-4">
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
          class="soc-admin-member-link soc-admin-member-link--full disabled:opacity-50"
        >
          <div class="flex h-8 w-8 flex-shrink-0 items-center justify-center overflow-hidden rounded-full text-xs font-bold text-white" :style="{ background: member.avatarColor || getAvatarColor(member.fullName) }">
            <img v-if="member.profileImageUrl" :src="member.profileImageUrl" :alt="member.fullName" class="h-full w-full object-cover" />
            <span v-else>{{ getInitials(member.fullName) }}</span>
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
    <div v-if="loading && (!isAdmin || activeTab === 'mine')" class="flex flex-1 items-center justify-center">
      <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
    </div>
    <div v-else-if="!showNewConvo && conversations.length === 0 && (!isAdmin || activeTab === 'mine')" class="flex flex-col items-center justify-center gap-3 py-20 text-gray-400">
      <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/></svg>
      <span class="text-sm">Aucune conversation pour le moment.</span>
    </div>
    <div v-else-if="!showNewConvo && (!isAdmin || activeTab === 'mine')" ref="convoListContainer" class="flex-1 overflow-y-auto">
      <router-link
        v-for="conv in conversations"
        :key="conv.id"
        :to="{ name: 'socialConversation', params: { conversationId: conv.id } }"
        class="flex items-center gap-3 border-b px-4 py-3 transition hover:bg-gray-50"
        style="border-color: var(--soc-divider, #f0f0f0);"
      >
        <div class="flex h-11 w-11 flex-shrink-0 items-center justify-center overflow-hidden rounded-full text-sm font-bold text-white" :style="{ background: conv.otherMember.avatarColor || getAvatarColor(conv.otherMember.fullName) }">
          <img
            v-if="avatarRegistry.getAvatar(conv.otherMember.id, conv.otherMember.profileImageUrl)"
            :src="avatarRegistry.getAvatar(conv.otherMember.id, conv.otherMember.profileImageUrl)!"
            :alt="conv.otherMember.fullName"
            class="h-full w-full object-cover"
          />
          <span v-else>{{ getInitials(conv.otherMember.fullName) }}</span>
        </div>
        <div class="min-w-0 flex-1">
          <span :class="['text-sm', conv.unreadCount > 0 ? 'font-bold text-gray-900' : 'font-medium text-gray-700']">
            {{ conv.otherMember.fullName }}
          </span>
          <p :class="['truncate text-xs', conv.unreadCount > 0 ? 'font-semibold text-gray-700' : 'text-gray-500']">
            <span>{{ lastMessagePrefix(conv.lastMessage) }}</span><span :class="isMediaOnlyPreview(conv.lastMessage) && 'italic'">{{ lastMessageBody(conv.lastMessage) }}</span>
          </p>
        </div>
        <div class="flex flex-col items-end gap-1 flex-shrink-0">
          <span class="text-[10px] text-gray-400">{{ formatTime(conv.lastMessage?.created) }}</span>
          <div v-if="conv.unreadCount > 0" class="flex h-5 min-w-5 items-center justify-center rounded-full bg-[#dc2626] px-1.5 text-[10px] font-bold text-white">
            {{ conv.unreadCount }}
          </div>
        </div>
      </router-link>
      <div v-if="loadingMoreConvos" class="flex justify-center py-3">
        <div class="h-4 w-4 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
      </div>
    </div>

    <!-- Admin tab content -->
    <template v-if="isAdmin && activeTab === 'admin'">
      <!-- Member search -->
      <div v-if="!adminSelectedMember" class="border-b p-4" style="border-color: var(--soc-divider, #f0f0f0); background: var(--soc-bar-hover, #f5f3f0);">
        <h3 class="mb-3 text-sm font-semibold" style="color: var(--soc-text, #292524);">Choisir un membre</h3>
        <input
          v-model="adminMemberSearch"
          type="text"
          class="mb-3 w-full rounded-lg border px-3 py-2 text-sm placeholder-gray-400 focus:outline-none focus:ring-1"
          style="border-color: var(--soc-input-border, #e7e0da); background: var(--soc-input-bg, #faf9f7); color: var(--soc-text, #292524);"
          placeholder="Rechercher un membre..."
          @input="onAdminMemberSearch"
        />
        <div v-if="adminLoadingMembers" class="flex justify-center py-4">
          <div class="h-5 w-5 animate-spin rounded-full border-2 border-t-transparent" style="border-color: var(--soc-bar-text-strong, #1a1a1a); border-top-color: transparent;"></div>
        </div>
        <div v-else-if="adminSearchResults.length" class="max-h-60 space-y-1 overflow-y-auto">
          <button
            v-for="member in adminSearchResults"
            :key="member.id"
            @click="selectAdminMember(member)"
            class="soc-admin-member-link soc-admin-member-link--full"
          >
            <div class="flex h-8 w-8 flex-shrink-0 items-center justify-center overflow-hidden rounded-full text-xs font-bold text-white" :style="{ background: member.avatarColor || getAvatarColor(member.fullName) }">
              <img v-if="member.profileImageUrl" :src="member.profileImageUrl" :alt="member.fullName" class="h-full w-full object-cover" />
              <span v-else>{{ getInitials(member.fullName) }}</span>
            </div>
            <p class="text-sm font-medium truncate" style="color: var(--soc-text, #292524);">{{ member.fullName }}</p>
          </button>
        </div>
        <p v-else class="py-4 text-center text-xs" style="color: var(--soc-text-muted, #78716c);">
          {{ adminMemberSearch ? 'Aucun membre trouvé.' : 'Aucun membre disponible.' }}
        </p>
      </div>

      <!-- Admin conversation list -->
      <div v-if="adminSelectedMember && adminLoading" class="flex flex-1 items-center justify-center">
        <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
      </div>
      <div v-else-if="adminSelectedMember && adminConversations.length === 0" class="flex flex-col items-center justify-center gap-3 py-20 text-gray-400">
        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/></svg>
        <span class="text-sm">Aucune conversation pour ce membre.</span>
      </div>
      <div v-else-if="adminSelectedMember" ref="adminConvoListContainer" class="flex-1 overflow-y-auto">
        <router-link
          v-for="conv in adminConversations"
          :key="conv.id"
          :to="{ name: 'socialAdminConversation', params: { conversationId: conv.id }, query: { viewingId: adminSelectedMember.id, viewingName: adminSelectedMember.fullName, otherId: adminOtherMember(conv).id, otherName: adminOtherMember(conv).fullName, otherColor: adminOtherMember(conv).avatarColor } }"
          class="flex items-center gap-3 border-b px-4 py-3 transition hover:bg-gray-50"
          style="border-color: var(--soc-divider, #f0f0f0);"
        >
          <div class="flex h-11 w-11 flex-shrink-0 items-center justify-center overflow-hidden rounded-full text-sm font-bold text-white" :style="{ background: adminOtherMember(conv).avatarColor || getAvatarColor(adminOtherMember(conv).fullName) }">
            <img
              v-if="adminOtherMember(conv).profileImageUrl"
              :src="adminOtherMember(conv).profileImageUrl"
              :alt="adminOtherMember(conv).fullName"
              class="h-full w-full object-cover"
            />
            <span v-else>{{ getInitials(adminOtherMember(conv).fullName) }}</span>
          </div>
          <div class="min-w-0 flex-1">
            <span :class="['text-sm', conv.unreadCount > 0 ? 'font-bold text-gray-900' : 'font-medium text-gray-700']">
              {{ adminOtherMember(conv).fullName }}
            </span>
            <p :class="['truncate text-xs', conv.unreadCount > 0 ? 'font-semibold text-gray-700' : 'text-gray-500']">
              <span>{{ lastMessagePrefix(conv.lastMessage) }}</span><span :class="isMediaOnlyPreview(conv.lastMessage) && 'italic'">{{ lastMessageBody(conv.lastMessage) }}</span>
            </p>
          </div>
          <div class="flex flex-col items-end gap-1 flex-shrink-0">
            <span class="text-[10px] text-gray-400">{{ formatTime(conv.lastMessage?.created) }}</span>
            <div v-if="conv.unreadCount > 0" class="flex h-5 min-w-5 items-center justify-center rounded-full bg-[#dc2626] px-1.5 text-[10px] font-bold text-white">
              {{ conv.unreadCount }}
            </div>
          </div>
        </router-link>
        <div v-if="adminLoadingMore" class="flex justify-center py-3">
          <div class="h-4 w-4 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
        </div>
      </div>

      <!-- Admin viewing banner -->
      <div v-if="adminSelectedMember" class="soc-admin-viewing-banner" style="margin-top: auto;">
        <button @click="clearAdminMember" class="soc-admin-viewing-banner__back">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 19l-7-7 7-7"/></svg>
        </button>
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
        <span>Vu par <router-link :to="{ name: 'socialMemberProfile', params: { id: adminSelectedMember.id } }" class="soc-admin-viewing-banner__link">{{ adminSelectedMember.fullName }}</router-link></span>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch, onActivated, nextTick } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useSignalR } from '@/composables/useSignalR'
import { useMemberStore } from '@/stores/memberStore'
import { useAvatarRegistryStore } from '@/stores/avatarRegistryStore'
import { useInfiniteScroll } from '@/composables/useInfiniteScroll'
import { useUserStore } from '@/stores/userStore'
import { Role } from '@/types/enums'
import type { Conversation } from '@/types/entities'

const router = useRouter()
const route = useRoute()
const socialService = useSocialService()
const { onMessage, offMessage } = useSignalR()
const memberStore = useMemberStore()
const avatarRegistry = useAvatarRegistryStore()
const userStore = useUserStore()
const isAdmin = computed(() => userStore.hasRole(Role.Admin))
const activeTab = ref<'mine' | 'admin'>('mine')
const convoListContainer = ref<HTMLElement | null>(null)

const {
  items: rawConversations,
  loading,
  loadingMore: loadingMoreConvos,
  load: loadRawConversations,
  refreshFirst: refreshConvosFirst,
  attachScroll: attachConvosScroll,
} = useInfiniteScroll<Conversation>({
  fetchFn: (page) => socialService.getConversations(page),
  scrollContainer: convoListContainer,
  direction: 'down',
  threshold: 300,
})

const conversations = computed(() => rawConversations.value.filter((c: any) => c.lastMessage))

// New conversation state
const showNewConvo = ref(false)
const memberSearch = ref('')
const searchResults = ref<any[]>([])
const loadingMembers = ref(false)
const startingConvo = ref(false)
let searchTimeout: ReturnType<typeof setTimeout> | null = null

// Admin tab state
const adminSelectedMember = ref<any>(null)
const adminMemberSearch = ref('')
const adminSearchResults = ref<any[]>([])
const adminLoadingMembers = ref(false)
let adminSearchTimeout: ReturnType<typeof setTimeout> | null = null

const adminConvoListContainer = ref<HTMLElement | null>(null)

const {
  items: adminRawConversations,
  loading: adminLoading,
  loadingMore: adminLoadingMore,
  load: loadAdminConversations,
  attachScroll: attachAdminScroll,
} = useInfiniteScroll<any>({
  fetchFn: (page) => socialService.getAdminConversations(adminSelectedMember.value.id, page),
  scrollContainer: adminConvoListContainer,
  direction: 'down',
  threshold: 300,
})

const adminConversations = computed(() => adminRawConversations.value.filter((c: any) => c.lastMessage))

function adminOtherMember(conv: any) {
  if (!adminSelectedMember.value) return conv.participantA
  return conv.participantA.id === adminSelectedMember.value.id ? conv.participantB : conv.participantA
}

function onAdminMemberSearch() {
  if (adminSearchTimeout) clearTimeout(adminSearchTimeout)
  adminSearchTimeout = setTimeout(() => searchAdminMembers(adminMemberSearch.value), 300)
}

async function searchAdminMembers(query: string) {
  const showSpinner = adminSearchResults.value.length === 0
  if (showSpinner) adminLoadingMembers.value = true
  try {
    adminSearchResults.value = await socialService.searchMembers(query)
  } catch { adminSearchResults.value = [] }
  adminLoadingMembers.value = false
}

async function selectAdminMember(member: any) {
  adminSelectedMember.value = member
  await loadAdminConversations()
  nextTick(() => attachAdminScroll())
}

function clearAdminMember() {
  adminSelectedMember.value = null
  adminRawConversations.value = []
}

watch(activeTab, (val) => {
  if (val === 'admin' && adminSearchResults.value.length === 0) {
    searchAdminMembers('')
  }
})

function getInitials(name: string) {
  if (!name || !name.trim()) return '??'
  return name.split(' ').filter(n => n.length > 0).map(n => n[0]).join('').toUpperCase().slice(0, 2)
}

function isMediaOnlyPreview(lastMessage: any): boolean {
  if (!lastMessage) return false
  if (lastMessage.content && lastMessage.content.trim()) return false
  return (lastMessage.mediaCount ?? 0) > 0 || lastMessage.hasLegacyMedia === true
}

function lastMessagePrefix(lastMessage: any): string {
  if (!lastMessage) return ''
  return lastMessage.isMine === true ? 'Vous: ' : ''
}

function lastMessageBody(lastMessage: any): string {
  if (!lastMessage) return 'Aucun message'
  const isMine = lastMessage.isMine === true

  if (lastMessage.content && lastMessage.content.trim()) {
    return lastMessage.content
  }

  const mediaCount = lastMessage.mediaCount ?? 0
  const hasLegacy = lastMessage.hasLegacyMedia === true
  if (mediaCount === 0 && !hasLegacy) return 'Aucun message'

  const hasVideo = lastMessage.hasVideo === true
  const hasImage = lastMessage.hasImage === true || hasLegacy
  const count = Math.max(mediaCount, hasLegacy ? 1 : 0)

  if (isMine) {
    if (hasVideo && hasImage) return count > 1 ? `${count} fichiers` : 'Fichier'
    if (hasVideo) return count > 1 ? `${count} vidéos` : 'Vidéo'
    if (hasImage) return count > 1 ? `${count} photos` : 'Photo'
    return 'Aucun message'
  }

  let label: string
  if (hasVideo && hasImage) label = count > 1 ? `${count} fichiers` : 'un fichier'
  else if (hasVideo) label = count > 1 ? `${count} vidéos` : 'une vidéo'
  else if (hasImage) label = count > 1 ? `${count} photos` : 'une photo'
  else return 'Aucun message'

  return `Vous a envoyé ${label}`
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

function populateRegistryFromConvos(list: Conversation[]) {
  for (const c of list) {
    if (c.otherMember?.id) {
      avatarRegistry.setAvatar(c.otherMember.id, c.otherMember.profileImageUrl ?? null)
    }
  }
}

async function loadConversations() {
  await loadRawConversations()
  populateRegistryFromConvos(conversations.value)
}

let pollInterval: ReturnType<typeof setInterval> | null = null

// Refresh list instantly when a new DM arrives via SignalR
const onNewMessage = () => loadConversations()

// Refresh list when unread count changes (syncs with nav badge)
watch(() => memberStore.unreadMessageCount, () => loadConversations())

onMounted(() => {
  if (route.query.tab === 'admin' && isAdmin.value) {
    activeTab.value = 'admin'
  }
  loadConversations()
  onMessage(onNewMessage)
  nextTick(() => attachConvosScroll())
  pollInterval = setInterval(async () => {
    try {
      await refreshConvosFirst()
      populateRegistryFromConvos(conversations.value)
    } catch { /* */ }
  }, 3000)
})

onActivated(loadConversations)

onUnmounted(() => {
  offMessage(onNewMessage)
  if (pollInterval) clearInterval(pollInterval)
})
</script>

<style lang="scss">
.soc-admin-member-link {
  display: flex;
  align-items: center;
  gap: 12px;
  border-radius: 8px;
  padding: 4px 8px;
  margin: -4px -8px;
  transition: background 0.15s;
  text-decoration: none;
  &:hover { background: var(--soc-bar-active, #eae8e4); }

  &--full {
    width: 100%;
    padding: 8px 12px;
    margin: 0;
    text-align: left;
  }
}

.soc-admin-viewing-banner {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px 16px;
  border-top: 1px solid #c5beb7;
  flex-shrink: 0;
  font-size: 0.8rem;
  color: var(--soc-text, #292524);
  background: #e0dbd6;

  .soc--dark & {
    border-top-color: var(--soc-border);
    background: var(--soc-bar-active);
  }

  &__back {
    position: absolute;
    left: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 28px;
    height: 28px;
    border-radius: 6px;
    color: var(--soc-text-muted, #78716c);
    cursor: pointer;
    transition: color 0.15s, background 0.15s;
    &:hover { color: var(--soc-bar-text-strong, #1a1a1a); background: var(--soc-bar-hover, #f5f3f0); }
  }

  &__link {
    font-weight: 600;
    color: var(--soc-bar-text-strong, #1a1a1a);
    text-decoration: underline;
    text-underline-offset: 2px;
    &:hover { opacity: 0.7; }
  }
}
</style>
