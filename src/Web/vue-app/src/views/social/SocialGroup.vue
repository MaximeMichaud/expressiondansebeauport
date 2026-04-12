<template>
  <div ref="feedContainer" class="flex min-h-[calc(100vh-120px)] flex-col overflow-y-auto">
    <!-- Group header -->
    <div class="group-banner flex items-center gap-3 border-b border-gray-200 bg-white px-4 py-3">
      <button @click="$router.push({ name: 'socialPortal' })" class="text-gray-600">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
        </svg>
      </button>
      <AvatarUploader
        :image-url="group?.imageUrl"
        :fallback-initials="'EDB'"
        :fallback-color="'#1a1a1a'"
        :size="32"
        shape="square"
        :can-edit="canEditGroupImage"
        :uploading="uploadingGroupImage"
        @upload="handleGroupImageUpload"
        @remove="confirmGroupImageRemove = true"
      />
      <h1 class="text-base font-semibold text-gray-900">{{ group?.name || 'Groupe' }}</h1>
    </div>

    <!-- Feed -->
    <div class="flex-1">
        <!-- Post composer -->
        <div
          class="border-b-[6px] border-[var(--soc-page-bg,#f0f0f0)] px-4 py-3 relative"
          @dragenter="attachment.handleDragEnter"
          @dragleave="attachment.handleDragLeave"
          @dragover="attachment.handleDragOver"
          @drop="attachment.handleDrop"
        >
          <div v-if="attachment.isDraggingOver.value" class="absolute inset-0 z-20 flex items-center justify-center bg-black/85 text-white font-semibold border-4 border-dashed border-white/40 rounded-lg pointer-events-none">
            Déposer les images ici
          </div>
          <div class="flex items-start gap-3">
            <div class="flex h-9 w-9 flex-shrink-0 items-center justify-center overflow-hidden rounded-full text-xs font-bold text-white" :style="{ background: myAvatarColor }">
              <img
                v-if="avatarRegistry.getAvatar(myMemberId, memberStore.member?.profileImageUrl)"
                :src="avatarRegistry.getAvatar(myMemberId, memberStore.member?.profileImageUrl)!"
                :alt="userInitials"
                class="h-full w-full object-cover"
              />
              <span v-else>{{ userInitials }}</span>
            </div>
            <div class="flex-1 min-w-0">
              <textarea
                v-model="newPostContent"
                rows="2"
                class="w-full resize-none rounded-lg border-0 bg-gray-100 px-3 py-2 text-sm placeholder-gray-400 focus:bg-white focus:ring-1 focus:ring-[#1a1a1a]"
                placeholder="Partager quelque chose..."
              ></textarea>

              <div v-if="attachment.previews.value.length" class="mt-2 mb-4 flex gap-2 overflow-x-auto pt-2 pr-2">
                <div
                  v-for="(p, i) in attachment.previews.value"
                  :key="p.url"
                  class="relative h-20 w-20 flex-shrink-0"
                >
                  <video
                    v-if="p.file.type.startsWith('video/')"
                    :src="p.url"
                    muted
                    playsinline
                    preload="metadata"
                    class="h-full w-full rounded-lg object-cover bg-black"
                  />
                  <img v-else :src="p.url" class="h-full w-full rounded-lg object-cover" alt="" />
                  <button
                    type="button"
                    @click="attachment.removeFile(i)"
                    class="absolute -top-1.5 -right-1.5 flex h-5 w-5 items-center justify-center rounded-full shadow"
                    style="background: #dc2626;"
                    aria-label="Retirer"
                  >
                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="#ffffff" stroke-width="4" stroke-linecap="round" stroke-linejoin="round">
                      <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                    </svg>
                  </button>
                </div>
              </div>

              <p v-if="attachment.error.value" class="mt-2 text-xs text-red-600">{{ attachment.error.value }}</p>

              <div class="mt-2 flex items-center justify-between">
                <div class="flex items-center gap-2">
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
                    @click="triggerFilePicker"
                    :disabled="attachment.files.value.length >= 10"
                    class="soc-composer-icon flex h-9 w-9 items-center justify-center rounded-lg transition cursor-pointer disabled:opacity-40 disabled:cursor-default"
                    title="Joindre un fichier"
                    aria-label="Joindre un fichier"
                  >
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21.44 11.05l-9.19 9.19a6 6 0 01-8.49-8.49l9.19-9.19a4 4 0 015.66 5.66l-9.2 9.19a2 2 0 01-2.83-2.83l8.49-8.48"/></svg>
                  </button>
                  <button
                    v-if="canCreatePolls"
                    type="button"
                    class="soc-composer-icon flex h-9 w-9 items-center justify-center rounded-lg transition cursor-pointer"
                    title="Créer un sondage"
                    @click="showPollModal = true"
                  >
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                      <line x1="12" y1="20" x2="12" y2="10"/>
                      <line x1="18" y1="20" x2="18" y2="4"/>
                      <line x1="6" y1="20" x2="6" y2="16"/>
                    </svg>
                  </button>
                </div>
                <button
                  @click="submitPost"
                  :disabled="(!newPostContent.trim() && !attachment.files.value.length) || submittingPost"
                  class="btn-publish rounded-lg bg-[#1a1a1a] px-4 py-2 text-sm font-semibold text-white transition hover:bg-[#000] disabled:opacity-50 cursor-pointer"
                >
                  Publier
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Posts feed -->
        <div v-if="loadingPosts" class="flex justify-center py-10">
          <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
        </div>
        <div v-else-if="posts.length === 0" class="flex flex-col items-center justify-center gap-3 py-20 text-gray-400">
          <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 013 3L7 19l-4 1 1-4L16.5 3.5z"/></svg>
          <span class="text-sm">Aucun post pour le moment. Soyez le premier à publier!</span>
        </div>
        <div v-else>
          <div v-for="post in posts" :key="post.id" class="border-b-[6px] border-[var(--soc-page-bg,#f0f0f0)] px-4 py-4">
            <!-- Author info -->
            <div class="mb-3 flex items-center gap-2.5">
              <div class="flex h-9 w-9 flex-shrink-0 items-center justify-center overflow-hidden rounded-full text-xs font-bold text-white" :style="{ background: post.authorAvatarColor || '#1a1a1a' }">
                <img
                  v-if="avatarRegistry.getAvatar(post.authorMemberId, post.authorProfileImageUrl)"
                  :src="avatarRegistry.getAvatar(post.authorMemberId, post.authorProfileImageUrl)!"
                  :alt="post.authorName"
                  class="h-full w-full object-cover"
                />
                <span v-else>{{ getInitials(post.authorName) }}</span>
              </div>
              <div class="flex-1">
                <p class="text-sm font-semibold text-gray-900">{{ post.authorName }}</p>
                <p class="text-xs text-gray-500">{{ formatDate(post.created) }}</p>
              </div>
              <span v-if="post.isPinned" class="rounded bg-amber-100 px-2 py-0.5 text-[10px] font-medium text-amber-700">
                Épinglé
              </span>
              <button
                v-if="isAdmin || post.authorMemberId === myMemberId"
                @click="deleteTarget = post"
                class="soc-header__icon-btn soc-header__icon-btn--logout"
                style="width: 30px; height: 30px;"
                title="Supprimer"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/></svg>
              </button>
            </div>

            <!-- Content -->
            <p v-if="post.content" class="mb-3 whitespace-pre-wrap text-sm leading-relaxed text-gray-800">{{ post.content }}</p>

            <!-- Poll -->
            <PollCard
              v-if="post.type === 'Poll' && post.poll"
              :post-id="post.id"
              :poll="post.poll"
              @voted="refreshPostsFirst"
            />

            <!-- Media -->
            <div v-if="post.media && post.media.length" class="mb-3 flex flex-wrap justify-center gap-1">
              <template v-for="media in post.media" :key="media.id">
                <div
                  v-if="media.contentType && media.contentType.startsWith('video/')"
                  class="relative w-[calc(25%-3px)] aspect-square rounded-lg overflow-hidden bg-black cursor-pointer"
                  @click="openLightbox(media.mediaUrl, media.originalUrl, media.contentType)"
                >
                  <video
                    :src="media.mediaUrl"
                    muted
                    playsinline
                    preload="metadata"
                    class="w-full h-full object-cover pointer-events-none"
                  />
                  <div class="absolute inset-0 flex items-center justify-center pointer-events-none">
                    <div class="flex items-center justify-center w-12 h-12 rounded-full bg-black/50 backdrop-blur-sm">
                      <svg width="22" height="22" viewBox="0 0 24 24" fill="white"><path d="M8 5v14l11-7z"/></svg>
                    </div>
                  </div>
                </div>
                <img
                  v-else
                  :src="media.thumbnailUrl || media.mediaUrl"
                  class="w-[calc(25%-3px)] aspect-square rounded-lg object-cover cursor-pointer"
                  @click="openLightbox(media.mediaUrl, media.originalUrl, media.contentType)"
                />
              </template>
            </div>

            <!-- Actions -->
            <div class="mt-3 flex border-t border-[var(--soc-divider,#f0f0f0)] pt-2">
              <button
                @click="toggleLike(post)"
                :class="['flex-1 flex items-center justify-center gap-1.5 py-1.5 text-xs font-medium transition cursor-pointer', post.hasLiked ? 'text-red-600' : 'text-gray-400 hover:text-gray-600']"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" :fill="post.hasLiked ? 'currentColor' : 'none'" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 00-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 00-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 000-7.78z"/></svg>
                {{ post.likeCount || 0 }}
              </button>
              <button
                @click="toggleComments(post)"
                class="flex-1 flex items-center justify-center gap-1.5 py-1.5 text-xs font-medium text-gray-400 hover:text-gray-600 transition cursor-pointer"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/></svg>
                {{ post.commentCount || 0 }}
              </button>
            </div>

            <!-- Comments section -->
            <div v-if="expandedComments === post.id" class="mt-3 border-t border-[var(--soc-divider,#f0f0f0)] pt-3">
              <div v-if="loadingComments" class="flex justify-center py-3">
                <div class="h-4 w-4 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
              </div>
              <div v-else>
                <div v-for="comment in postComments" :key="comment.id" class="mb-3 flex gap-2">
                  <div class="flex h-7 w-7 flex-shrink-0 items-center justify-center overflow-hidden rounded-full text-[9px] font-bold text-white" :style="{ background: comment.authorAvatarColor || '#1a1a1a' }">
                    <img
                      v-if="avatarRegistry.getAvatar(comment.authorMemberId, comment.authorProfileImageUrl)"
                      :src="avatarRegistry.getAvatar(comment.authorMemberId, comment.authorProfileImageUrl)!"
                      :alt="comment.authorName"
                      class="h-full w-full object-cover"
                    />
                    <span v-else>{{ getInitials(comment.authorName) }}</span>
                  </div>
                  <div class="flex-1">
                    <div class="rounded-lg bg-gray-50 px-3 py-2">
                      <div class="flex items-center justify-between">
                        <p class="text-xs font-semibold text-gray-900">{{ comment.authorName }}</p>
                        <button
                          v-if="isAdmin || comment.authorMemberId === myMemberId"
                          @click="removeComment(comment.id)"
                          class="soc-header__icon-btn soc-header__icon-btn--logout !w-5 !h-5 !rounded-md"
                        >
                          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/></svg>
                        </button>
                      </div>
                      <p class="text-xs text-gray-700">{{ comment.content }}</p>
                    </div>
                    <p class="mt-0.5 text-[10px] text-gray-400">{{ formatDate(comment.created) }}</p>
                  </div>
                </div>
                <p v-if="postComments.length === 0" class="text-center text-xs text-gray-400 py-2">Aucun commentaire</p>
              </div>
              <!-- Add comment -->
              <div class="mt-2 flex gap-2">
                <input
                  v-model="newComment"
                  type="text"
                  class="flex-1 rounded-full border border-gray-200 px-3 py-1.5 text-xs focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
                  placeholder="Écrire un commentaire..."
                  @keyup.enter="submitComment(post)"
                />
                <button
                  @click="submitComment(post)"
                  :disabled="!newComment.trim() || submittingComment"
                  class="btn-publish flex items-center justify-center rounded-full bg-[#1a1a1a] w-7 h-7 text-white disabled:opacity-50 cursor-pointer flex-shrink-0"
                >
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 2L11 13"/><path d="M22 2l-7 20-4-9-9-4 20-7z"/></svg>
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Load more spinner -->
        <div v-if="loadingMorePosts" class="flex justify-center py-4">
          <div class="h-5 w-5 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
        </div>

    </div>

    <!-- Delete confirmation modal -->
    <div v-if="deleteTarget" class="fixed inset-0 z-[9999] flex items-center justify-center bg-black/50 backdrop-blur-sm p-5" @click.self="deleteTarget = null">
      <div class="w-full max-w-[380px] rounded-2xl bg-white p-8 pt-7 text-center shadow-2xl">
        <div class="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-red-50">
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#dc2626" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/></svg>
        </div>
        <h3 class="mb-1 text-base font-bold text-gray-900">Supprimer cette publication?</h3>
        <p class="mb-5 text-sm text-gray-500">Cette publication sera définitivement supprimée.</p>
        <div class="flex gap-3">
          <button @click="deleteTarget = null" class="flex-1 rounded-lg bg-gray-100 px-4 py-2.5 text-sm font-semibold text-gray-700 transition hover:bg-gray-200 cursor-pointer">Annuler</button>
          <button @click="confirmDelete" :disabled="deleting" class="flex-1 rounded-lg bg-red-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-red-700 disabled:opacity-50 cursor-pointer">
            {{ deleting ? 'Suppression...' : 'Supprimer' }}
          </button>
        </div>
      </div>
    </div>

    <CreatePollModal
      :group-id="groupId"
      :open="showPollModal"
      @close="showPollModal = false"
      @created="resetPosts"
    />

    <ConfirmModal
      :open="confirmGroupImageRemove"
      title="Retirer l'image du groupe?"
      message="L'image sera remplacée par le logo par défaut."
      confirm-label="Retirer"
      :danger="true"
      @confirm="handleGroupImageRemove"
      @cancel="confirmGroupImageRemove = false"
    />

    <ImageLightbox
      v-model:open="lightboxOpen"
      :display-url="lightboxDisplayUrl"
      :original-url="lightboxOriginalUrl"
      :content-type="lightboxContentType"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useUserStore } from '@/stores/userStore'
