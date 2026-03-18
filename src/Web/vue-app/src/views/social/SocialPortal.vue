<template>
  <div class="p-4">
    <!-- Header -->
    <div class="mb-4 flex items-center justify-between">
      <h2 class="text-lg font-bold text-gray-900">Portail EDB</h2>
    </div>

    <!-- Invite code input -->
    <div class="mb-6 flex gap-2">
      <input
        v-model="inviteCode"
        type="text"
        class="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
        placeholder="Entrer un code d'invitation..."
        @keyup.enter="joinGroup"
      />
      <button
        @click="joinGroup"
        :disabled="!inviteCode || joining"
        class="rounded-lg bg-[#1a1a1a] px-4 py-2 text-sm font-semibold text-white transition hover:bg-[#000000] disabled:opacity-50"
      >
        Rejoindre
      </button>
    </div>

    <div v-if="joinError" class="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">{{ joinError }}</div>
    <div v-if="joinSuccess" class="mb-4 rounded-lg bg-green-50 p-3 text-sm text-green-700">{{ joinSuccess }}</div>

    <!-- My Groups -->
    <div class="mb-6">
      <h3 class="mb-3 text-xs font-bold uppercase tracking-wide text-gray-500">Mes groupes</h3>
      <div v-if="loadingGroups" class="flex justify-center py-8">
        <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
      </div>
      <div v-else-if="myGroups.length === 0" class="rounded-lg bg-gray-50 p-4 text-center text-sm text-gray-500">
        Vous n'avez pas encore rejoint de groupe. Entrez un code d'invitation pour commencer.
      </div>
      <div v-else class="grid grid-cols-3 gap-3 sm:grid-cols-4">
        <router-link
          v-for="group in myGroups"
          :key="group.id"
          :to="{ name: 'socialGroup', params: { id: group.id } }"
          class="text-center"
        >
          <div class="mx-auto flex h-20 w-20 items-center justify-center rounded-lg bg-[#1a1a1a]">
            <img v-if="group.imageUrl" :src="group.imageUrl" :alt="group.name" class="h-full w-full rounded-lg object-cover" />
            <span v-else class="text-xs font-bold text-white">EDB</span>
          </div>
          <p class="mt-1 truncate text-xs font-medium text-gray-900">{{ group.name }}</p>
        </router-link>
      </div>
    </div>

    <!-- Suggested Groups -->
    <div>
      <h3 class="mb-3 text-xs font-bold uppercase tracking-wide text-gray-500">Groupes suggérés</h3>
      <div v-if="suggestedGroups.length === 0" class="text-center text-sm text-gray-500">
        Aucun groupe suggéré pour le moment.
      </div>
      <div v-else class="space-y-3">
        <div
          v-for="group in suggestedGroups"
          :key="group.id"
          class="overflow-hidden rounded-xl border border-gray-200"
        >
          <div class="flex items-center gap-3 p-4">
            <div class="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-full bg-[#1a1a1a]">
              <img v-if="group.imageUrl" :src="group.imageUrl" :alt="group.name" class="h-full w-full rounded-full object-cover" />
              <span v-else class="text-[10px] font-bold text-white">EDB</span>
            </div>
            <div>
              <p class="font-semibold text-gray-900">{{ group.name }}</p>
              <p class="text-xs text-gray-500">Privé · {{ group.memberCount }} membres</p>
            </div>
          </div>
          <div class="border-t border-gray-200 px-4 py-2.5 text-center">
            <span class="text-sm font-medium text-[#1a1a1a]">Code requis</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useSocialService } from '@/inversify.config'
import type { Group } from '@/types/entities'

const socialService = useSocialService()

const myGroups = ref<Group[]>([])
const allActiveGroups = ref<Group[]>([])
const loadingGroups = ref(true)
const inviteCode = ref('')
const joining = ref(false)
const joinError = ref('')
const joinSuccess = ref('')

const myGroupIds = computed(() => new Set(myGroups.value.map(g => g.id)))
const suggestedGroups = computed(() =>
  allActiveGroups.value.filter(g => !myGroupIds.value.has(g.id))
)

async function loadGroups() {
  loadingGroups.value = true
  try {
    const [mine, active] = await Promise.all([
      socialService.getMyGroups(),
      socialService.getActiveGroups()
    ])
    myGroups.value = mine
    allActiveGroups.value = active
  } catch (e) {
    // silently fail
  } finally {
    loadingGroups.value = false
  }
}

async function joinGroup() {
  if (!inviteCode.value) return
  joining.value = true
  joinError.value = ''
  joinSuccess.value = ''

  try {
    const result = await socialService.joinGroup(inviteCode.value)
    if (result.succeeded) {
      joinSuccess.value = 'Vous avez rejoint le groupe!'
      inviteCode.value = ''
      await loadGroups()
    } else {
      joinError.value = result.errors?.[0]?.errorMessage || 'Code invalide.'
    }
  } catch (e) {
    joinError.value = 'Code invalide ou erreur de connexion.'
  } finally {
    joining.value = false
  }
}

onMounted(loadGroups)
</script>
