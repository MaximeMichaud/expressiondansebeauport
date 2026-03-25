<template>
  <div class="flex min-h-[calc(100vh-120px)] flex-col">
    <!-- Group header -->
    <div class="flex items-center gap-3 bg-[#1a1a1a] px-4 py-3">
      <button @click="$router.push({ name: 'socialPortal' })" class="text-white">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
        </svg>
      </button>
      <div class="flex h-8 w-8 items-center justify-center rounded-full bg-gray-700">
        <span class="text-[8px] font-bold text-white">EDB</span>
      </div>
      <h1 class="text-base font-semibold text-white">{{ group?.name || 'Groupe' }}</h1>
    </div>

    <!-- Tabs -->
    <div class="flex flex-shrink-0 gap-2 overflow-x-auto px-4 py-3">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        @click="activeTab = tab.id"
        :class="[
          'whitespace-nowrap rounded-full px-4 py-1.5 text-xs font-semibold transition',
          activeTab === tab.id ? 'bg-gray-900 text-white' : 'bg-gray-200 text-gray-600'
        ]"
      >
        {{ tab.label }}
      </button>
    </div>

    <!-- Tab content -->
    <div class="flex-1">
      <!-- Feed tab -->
      <div v-if="activeTab === 'feed'">
        <!-- Post composer -->
        <div class="border-b-8 border-gray-100 px-4 py-3">
          <div class="flex items-start gap-3">
            <div v-if="!showPollComposer && !showPhotoComposer" class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full text-xs font-bold text-white" :style="{ background: myAvatarColor }">
              {{ userInitials }}
            </div>
            <div class="flex-1">
              <!-- Default text mode -->
              <template v-if="!showPollComposer && !showPhotoComposer">
                <textarea
                  v-model="newPostContent"
                  rows="2"
                  class="w-full resize-none rounded-lg border-0 bg-gray-100 px-3 py-2 text-sm placeholder-gray-400 focus:bg-white focus:ring-1 focus:ring-[#1a1a1a]"
                  placeholder="Partager quelque chose..."
                ></textarea>
                <div class="mt-2 flex items-center justify-between">
                  <div class="flex gap-2 text-gray-400">
                    <button @click="showPhotoComposer = true" class="hover:text-[#1a1a1a] transition cursor-pointer" title="Photo">
                      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="8.5" cy="8.5" r="1.5"/><path d="M21 15l-5-5L5 21"/></svg>
                    </button>
                    <button @click="showPollComposer = true" class="hover:text-[#1a1a1a] transition cursor-pointer" title="Sondage">
                      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="12" width="4" height="9" rx="1"/><rect x="10" y="7" width="4" height="14" rx="1"/><rect x="17" y="3" width="4" height="18" rx="1"/></svg>
                    </button>
                  </div>
                  <button
                    @click="submitPost"
                    :disabled="!newPostContent.trim() || submittingPost"
                    class="rounded-lg bg-[#1a1a1a] px-4 py-2 text-sm font-semibold text-white transition hover:bg-[#000] disabled:opacity-50 cursor-pointer"
                  >
                    Publier
                  </button>
                </div>
              </template>

              <!-- Poll composer mode -->
              <template v-if="showPollComposer">
                <div class="space-y-3">
                  <div class="flex items-center justify-between">
                    <span class="text-sm font-semibold text-gray-700">Nouveau sondage</span>
                    <button @click="cancelPoll" class="text-xs text-gray-500 hover:text-gray-700">Annuler</button>
                  </div>
                  <input
                    v-model="pollQuestion"
                    type="text"
                    class="w-full rounded-lg border-0 bg-gray-100 px-3 py-2 text-sm placeholder-gray-400 focus:bg-white focus:ring-1 focus:ring-[#1a1a1a]"
                    placeholder="Posez votre question..."
                  />
                  <div class="space-y-2">
                    <div v-for="(_, index) in pollOptions" :key="index" class="flex items-center gap-2">
                      <input
                        v-model="pollOptions[index]"
                        type="text"
                        class="flex-1 rounded-lg border-0 bg-gray-100 px-3 py-2 text-sm placeholder-gray-400 focus:bg-white focus:ring-1 focus:ring-[#1a1a1a]"
                        :placeholder="`Option ${index + 1}`"
                      />
                      <button
                        v-if="pollOptions.length > 2"
                        @click="pollOptions.splice(index, 1)"
                        class="text-gray-400 hover:text-red-500 text-sm"
                      >
                        &times;
                      </button>
                    </div>
                  </div>
                  <button
                    v-if="pollOptions.length < 6"
                    @click="pollOptions.push('')"
                    class="text-xs text-[#1a1a1a] hover:underline"
                  >
                    + Ajouter une option
                  </button>
                  <label class="flex items-center gap-2 text-xs text-gray-600">
                    <input v-model="pollAllowMultiple" type="checkbox" class="rounded border-gray-300 text-[#1a1a1a] focus:ring-[#1a1a1a]" />
                    Autoriser les réponses multiples
                  </label>
                  <button
                    @click="submitPoll"
                    :disabled="!pollQuestion.trim() || pollOptions.filter(o => o.trim()).length < 2 || submittingPost"
                    class="w-full rounded-lg bg-[#1a1a1a] px-3 py-2 text-xs font-semibold text-white disabled:opacity-50"
                  >
                    Publier le sondage
                  </button>
                </div>
              </template>

              <!-- Photo composer mode -->
              <template v-if="showPhotoComposer">
                <div class="space-y-3">
                  <div class="flex items-center justify-between">
                    <span class="text-sm font-semibold text-gray-700">Publier avec photo</span>
                    <button @click="cancelPhoto" class="text-xs text-gray-500 hover:text-gray-700">Annuler</button>
                  </div>
                  <textarea
                    v-model="newPostContent"
                    rows="2"
                    class="w-full resize-none rounded-lg border-0 bg-gray-100 px-3 py-2 text-sm placeholder-gray-400 focus:bg-white focus:ring-1 focus:ring-[#1a1a1a]"
                    placeholder="Ajouter une description..."
                  ></textarea>
                  <input
                    ref="photoInput"
                    type="file"
                    accept="image/*"
                    multiple
                    class="hidden"
                    @change="onPhotoSelected"
                  />
                  <button
                    @click="($refs.photoInput as HTMLInputElement)?.click()"
                    class="flex items-center gap-2 rounded-lg border border-dashed border-gray-300 px-4 py-3 text-sm text-gray-500 hover:border-[#1a1a1a] hover:text-[#1a1a1a] transition w-full justify-center"
                  >
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                    Choisir des photos
                  </button>
                  <!-- Image previews -->
                  <div v-if="photoPreviews.length" class="grid grid-cols-3 gap-2">
                    <div v-for="(preview, index) in photoPreviews" :key="index" class="relative">
                      <img :src="preview" class="h-24 w-full rounded-lg object-cover" />
                      <button
                        @click="removePhoto(index)"
                        class="absolute -right-1 -top-1 flex h-5 w-5 items-center justify-center rounded-full bg-red-500 text-xs text-white"
                      >
                        &times;
                      </button>
                    </div>
                  </div>
                  <!-- TODO: Actual upload to POST /api/social/upload not yet implemented on backend -->
                  <button
                    @click="submitPhotoPost"
                    :disabled="!newPostContent.trim() && !photoFiles.length || submittingPost"
                    class="w-full rounded-lg bg-[#1a1a1a] px-3 py-2 text-xs font-semibold text-white disabled:opacity-50"
                  >
                    Publier
                  </button>
                </div>
              </template>
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
          <div v-for="post in posts" :key="post.id" class="border-b-8 border-gray-100 px-4 py-4">
            <!-- Author info -->
            <div class="mb-3 flex items-center gap-2.5">
              <div class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full text-xs font-bold text-white" :style="{ background: post.authorAvatarColor || '#1a1a1a' }">
                {{ getInitials(post.authorName) }}
              </div>
              <div class="flex-1">
                <p class="text-sm font-semibold text-gray-900">{{ post.authorName }}</p>
                <p class="text-xs text-gray-500">{{ formatDate(post.created) }}</p>
              </div>
              <span v-if="post.isPinned" class="rounded bg-amber-100 px-2 py-0.5 text-[10px] font-medium text-amber-700">
                Épinglé
              </span>
            </div>

            <!-- Content -->
            <p class="mb-3 whitespace-pre-wrap text-sm leading-relaxed text-gray-800">{{ post.content }}</p>

            <!-- Media -->
            <div v-if="post.media && post.media.length" class="mb-3 grid gap-1" :class="post.media.length > 1 ? 'grid-cols-2' : ''">
              <img v-for="media in post.media" :key="media.id" :src="media.thumbnailUrl || media.mediaUrl" class="w-full rounded-lg object-cover" />
            </div>

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
                class="flex-1 flex items-center justify-center gap-1.5 py-1.5 text-xs font-medium text-gray-400 hover:text-gray-600 transition cursor-pointer"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/></svg>
                {{ post.commentCount || 0 }}
              </button>
            </div>

            <!-- Comments section -->
            <div v-if="expandedComments === post.id" class="mt-3 border-t border-gray-100 pt-3">
              <div v-if="loadingComments" class="flex justify-center py-3">
                <div class="h-4 w-4 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
              </div>
              <div v-else>
                <div v-for="comment in postComments" :key="comment.id" class="mb-3 flex gap-2">
                  <div class="flex h-7 w-7 flex-shrink-0 items-center justify-center rounded-full text-[9px] font-bold text-white" :style="{ background: comment.authorAvatarColor || '#1a1a1a' }">
                    {{ getInitials(comment.authorName) }}
                  </div>
                  <div class="flex-1">
                    <div class="rounded-lg bg-gray-50 px-3 py-2">
                      <p class="text-xs font-semibold text-gray-900">{{ comment.authorName }}</p>
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
                  class="flex items-center justify-center rounded-full bg-[#1a1a1a] w-7 h-7 text-white disabled:opacity-50 cursor-pointer flex-shrink-0"
                >
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 2L11 13"/><path d="M22 2l-7 20-4-9-9-4 20-7z"/></svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Members tab -->
      <div v-else-if="activeTab === 'members'" class="p-4">
        <div v-if="loadingMembers" class="flex justify-center py-10">
          <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
        </div>
        <div v-else class="space-y-3">
          <div v-for="gm in groupMembers" :key="gm.id" class="flex items-center gap-3">
            <div class="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-full text-sm font-bold text-white" :style="{ background: gm.avatarColor || getAvatarColor(gm.fullName) }">
              {{ getInitials(gm.fullName) }}
            </div>
            <div class="flex-1">
              <p class="text-sm font-medium text-gray-900">{{ gm.fullName }}</p>
            </div>
            <button
              @click="startConversationWith(gm)"
              :disabled="startingConversation === gm.id"
              class="rounded-lg border border-gray-200 px-3 py-1 text-xs font-medium text-gray-600 transition hover:border-[#1a1a1a] hover:text-[#1a1a1a] disabled:opacity-50"
            >
              {{ startingConversation === gm.id ? '...' : 'Message' }}
            </button>
          </div>
        </div>
      </div>

      <!-- About tab -->
      <div v-else-if="activeTab === 'about'" class="p-4">
        <h3 class="mb-2 font-semibold text-gray-900">{{ group?.name }}</h3>
        <p v-if="group?.description" class="mb-4 text-sm text-gray-600">{{ group.description }}</p>
        <p class="text-sm text-gray-500">Saison : {{ group?.season }}</p>
        <div v-if="group?.inviteCode" class="mt-4 rounded-lg border border-gray-200 p-3">
          <p class="text-xs font-medium text-gray-500">Code d'invitation</p>
          <p class="mt-1 text-lg font-bold text-gray-900">{{ group.inviteCode }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useUserStore } from '@/stores/userStore'
