<template>
  <div class="member-profile">
    <!-- Loading -->
    <div v-if="loading" class="member-profile__loading">
      <div class="member-profile__spinner" />
    </div>

    <template v-else-if="member">
      <!-- Header -->
      <div class="member-profile__header">
        <button @click="$router.back()" class="member-profile__back">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 19l-7-7 7-7"/></svg>
        </button>

        <div class="member-profile__avatar" :style="{ background: getAvatarColor(member.fullName) }">
          <img v-if="member.profileImageUrl" :src="member.profileImageUrl" :alt="member.fullName" class="member-profile__avatar-img" />
          <span v-else class="member-profile__avatar-initials">{{ getInitials(member.fullName) }}</span>
        </div>

        <h1 class="member-profile__name">{{ member.fullName }}</h1>

        <div class="member-profile__roles">
          <span v-for="role in member.roles" :key="role" class="member-profile__role-badge">
            {{ getRoleLabel(role) }}
          </span>
        </div>

        <button @click="startDM" :disabled="startingDM" class="member-profile__dm-btn">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/>
          </svg>
          {{ startingDM ? '...' : 'Envoyer un message' }}
        </button>
      </div>

      <!-- Info -->
      <div class="member-profile__section">
        <h2 class="member-profile__section-title">Informations</h2>
        <div class="member-profile__info-row">
          <span class="member-profile__info-label">Courriel</span>
          <span class="member-profile__info-value">{{ member.email }}</span>
        </div>
      </div>

      <!-- Groups -->
      <div v-if="member.groups && member.groups.length" class="member-profile__section">
        <h2 class="member-profile__section-title">Groupes</h2>
        <div class="member-profile__groups">
          <router-link
            v-for="group in member.groups"
            :key="group.id"
            :to="{ name: 'socialGroup', params: { id: group.id } }"
            class="member-profile__group-card"
          >
            <div class="member-profile__group-img" :style="group.imageUrl ? { backgroundImage: `url(${group.imageUrl})` } : {}">
              <span v-if="!group.imageUrl">EDB</span>
            </div>
            <div class="member-profile__group-info">
              <span class="member-profile__group-name">{{ group.name }}</span>
              <span class="member-profile__group-season">{{ group.season }}</span>
            </div>
          </router-link>
        </div>
      </div>
      <div v-else class="member-profile__section">
        <h2 class="member-profile__section-title">Groupes</h2>
        <p class="member-profile__empty">Aucun groupe en commun.</p>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSocialService } from '@/inversify.config'

const route = useRoute()
const router = useRouter()
const socialService = useSocialService()

const member = ref<any>(null)
const loading = ref(true)
const startingDM = ref(false)

function getInitials(name: string) {
  return name?.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2) || '?'
}

const avatarColors = ['#1a1a1a', '#3b3b3b', '#6b4c3b', '#4a5568', '#2d3748', '#553c2e', '#44403c', '#1e293b', '#374151', '#292524']
function getAvatarColor(name: string) {
  let hash = 0
  for (let i = 0; i < (name?.length || 0); i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

function getRoleLabel(role: string) {
  const labels: Record<string, string> = { admin: 'Admin', professor: 'Professeur', member: 'Membre' }
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

onMounted(async () => {
  try {
    member.value = await socialService.getMemberProfile(route.params.id as string)
  } catch { /* */ }
  loading.value = false
})
</script>

<style lang="scss">
$mp-black: #1a1a1a;
$mp-warm: #f5f3f0;
$mp-border: #e7e0da;
$mp-muted: #78716c;
$mp-font-display: 'Montserrat', sans-serif;
$mp-font-body: 'Karla', sans-serif;

.member-profile {
  &__loading {
    display: flex;
    justify-content: center;
    padding: 80px 0;
  }

  &__spinner {
    width: 24px;
    height: 24px;
    border: 2.5px solid $mp-border;
    border-top-color: $mp-black;
    border-radius: 50%;
    animation: mp-spin 0.7s linear infinite;
  }

  &__header {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 32px 24px 28px;
    border-bottom: 1px solid $mp-border;
    position: relative;
  }

  &__back {
    position: absolute;
    top: 20px;
    left: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    border-radius: 10px;
    color: $mp-muted;
    cursor: pointer;
    transition: background 0.15s, color 0.15s;
    &:hover { background: $mp-warm; color: $mp-black; }
  }

  &__avatar {
    width: 80px;
    height: 80px;
    border-radius: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    box-shadow: 0 4px 16px rgba(0,0,0,0.1);
  }

  &__avatar-img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

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
    font-size: 1.25rem;
    color: $mp-black;
    letter-spacing: -0.02em;
  }

  &__roles {
    display: flex;
    gap: 6px;
    margin-top: 8px;
  }

  &__role-badge {
    padding: 3px 10px;
    font-family: $mp-font-display;
    font-size: 0.65rem;
    font-weight: 700;
    letter-spacing: 0.04em;
    text-transform: uppercase;
    color: $mp-black;
    background: $mp-warm;
    border-radius: 8px;
  }

  &__dm-btn {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    margin-top: 20px;
    padding: 10px 22px;
    font-family: $mp-font-display;
    font-size: 0.82rem;
    font-weight: 600;
    color: white;
    background: $mp-black;
    border-radius: 10px;
    cursor: pointer;
    transition: background 0.15s, transform 0.1s;
    &:hover { background: #333; }
    &:active { transform: scale(0.98); }
    &:disabled { opacity: 0.5; cursor: default; }
  }

  &__section {
    padding: 24px 24px 20px;
    border-bottom: 1px solid $mp-border;
  }

  &__section-title {
    font-family: $mp-font-display;
    font-size: 0.7rem;
    font-weight: 700;
    letter-spacing: 0.06em;
    text-transform: uppercase;
    color: $mp-muted;
    margin-bottom: 14px;
  }

  &__info-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 0;
  }

  &__info-label {
    font-family: $mp-font-body;
    font-size: 0.85rem;
    color: $mp-muted;
  }

  &__info-value {
    font-family: $mp-font-body;
    font-size: 0.85rem;
    font-weight: 500;
    color: $mp-black;
  }

  &__groups {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  &__group-card {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 10px 12px;
    border-radius: 12px;
    text-decoration: none;
    color: $mp-black;
    transition: background 0.15s;
    &:hover { background: $mp-warm; }
  }

  &__group-img {
    width: 40px;
    height: 40px;
    border-radius: 10px;
    background: $mp-black;
    background-size: cover;
    background-position: center;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    font-family: $mp-font-display;
    font-size: 0.6rem;
    font-weight: 700;
    color: white;
  }

  &__group-info {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  &__group-name {
    font-family: $mp-font-display;
    font-size: 0.85rem;
    font-weight: 600;
    color: $mp-black;
  }

  &__group-season {
    font-size: 0.75rem;
    color: $mp-muted;
  }

  &__empty {
    font-size: 0.85rem;
    color: $mp-muted;
  }
}

@keyframes mp-spin { to { transform: rotate(360deg); } }
</style>
