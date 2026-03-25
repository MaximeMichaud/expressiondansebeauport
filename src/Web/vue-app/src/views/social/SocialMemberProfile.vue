<template>
  <div class="mp">
    <!-- Loading -->
    <div v-if="loading" class="mp__loading">
      <div class="mp__spinner" />
    </div>

    <template v-else-if="member">
      <!-- Header -->
      <div class="mp__header">
        <button @click="$router.back()" class="mp__back">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 19l-7-7 7-7"/></svg>
        </button>
        <h2 class="mp__title">Profil</h2>
      </div>

      <!-- Identity -->
      <div class="mp__identity">
        <div class="mp__avatar" :style="{ background: member.avatarColor || getAvatarColor(member.fullName) }">
          <img v-if="member.profileImageUrl" :src="member.profileImageUrl" :alt="member.fullName" class="mp__avatar-img" />
          <span v-else class="mp__avatar-initials">{{ getInitials(member.fullName) }}</span>
        </div>
        <h1 class="mp__name">{{ member.fullName }}</h1>
        <div class="mp__roles">
          <span :class="['mp__role-badge', `mp__role-badge--${highestRoleKey}`]">{{ highestRole }}</span>
        </div>
        <button @click="startDM" :disabled="startingDM" class="mp__dm-btn">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/>
          </svg>
          {{ startingDM ? '...' : 'Envoyer un message' }}
        </button>
      </div>

      <!-- Info card -->
      <section class="mp__card">
        <div class="mp__card-header">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="4"/><path d="M20 21a8 8 0 00-16 0"/></svg>
          <h3>Informations</h3>
        </div>
        <div class="mp__card-body">
          <div class="mp__info-row">
            <span class="mp__info-label">Courriel</span>
            <span class="mp__info-value">{{ member.email }}</span>
          </div>
          <div v-if="member.firstName" class="mp__info-row">
            <span class="mp__info-label">Prénom</span>
            <span class="mp__info-value">{{ member.firstName }}</span>
          </div>
          <div v-if="member.lastName" class="mp__info-row">
            <span class="mp__info-label">Nom</span>
            <span class="mp__info-value">{{ member.lastName }}</span>
          </div>
        </div>
      </section>

      <!-- Groups card -->
      <section class="mp__card">
        <div class="mp__card-header">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>
          <h3>Groupes</h3>
        </div>
        <div class="mp__card-body">
          <div v-if="member.groups && member.groups.length" class="mp__groups">
            <router-link
              v-for="group in member.groups"
              :key="group.id"
              :to="{ name: 'socialGroup', params: { id: group.id } }"
              class="mp__group-row"
            >
              <div class="mp__group-img" :style="group.imageUrl ? { backgroundImage: `url(${group.imageUrl})` } : {}">
                <span v-if="!group.imageUrl">EDB</span>
              </div>
              <div class="mp__group-info">
                <span class="mp__group-name">{{ group.name }}</span>
                <span class="mp__group-season">{{ group.season }}</span>
              </div>
              <svg class="mp__group-arrow" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 18l6-6-6-6"/></svg>
            </router-link>
          </div>
          <p v-else class="mp__empty">Aucun groupe en commun.</p>
        </div>
      </section>

      <!-- Admin card -->
      <section v-if="isAdmin" class="mp__card mp__card--admin">
        <div class="mp__card-header">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4"/></svg>
          <h3>Administration</h3>
        </div>
        <div class="mp__card-body mp__admin-actions">
          <!-- Promote / Demote -->
          <button
            v-if="!isProfessor"
            @click="promoteMember"
            :disabled="promoting"
            class="mp__promote-btn"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 00-3-3.87"/><path d="M16 3.13a4 4 0 010 7.75"/>
            </svg>
            {{ promoting ? '...' : 'Promouvoir en professeur' }}
          </button>
          <button
            v-else
            @click="demoteMember"
            :disabled="promoting"
            class="mp__demote-btn"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><line x1="23" y1="11" x2="17" y2="11"/>
            </svg>
            {{ promoting ? '...' : 'Retirer le rôle professeur' }}
          </button>

          <!-- Delete -->
          <button @click="showDeleteModal = true" :disabled="deleting" class="mp__delete-btn">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/>
            </svg>
            {{ deleting ? 'Suppression...' : 'Supprimer ce compte' }}
          </button>
        </div>
      </section>
    </template>

    <!-- Delete confirmation modal -->
    <Teleport to="body">
      <Transition name="mp-modal">
        <div v-if="showDeleteModal" class="mp-modal__overlay" @click.self="showDeleteModal = false">
          <div class="mp-modal__card">
            <div class="mp-modal__icon-ring">
              <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#dc2626" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/>
              </svg>
            </div>
            <h3 class="mp-modal__title">Supprimer ce compte?</h3>
            <p class="mp-modal__text">
              Le compte de <strong>{{ member?.fullName }}</strong> sera désactivé. Cette action ne peut pas être annulée.
            </p>
            <div class="mp-modal__actions">
              <button @click="showDeleteModal = false" class="mp-modal__btn mp-modal__btn--cancel">Annuler</button>
              <button @click="executeDelete" :disabled="deleting" class="mp-modal__btn mp-modal__btn--danger">
                <svg v-if="deleting" class="mp-modal__spinner" width="16" height="16" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="3" fill="none" stroke-dasharray="31.4 31.4" stroke-linecap="round"><animateTransform attributeName="transform" type="rotate" values="0 12 12;360 12 12" dur="0.7s" repeatCount="indefinite"/></circle></svg>
                {{ deleting ? 'Suppression...' : 'Oui, supprimer' }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useUserStore } from '@/stores/userStore'
