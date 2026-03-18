<template>
  <div class="p-4">
    <!-- Header -->
    <div class="mb-4 flex items-center justify-between">
      <h2 class="text-lg font-bold text-gray-900">Portail EDB</h2>
      <button v-if="isAdmin" @click="showCreateGroup = !showCreateGroup" class="rounded-lg bg-[#1a1a1a] px-3 py-1.5 text-xs font-semibold text-white transition hover:bg-[#000] cursor-pointer">
        {{ showCreateGroup ? 'Fermer' : '+ Créer un groupe' }}
      </button>
    </div>

    <!-- Create group form (admin only) -->
    <div v-if="isAdmin && showCreateGroup" class="mb-6 rounded-xl border border-gray-200 bg-gray-50 p-4">
      <h3 class="mb-3 text-sm font-semibold text-gray-700">Nouveau groupe</h3>
      <div class="space-y-3">
        <div>
          <label class="mb-1 block text-xs font-medium text-gray-500">Nom du groupe</label>
          <input v-model="newGroup.name" type="text" placeholder="Ex: Multi 5-7 ans" class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]" />
        </div>
        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="mb-1 block text-xs font-medium text-gray-500">Saison</label>
            <input v-model="newGroup.season" type="text" placeholder="Ex: Hiver 2026" class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]" />
          </div>
          <div>
            <label class="mb-1 block text-xs font-medium text-gray-500">Code d'invitation (optionnel)</label>
            <input v-model="newGroup.inviteCode" type="text" placeholder="Auto-généré" class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]" />
          </div>
        </div>
        <div>
          <label class="mb-1 block text-xs font-medium text-gray-500">Description (optionnel)</label>
          <textarea v-model="newGroup.description" rows="2" placeholder="Description du groupe..." class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"></textarea>
        </div>
        <button @click="createGroup" :disabled="!newGroup.name || !newGroup.season || creatingGroup" class="rounded-lg bg-[#1a1a1a] px-4 py-2 text-sm font-semibold text-white transition hover:bg-[#000] disabled:opacity-50 cursor-pointer">
          {{ creatingGroup ? 'Création...' : 'Créer le groupe' }}
        </button>
      </div>
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

    <!-- All Groups -->
    <div>
      <h3 class="mb-3 text-xs font-bold uppercase tracking-wide text-gray-500">Tous les groupes</h3>
      <div v-if="allActiveGroups.length === 0" class="text-center text-sm text-gray-500">
        Aucun groupe pour le moment.
      </div>
      <div v-else class="space-y-3">
        <router-link
          v-for="group in allActiveGroups"
          :key="group.id"
          :to="myGroupIds.has(group.id) ? { name: 'socialGroup', params: { id: group.id } } : ''"
          :class="['overflow-hidden rounded-xl border border-gray-200 block', myGroupIds.has(group.id) ? 'hover:bg-gray-50' : '']"
          @click.prevent="!myGroupIds.has(group.id) && null"
        >
          <div class="flex items-center gap-3 p-4">
            <div class="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-full bg-[#1a1a1a]">
              <img v-if="group.imageUrl" :src="group.imageUrl" :alt="group.name" class="h-full w-full rounded-full object-cover" />
              <span v-else class="text-[10px] font-bold text-white">EDB</span>
            </div>
            <div class="flex-1">
              <p class="font-semibold text-gray-900">{{ group.name }}</p>
              <p class="text-xs text-gray-500">{{ group.season }} · {{ group.memberCount }} membres</p>
            </div>
            <span v-if="myGroupIds.has(group.id)" class="rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-700">Membre</span>
            <span v-else class="text-xs font-medium text-gray-400">Code requis</span>
          </div>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useSocialService } from '@/inversify.config'
import { useSocialToast } from '@/composables/useSocialToast'
import { useUserStore } from '@/stores/userStore'
import { Role } from '@/types/enums'
import type { Group } from '@/types/entities'

const socialService = useSocialService()
const toast = useSocialToast()
const userStore = useUserStore()

const isAdmin = computed(() => userStore.hasRole(Role.Admin))

const myGroups = ref<Group[]>([])
const allActiveGroups = ref<Group[]>([])
const loadingGroups = ref(true)
const inviteCode = ref('')
const joining = ref(false)
const showCreateGroup = ref(false)
const creatingGroup = ref(false)
const newGroup = ref({ name: '', season: '', inviteCode: '', description: '' })

const myGroupIds = computed(() => new Set(myGroups.value.map(g => g.id)))

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

async function createGroup() {
  creatingGroup.value = true
  try {
    const result = await socialService.createGroup(newGroup.value.name, newGroup.value.description, newGroup.value.season, newGroup.value.inviteCode)
    if (result.succeeded) {
      toast.success('Groupe créé!')
      newGroup.value = { name: '', season: '', inviteCode: '', description: '' }
      showCreateGroup.value = false
      await loadGroups()
    } else {
      toast.error(result.errors?.[0]?.errorMessage || 'Erreur lors de la création.')
    }
  } catch (e) {
    toast.error('Erreur lors de la création du groupe.')
  }
  creatingGroup.value = false
}

async function joinGroup() {
  if (!inviteCode.value) return
  joining.value = true

  try {
    const result = await socialService.joinGroup(inviteCode.value)
    if (result.succeeded) {
      toast.success('Vous avez rejoint le groupe!')
      inviteCode.value = ''
      await loadGroups()
    } else {
      toast.error(result.errors?.[0]?.errorMessage || 'Code invalide.')
    }
  } catch (e) {
    toast.error('Code invalide ou erreur de connexion.')
  } finally {
    joining.value = false
  }
}

onMounted(loadGroups)
</script>
