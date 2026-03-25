<template>
  <div class="p-4">
    <!-- Back + Delete -->
    <div class="mb-4 flex items-center justify-between">
      <button @click="$router.push({ name: 'socialImportant' })" class="flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700 cursor-pointer">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 19l-7-7 7-7"/></svg>
        Annonces
      </button>
      <button
        v-if="isAdmin"
        @click="showDeleteModal = true"
        class="soc-header__icon-btn soc-header__icon-btn--logout"
        style="width: 30px; height: 30px;"
      >
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/></svg>
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-20">
      <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
    </div>

    <template v-else-if="post">
      <!-- Content -->
      <div class="mb-4">
        <h1 class="text-xl font-bold text-gray-900" style="font-family: 'Montserrat', sans-serif;">{{ getTitle(post.content) }}</h1>
        <div class="mt-2 flex items-center gap-3 text-xs text-gray-400">
          <span>{{ post.authorName }}</span>
          <span>{{ formatDate(post.created) }}</span>
        </div>
        <p v-if="getDescription(post.content)" class="mt-4 text-sm leading-relaxed text-gray-700 whitespace-pre-wrap">{{ getDescription(post.content) }}</p>
      </div>

      <!-- Media -->
      <div v-if="post.media && post.media.length" class="mb-4 grid gap-2" :class="post.media.length > 1 ? 'grid-cols-2' : ''">
        <img v-for="media in post.media" :key="media.id" :src="media.thumbnailUrl || media.mediaUrl" class="w-full rounded-lg object-cover" />
      </div>

      <!-- Actions -->
      <div class="mt-3 flex border-t border-gray-100 py-2">
        <button
          @click="toggleLike"
          :class="['flex-1 flex items-center justify-center gap-1.5 py-1.5 text-xs font-medium transition cursor-pointer', post.hasLiked ? 'text-red-600' : 'text-gray-400 hover:text-gray-600']"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" :fill="post.hasLiked ? 'currentColor' : 'none'" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.84 4.61a5.5 5.5 0 00-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 00-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 000-7.78z"/></svg>
          {{ post.likeCount || 0 }}
        </button>
        <button
          @click="focusCommentInput"
          class="flex-1 flex items-center justify-center gap-1.5 py-1.5 text-xs font-medium text-gray-400 hover:text-gray-600 transition cursor-pointer"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/></svg>
          {{ comments.length }}
        </button>
      </div>

      <!-- Comments -->
      <div class="border-t border-gray-100 pt-4">
        <h3 class="mb-3 text-xs font-bold uppercase tracking-wide text-gray-500">Commentaires</h3>

        <div v-if="loadingComments" class="flex justify-center py-4">
          <div class="h-4 w-4 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
        </div>
        <div v-else>
          <div v-for="comment in comments" :key="comment.id" class="mb-3 flex gap-2">
            <div class="flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full text-[10px] font-bold text-white" :style="{ background: comment.authorAvatarColor || '#1a1a1a' }">
              {{ getInitials(comment.authorName) }}
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
          <p v-if="comments.length === 0" class="py-4 text-center text-xs text-gray-400">Aucun commentaire</p>
        </div>

        <!-- Add comment -->
        <div class="mt-3 flex gap-2">
          <input
            ref="commentInputEl"
            v-model="newComment"
            type="text"
            class="flex-1 rounded-full border border-gray-200 px-4 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
            placeholder="Écrire un commentaire..."
            @keyup.enter="submitComment"
          />
          <button
            @click="submitComment"
            :disabled="!newComment.trim() || submitting"
            class="btn-publish flex items-center justify-center rounded-full bg-[#1a1a1a] w-9 h-9 text-white disabled:opacity-50 cursor-pointer flex-shrink-0"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 2L11 13"/><path d="M22 2l-7 20-4-9-9-4 20-7z"/></svg>
          </button>
        </div>
      </div>
    </template>

    <!-- Delete modal -->
    <Teleport to="body">
      <Transition name="ann-d-modal">
        <div v-if="showDeleteModal" class="ann-d-modal__overlay" @click.self="showDeleteModal = false">
          <div class="ann-d-modal__card">
            <div class="ann-d-modal__icon-ring">
              <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#dc2626" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/>
              </svg>
            </div>
            <h3 class="ann-d-modal__title">Supprimer cette annonce?</h3>
            <p class="ann-d-modal__text">Cette annonce et ses commentaires seront supprimés.</p>
            <div class="ann-d-modal__actions">
              <button @click="showDeleteModal = false" class="ann-d-modal__btn ann-d-modal__btn--cancel">Annuler</button>
              <button @click="confirmDelete" :disabled="deleting" class="ann-d-modal__btn ann-d-modal__btn--danger">
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
import { useRoute, useRouter } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useSocialToast } from '@/composables/useSocialToast'
import { useUserStore } from '@/stores/userStore'
import { useMemberStore } from '@/stores/memberStore'
import { Role } from '@/types/enums'
import type { Post } from '@/types/entities'