import { Role } from '@/types/enums'

const route = useRoute()
const router = useRouter()
const socialService = useSocialService()
const userStore = useUserStore()

const member = ref<any>(null)
const loading = ref(true)
const startingDM = ref(false)
const deleting = ref(false)
const promoting = ref(false)

const isAdmin = computed(() => userStore.hasRole(Role.Admin))
const isProfessor = computed(() => member.value?.roles?.includes('professor'))
const highestRoleKey = computed(() => {
  const roles = member.value?.roles || []
  if (roles.includes('admin')) return 'admin'
  if (roles.includes('professor')) return 'professor'
  return 'member'
})
const highestRole = computed(() => {
  const map: Record<string, string> = { admin: 'Admin', professor: 'Prof', member: 'Membre' }
  return map[highestRoleKey.value]
})

async function promoteMember() {
  promoting.value = true
  const memberId = member.value.id || member.value.Id
  try {
    await socialService.promoteMember(memberId)
    member.value = await socialService.getMemberProfile(route.params.id as string)
  } catch (e) { console.error('Promote error:', e) }
  promoting.value = false
}

async function demoteMember() {
  promoting.value = true
  const memberId = member.value.id || member.value.Id
  try {
    await socialService.demoteMember(memberId)
    member.value = await socialService.getMemberProfile(route.params.id as string)
  } catch (e) { console.error('Demote error:', e) }
  promoting.value = false
}

function getInitials(name: string) {
  if (!name || !name.trim()) {
    const m = member.value
    if (m?.firstName && m?.lastName) return (m.firstName[0] + m.lastName[0]).toUpperCase()
    return '??'
  }
  return name.split(' ').filter(n => n.length > 0).map(n => n[0]).join('').toUpperCase().slice(0, 2)
}