import { useMemberStore } from '@/stores/memberStore'
import type { Post, GroupMember } from '@/types/entities'

const route = useRoute()
const router = useRouter()
const socialService = useSocialService()
const userStore = useUserStore()
const memberStore = useMemberStore()

const groupId = computed(() => route.params.id as string)

const group = ref<any>(null)
const posts = ref<Post[]>([])
const groupMembers = ref<GroupMember[]>([])
const activeTab = ref('feed')
const loadingPosts = ref(true)
const loadingMembers = ref(false)
const newPostContent = ref('')
const submittingPost = ref(false)

// Poll composer state
const showPollComposer = ref(false)
const pollQuestion = ref('')
const pollOptions = ref<string[]>(['', ''])
const pollAllowMultiple = ref(false)

// Photo composer state
const showPhotoComposer = ref(false)
const photoFiles = ref<File[]>([])
const photoPreviews = ref<string[]>([])
const photoInput = ref<HTMLInputElement | null>(null)

// Comments state
const expandedComments = ref<string | null>(null)
const postComments = ref<any[]>([])
const newComment = ref('')
const loadingComments = ref(false)
const submittingComment = ref(false)

// Message from member list
const startingConversation = ref<string | null>(null)

const tabs = [
  { id: 'feed', label: 'Fil' },
  { id: 'members', label: 'Membres' },
  { id: 'about', label: 'À propos' },
]

