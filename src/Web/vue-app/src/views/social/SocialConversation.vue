<template>
  <div class="soc-convo">
    <!-- Header -->
    <div class="soc-convo__header">
      <button @click="$router.push({ name: 'socialMessages' })" class="soc-convo__back">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 19l-7-7 7-7"/></svg>
      </button>
      <router-link v-if="otherMemberId" :to="{ name: 'socialMemberProfile', params: { id: otherMemberId } }" class="soc-convo__profile-link">
        <div class="soc-convo__avatar" :style="{ background: otherMemberColor || '#1a1a1a' }">
          <img v-if="otherMemberPfp" :src="otherMemberPfp" :alt="otherMemberName" class="soc-convo__avatar-img" />
          <span v-else class="soc-convo__avatar-initials">{{ getInitials(otherMemberName) }}</span>
        </div>
        <h2 class="soc-convo__name">{{ otherMemberName }}</h2>
      </router-link>
      <span v-else class="soc-convo__name">{{ otherMemberName }}</span>
    </div>

    <!-- Messages (scrollable) -->
    <div ref="messagesContainer" :class="['soc-convo__messages', !ready && 'soc-convo__messages--hidden']">
      <div v-if="loading" class="soc-convo__loading">
        <div class="soc-convo__spinner" />
      </div>
      <template v-else>
        <div
          v-for="msg in allMessages"
          :key="msg.id"
          :class="['soc-convo__row', msg.isMine && 'soc-convo__row--mine']"
        >
          <div
            :class="[
              'soc-convo__bubble',
              msg.isMine ? (msg.pending ? 'soc-convo__bubble--pending' : 'soc-convo__bubble--mine') : 'soc-convo__bubble--other'
            ]"
          >
            {{ msg.content }}
          </div>
          <div :class="['soc-convo__meta', msg.isMine && 'soc-convo__meta--mine']">
            <span>{{ formatTime(msg.created) }}</span>
            <span v-if="msg.isMine && msg.pending" class="soc-convo__status">Envoi...</span>
            <span v-else-if="msg.isMine && msg.isRead" class="soc-convo__status soc-convo__status--read">Lu</span>
            <span v-else-if="msg.isMine" class="soc-convo__status">Envoyé</span>
          </div>
        </div>
      </template>
    </div>

    <!-- Input -->
    <div class="soc-convo__input-bar">
      <input
        v-model="newMessage"
        type="text"
        class="soc-convo__input"
        placeholder="Écrire un message..."
        @keyup.enter="sendMessage"
      />
      <button
        @click="sendMessage"
        :disabled="!newMessage.trim()"
        class="soc-convo__send"
      >
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M22 2L11 13"/><path d="M22 2l-7 20-4-9-9-4 20-7z"/>
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useMemberStore } from '@/stores/memberStore'

interface ChatMessage {
  id: string
  content: string
  senderMemberId: string
  created: string
  isMine: boolean
  pending?: boolean
  isRead?: boolean
}

const route = useRoute()
const socialService = useSocialService()
const memberStore = useMemberStore()

const conversationId = computed(() => route.params.conversationId as string)
const serverMessages = ref<ChatMessage[]>([])
const pendingMessages = ref<ChatMessage[]>([])
const loading = ref(true)
const newMessage = ref('')
const otherMemberName = ref('Conversation')
const otherMemberId = ref('')
const otherMemberPfp = ref('')
const otherMemberColor = ref('#1a1a1a')
const currentMemberId = ref('')