const avatarColors = ['#e53e3e', '#dd6b20', '#d69e2e', '#38a169', '#319795', '#3182ce', '#5a67d8', '#805ad5', '#d53f8c', '#e53e3e']
function getAvatarColor(name: string) {
  let hash = 0
  for (let i = 0; i < (name?.length || 0); i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

function getRoleLabel(role: string) {
  const labels: Record<string, string> = { admin: 'Admin', professor: 'Prof', member: 'Membre' }
  return labels[role] || role
}

async function startDM() {
  startingDM.value = true
  try {
    const conversation = await socialService.startConversation(member.value.id)
    await router.push({ name: 'socialConversation', params: { conversationId: conversation.id || conversation.Id } })
  } catch { /* */ }
  startingDM.value = false
}

const showDeleteModal = ref(false)

async function executeDelete() {
  deleting.value = true
  const memberId = member.value.id || member.value.Id
  console.log('Deleting member:', memberId)
  try {
    const result = await socialService.deleteMember(memberId)
    console.log('Delete result:', result)
    showDeleteModal.value = false
    await router.push('/membres')
  } catch (e: any) {
    console.error('Delete error:', e?.response?.status, e?.response?.data, e)
    showDeleteModal.value = false
  }
  deleting.value = false
}

onMounted(async () => {
  try {
    member.value = await socialService.getMemberProfile(route.params.id as string)
  } catch { /* */ }
  loading.value = false
})
</script>

<style lang="scss">
$mp-font-display: 'Montserrat', sans-serif;
$mp-font-body: 'Karla', sans-serif;

.mp {
  a, button, [role="button"] { cursor: pointer; }
  padding-bottom: 24px;

  &__loading {
    display: flex;
    justify-content: center;
    padding: 80px 0;
  }

  &__spinner {
    width: 24px;
    height: 24px;
    border: 2.5px solid var(--soc-border, #e7e0da);
    border-top-color: var(--soc-bar-text-strong, #1a1a1a);
    border-radius: 50%;
    animation: mp-spin 0.7s linear infinite;
  }

  // Header bar
  &__header {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 16px 20px;
    border-bottom: 1px solid var(--soc-divider, #f0f0f0);
  }

  &__back {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    border-radius: 8px;
    color: var(--soc-text-muted, #78716c);
    transition: color 0.15s, background 0.15s;
    cursor: pointer;
    &:hover { color: var(--soc-bar-text-strong, #1a1a1a); background: var(--soc-bar-hover, #f5f3f0); }
  }

  &__title {
    font-family: $mp-font-display;
    font-weight: 700;
    font-size: 1.1rem;
    color: var(--soc-bar-text-strong, #1c1917);
    letter-spacing: -0.01em;
  }

  // Identity section
  &__identity {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 32px 20px 24px;
    text-align: center;
  }

  &__avatar {
    width: 80px;
    height: 80px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    flex-shrink: 0;
    background: var(--soc-avatar-bg, #1a1a1a);
    color: var(--soc-avatar-text, white);
    box-shadow: 0 4px 16px rgba(0,0,0,0.1);
  }

  &__avatar-img { width: 100%; height: 100%; object-fit: cover; }

  &__avatar-initials {
    font-family: $mp-font-display;
    font-weight: 700;
    font-size: 1.4rem;
    color: white;
    letter-spacing: 0.02em;
  }

  &__name {
    margin-top: 16px;
    font-family: $mp-font-display;
    font-weight: 700;
    font-size: 1.2rem;
    color: var(--soc-bar-text-strong, #1a1a1a);
    letter-spacing: -0.02em;
  }

  &__roles { display: flex; gap: 6px; margin-top: 8px; }

  &__role-badge {
    padding: 2px 9px;
    font-family: $mp-font-display;
    font-size: 0.6rem;
    font-weight: 700;
    letter-spacing: 0.04em;
    text-transform: uppercase;
    border-radius: 6px;
    // Default
    color: var(--soc-text-muted, #78716c);
    background: var(--soc-bar-hover, #f5f3f0);

    &--professor { color: #15803d; background: rgba(21, 128, 61, 0.1); }
    &--admin { color: #1d4ed8; background: rgba(29, 78, 216, 0.1); }
    &--member { color: #b45309; background: rgba(180, 83, 9, 0.1); }
  }

  &__dm-btn {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    margin-top: 20px;
    padding: 9px 20px;
    font-family: $mp-font-display;
    font-size: 0.8rem;
    font-weight: 600;
    color: white;
    background: #1a1a1a;
    border-radius: 10px;
    cursor: pointer;
    transition: opacity 0.15s, transform 0.1s;
    &:hover { opacity: 0.85; }
    &:active { transform: scale(0.98); }
    &:disabled { opacity: 0.45; cursor: default; }
  }

  // Cards
  &__card {
    margin: 16px 16px 0;
    background: var(--soc-card-bg, white);
    border: 1px solid var(--soc-card-border, #ece9e4);
    border-radius: 14px;
    overflow: hidden;
    transition: background 0.3s, border-color 0.3s;

    &--danger {
      border-color: rgba(220, 38, 38, 0.2);
    }
  }

  &__card-header {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 14px 18px;
    border-bottom: 1px solid var(--soc-divider, #f0eeeb);
    color: var(--soc-text-muted, #78716c);

    h3 {
      font-family: $mp-font-display;
      font-weight: 600;
      font-size: 0.8rem;
      letter-spacing: 0.02em;
      text-transform: uppercase;
      color: var(--soc-text-muted, #57534e);
    }

    &--danger {
      color: #dc2626;
      h3 { color: #dc2626; }
    }
  }

  &__card-body {
    padding: 16px 18px 18px;
  }

  // Info rows
  &__info-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 9px 0;
    border-bottom: 1px solid var(--soc-divider, #f0f0f0);
    &:last-child { border-bottom: none; padding-bottom: 0; }
    &:first-child { padding-top: 0; }
  }

  &__info-label {
    font-family: $mp-font-body;
    font-size: 0.82rem;
    color: var(--soc-text-muted, #78716c);
  }

  &__info-value {
    font-family: $mp-font-body;
    font-size: 0.82rem;
    font-weight: 500;
    color: var(--soc-bar-text-strong, #1a1a1a);
  }

  // Groups
  &__groups {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  &__group-row {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 10px 10px;
    border-radius: 10px;
    text-decoration: none;
    color: var(--soc-bar-text-strong, #1a1a1a);
    cursor: pointer;
    transition: background 0.15s;
    &:hover { background: var(--soc-bar-hover, #f5f3f0); }
  }

  &__group-img {
    width: 38px;
    height: 38px;
    border-radius: 10px;
    background: var(--soc-notif-bg, #1a1a1a);
    background-size: cover;
    background-position: center;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    font-family: $mp-font-display;
    font-size: 0.55rem;
    font-weight: 700;
    color: var(--soc-notif-text, white);
  }

  &__group-info {
    display: flex;
    flex-direction: column;
    gap: 2px;
    flex: 1;
    min-width: 0;
  }

  &__group-name {
    font-family: $mp-font-display;
    font-size: 0.82rem;
    font-weight: 600;
    color: var(--soc-bar-text-strong, #1a1a1a);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  &__group-season {
    font-size: 0.72rem;
    color: var(--soc-text-muted, #78716c);
  }

  &__group-arrow {
    flex-shrink: 0;
    color: var(--soc-border, #d6d3d1);
    transition: color 0.15s, transform 0.15s;
    .mp__group-row:hover & { color: var(--soc-text-muted, #78716c); transform: translateX(2px); }
  }

  &__empty {
    font-size: 0.82rem;
    color: var(--soc-text-muted, #78716c);
  }

  &__admin-actions {
    display: flex;
    flex-direction: column;
    gap: 10px;
  }

  &__promote-btn, &__demote-btn {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 10px 20px;
    font-family: $mp-font-display;
    font-size: 0.8rem;
    font-weight: 600;
    border-radius: 10px;
    cursor: pointer;
    transition: background 0.15s, border-color 0.15s;
    &:disabled { opacity: 0.5; cursor: default; }
  }

  &__promote-btn {
    color: #15803d;
    background: rgba(21, 128, 61, 0.06);
    border: 1px solid rgba(21, 128, 61, 0.15);
    &:hover { background: rgba(21, 128, 61, 0.12); border-color: rgba(21, 128, 61, 0.25); }
  }

  &__demote-btn {
    color: #b45309;
    background: rgba(180, 83, 9, 0.06);
    border: 1px solid rgba(180, 83, 9, 0.15);
    &:hover { background: rgba(180, 83, 9, 0.12); border-color: rgba(180, 83, 9, 0.25); }
  }

  // Delete button
  &__delete-btn {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 10px 20px;
    font-family: $mp-font-display;
    font-size: 0.8rem;
    font-weight: 600;
    color: #dc2626;
    background: rgba(220, 38, 38, 0.06);
    border: 1px solid rgba(220, 38, 38, 0.15);
    border-radius: 10px;
    cursor: pointer;
    transition: background 0.15s, border-color 0.15s;
    &:hover { background: rgba(220, 38, 38, 0.12); border-color: rgba(220, 38, 38, 0.25); }
    &:disabled { opacity: 0.5; cursor: default; }
  }
}

@keyframes mp-spin { to { transform: rotate(360deg); } }

// Delete confirmation modal
.mp-modal {
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
    background: rgba(220, 38, 38, 0.08);
    border: 2px solid rgba(220, 38, 38, 0.2);
    margin-bottom: 16px;
  }

  &__title {
    font-family: $mp-font-display;
    font-weight: 700;
    font-size: 1.1rem;
    color: var(--soc-bar-text-strong, #1a1a1a);
    margin-bottom: 8px;
  }

  &__text {
    font-size: 0.85rem;
    line-height: 1.5;
    color: var(--soc-text-muted, #78716c);
    margin-bottom: 24px;
    strong { color: var(--soc-bar-text-strong, #1a1a1a); font-weight: 600; }
  }

  &__actions { display: flex; gap: 10px; }

  &__btn {
    flex: 1;
    padding: 11px 16px;
    font-family: $mp-font-display;
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
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 6px;
      background: #dc2626;
      color: white;
      &:hover { background: #b91c1c; }
    }
  }

  &__spinner { animation: mp-spin 0.7s linear infinite; }
}

.mp-modal-enter-active { transition: all 0.2s ease; }
.mp-modal-leave-active { transition: all 0.15s ease; }
.mp-modal-enter-from { opacity: 0; .mp-modal__card { transform: scale(0.95); } }
.mp-modal-leave-to { opacity: 0; .mp-modal__card { transform: scale(0.95); } }
</style>
