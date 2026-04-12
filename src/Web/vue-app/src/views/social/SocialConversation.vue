<template>
  <div
    class="soc-convo"
    @dragenter="!isAdminView && attachment.handleDragEnter($event)"
    @dragleave="!isAdminView && attachment.handleDragLeave($event)"
    @dragover="!isAdminView && attachment.handleDragOver($event)"
    @drop="!isAdminView && attachment.handleDrop($event)"
  >
    <div v-if="!isAdminView && attachment.isDraggingOver.value" class="soc-convo__drag-overlay">
      <span>Déposer l'image ici</span>
    </div>
    <!-- Header -->
    <div class="soc-convo__header">
      <button @click="$router.push(isAdminView ? { name: 'socialMessages', query: { tab: 'admin' } } : { name: 'socialMessages' })" class="soc-convo__back">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 19l-7-7 7-7"/></svg>
      </button>
      <template v-if="isAdminView">
        <router-link v-if="adminOtherMember.id" :to="{ name: 'socialMemberProfile', params: { id: adminOtherMember.id } }" class="soc-convo__profile-link">
          <div class="soc-convo__avatar" :style="{ background: getAvatarColor(adminOtherMember.name) }">
            <span class="soc-convo__avatar-initials">{{ getInitials(adminOtherMember.name) }}</span>
          </div>
          <h2 class="soc-convo__name">{{ adminOtherMember.name }}</h2>
        </router-link>
      </template>
      <template v-else>
        <router-link v-if="otherMemberId" :to="{ name: 'socialMemberProfile', params: { id: otherMemberId } }" class="soc-convo__profile-link">
          <div class="soc-convo__avatar" :style="{ background: otherMemberColor || getAvatarColor(otherMemberName) }">
            <img v-if="effectiveOtherMemberPfp" :src="effectiveOtherMemberPfp" :alt="otherMemberName" class="soc-convo__avatar-img" />
            <span v-else class="soc-convo__avatar-initials">{{ getInitials(otherMemberName) }}</span>
          </div>
          <h2 class="soc-convo__name">{{ otherMemberName }}</h2>
        </router-link>
        <template v-else>
          <span class="soc-convo__name">{{ otherMemberName }}</span>
        </template>
      </template>
    </div>

    <!-- Messages (scrollable) -->
    <div ref="messagesContainer" :class="['soc-convo__messages', !ready && 'soc-convo__messages--hidden']">
      <div v-if="loadingMoreMessages" class="flex justify-center py-3">
        <div class="h-4 w-4 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
      </div>
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
          <!-- Join request card (with form) -->
          <JoinRequestCard
            v-else-if="msg.messageType === 'JoinRequest' && msg.joinRequest"
            :join-request-id="msg.joinRequest.id"
            :group-name="msg.joinRequest.groupName"
            :requester-name="msg.joinRequest.requesterName"
            :status="msg.joinRequest.status"
            :resolved-by-name="msg.joinRequest.resolvedByName"
            :is-mine="msg.isMine"
            @resolved="pollMessages"
          />
          <!-- Join request notification (hidden — visible only in DM list) -->
          <template v-else-if="msg.messageType === 'JoinRequest' && !msg.joinRequest" />
          <!-- Normal message -->
          <div
            v-else
            :class="[
              'soc-convo__bubble',
              msg.isMine ? (msg.pending ? 'soc-convo__bubble--pending' : 'soc-convo__bubble--mine') : 'soc-convo__bubble--other',
              ((msg.media && msg.media.length) || (msg.pendingPreviewUrls && msg.pendingPreviewUrls.length)) && 'soc-convo__bubble--has-media'
            ]"
            @contextmenu.prevent="!isAdminView && msg.isMine && !msg.pending ? openDeleteMenu(msg) : null"
          >
            <div
              v-if="msg.pendingPreviewUrls && msg.pendingPreviewUrls.length"
              :class="['soc-convo__media-grid', msg.pendingPreviewUrls.length > 1 && 'soc-convo__media-grid--multi']"
            >
              <img
                v-for="(url, idx) in msg.pendingPreviewUrls"
                :key="idx"
                :src="url"
                class="soc-convo__bubble-img soc-convo__bubble-img--pending"
                alt=""
                @load="handleMediaLoad"
              />
            </div>
            <div
              v-else-if="msg.media && msg.media.length"
              :class="['soc-convo__media-grid', msg.media.length > 1 && 'soc-convo__media-grid--multi']"
            >
              <template v-for="m in msg.media" :key="m.id">
                <div
                  v-if="m.contentType && m.contentType.startsWith('video/')"
                  class="soc-convo__video-thumb"
                  @click="openLightbox(m.mediaUrl, m.originalUrl, m.contentType)"
                >
                  <video
                    :src="m.mediaUrl"
                    muted
                    playsinline
                    preload="metadata"
                    class="soc-convo__bubble-img"
                    style="background: #000; pointer-events: none;"
                    @loadedmetadata="handleMediaLoad"
                  />
                  <div class="soc-convo__video-play">
                    <svg width="28" height="28" viewBox="0 0 24 24" fill="white"><path d="M8 5v14l11-7z"/></svg>
                  </div>
                </div>
                <img
                  v-else
                  :src="m.thumbnailUrl || m.mediaUrl"
                  class="soc-convo__bubble-img"
                  alt=""
                  @load="handleMediaLoad"
                  @click="openLightbox(m.mediaUrl, m.originalUrl, m.contentType)"
                />
              </template>
            </div>
            <span v-if="msg.content" class="soc-convo__bubble-text">{{ msg.content }}</span>
            <button
              v-if="!isAdminView && msg.isMine && !msg.pending"
              @click.stop="openDeleteMenu(msg)"
              class="soc-convo__delete-trigger"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/></svg>
            </button>
          </div>
          <div v-if="!msg.isDeleted && !(msg.messageType === 'JoinRequest' && !msg.joinRequest)" :class="['soc-convo__meta', msg.isMine && 'soc-convo__meta--mine']">
            <span>{{ formatTime(msg.created) }}</span>
            <span v-if="msg.isMine && msg.pending" class="soc-convo__status">Envoi...</span>
            <span v-else-if="msg.isMine && msg.isRead" class="soc-convo__status soc-convo__status--read">Lu</span>
            <span v-else-if="msg.isMine" class="soc-convo__status">Envoyé</span>
          </div>
        </div>
      </template>
    </div>

    <!-- Preview strip (above input bar, only when attachments) -->
    <div v-if="!isAdminView && attachment.previews.value.length" class="soc-convo__preview-strip">
      <div
        v-for="(p, i) in attachment.previews.value"
        :key="p.url"
        class="soc-convo__preview-item"
      >
        <video
          v-if="p.file.type.startsWith('video/')"
          :src="p.url"
          muted
          playsinline
          preload="metadata"
          class="soc-convo__preview-img"
          style="background: #000;"
        />
        <img v-else :src="p.url" class="soc-convo__preview-img" alt="" />
        <button
          type="button"
          class="soc-convo__preview-remove"
          style="background: #dc2626;"
          @click="attachment.removeFile(i)"
          aria-label="Retirer"
        >
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="#ffffff" stroke-width="4" stroke-linecap="round" stroke-linejoin="round">
            <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
          </svg>
        </button>
      </div>
    </div>

    <!-- Inline error under preview strip / above input -->
    <p v-if="!isAdminView && attachment.error.value" class="soc-convo__error-inline">
      {{ attachment.error.value }}
    </p>

    <!-- Admin readonly banner -->
    <div v-if="isAdminView" class="soc-convo__admin-banner">
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
      <span>Lecture seule, vu par <router-link v-if="adminViewingMember.id" :to="{ name: 'socialMemberProfile', params: { id: adminViewingMember.id } }" class="soc-convo__admin-banner-link">{{ adminViewingMember.name }}</router-link></span>
    </div>

    <!-- Input -->
    <div v-if="!isAdminView" class="soc-convo__input-bar">
      <input
        ref="fileInputRef"
        type="file"
        accept="image/*,video/*"
        multiple
        hidden
        @change="attachment.handleFileInput"
      />
      <button
        type="button"
        class="soc-convo__attach"
        :disabled="uploading || attachment.files.value.length >= 10"
        @click="triggerFilePicker"
        aria-label="Joindre un fichier"
      >
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M21.44 11.05l-9.19 9.19a6 6 0 01-8.49-8.49l9.19-9.19a4 4 0 015.66 5.66l-9.2 9.19a2 2 0 01-2.83-2.83l8.49-8.48"/>
        </svg>
      </button>
      <input
        v-model="newMessage"
        type="text"
        class="soc-convo__input"
        placeholder="Écrire un message..."
        @keyup.enter="sendMessage"
      />
      <button
        @click="sendMessage"
        :disabled="(!newMessage.trim() && !attachment.files.value.length) || uploading"
        class="soc-convo__send"
      >
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M22 2L11 13"/><path d="M22 2l-7 20-4-9-9-4 20-7z"/>
        </svg>
      </button>
    </div>

    <!-- Delete confirmation modal -->
    <Transition name="convo-modal">
      <div v-if="!isAdminView && deleteTarget" class="convo-modal__overlay" @click.self="deleteTarget = null">
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
    <ImageLightbox
      v-model:open="lightboxOpen"
      :display-url="lightboxDisplayUrl"
      :original-url="lightboxOriginalUrl"
      :content-type="lightboxContentType"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useMemberStore } from '@/stores/memberStore'
