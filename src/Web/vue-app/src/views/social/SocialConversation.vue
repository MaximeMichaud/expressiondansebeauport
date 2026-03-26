<template>
  <div class="soc-convo">
    <!-- Header -->
    <div class="soc-convo__header">
      <button @click="$router.push({ name: 'socialMessages' })" class="soc-convo__back">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 19l-7-7 7-7"/></svg>
      </button>
      <router-link v-if="otherMemberId" :to="{ name: 'socialMemberProfile', params: { id: otherMemberId } }" class="soc-convo__profile-link">
        <div class="soc-convo__avatar" :style="{ background: otherMemberColor || getAvatarColor(otherMemberName) }">
          <img v-if="otherMemberPfp" :src="otherMemberPfp" :alt="otherMemberName" class="soc-convo__avatar-img" />
          <span v-else class="soc-convo__avatar-initials">{{ getInitials(otherMemberName) }}</span>
        </div>
        <h2 class="soc-convo__name">{{ otherMemberName }}</h2>
      </router-link>
      <template v-else>
        <span class="soc-convo__name">{{ otherMemberName }}</span>
      </template>
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
          <!-- Deleted message -->
          <div v-if="msg.isDeleted" class="soc-convo__bubble soc-convo__bubble--deleted">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="4.93" y1="4.93" x2="19.07" y2="19.07"/></svg>
            Ce message a été supprimé
          </div>
          <!-- Normal message -->
          <div
            v-else
            :class="[
              'soc-convo__bubble',
              msg.isMine ? (msg.pending ? 'soc-convo__bubble--pending' : 'soc-convo__bubble--mine') : 'soc-convo__bubble--other'
            ]"
            @contextmenu.prevent="msg.isMine && !msg.pending ? openDeleteMenu(msg) : null"
            @click.self="msg.isMine && !msg.pending ? openDeleteMenu(msg) : null"
          >
            {{ msg.content }}
            <button
              v-if="msg.isMine && !msg.pending"
              @click.stop="openDeleteMenu(msg)"
              class="soc-convo__delete-trigger"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/></svg>
            </button>
          </div>
          <div v-if="!msg.isDeleted" :class="['soc-convo__meta', msg.isMine && 'soc-convo__meta--mine']">
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
        ref="messageInput"
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

    <!-- Delete confirmation modal -->
    <Teleport to="body">
      <Transition name="convo-modal">
        <div v-if="deleteTarget" class="convo-modal__overlay" @click.self="deleteTarget = null">
          <div class="convo-modal__card">
            <div class="convo-modal__icon-ring">
              <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#dc2626" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/>
              </svg>
            </div>
            <h3 class="convo-modal__title">Supprimer ce message?</h3>
            <p class="convo-modal__text">Le message sera remplacé par « Ce message a été supprimé ».</p>
            <div class="convo-modal__actions">
              <button @click="deleteTarget = null" class="convo-modal__btn convo-modal__btn--cancel">Annuler</button>
              <button @click="confirmDelete" :disabled="deleting" class="convo-modal__btn convo-modal__btn--danger">
                {{ deleting ? 'Suppression...' : 'Supprimer' }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
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
  isDeleted?: boolean
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
const otherMemberColor = ref('')
const currentMemberId = ref('')

const avatarColors = ['#e53e3e', '#dd6b20', '#d69e2e', '#38a169', '#319795', '#3182ce', '#5a67d8', '#805ad5', '#d53f8c', '#e53e3e']
function getAvatarColor(name: string) {
  let hash = 0
  for (let i = 0; i < (name?.length || 0); i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

function getInitials(name: string) {
  if (!name || !name.trim()) return '??'
  return name.split(' ').filter(n => n.length > 0).map(n => n[0]).join('').toUpperCase().slice(0, 2)
}
const messageInput = ref<HTMLInputElement | null>(null)
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
      otherMemberColor.value = convo.otherMember.avatarColor || ''
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
      isDeleted: m.isDeleted ?? false,
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

// Delete message
const deleteTarget = ref<ChatMessage | null>(null)
const deleting = ref(false)

function openDeleteMenu(msg: ChatMessage) {
  deleteTarget.value = msg
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    await socialService.deleteMessage(deleteTarget.value.id)
    deleteTarget.value = null
    await pollMessages()
  } catch { /* */ }
  deleting.value = false
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
      isDeleted: m.isDeleted ?? false,
    }))
    pendingMessages.value = pendingMessages.value.filter(
      pm => !serverMessages.value.some(sm => sm.content === pm.content)
    )
    // New message received — scroll down and update unread count
    if (serverMessages.value.length > prevCount) {
      scrollToBottom(true)
      await socialService.markAsRead(conversationId.value)
      try {
        const count = await socialService.getUnreadCount()
        memberStore.setUnreadCount(count)
      } catch { /* */ }
    }
  } catch { /* */ }
}

