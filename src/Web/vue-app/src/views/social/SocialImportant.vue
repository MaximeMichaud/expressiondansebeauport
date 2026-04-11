<template>
  <div class="p-4">
    <!-- Header -->
    <div class="mb-4 flex items-center justify-between">
      <h2 class="text-lg font-bold text-gray-900">Annonces</h2>
      <button v-if="isAdmin" @click="showCreate = !showCreate" class="rounded-lg border border-[rgba(21,128,61,0.15)] bg-[rgba(21,128,61,0.06)] px-3 py-1.5 text-xs font-semibold text-[#15803d] transition hover:bg-[rgba(21,128,61,0.12)] cursor-pointer">
        {{ showCreate ? 'Fermer' : '+ Nouvelle annonce' }}
      </button>
    </div>

    <!-- Create form -->
    <div v-if="isAdmin && showCreate" class="mb-6 rounded-xl border border-gray-200 bg-gray-50 p-4">
      <h3 class="mb-3 text-sm font-semibold text-gray-700">Nouvelle annonce</h3>
      <div class="space-y-3">
        <input
          v-model="newTitle"
          type="text"
          class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm font-semibold focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
          placeholder="Titre de l'annonce"
        />
        <textarea
          v-model="newDescription"
          rows="3"
          class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
          placeholder="Description (optionnel)"
        ></textarea>
      </div>
      <div class="mt-3 flex justify-end">
        <button
          @click="createAnnouncement"
          :disabled="!newTitle.trim() || creating"
          class="btn-publish rounded-lg bg-[#1a1a1a] px-4 py-2 text-sm font-semibold text-white transition hover:bg-[#000] disabled:opacity-50 cursor-pointer"
        >
          {{ creating ? 'Publication...' : 'Publier' }}
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-10">
      <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
    </div>

    <!-- Empty -->
    <div v-else-if="announcements.length === 0" class="flex flex-col items-center justify-center gap-3 py-20 text-gray-400">
      <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M18 8A6 6 0 006 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 01-3.46 0"/></svg>
      <span class="text-sm">Aucune annonce pour le moment.</span>
    </div>

    <!-- List -->
    <div v-else class="space-y-3">
      <div
        v-for="post in announcements"
        :key="post.id"
        class="rounded-xl border border-gray-200 overflow-hidden"
      >
        <div class="p-4">
          <router-link :to="{ name: 'socialAnnouncement', params: { id: post.id } }" class="flex items-center justify-between gap-3 cursor-pointer">
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-gray-900">{{ getTitle(post.content) }}</p>
              <div class="mt-2 flex items-center gap-4 text-[11px] text-gray-400">
                <span>{{ post.authorName }}</span>
                <span>{{ formatDate(post.created) }}</span>
              </div>
            </div>
            <div class="flex items-center gap-1 flex-shrink-0">
              <span class="text-[11px] font-medium text-gray-400">Voir plus</span>
              <svg class="text-gray-300" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 18l6-6-6-6"/></svg>
            </div>
          </router-link>

          <!-- Actions -->
          <div class="mt-3 flex border-t border-gray-100 pt-2">
            <button
              @click="toggleLike(post)"
              :class="['flex-1 flex items-center justify-center gap-1.5 py-1.5 text-xs font-medium transition cursor-pointer', post.hasLiked ? 'text-red-600' : 'text-gray-400 hover:text-gray-600']"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" :fill="post.hasLiked ? 'currentColor' : 'none'" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 00-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 00-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 000-7.78z"/></svg>
              {{ post.likeCount || 0 }}
            </button>
            <button
              @click="toggleComments(post)"
              :class="['flex-1 flex items-center justify-center gap-1.5 py-1.5 text-xs font-medium transition cursor-pointer', expandedComments === post.id ? 'text-gray-700' : 'text-gray-400 hover:text-gray-600']"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/></svg>
              {{ post.commentCount || 0 }}
            </button>
          </div>
        </div>

        <!-- Comments section -->
        <div v-if="expandedComments === post.id" class="border-t border-gray-100 bg-gray-50 px-4 py-3">
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
                <div class="rounded-lg bg-white px-3 py-2">
                  <div class="flex items-center justify-between">
                    <p class="text-xs font-semibold text-gray-900">{{ comment.authorName }}</p>
                    <button
                      v-if="isAdmin || comment.authorMemberId === myMemberId"
                      @click="deleteComment(comment.id, post)"
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
              ref="commentInputRef"
              class="flex-1 rounded-full border border-gray-200 bg-white px-3 py-1.5 text-xs focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
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

    <!-- Delete modal -->
    <Teleport to="body">
      <Transition name="ann-modal">
        <div v-if="deleteTarget" class="ann-modal__overlay" @click.self="deleteTarget = null">
          <div class="ann-modal__card">
            <div class="ann-modal__icon-ring">
              <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#dc2626" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/>
              </svg>
            </div>
            <h3 class="ann-modal__title">Supprimer cette annonce?</h3>
            <p class="ann-modal__text">Cette annonce sera définitivement supprimée.</p>
            <div class="ann-modal__actions">
              <button @click="deleteTarget = null" class="ann-modal__btn ann-modal__btn--cancel">Annuler</button>
              <button @click="confirmDelete" :disabled="deleting" class="ann-modal__btn ann-modal__btn--danger">
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
import { useSocialService } from '@/inversify.config'
import { useSocialToast } from '@/composables/useSocialToast'
import { useUserStore } from '@/stores/userStore'
import { useMemberStore } from '@/stores/memberStore'
import { useAvatarRegistryStore } from '@/stores/avatarRegistryStore'
import { Role } from '@/types/enums'
import type { Post } from '@/types/entities'