import { useInfiniteScroll } from '@/composables/useInfiniteScroll'
import { useAvatarRegistryStore } from '@/stores/avatarRegistryStore'
import ImageLightbox from '@/components/social/ImageLightbox.vue'
import JoinRequestCard from '@/components/social/JoinRequestCard.vue'
import { useImageAttachment } from '@/composables/useImageAttachment'

interface ChatMessageMedia {
  id: string
  mediaUrl: string
  thumbnailUrl?: string
  originalUrl?: string
  contentType?: string
  sortOrder: number
}

interface ChatMessage {
  id: string
  content: string
  senderMemberId: string
  created: string
  isMine: boolean
  pending?: boolean
  isRead?: boolean
  isDeleted?: boolean
  media?: ChatMessageMedia[]
  pendingPreviewUrls?: string[]
  messageType?: string
  joinRequest?: {
    id: string
    groupId: string
    groupName: string
    requesterMemberId: string
    requesterName: string
    status: 'Pending' | 'Accepted' | 'Rejected'
    resolvedByName?: string
  }
}

const route = useRoute()
const isAdminView = computed(() => route.name === 'socialAdminConversation')
const socialService = useSocialService()
const memberStore = useMemberStore()
const avatarRegistry = useAvatarRegistryStore()

const conversationId = computed(() => route.params.conversationId as string)
const pendingMessages = ref<ChatMessage[]>([])
const loading = ref(true)
const newMessage = ref('')
const attachment = useImageAttachment({ mode: 'multi', maxFiles: 10 })
const uploading = ref(false)
const fileInputRef = ref<HTMLInputElement | null>(null)