const route = useRoute()
const router = useRouter()
const socialService = useSocialService()
const toast = useSocialToast()
const userStore = useUserStore()
const memberStore = useMemberStore()

const postId = computed(() => route.params.id as string)
const isAdmin = computed(() => userStore.hasRole(Role.Admin))
const myMemberId = computed(() => memberStore.member?.id || '')

const post = ref<Post | null>(null)
const comments = ref<any[]>([])
const loading = ref(true)
const loadingComments = ref(true)
const newComment = ref('')
const submitting = ref(false)
const showDeleteModal = ref(false)
const deleting = ref(false)
const commentInputEl = ref<HTMLInputElement | null>(null)

function focusCommentInput() {
  nextTick(() => commentInputEl.value?.focus())
}

function getTitle(content: string) {
  return content?.split('\n')[0] || ''
}

function getDescription(content: string) {
  const lines = content?.split('\n')
  return lines?.length > 1 ? lines.slice(1).join('\n').trim() : ''
}

function getInitials(name: string) {
  if (!name || !name.trim()) return '??'
  return name.split(' ').filter(n => n.length > 0).map(n => n[0]).join('').toUpperCase().slice(0, 2)
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString('fr-CA', { year: 'numeric', month: 'short', day: 'numeric' })
}

async function toggleLike() {
  if (!post.value) return
  likeDebounce = Date.now()
  try {
    await socialService.toggleLike(post.value.id)
    post.value.hasLiked = !post.value.hasLiked
    post.value.likeCount += post.value.hasLiked ? 1 : -1
  } catch { /* */ }
}

async function loadComments() {
  loadingComments.value = true
  try {
    comments.value = await socialService.getComments(postId.value)
  } catch { comments.value = [] }
  loadingComments.value = false
}

async function submitComment() {
  if (!newComment.value.trim()) return
  submitting.value = true
  try {
    await socialService.addComment(postId.value, newComment.value)
    newComment.value = ''
    await loadComments()
    if (post.value) post.value.commentCount = comments.value.length
  } catch { /* */ }
  submitting.value = false
}

async function removeComment(commentId: string) {
  try {
    await socialService.deleteComment(commentId)
    comments.value = comments.value.filter(c => c.id !== commentId)
    if (post.value) post.value.commentCount = Math.max(0, (post.value.commentCount || 0) - 1)
    toast.success('Commentaire supprimé.')
  } catch {
    toast.error('Erreur lors de la suppression.')
  }
}

async function confirmDelete() {
  if (!post.value) return
  deleting.value = true
  try {
    await socialService.deletePost(post.value.id)
    showDeleteModal.value = false
    toast.success('Annonce supprimée.')
    await router.push({ name: 'socialImportant' })
  } catch {
    toast.error('Erreur lors de la suppression.')
  }
  deleting.value = false
}

let pollInterval: ReturnType<typeof setInterval> | null = null
let likeDebounce = 0

onMounted(async () => {
  try {
    post.value = await socialService.getPost(postId.value)
  } catch { /* */ }
  loading.value = false
  await loadComments()
  pollInterval = setInterval(async () => {
    try {
      const data = await socialService.getPost(postId.value)
      data.likeCount = data.likeCount ?? 0
      data.hasLiked = data.hasLiked ?? false
      if (post.value && Date.now() - likeDebounce < 5000) {
        data.likeCount = post.value.likeCount
        data.hasLiked = post.value.hasLiked
      }
      post.value = data
      comments.value = await socialService.getComments(postId.value)
    } catch { /* */ }
  }, 30000)
})

onUnmounted(() => {
  if (pollInterval) clearInterval(pollInterval)
})
</script>

<style lang="scss">
$ann-d-font: 'Montserrat', sans-serif;

.ann-d-modal {
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
    font-family: $ann-d-font;
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
    font-family: $ann-d-font;
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
.ann-d-modal-enter-active { transition: all 0.2s ease; }
.ann-d-modal-leave-active { transition: all 0.15s ease; }
.ann-d-modal-enter-from { opacity: 0; }
.ann-d-modal-leave-to { opacity: 0; }
</style>