const socialService = useSocialService()
const toast = useSocialToast()
const userStore = useUserStore()
const memberStore = useMemberStore()
const avatarRegistry = useAvatarRegistryStore()

const isAdmin = computed(() => userStore.hasRole(Role.Admin))
const myMemberId = computed(() => memberStore.member?.id || '')
const announcements = ref<Post[]>([])
const loading = ref(true)
const showCreate = ref(false)
const newTitle = ref('')
const newDescription = ref('')
const creating = ref(false)
const deleteTarget = ref<Post | null>(null)
const deleting = ref(false)

// Comments
const expandedComments = ref<string | null>(null)
const postComments = ref<any[]>([])
const newComment = ref('')
const loadingComments = ref(false)
const submittingComment = ref(false)
const commentInputRef = ref<HTMLInputElement | null>(null)

function getTitle(content: string) {
  return content?.split('\n')[0] || ''
}

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
  return date.toLocaleDateString('fr-CA', { year: 'numeric', month: 'short', day: 'numeric' })
}

let likeDebounce = 0

async function toggleLike(post: Post) {
  try {
    likeDebounce = Date.now()
    await socialService.toggleLike(post.id)
    post.hasLiked = !post.hasLiked
    post.likeCount += post.hasLiked ? 1 : -1
  } catch { /* */ }
}

async function toggleComments(post: Post) {
  if (expandedComments.value === post.id) {
    expandedComments.value = null
    return
  }
  expandedComments.value = post.id
  loadingComments.value = true
  try {
    postComments.value = await socialService.getComments(post.id)
    avatarRegistry.populateFromList(postComments.value, 'authorMemberId', 'authorProfileImageUrl')
  } catch { postComments.value = [] }
  loadingComments.value = false
  await nextTick()
  if (commentInputRef.value instanceof HTMLElement) commentInputRef.value.focus()
}

async function submitComment(post: Post) {
  if (!newComment.value.trim()) return
  submittingComment.value = true
  try {
    await socialService.addComment(post.id, newComment.value)
    newComment.value = ''
    postComments.value = await socialService.getComments(post.id)
    post.commentCount = (post.commentCount || 0) + 1
  } catch { /* */ }
  submittingComment.value = false
}

async function deleteComment(commentId: string, post: Post) {
  try {
    await socialService.deleteComment(commentId)
    postComments.value = postComments.value.filter(c => c.id !== commentId)
    post.commentCount = Math.max(0, (post.commentCount || 0) - 1)
    toast.success('Commentaire supprimé.')
  } catch {
    toast.error('Erreur lors de la suppression.')
  }
}

async function loadAnnouncements() {
  try {
    announcements.value = await socialService.getAnnouncements()
    avatarRegistry.populateFromList(announcements.value as any[], 'authorMemberId', 'authorProfileImageUrl')
  } catch { /* */ }
  loading.value = false
}

async function createAnnouncement() {
  if (!newTitle.value.trim()) return
  creating.value = true
  const content = newDescription.value.trim()
    ? newTitle.value.trim() + '\n' + newDescription.value.trim()
    : newTitle.value.trim()
  try {
    const result = await socialService.createAnnouncement(content)
    if (result.succeeded) {
      toast.success('Annonce publiée!')
      newTitle.value = ''
      newDescription.value = ''
      showCreate.value = false
      await loadAnnouncements()
    } else {
      toast.error('Erreur lors de la publication.')
    }
  } catch {
    toast.error('Erreur lors de la publication.')
  }
  creating.value = false
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    await socialService.deletePost(deleteTarget.value.id)
    deleteTarget.value = null
    toast.success('Annonce supprimée.')
    await loadAnnouncements()
  } catch {
    toast.error('Erreur lors de la suppression.')
  }
  deleting.value = false
}

let pollInterval: ReturnType<typeof setInterval> | null = null

onMounted(() => {
  loadAnnouncements()
  pollInterval = setInterval(async () => {
    if (Date.now() - likeDebounce < 3000) return
    try { announcements.value = await socialService.getAnnouncements() } catch { /* */ }
  }, 30000)
})

onUnmounted(() => {
  if (pollInterval) clearInterval(pollInterval)
})
</script>

<style lang="scss">
$ann-font-display: 'Montserrat', sans-serif;

.ann-modal {
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
    font-family: $ann-font-display;
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
    font-family: $ann-font-display;
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

.ann-modal-enter-active { transition: all 0.2s ease; }
.ann-modal-leave-active { transition: all 0.15s ease; }
.ann-modal-enter-from { opacity: 0; }
.ann-modal-leave-to { opacity: 0; }
</style>