const lightboxOpen = ref(false)
const lightboxDisplayUrl = ref('')
const lightboxOriginalUrl = ref<string | undefined>(undefined)
const lightboxContentType = ref<string | undefined>(undefined)

function openLightbox(displayUrl: string, originalUrl?: string, contentType?: string) {
  lightboxDisplayUrl.value = displayUrl
  lightboxOriginalUrl.value = originalUrl
  lightboxContentType.value = contentType
  lightboxOpen.value = true
}

function triggerFilePicker() {
  fileInputRef.value?.click()
}

const otherMemberName = ref('Conversation')
const otherMemberId = ref('')
const otherMemberPfp = ref('')
const otherMemberColor = ref('')
const adminViewingMember = ref<{ id: string; name: string }>({ id: '', name: '' })
const adminOtherMember = ref<{ id: string; name: string }>({ id: '', name: '' })

const effectiveOtherMemberPfp = computed(() => {
  return avatarRegistry.getAvatar(otherMemberId.value, otherMemberPfp.value) || ''
})
const currentMemberId = ref('')
const messagesContainer = ref<HTMLElement | null>(null)

const {
  items: scrollMessages,
  loadingMore: loadingMoreMessages,
  load: loadScrollMessages,
  refreshFirst: refreshMessagesFirst,
  attachScroll: attachMessagesScroll,
} = useInfiniteScroll<ChatMessage>({
  fetchFn: async (page) => {
    const raw = isAdminView.value
      ? await socialService.getAdminMessages(conversationId.value, page)
      : await socialService.getMessages(conversationId.value, page)
    return {
      items: raw.items.map((m: any) => ({
        id: m.id,
        content: m.content,
        senderMemberId: m.senderMemberId,
        created: m.created,
        isMine: isAdminView.value ? false : m.senderMemberId === currentMemberId.value,
        isRead: m.isRead ?? false,
        isDeleted: m.isDeleted ?? false,
        media: Array.isArray(m.media) ? m.media : undefined,
        messageType: m.messageType,
        joinRequest: m.joinRequest,
      })),
      hasMore: raw.hasMore,
    }
  },
  scrollContainer: messagesContainer,
  direction: 'up',
  threshold: 200,
})

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
const ready = ref(false)

