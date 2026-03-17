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
        <span class="text-[8px] font-bold text-[#be1e2c]">EDB</span>
      </div>
      <h1 class="text-base font-semibold text-white">{{ group?.name || 'Groupe' }}</h1>
    </div>

    <!-- Tabs -->
    <div class="flex gap-2 overflow-x-auto px-4 py-3">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        @click="activeTab = tab.id"
        :class="[
          'whitespace-nowrap rounded-full px-4 py-1.5 text-xs font-semibold transition',
          activeTab === tab.id ? 'bg-[#be1e2c] text-white' : 'bg-gray-100 text-gray-600'
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
            <div class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full bg-gray-200 text-xs font-bold text-gray-600">
              {{ userInitials }}
            </div>
            <div class="flex-1">
              <textarea
                v-model="newPostContent"
                rows="2"
                class="w-full resize-none rounded-lg border-0 bg-gray-100 px-3 py-2 text-sm placeholder-gray-400 focus:bg-white focus:ring-1 focus:ring-[#be1e2c]"
                placeholder="Partager quelque chose..."
              ></textarea>
              <div class="mt-2 flex items-center justify-between">
                <div class="flex gap-4 text-xs text-gray-500">
                  <span>Photo</span>
                  <span>Sondage</span>
                </div>
                <button
                  @click="submitPost"
                  :disabled="!newPostContent.trim() || submittingPost"
                  class="rounded-lg bg-[#be1e2c] px-3 py-1 text-xs font-semibold text-white disabled:opacity-50"
                >
                  Publier
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Posts feed -->
        <div v-if="loadingPosts" class="flex justify-center py-10">
          <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#be1e2c] border-t-transparent"></div>
        </div>
        <div v-else-if="posts.length === 0" class="p-6 text-center text-sm text-gray-500">
          Aucun post pour le moment. Soyez le premier à publier!
        </div>
        <div v-else>
          <div v-for="post in posts" :key="post.id" class="border-b-8 border-gray-100 px-4 py-4">
            <!-- Author info -->
            <div class="mb-3 flex items-center gap-2.5">
              <div class="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full bg-[#1a1a1a] text-[8px] font-bold text-[#be1e2c]">
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

            <!-- Stats -->
            <div class="mb-2 flex items-center gap-4 text-xs text-gray-500">
              <span>{{ post.likeCount }} j'aime</span>
              <span>{{ post.commentCount }} commentaires</span>
              <span class="ml-auto">{{ post.viewCount }} vues</span>
            </div>

            <!-- Actions -->
            <div class="flex border-t border-gray-100 pt-2">
              <button
                @click="toggleLike(post)"
                :class="['flex-1 py-1 text-center text-sm font-medium transition', post.hasLiked ? 'text-[#be1e2c]' : 'text-gray-500 hover:text-gray-700']"
              >
                {{ post.hasLiked ? 'Aimé' : 'Aimer' }}
              </button>
              <button class="flex-1 py-1 text-center text-sm font-medium text-gray-500 hover:text-gray-700">
                Commenter
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Members tab -->
      <div v-else-if="activeTab === 'members'" class="p-4">
        <div v-if="loadingMembers" class="flex justify-center py-10">
          <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#be1e2c] border-t-transparent"></div>
        </div>
        <div v-else class="space-y-3">
          <div v-for="gm in groupMembers" :key="gm.id" class="flex items-center gap-3">
            <div class="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-full bg-gray-200 text-xs font-bold text-gray-600">
              {{ getInitials(gm.fullName) }}
            </div>
            <div>
              <p class="text-sm font-medium text-gray-900">{{ gm.fullName }}</p>
              <p v-if="gm.role === 'Professor'" class="text-xs text-[#be1e2c]">Professeur</p>
            </div>
          </div>
        </div>
      </div>

      <!-- About tab -->
      <div v-else-if="activeTab === 'about'" class="p-4">
        <h3 class="mb-2 font-semibold text-gray-900">{{ group?.name }}</h3>
        <p v-if="group?.description" class="mb-4 text-sm text-gray-600">{{ group.description }}</p>
        <p class="text-sm text-gray-500">Saison : {{ group?.season }}</p>
        <div v-if="group?.inviteCode" class="mt-4 rounded-lg bg-gray-50 p-3">
          <p class="text-xs font-medium text-gray-500">Code d'invitation</p>
          <p class="mt-1 text-lg font-bold text-[#be1e2c]">{{ group.inviteCode }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useUserStore } from '@/stores/userStore'
import type { Post, GroupMember } from '@/types/entities'

const route = useRoute()
const socialService = useSocialService()
const userStore = useUserStore()

const groupId = computed(() => route.params.id as string)

const group = ref<any>(null)
const posts = ref<Post[]>([])
const groupMembers = ref<GroupMember[]>([])
const activeTab = ref('feed')
const loadingPosts = ref(true)
const loadingMembers = ref(false)
const newPostContent = ref('')
const submittingPost = ref(false)

const tabs = [
  { id: 'feed', label: 'Fil' },
  { id: 'members', label: 'Membres' },
  { id: 'about', label: 'À propos' },
]

const userInitials = computed(() => {
  const user = userStore.user
  if (!user?.email) return '?'
  return user.email.charAt(0).toUpperCase()
})

function getInitials(name: string) {
  return name?.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2) || '?'
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

onMounted(async () => {
  await loadGroup()
  await loadPosts()
  loadMembers()
})
</script>