const avatarColors = ['#1a1a1a', '#3b3b3b', '#6b4c3b', '#4a5568', '#2d3748', '#553c2e', '#44403c', '#1e293b', '#374151', '#292524']
function getAvatarColor(name: string) {
  let hash = 0
  for (let i = 0; i < (name?.length || 0); i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

function getInitials(name: string) {
  if (!name || !name.trim()) return '??'
  return name.split(' ').filter(n => n.length > 0).map(n => n[0]).join('').toUpperCase().slice(0, 2)
}
const messagesContainer = ref<HTMLElement | null>(null)
const ready = ref(false)

const allMessages = computed(() => {
  // Server messages (oldest first) + pending at end
  const sorted = [...serverMessages.value].reverse()
  return [...sorted, ...pendingMessages.value]
})

function formatTime(dateStr: string) {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleTimeString('fr-CA', { hour: '2-digit', minute: '2-digit' })
}

function scrollToBottom(smooth = false) {
  nextTick(() => {
    requestAnimationFrame(() => {
      if (messagesContainer.value) {
        messagesContainer.value.scrollTo({ top: messagesContainer.value.scrollHeight, behavior: smooth ? 'smooth' : 'instant' })
        if (!ready.value) ready.value = true
      }
    })
  })
}

async function loadConversationInfo() {
  const m = memberStore.member
  if (m?.id) {
    currentMemberId.value = m.id
  } else {
    try {
      const profile = await socialService.getMyProfile()
      currentMemberId.value = profile.id || profile.Id
      memberStore.setMember(profile)
    } catch { /* */ }
  }

  try {
    const conversations = await socialService.getConversations()
    const convo = conversations.find((c: any) => c.id === conversationId.value)
    if (convo?.otherMember) {
      otherMemberName.value = convo.otherMember.fullName || 'Conversation'
      otherMemberId.value = convo.otherMember.id || ''
      otherMemberPfp.value = convo.otherMember.profileImageUrl || ''
      otherMemberColor.value = convo.otherMember.avatarColor || '#1a1a1a'
    }
  } catch { /* */ }
}

async function loadMessages(smooth = false) {
  const isFirstLoad = serverMessages.value.length === 0
  if (isFirstLoad) loading.value = true
  try {
    const raw = await socialService.getMessages(conversationId.value)
    serverMessages.value = raw.map((m: any) => ({
      id: m.id,
      content: m.content,
      senderMemberId: m.senderMemberId,
      created: m.created,
      isMine: m.senderMemberId === currentMemberId.value,
      isRead: m.isRead ?? false,
    }))
    pendingMessages.value = pendingMessages.value.filter(
      pm => !serverMessages.value.some(sm => sm.content === pm.content)
    )
    await socialService.markAsRead(conversationId.value)
    try {
      const count = await socialService.getUnreadCount()
      memberStore.setUnreadCount(count)
    } catch { /* */ }
  } catch { /* */ }
  loading.value = false
  scrollToBottom(smooth)
}

async function sendMessage() {
  const text = newMessage.value.trim()
  if (!text) return

  // Add optimistic pending message
  const tempId = 'pending-' + Date.now()
  pendingMessages.value.push({
    id: tempId,
    content: text,
    senderMemberId: currentMemberId.value,
    created: new Date().toISOString(),
    isMine: true,
    pending: true,
  })
  newMessage.value = ''
  scrollToBottom(true)

  try {
    await socialService.sendMessage(conversationId.value, text)
    await loadMessages(true)
  } catch {
    // Remove failed pending message
    pendingMessages.value = pendingMessages.value.filter(m => m.id !== tempId)
  }
}

// Silent poll for read receipts and new messages
let pollInterval: ReturnType<typeof setInterval> | null = null

async function pollMessages() {
  const prevCount = serverMessages.value.length
  try {
    const raw = await socialService.getMessages(conversationId.value)
    serverMessages.value = raw.map((m: any) => ({
      id: m.id,
      content: m.content,
      senderMemberId: m.senderMemberId,
      created: m.created,
      isMine: m.senderMemberId === currentMemberId.value,
      isRead: m.isRead ?? false,
    }))
    pendingMessages.value = pendingMessages.value.filter(
      pm => !serverMessages.value.some(sm => sm.content === pm.content)
    )
    // New message received — scroll down
    if (serverMessages.value.length > prevCount) {
      scrollToBottom(true)
      await socialService.markAsRead(conversationId.value)
    }
  } catch { /* */ }
}

onMounted(async () => {
  await loadConversationInfo()
  await loadMessages()
  pollInterval = setInterval(pollMessages, 1000)
})

onUnmounted(() => {
  if (pollInterval) clearInterval(pollInterval)
})
</script>

<style lang="scss">
$convo-font-display: 'Montserrat', sans-serif;
$convo-font-body: 'Karla', sans-serif;

.soc-convo {
  display: flex;
  flex-direction: column;
  height: 100%;

  &__header {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 14px 16px;
    border-bottom: 1px solid var(--soc-divider, #f0f0f0);
    flex-shrink: 0;
  }

  &__back {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    border-radius: 8px;
    color: var(--soc-text-muted, #78716c);
    cursor: pointer;
    transition: color 0.15s, background 0.15s;
    &:hover { color: var(--soc-bar-text-strong, #1a1a1a); background: var(--soc-bar-hover, #f5f3f0); }
  }

  &__profile-link {
    display: flex;
    align-items: center;
    gap: 10px;
    text-decoration: none;
    cursor: pointer;
    border-radius: 8px;
    padding: 4px 8px 4px 4px;
    margin: -4px;
    transition: background 0.15s;
    &:hover { background: var(--soc-bar-hover, #f5f3f0); }
  }

  &__avatar {
    width: 34px;
    height: 34px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    flex-shrink: 0;
  }

  &__avatar-img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  &__avatar-initials {
    font-family: $convo-font-display;
    font-weight: 700;
    font-size: 0.7rem;
    color: white;
  }

  &__name {
    font-family: $convo-font-display;
    font-weight: 600;
    font-size: 0.95rem;
    color: var(--soc-bar-text-strong, #1a1a1a);
  }

  // Scrollable messages area
  &__messages {
    flex: 1;
    overflow-y: auto;
    padding: 16px;
    display: flex;
    flex-direction: column;
    gap: 4px;
    min-height: 0;

    &--hidden { visibility: hidden; }
  }

  &__loading {
    display: flex;
    justify-content: center;
    align-items: center;
    flex: 1;
  }

  &__spinner {
    width: 24px;
    height: 24px;
    border: 2.5px solid var(--soc-border, #e7e0da);
    border-top-color: var(--soc-bar-text-strong, #1a1a1a);
    border-radius: 50%;
    animation: convo-spin 0.7s linear infinite;
  }

  // Message rows
  &__row {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    &--mine { align-items: flex-end; }
  }

  &__bubble {
    max-width: 75%;
    padding: 9px 14px;
    border-radius: 18px;
    font-size: 0.88rem;
    line-height: 1.45;
    word-break: break-word;

    &--mine {
      background: #1a1a1a;
      color: white;
      border-bottom-right-radius: 6px;
    }

    &--pending {
      background: #a8a29e;
      color: white;
      border-bottom-right-radius: 6px;
    }

    &--other {
      background: var(--soc-bar-hover, #f0f0f0);
      color: var(--soc-bar-text-strong, #1a1a1a);
      border-bottom-left-radius: 6px;
    }
  }

  &__meta {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 2px 4px 0;
    font-size: 0.62rem;
    color: var(--soc-text-muted, #a8a29e);
    &--mine { justify-content: flex-end; }
  }

  &__status {
    font-weight: 500;
    &--read { color: #16a34a; }
  }

  // Input bar
  &__input-bar {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 12px 16px;
    border-top: 1px solid var(--soc-divider, #f0f0f0);
    flex-shrink: 0;
  }

  &__input {
    flex: 1;
    padding: 10px 16px;
    border: 1px solid var(--soc-input-border, #e7e0da);
    border-radius: 999px;
    font-size: 0.88rem;
    background: var(--soc-input-bg, #faf9f7);
    color: var(--soc-text, #292524);
    outline: none;
    transition: border-color 0.15s, background 0.15s;
    &::placeholder { color: var(--soc-text-muted, #a8a29e); }
    &:focus { border-color: var(--soc-bar-text-strong, #1a1a1a); background: var(--soc-content-bg, white); }
  }

  &__send {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 40px;
    border-radius: 50%;
    background: #1a1a1a;
    color: white;
    flex-shrink: 0;
    transition: opacity 0.15s;
    &:hover { opacity: 0.85; }
    &:disabled { opacity: 0.35; }
  }
}

@keyframes convo-spin { to { transform: rotate(360deg); } }
</style>