import { useMemberStore } from '@/stores/memberStore'
import { useAvatarRegistryStore } from '@/stores/avatarRegistryStore'
import { useSocialToast } from '@/composables/useSocialToast'
import { Role } from '@/types/enums'
import type { Post } from '@/types/entities'
import CreatePollModal from '@/components/social/CreatePollModal.vue'
import PollCard from '@/components/social/PollCard.vue'
import AvatarUploader from '@/components/social/AvatarUploader.vue'
import ConfirmModal from '@/components/social/ConfirmModal.vue'
import ImageLightbox from '@/components/social/ImageLightbox.vue'
import { useImageAttachment } from '@/composables/useImageAttachment'
import { useInfiniteScroll } from '@/composables/useInfiniteScroll'

const route = useRoute()
const socialService = useSocialService()
const toast = useSocialToast()
const userStore = useUserStore()
const memberStore = useMemberStore()
const avatarRegistry = useAvatarRegistryStore()

const isAdmin = computed(() => userStore.hasRole(Role.Admin))
const canCreatePolls = computed(() => userStore.hasOneOfTheseRoles([Role.Professor, Role.Admin]))
const canEditGroupImage = computed(() => userStore.hasOneOfTheseRoles([Role.Professor, Role.Admin]))
const myMemberId = computed(() => memberStore.member?.id || '')
const groupId = computed(() => route.params.id as string)
const showPollModal = ref(false)
const uploadingGroupImage = ref(false)
const confirmGroupImageRemove = ref(false)