const avatarColors = ['#e53e3e', '#dd6b20', '#d69e2e', '#38a169', '#319795', '#3182ce', '#5a67d8', '#805ad5', '#d53f8c', '#e53e3e']
function getAvatarColor(name: string) {
  let hash = 0
  for (let i = 0; i < (name?.length || 0); i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

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

async function loadGroup() {
  try {
    group.value = await socialService.getGroupDetails(groupId.value)
  } catch (e) { /* */ }
}

async function loadPosts() {
  loadingPosts.value = true
  try {
    posts.value = await socialService.getGroupFeed(groupId.value)
  } catch (e) { /* */ }
  loadingPosts.value = false
}

async function loadMembers() {
  loadingMembers.value = true
  try {
    groupMembers.value = await socialService.getGroupMembers(groupId.value)
  } catch (e) { /* */ }
  loadingMembers.value = false
}

async function submitPost() {
  if (!newPostContent.value.trim()) return
  submittingPost.value = true
  try {
    await socialService.createPost(groupId.value, newPostContent.value)
    newPostContent.value = ''
    await loadPosts()
  } catch (e) { /* */ }
  submittingPost.value = false
}

async function toggleLike(post: Post) {
  try {
    await socialService.toggleLike(post.id)
    post.hasLiked = !post.hasLiked
    post.likeCount += post.hasLiked ? 1 : -1
  } catch (e) { /* */ }
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
    postComments.value = await socialService.getComments(post.id)
  } catch { postComments.value = [] }
  loadingComments.value = false
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

// Poll methods
function cancelPoll() {
  showPollComposer.value = false
  pollQuestion.value = ''
  pollOptions.value = ['', '']
  pollAllowMultiple.value = false
}

async function submitPoll() {
  const validOptions = pollOptions.value.filter(o => o.trim())
  if (!pollQuestion.value.trim() || validOptions.length < 2) return
  submittingPost.value = true
  try {
    // Send poll data as JSON in content since backend PostService.CreatePost
    // doesn't handle poll creation yet. The question is the post content with type 'Poll'.
    const pollData = JSON.stringify({
      question: pollQuestion.value,
      options: validOptions,
      allowMultiple: pollAllowMultiple.value,
    })
    await socialService.createPost(groupId.value, pollData, 'Poll')
    cancelPoll()
    await loadPosts()
  } catch (e) { /* */ }
  submittingPost.value = false
}

// Photo methods
function cancelPhoto() {
  showPhotoComposer.value = false
  newPostContent.value = ''
  photoFiles.value = []
  photoPreviews.value = []
}

function onPhotoSelected(event: Event) {
  const input = event.target as HTMLInputElement
  if (!input.files) return
  for (const file of Array.from(input.files)) {
    photoFiles.value.push(file)
    const reader = new FileReader()
    reader.onload = (e) => {
      photoPreviews.value.push(e.target?.result as string)
    }
    reader.readAsDataURL(file)
  }
  // Reset input so the same file can be selected again
  input.value = ''
}

function removePhoto(index: number) {
  photoFiles.value.splice(index, 1)
  photoPreviews.value.splice(index, 1)
}

async function submitPhotoPost() {
  if (!newPostContent.value.trim() && !photoFiles.value.length) return
  submittingPost.value = true
  try {
    // TODO: Upload photos to POST /api/social/upload when backend endpoint is ready.
    // For now, create a text post with the description.
    await socialService.createPost(groupId.value, newPostContent.value || '(Photo)')
    cancelPhoto()
    await loadPosts()
  } catch (e) { /* */ }
  submittingPost.value = false
}

// Start conversation from member list (Task 5)
async function startConversationWith(member: GroupMember) {
  startingConversation.value = member.id
  try {
    const conversation = await socialService.startConversation(member.memberId)
    if (conversation?.id) {
      router.push({ name: 'socialConversation', params: { conversationId: conversation.id } })
    } else {
      router.push({ name: 'socialMessages' })
    }
  } catch (e) { /* */ }
  startingConversation.value = null
}

let pollInterval: ReturnType<typeof setInterval> | null = null

onMounted(async () => {
  await loadGroup()
  await loadPosts()
  loadMembers()
  pollInterval = setInterval(async () => {
    try { posts.value = await socialService.getGroupFeed(groupId.value) } catch { /* */ }
  }, 1000)
})

onUnmounted(() => {
  if (pollInterval) clearInterval(pollInterval)
})
</script>
