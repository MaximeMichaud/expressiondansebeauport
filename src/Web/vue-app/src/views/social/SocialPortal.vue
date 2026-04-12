<template>
  <div class="p-4">
    <!-- Header -->
    <div class="mb-4 flex items-center justify-between">
      <h2 class="text-lg font-bold text-gray-900">Portail EDB</h2>
      <button v-if="isAdmin" @click="showCreateGroup = !showCreateGroup" class="rounded-lg border border-[rgba(21,128,61,0.15)] bg-[rgba(21,128,61,0.06)] px-3 py-1.5 text-xs font-semibold text-[#15803d] transition hover:bg-[rgba(21,128,61,0.12)] cursor-pointer">
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

        <!-- Group image -->
        <div v-if="groupAttachment.previews.value.length" class="flex flex-wrap gap-2">
          <div
            v-for="(p, i) in groupAttachment.previews.value"
            :key="i"
            class="relative h-20 w-20"
          >
            <img :src="p.url" class="h-full w-full rounded-lg object-cover" alt="" />
            <button
              type="button"
              @click="groupAttachment.removeFile(i)"
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
        <p v-if="groupAttachment.error.value" class="text-xs text-red-600">{{ groupAttachment.error.value }}</p>

        <div class="flex items-center justify-between">
          <input
            ref="groupFileInputRef"
            type="file"
            accept="image/*"
            hidden
            @change="groupAttachment.handleFileInput"
          />
          <button
            type="button"
            @click="groupFileInputRef?.click()"
            :disabled="groupAttachment.files.value.length >= 1"
            class="soc-composer-icon flex h-9 w-9 items-center justify-center rounded-lg transition cursor-pointer disabled:opacity-40 disabled:cursor-default"
            title="Image du groupe"
            aria-label="Image du groupe"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21.44 11.05l-9.19 9.19a6 6 0 01-8.49-8.49l9.19-9.19a4 4 0 015.66 5.66l-9.2 9.19a2 2 0 01-2.83-2.83l8.49-8.48"/></svg>
          </button>
          <button @click="createGroup" :disabled="!newGroup.name || !newGroup.season || creatingGroup" class="btn-publish rounded-lg bg-[#1a1a1a] px-4 py-2 text-sm font-semibold text-white transition hover:bg-[#000] disabled:opacity-50 cursor-pointer">
            {{ creatingGroup ? 'Création...' : 'Créer le groupe' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Search -->
    <div class="mb-4 relative">
      <svg class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><path d="M21 21l-4.35-4.35"/></svg>
      <input
        v-model="searchQuery"
        type="text"
        class="w-full rounded-lg border border-gray-300 py-2.5 pl-9 pr-3 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
        placeholder="Rechercher un groupe..."
      />
    </div>

    <!-- Tabs -->
    <div class="mb-4 flex border-b border-gray-200">
      <button
        @click="groupTab = 'mine'"
        :class="['flex-1 py-2.5 text-center text-sm font-semibold transition cursor-pointer', groupTab === 'mine' ? 'border-b-2 border-gray-900 text-gray-900' : 'text-gray-400 hover:text-gray-600']"
      >
        Mes groupes
      </button>
      <button
        @click="groupTab = 'all'"
        :class="['flex-1 py-2.5 text-center text-sm font-semibold transition cursor-pointer', groupTab === 'all' ? 'border-b-2 border-gray-900 text-gray-900' : 'text-gray-400 hover:text-gray-600']"
      >
        Tous les groupes
      </button>
    </div>

    <!-- My Groups tab -->
    <div v-if="groupTab === 'mine'">
      <div v-if="loadingGroups" class="flex justify-center py-8">
        <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
      </div>
      <div v-else-if="filteredMyGroups.length === 0" class="flex flex-col items-center justify-center gap-3 py-20 text-gray-400">
        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>
        <span class="text-sm">{{ searchQuery ? 'Aucun groupe trouvé.' : 'Vous n\'avez pas encore rejoint de groupe.' }}</span>
      </div>
      <div v-else class="grid grid-cols-2 gap-3">
        <router-link
          v-for="group in filteredMyGroups"
          :key="group.id"
          :to="{ name: 'socialGroup', params: { id: group.id } }"
          class="flex items-center gap-3 rounded-xl border border-gray-200 p-3 cursor-pointer hover:bg-gray-50 transition"
        >
          <div class="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-lg bg-[#1a1a1a] group-logo">
            <img v-if="group.imageUrl" :src="group.imageUrl" :alt="group.name" class="h-full w-full rounded-lg object-cover" />
            <span v-else class="text-[10px] font-bold text-white">EDB</span>
          </div>
          <div class="flex-1 min-w-0">
            <p class="truncate text-sm font-semibold text-gray-900">{{ group.name }}</p>
            <p class="text-[11px] text-gray-400">{{ group.season }} · {{ group.memberCount }} membres</p>
          </div>
        </router-link>
      </div>
    </div>

    <!-- All Groups tab -->
    <div v-if="groupTab === 'all'">
      <div v-if="filteredAllGroups.length === 0" class="flex flex-col items-center justify-center gap-3 py-20 text-gray-400">
        <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>
        <span class="text-sm">{{ searchQuery ? 'Aucun groupe trouvé.' : 'Aucun groupe pour le moment.' }}</span>
      </div>
      <div v-else class="grid grid-cols-2 gap-3">
        <div
          v-for="group in filteredAllGroups"
          :key="group.id"
          class="flex items-center gap-3 rounded-xl border border-gray-200 p-3 cursor-pointer hover:bg-gray-50 transition"
          @click="onGroupClick(group)"
        >
          <div class="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-lg bg-[#1a1a1a] group-logo">
            <img v-if="group.imageUrl" :src="group.imageUrl" :alt="group.name" class="h-full w-full rounded-lg object-cover" />
            <span v-else class="text-[10px] font-bold text-white">EDB</span>
          </div>
          <div class="flex-1 min-w-0">
            <p class="truncate text-sm font-semibold text-gray-900">{{ group.name }}</p>
            <p class="text-[11px] text-gray-400">{{ group.season }} · {{ group.memberCount }} membres</p>
          </div>
          <button
            v-if="isAdmin && group.inviteCode"
            @click.stop="copyCode(group.inviteCode)"
            class="flex flex-shrink-0 items-center gap-1 rounded px-1.5 py-0.5 font-mono text-[11px] font-semibold text-indigo-600 transition hover:bg-[rgba(99,102,241,0.08)] cursor-pointer"
            :title="'Copier le code'"
          >
            {{ group.inviteCode }}
            <svg v-if="copiedCode !== group.inviteCode" width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="9" y="9" width="13" height="13" rx="2"/><path d="M5 15H4a2 2 0 01-2-2V4a2 2 0 012-2h9a2 2 0 012 2v1"/></svg>
            <svg v-else width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6L9 17l-5-5"/></svg>
          </button>
        </div>
      </div>
    </div>

    <!-- Join group modal -->
    <Teleport to="body">
      <Transition name="portal-modal">
        <div v-if="showJoinModal" class="portal-modal__overlay" @click.self="closeJoinModal">
          <div class="portal-modal__card">
            <div class="portal-modal__icon-ring">
              <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="var(--soc-bar-text-strong, #1a1a1a)" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
                <rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/>
              </svg>
            </div>
            <h3 class="portal-modal__title" style="margin-bottom: 20px;">Rejoindre « {{ joinModalGroup?.name }} »</h3>

            <!-- Choice mode -->
            <template v-if="joinModalMode === 'choice'">
              <div v-if="checkingPending" class="flex justify-center py-4">
                <div class="h-5 w-5 animate-spin rounded-full border-2 border-[var(--soc-bar-text-strong,#1a1a1a)] border-t-transparent"></div>
              </div>
              <template v-else>
                <button
                  v-if="pendingJoinRequestId"
                  disabled
                  class="portal-modal__btn portal-modal__btn--primary w-full opacity-50 cursor-not-allowed"
                >
                  Demande envoyée ✓
                </button>
                <button
                  v-else-if="isAdmin"
                  disabled
                  class="portal-modal__btn portal-modal__btn--primary w-full opacity-40 cursor-not-allowed"
                >
                  Demander à rejoindre
                </button>
                <button
                  v-else
                  @click="requestToJoin"
                  :disabled="requestingJoin"
                  class="portal-modal__btn portal-modal__btn--primary w-full"
                >
                  {{ requestingJoin ? 'Envoi...' : 'Demander à rejoindre' }}
                </button>

                <div v-if="joinModalError" class="portal-modal__error" style="margin-top: 16px;">{{ joinModalError }}</div>

                <div class="portal-modal__actions" style="margin-top: 16px;">
                  <button @click="closeJoinModal" class="portal-modal__btn portal-modal__btn--cancel">Annuler</button>
                  <button @click="joinModalMode = 'code'" class="portal-modal__btn portal-modal__btn--cancel">J'ai un code</button>
                </div>
              </template>
            </template>

            <!-- Code mode -->
            <template v-if="joinModalMode === 'code'">
              <p class="portal-modal__text">Entrez le code d'invitation pour rejoindre ce groupe.</p>
              <input
                v-model="joinModalCode"
                type="text"
                class="portal-modal__input"
                placeholder="Code d'invitation"
                @keyup.enter="joinFromModal"
              />
              <div v-if="joinModalError" class="portal-modal__error">{{ joinModalError }}</div>
              <div class="portal-modal__actions">
                <button @click="joinModalMode = 'choice'" class="portal-modal__btn portal-modal__btn--cancel">Retour</button>
                <button @click="joinFromModal" :disabled="!joinModalCode.trim() || joiningFromModal" class="portal-modal__btn portal-modal__btn--primary">
                  {{ joiningFromModal ? 'Rejoindre...' : 'Rejoindre' }}
                </button>
              </div>
            </template>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useSocialToast } from '@/composables/useSocialToast'
import { useUserStore } from '@/stores/userStore'
import { useImageAttachment } from '@/composables/useImageAttachment'
import { Role } from '@/types/enums'
import type { Group } from '@/types/entities'

const router = useRouter()
const socialService = useSocialService()
const toast = useSocialToast()
const userStore = useUserStore()

const isAdmin = computed(() => userStore.hasRole(Role.Admin))

const myGroups = ref<Group[]>([])
const allActiveGroups = ref<Group[]>([])
const loadingGroups = ref(true)
const searchQuery = ref('')
const groupTab = ref<'mine' | 'all'>('mine')
const copiedCode = ref('')

function copyCode(code: string) {
  navigator.clipboard.writeText(code)
  copiedCode.value = code
  setTimeout(() => { copiedCode.value = '' }, 2000)
}
const showCreateGroup = ref(false)
const creatingGroup = ref(false)
const newGroup = ref({ name: '', season: '', inviteCode: '', description: '' })
const groupAttachment = useImageAttachment({ mode: 'single', maxFiles: 1, allowedTypes: ['image/jpeg', 'image/png', 'image/webp', 'image/gif'] })
const groupFileInputRef = ref<HTMLInputElement | null>(null)

const myGroupIds = computed(() => new Set(myGroups.value.map(g => g.id)))

const filteredMyGroups = computed(() => {
  const q = searchQuery.value.toLowerCase().trim()
  if (!q) return myGroups.value
  return myGroups.value.filter(g => g.name.toLowerCase().includes(q) || g.season?.toLowerCase().includes(q))
})

const filteredAllGroups = computed(() => {
  const q = searchQuery.value.toLowerCase().trim()
  if (!q) return allActiveGroups.value
  return allActiveGroups.value.filter(g => g.name.toLowerCase().includes(q) || g.season?.toLowerCase().includes(q))
})

async function loadGroups() {
  loadingGroups.value = true
  try {
    const [mine, active] = await Promise.all([
      socialService.getMyGroups(),
      socialService.getActiveGroups()
    ])
    myGroups.value = mine
    allActiveGroups.value = active
  } catch {
    // silently fail
  } finally {
    loadingGroups.value = false
  }
}

async function createGroup() {
  creatingGroup.value = true
  try {
    let imageUrl: string | undefined

    if (groupAttachment.files.value.length > 0) {
      const uploaded = await socialService.uploadFile(groupAttachment.files.value[0])
      if (!uploaded.succeeded || !uploaded.displayUrl) {
        toast.error("Échec du téléversement de l'image.")
        creatingGroup.value = false
        return
      }
      imageUrl = uploaded.displayUrl
    }

    const result = await socialService.createGroup(newGroup.value.name, newGroup.value.description, newGroup.value.season, newGroup.value.inviteCode, imageUrl)
    if (result.succeeded) {
      toast.success('Groupe créé!')
      newGroup.value = { name: '', season: '', inviteCode: '', description: '' }
      groupAttachment.clear()
      showCreateGroup.value = false
      await loadGroups()
    } else {
      toast.error(result.errors?.[0]?.errorMessage || 'Erreur lors de la création.')
    }
  } catch {
    toast.error('Erreur lors de la création du groupe.')
  }
  creatingGroup.value = false
}

// Join modal
const showJoinModal = ref(false)
const joinModalGroup = ref<Group | null>(null)
const joinModalCode = ref('')
const joinModalError = ref('')
const joiningFromModal = ref(false)
const joinModalMode = ref<'choice' | 'code'>('choice')
const requestingJoin = ref(false)
const pendingJoinRequestId = ref<string | null>(null)
const checkingPending = ref(false)

async function onGroupClick(group: Group) {
  if (myGroupIds.value.has(group.id)) {
    router.push({ name: 'socialGroup', params: { id: group.id } })
    return
  }

  joinModalGroup.value = group
  joinModalCode.value = ''
  joinModalError.value = ''
  joinModalMode.value = 'choice'
  pendingJoinRequestId.value = null
  showJoinModal.value = true

  checkingPending.value = true
  try {
    const result = await socialService.getMyJoinRequest(group.id)
    if (result?.found) {
      pendingJoinRequestId.value = result.joinRequestId
    }
  } catch { /* */ }
  checkingPending.value = false
}

function closeJoinModal() {
  showJoinModal.value = false
  joinModalGroup.value = null
  joinModalCode.value = ''
  joinModalError.value = ''
  joinModalMode.value = 'choice'
  pendingJoinRequestId.value = null
}

async function joinFromModal() {
  if (!joinModalCode.value.trim()) return
  joiningFromModal.value = true
  joinModalError.value = ''
  try {
    const result = await socialService.joinGroup(joinModalCode.value)
    if (result.succeeded) {
      closeJoinModal()
      toast.success('Vous avez rejoint le groupe!')
      await loadGroups()
      groupTab.value = 'mine'
    } else {
      joinModalError.value = result.errors?.[0]?.errorMessage || 'Code invalide.'
    }
  } catch {
    joinModalError.value = 'Code invalide ou erreur de connexion.'
  }
  joiningFromModal.value = false
}

async function requestToJoin() {
  if (!joinModalGroup.value) return
  requestingJoin.value = true
  joinModalError.value = ''
  try {
    const result = await socialService.requestJoinGroup(joinModalGroup.value.id)
    if (result?.succeeded) {
      closeJoinModal()
      toast.success('Demande envoyée!')
    } else {
      joinModalError.value = result?.errors?.[0]?.errorMessage || 'Erreur.'
    }
  } catch {
    joinModalError.value = 'Erreur de connexion.'
  }
  requestingJoin.value = false
}

onMounted(loadGroups)
</script>

<style lang="scss">
$portal-font-display: 'Montserrat', sans-serif;

.soc-composer-icon {
  color: var(--soc-text-muted);
}
.soc-composer-icon:hover {
  background: var(--soc-bar-hover);
  color: var(--soc-text);
}

.portal-modal {
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
    max-width: 380px;
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
    background: var(--soc-bar-hover, #f5f3f0);
    margin-bottom: 16px;
  }

  &__title {
    font-family: $portal-font-display;
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

  &__input {
    width: 100%;
    padding: 11px 16px;
    font-family: $portal-font-display;
    font-size: 0.9rem;
    font-weight: 600;
    text-align: center;
    letter-spacing: 0.08em;
    border: 1px solid var(--soc-border, #e7e0da);
    border-radius: 10px;
    background: var(--soc-input-bg, #faf9f7);
    color: var(--soc-bar-text-strong, #1a1a1a);
    outline: none;
    margin-bottom: 16px;
    &:focus { border-color: #1a1a1a; }
    &::placeholder { letter-spacing: 0; font-weight: 400; color: #a8a29e; }
  }

  &__error {
    font-size: 0.8rem;
    color: #dc2626;
    margin-bottom: 12px;
    margin-top: -8px;
  }

  &__actions {
    display: flex;
    gap: 10px;
  }

  &__btn {
    flex: 1;
    padding: 11px 16px;
    font-family: $portal-font-display;
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
      &:hover:not(:disabled) { background: var(--soc-bar-active, #eae8e4); }
    }

    &--primary {
      background: var(--soc-bar-text-strong, #1a1a1a);
      color: var(--soc-card-bg, white);
      &:hover:not(:disabled) { opacity: 0.85; }
    }
  }

  &__input:focus {
    border-color: var(--soc-bar-text-strong, #1a1a1a);
  }
}

.portal-modal-enter-active { transition: all 0.2s ease; }
.portal-modal-leave-active { transition: all 0.15s ease; }
.portal-modal-enter-from { opacity: 0; }
.portal-modal-leave-to { opacity: 0; }
</style>