const group = ref<any>(null)
const feedContainer = ref<HTMLElement | null>(null)
const newPostContent = ref('')
const submittingPost = ref(false)

const attachment = useImageAttachment({ mode: 'multi', maxFiles: 10 })
const fileInputRef = ref<HTMLInputElement | null>(null)

const {
  items: posts,
  loading: loadingPosts,
  loadingMore: loadingMorePosts,
  hasMore: hasMorePosts,
  load: loadPosts,
  refreshFirst: refreshPostsFirst,
  reset: resetPosts,
  attachScroll: attachFeedScroll,
} = useInfiniteScroll<Post>({
  fetchFn: (page) => socialService.getGroupFeed(groupId.value, page),
  scrollContainer: feedContainer,
  direction: 'down',
  threshold: 300,
})

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

// Delete state
const deleteTarget = ref<Post | null>(null)
const deleting = ref(false)

// Comments state
const expandedComments = ref<string | null>(null)
const postComments = ref<any[]>([])
const newComment = ref('')
const loadingComments = ref(false)
const submittingComment = ref(false)

const myAvatarColor = computed(() => memberStore.member.avatarColor || '#1a1a1a')

const userInitials = computed(() => {
  const m = memberStore.member
  if (m?.firstName) return getInitials(`${m.firstName} ${m.lastName || ''}`)
  const user = userStore.user
  if (!user?.email) return '?'
  return user.email.charAt(0).toUpperCase()
})