const allMessages = computed(() => {
  const sorted = [...scrollMessages.value].reverse()
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

// Called by @load on <img> and @loadedmetadata on <video>. We only re-pin to
// the bottom if the user is already near the bottom, so it doesn't yank them
// up when they're reading older messages while new media loads.
function handleMediaLoad() {
  const el = messagesContainer.value
  if (!el) return
  const distanceFromBottom = el.scrollHeight - el.scrollTop - el.clientHeight
  const nearBottom = distanceFromBottom < 200
  if (nearBottom || !ready.value) {
    el.scrollTo({ top: el.scrollHeight, behavior: 'instant' })
  }
}

async function loadConversationInfo() {
  if (isAdminView.value) {
    adminViewingMember.value = { id: (route.query.viewingId as string) || '', name: (route.query.viewingName as string) || '' }
    adminOtherMember.value = { id: (route.query.otherId as string) || '', name: (route.query.otherName as string) || '' }
    return
  }

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
    const convResult = await socialService.getConversations()
    const convo = convResult.items.find((c: any) => c.id === conversationId.value)
    if (convo?.otherMember) {
      otherMemberName.value = convo.otherMember.fullName || 'Conversation'
      otherMemberId.value = convo.otherMember.id || ''
      otherMemberPfp.value = convo.otherMember.profileImageUrl || ''
      otherMemberColor.value = convo.otherMember.avatarColor || ''
      avatarRegistry.setAvatar(convo.otherMember.id, convo.otherMember.profileImageUrl ?? null)
    }
  } catch { /* */ }
}

async function loadMessages(smooth = false) {
  const isFirstLoad = scrollMessages.value.length === 0
  if (isFirstLoad) loading.value = true
  try {
    await loadScrollMessages()
    pendingMessages.value = pendingMessages.value.filter(
      pm => !scrollMessages.value.some(sm => sm.content === pm.content)
    )
    if (!isAdminView.value) {
      await socialService.markAsRead(conversationId.value)
      try {
        const result = await socialService.getUnreadCount()
        memberStore.setUnreadCount(result.count)
      } catch { /* */ }
    }
  } catch { /* */ }
  loading.value = false
  scrollToBottom(smooth)
}

async function sendMessage() {
  const text = newMessage.value.trim()
  const files = attachment.files.value

  if (!text && files.length === 0) return

  // Optimistic pending message with preview URLs
  const tempId = 'pending-' + Date.now()
  const pendingPreviewUrls = files.length > 0
    ? attachment.previews.value.map(p => p.url)
    : undefined
  pendingMessages.value.push({
    id: tempId,
    content: text,
    senderMemberId: currentMemberId.value,
    created: new Date().toISOString(),
    isMine: true,
    pending: true,
    pendingPreviewUrls,
  })
  newMessage.value = ''
  scrollToBottom(true)

  let media: Array<{
    displayUrl: string
    thumbnailUrl: string
    originalUrl: string
    contentType: string
    size: number
  }> = []

  if (files.length > 0) {
    uploading.value = true
    try {
      const uploads = await Promise.all(files.map(f => socialService.uploadFile(f)))
      for (const u of uploads) {
        if (!u.succeeded || !u.displayUrl || !u.thumbnailUrl || !u.originalUrl || !u.contentType || u.size == null) {
          throw new Error('upload-failed')
        }
      }
      media = uploads.map(u => ({
        displayUrl: u.displayUrl!,
        thumbnailUrl: u.thumbnailUrl!,
        originalUrl: u.originalUrl!,
        contentType: u.contentType!,
        size: u.size!,
      }))
    } catch {
      pendingMessages.value = pendingMessages.value.filter(m => m.id !== tempId)
      uploading.value = false
      return
    }
    uploading.value = false
  }

  try {
    await socialService.sendMessage(conversationId.value, text, media.length > 0 ? media : undefined)
    attachment.clear()
    await loadMessages(true)
  } catch {
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
  const prevCount = scrollMessages.value.length
  try {
    await refreshMessagesFirst()
    pendingMessages.value = pendingMessages.value.filter(
      pm => !scrollMessages.value.some(sm => sm.content === pm.content)
    )
    if (scrollMessages.value.length > prevCount) {
      scrollToBottom(true)
      if (!isAdminView.value) {
        await socialService.markAsRead(conversationId.value)
        try {
          const r = await socialService.getUnreadCount()
          memberStore.setUnreadCount(r.count)
        } catch { /* */ }
      }
    }
  } catch { /* */ }
}

// When the preview strip appears or grows, the messages pane shrinks; keep
// the latest messages visible by snapping back to the bottom.
watch(() => attachment.previews.value.length, () => {
  nextTick(() => scrollToBottom(true))
})

onMounted(async () => {
  await loadConversationInfo()
  await loadMessages()
  nextTick(() => {
    messageInput.value?.focus()
    attachMessagesScroll()
  })
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
  position: relative;

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
    padding: 16px 24px;
    display: flex;
    flex-direction: column;
    gap: 12px;
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
  &__admin-banner {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    padding: 12px 16px;
    border-top: 1px solid var(--soc-border, #d6cfc9);
    flex-shrink: 0;
    font-size: 0.8rem;
    color: var(--soc-text, #292524);
    background: var(--soc-input-bg, #faf9f7);
  }

  &__admin-banner-link {
    font-weight: 600;
    color: var(--soc-bar-text-strong, #1a1a1a);
    text-decoration: underline;
    text-underline-offset: 2px;
    &:hover { opacity: 0.7; }
  }

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

  &__drag-overlay {
    position: absolute;
    inset: 0;
    z-index: 100;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(26, 26, 26, 0.85);
    color: white;
    font-family: $convo-font-display;
    font-weight: 600;
    font-size: 1rem;
    pointer-events: none;
    border: 3px dashed rgba(255, 255, 255, 0.4);
  }

  &__bubble--has-media {
    padding: 4px;
    overflow: visible;
  }

  &__bubble-img {
    display: block;
    max-width: 280px;
    width: 100%;
    border-radius: 14px;
    cursor: pointer;
  }

  &__bubble-img--pending { opacity: 0.6; }

  &__media-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: 2px;
    border-radius: 14px;
    overflow: hidden;

    &--multi {
      grid-template-columns: 1fr 1fr;
      max-width: 240px;
    }

    &--multi .soc-convo__bubble-img {
      aspect-ratio: 1 / 1;
      object-fit: cover;
      border-radius: 0;
      max-width: none;
    }

    &--multi .soc-convo__video-thumb {
      aspect-ratio: 1 / 1;
    }
  }

  &__video-thumb {
    position: relative;
    display: block;
    cursor: pointer;
    line-height: 0;
  }

  &__video-play {
    position: absolute;
    inset: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    pointer-events: none;
    svg {
      width: 48px;
      height: 48px;
      padding: 12px;
      border-radius: 50%;
      background: rgba(0, 0, 0, 0.5);
      backdrop-filter: blur(4px);
    }
  }

  &__bubble-text {
    display: block;
    padding: 6px 10px 4px;
  }

  &__attach {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    border-radius: 10px;
    background: var(--soc-bar-hover, #f5f3f0);
    color: var(--soc-text-muted, #78716c);
    cursor: pointer;
    flex-shrink: 0;
    transition: color 0.15s, background 0.15s;
    &:hover { color: var(--soc-bar-text-strong, #1a1a1a); background: var(--soc-bar-active, #eae8e4); }
    &:disabled { opacity: 0.35; cursor: default; }
  }

  &__preview-strip {
    display: flex;
    gap: 8px;
    padding: 12px 16px 16px;
    overflow-x: auto;
    flex-shrink: 0;
    border-top: 1px solid var(--soc-divider, #f0f0f0);
  }

  &__preview-item {
    position: relative;
    width: 80px;
    height: 80px;
    flex-shrink: 0;
    margin-top: 6px;
    margin-right: 6px;
  }

  &__preview-img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    border-radius: 8px;
    border: 1px solid var(--soc-input-border, #e7e0da);
  }

  &__preview-remove {
    position: absolute;
    top: -6px;
    right: -6px;
    width: 20px;
    height: 20px;
    border-radius: 50%;
    background: #1a1a1a;
    color: white;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    box-shadow: 0 1px 3px rgba(0,0,0,0.2);
  }

  &__error-inline {
    padding: 6px 20px 0;
    color: #dc2626;
    font-size: 0.78rem;
    font-weight: 500;
    flex-shrink: 0;
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