onMounted(async () => {
  await loadConversationInfo()
  await loadMessages()
  nextTick(() => messageInput.value?.focus())
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
    position: relative;

    &--mine {
      background: #1a1a1a;
      color: white;
      border-bottom-right-radius: 6px;

      .soc-convo__delete-trigger { opacity: 0; }
      &:hover .soc-convo__delete-trigger { opacity: 1; }
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

    &--deleted {
      display: flex;
      align-items: center;
      gap: 6px;
      max-width: 75%;
      padding: 8px 14px;
      border-radius: 18px;
      font-size: 0.78rem;
      font-style: italic;
      color: var(--soc-text-muted, #a8a29e);
      background: transparent;
      border: 1px dashed var(--soc-border, #e7e0da);
    }
  }

  &__delete-trigger {
    position: absolute;
    top: -8px;
    right: -8px;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 26px;
    height: 26px;
    border-radius: 50%;
    background: #dc2626;
    color: white;
    transition: opacity 0.15s;
    box-shadow: 0 2px 6px rgba(0,0,0,0.15);
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

// Delete modal
.convo-modal {
  &__overlay {
    position: fixed;
    inset: 0;
    z-index: 9999;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(0, 0, 0, 0.5);
    backdrop-filter: blur(4px);
    padding: 20px;
  }

  &__card {
    width: 100%;
    max-width: 360px;
    background: var(--soc-card-bg, white);
    border-radius: 16px;
    padding: 32px 28px 24px;
    text-align: center;
    box-shadow: 0 24px 48px rgba(0, 0, 0, 0.2);
  }

  &__icon-ring {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 56px;
    height: 56px;
    border-radius: 50%;
    background: rgba(220, 38, 38, 0.08);
    border: 2px solid rgba(220, 38, 38, 0.2);
    margin-bottom: 16px;
  }

  &__title {
    font-family: $convo-font-display;
    font-weight: 700;
    font-size: 1.05rem;
    color: var(--soc-bar-text-strong, #1a1a1a);
    margin-bottom: 6px;
  }

  &__text {
    font-size: 0.83rem;
    line-height: 1.5;
    color: var(--soc-text-muted, #78716c);
    margin-bottom: 20px;
  }

  &__actions { display: flex; gap: 10px; }

  &__btn {
    flex: 1;
    padding: 11px 16px;
    font-family: $convo-font-display;
    font-size: 0.82rem;
    font-weight: 600;
    border-radius: 10px;
    cursor: pointer;
    transition: background 0.15s, transform 0.1s;
    &:active { transform: scale(0.98); }
    &:disabled { opacity: 0.5; cursor: default; }

    &--cancel {
      background: var(--soc-bar-hover, #f5f3f0);
      color: var(--soc-bar-text-strong, #1a1a1a);
      &:hover { background: var(--soc-bar-active, #eae8e4); }
    }

    &--danger {
      background: #dc2626;
      color: white;
      &:hover { background: #b91c1c; }
    }
  }
}

.convo-modal-enter-active { transition: all 0.2s ease; }
.convo-modal-leave-active { transition: all 0.15s ease; }
.convo-modal-enter-from { opacity: 0; }
.convo-modal-leave-to { opacity: 0; }
</style>