function getInitials(name: string) {
  if (!name || !name.trim()) return '??'
  return name.split(' ').filter(n => n.length > 0).map(n => n[0]).join('').toUpperCase().slice(0, 2)
}

function formatDate(dateStr: string) {
  const date = new Date(dateStr)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMin = Math.floor(diffMs / 60000)
  if (diffMin < 1) return "à l'instant"
  if (diffMin < 60) return `il y a ${diffMin} min`
  const diffH = Math.floor(diffMin / 60)
  if (diffH < 24) return `il y a ${diffH}h`
  const diffD = Math.floor(diffH / 24)
  if (diffD < 7) return `il y a ${diffD}j`
  return date.toLocaleDateString('fr-CA')
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    await socialService.deletePost(deleteTarget.value.id)
    deleteTarget.value = null
    toast.success('Publication supprimée.')
    await resetPosts()
  } catch {
    toast.error('Erreur lors de la suppression.')
  }
  deleting.value = false
}

async function loadGroup() {
  try {
    group.value = await socialService.getGroupDetails(groupId.value)
    if (group.value?.name) {
      document.title = `EDB Social - ${group.value.name}`
    }
  } catch { /* */ }
}

async function handleGroupImageUpload(file: File) {
  if (uploadingGroupImage.value) return
  uploadingGroupImage.value = true
  try {
    const uploaded = await socialService.uploadFile(file)
    if (!uploaded.succeeded || !uploaded.displayUrl) {
      toast.error("Échec du téléversement de l'image.")
      return
    }
    const result = await socialService.setGroupImage(groupId.value, uploaded.displayUrl)
    if (result.succeeded) {
      await loadGroup()
      toast.success('Image du groupe mise à jour.')
    } else {
      toast.error("Impossible d'enregistrer l'image.")
    }
  } catch {
    toast.error('Erreur lors du téléversement.')
  } finally {
    uploadingGroupImage.value = false
  }
}

async function handleGroupImageRemove() {
  confirmGroupImageRemove.value = false
  try {
    const result = await socialService.removeGroupImage(groupId.value)
    if (result.succeeded) {
      await loadGroup()
      toast.success('Image du groupe retirée.')
    } else {
      toast.error("Impossible de retirer l'image.")
    }
  } catch {
    toast.error('Erreur lors du retrait.')
  }
}

async function submitPost() {
  const text = newPostContent.value.trim()
  const files = attachment.files.value
  if (!text && files.length === 0) return

  submittingPost.value = true
  try {
    let media: Array<{
      displayUrl: string
      thumbnailUrl: string
      originalUrl: string
      contentType: string
      size: number
    }> = []

    if (files.length > 0) {
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
    }

    const type = media.length > 0 ? 'Photo' : 'Text'
    await socialService.createPost(groupId.value, text, type, media.length > 0 ? media : undefined)
    newPostContent.value = ''
    attachment.clear()
    await resetPosts()
  } catch {
    toast.error("Impossible de publier. Veuillez réessayer.")
  }
  submittingPost.value = false
}

async function toggleLike(post: Post) {
  try {
    await socialService.toggleLike(post.id)
    post.hasLiked = !post.hasLiked
    post.likeCount += post.hasLiked ? 1 : -1
  } catch { /* */ }
}

// Comment methods
async function toggleComments(post: Post) {
  if (expandedComments.value === post.id) {
    expandedComments.value = null
    return
  }
  expandedComments.value = post.id
  loadingComments.value = true
  try {
    const result = await socialService.getComments(post.id)
    postComments.value = result.items
    avatarRegistry.populateFromList(postComments.value, 'authorMemberId', 'authorProfileImageUrl')
  } catch { postComments.value = [] }
  loadingComments.value = false
}

async function submitComment(post: Post) {
  if (!newComment.value.trim()) return
  submittingComment.value = true
  try {
    await socialService.addComment(post.id, newComment.value)
    newComment.value = ''
    const result = await socialService.getComments(post.id)
    postComments.value = result.items
    post.commentCount = (post.commentCount || 0) + 1
  } catch { /* */ }
  submittingComment.value = false
}

async function removeComment(commentId: string) {
  try {
    await socialService.deleteComment(commentId)
    postComments.value = postComments.value.filter(c => c.id !== commentId)
    const post = posts.value.find(p => p.id === expandedComments.value)
    if (post) post.commentCount = Math.max(0, (post.commentCount || 0) - 1)
    toast.success('Commentaire supprimé.')
  } catch {
    toast.error('Erreur lors de la suppression.')
  }
}

let pollInterval: ReturnType<typeof setInterval> | null = null

onMounted(async () => {
  await loadGroup()
  await loadPosts()
  avatarRegistry.populateFromList(posts.value as any[], 'authorMemberId', 'authorProfileImageUrl')
  nextTick(() => attachFeedScroll())
  pollInterval = setInterval(async () => {
    if (expandedComments.value || submittingComment.value) return
    try {
      const freshPage1 = await refreshPostsFirst()
      avatarRegistry.populateFromList(freshPage1 as any[], 'authorMemberId', 'authorProfileImageUrl')
    } catch { /* */ }
  }, 2000)
})

onUnmounted(() => {
  if (pollInterval) clearInterval(pollInterval)
})
</script>

<style scoped>
.soc-composer-icon {
  color: var(--soc-text-muted);
}
.soc-composer-icon:hover {
  background: var(--soc-bar-hover);
  color: var(--soc-text);
}
</style>
