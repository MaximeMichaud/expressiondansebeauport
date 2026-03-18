<template>
  <div class="members-dir">
    <!-- Search -->
    <div class="members-dir__search">
      <div class="members-dir__search-inner">
        <svg class="members-dir__search-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="11" cy="11" r="8"/><path d="M21 21l-4.35-4.35"/>
        </svg>
        <input
          v-model="searchQuery"
          type="text"
          class="members-dir__search-input"
          placeholder="Rechercher un membre..."
          @input="onSearch"
        />
        <span v-if="searchQuery" class="members-dir__search-clear" @click="clearSearch">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round"><path d="M18 6L6 18M6 6l12 12"/></svg>
        </span>
      </div>
      <p class="members-dir__count" v-if="!loading">{{ members.length }} membre{{ members.length !== 1 ? 's' : '' }}</p>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="members-dir__loading">
      <div class="members-dir__spinner" />
    </div>

    <!-- Empty -->
    <div v-else-if="members.length === 0" class="members-dir__empty">
      <p v-if="searchQuery">Aucun membre trouvé pour « {{ searchQuery }} »</p>
      <p v-else>Aucun membre pour le moment.</p>
    </div>

    <!-- Grid -->
    <div v-else class="members-dir__grid">
      <router-link
        v-for="(member, i) in members"
        :key="member.id"
        :to="{ name: 'socialMemberProfile', params: { id: member.id } }"
        class="members-dir__card"
        :style="{ animationDelay: `${i * 30}ms` }"
      >
        <!-- Avatar -->
        <div class="members-dir__avatar" :style="{ background: getAvatarColor(member.fullName) }">
          <img v-if="member.profileImageUrl" :src="member.profileImageUrl" :alt="member.fullName" class="members-dir__avatar-img" />
          <span v-else class="members-dir__avatar-initials">{{ getInitials(member.fullName) }}</span>
        </div>

        <!-- Info -->
        <div class="members-dir__info">
          <span class="members-dir__name">{{ member.fullName }}</span>
          <span v-if="isProfessor(member)" class="members-dir__badge">Professeur</span>
        </div>

        <!-- Arrow -->
        <svg class="members-dir__arrow" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M9 18l6-6-6-6"/>
        </svg>
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useSocialService } from '@/inversify.config'

const socialService = useSocialService()

const members = ref<any[]>([])
const loading = ref(true)
const searchQuery = ref('')
let searchTimeout: ReturnType<typeof setTimeout> | null = null

function getInitials(name: string) {
  return name?.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2) || '?'
}

const avatarColors = [
  '#1a1a1a', '#3b3b3b', '#6b4c3b', '#4a5568', '#2d3748',
  '#553c2e', '#44403c', '#1e293b', '#374151', '#292524'
]

function getAvatarColor(name: string) {
  let hash = 0
  for (let i = 0; i < (name?.length || 0); i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

function isProfessor(member: any) {
  return member.roles?.includes('professor')
}

async function loadMembers(query?: string) {
  loading.value = true
  try {
    members.value = await socialService.searchMembers(query || '')
  } catch { members.value = [] }
  loading.value = false
}

function onSearch() {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => loadMembers(searchQuery.value), 300)
}

function clearSearch() {
  searchQuery.value = ''
  loadMembers()
}

onMounted(() => loadMembers())
</script>

<style lang="scss">
$dir-black: #1a1a1a;
$dir-warm: #f5f3f0;
$dir-border: #e7e0da;
$dir-muted: #78716c;
$dir-font-display: 'Montserrat', sans-serif;
$dir-font-body: 'Karla', sans-serif;

.members-dir {
  padding: 0;

  &__search {
    position: sticky;
    top: 0;
    z-index: 10;
    background: white;
    padding: 20px 20px 12px;
    border-bottom: 1px solid $dir-border;
  }

  &__search-inner {
    position: relative;
    display: flex;
    align-items: center;
  }

  &__search-icon {
    position: absolute;
    left: 14px;
    color: $dir-muted;
    pointer-events: none;
  }

  &__search-input {
    width: 100%;
    padding: 12px 40px 12px 44px;
    font-family: $dir-font-body;
    font-size: 0.9rem;
    color: $dir-black;
    background: $dir-warm;
    border: 1px solid transparent;
    border-radius: 12px;
    outline: none;
    transition: border-color 0.15s, background 0.15s;

    &::placeholder { color: #a8a29e; }
    &:focus {
      background: white;
      border-color: $dir-black;
    }
  }

  &__search-clear {
    position: absolute;
    right: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 24px;
    height: 24px;
    border-radius: 50%;
    color: $dir-muted;
    cursor: pointer;
    transition: background 0.15s;
    &:hover { background: $dir-warm; color: $dir-black; }
  }

  &__count {
    margin-top: 8px;
    font-family: $dir-font-display;
    font-size: 0.7rem;
    font-weight: 600;
    letter-spacing: 0.04em;
    text-transform: uppercase;
    color: $dir-muted;
  }

  &__loading {
    display: flex;
    justify-content: center;
    padding: 60px 0;
  }

  &__spinner {
    width: 24px;
    height: 24px;
    border: 2.5px solid $dir-border;
    border-top-color: $dir-black;
    border-radius: 50%;
    animation: dir-spin 0.7s linear infinite;
  }

  &__empty {
    padding: 60px 20px;
    text-align: center;
    font-size: 0.9rem;
    color: $dir-muted;
  }

  &__grid {
    padding: 8px 12px 20px;
  }

  &__card {
    display: flex;
    align-items: center;
    gap: 14px;
    padding: 12px 14px;
    border-radius: 12px;
    text-decoration: none;
    color: $dir-black;
    transition: background 0.15s;
    animation: dir-fade-in 0.3s ease both;

    &:hover {
      background: $dir-warm;
    }
  }

  &__avatar {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 44px;
    height: 44px;
    border-radius: 12px;
    flex-shrink: 0;
    overflow: hidden;
  }

  &__avatar-img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  &__avatar-initials {
    font-family: $dir-font-display;
    font-weight: 700;
    font-size: 0.8rem;
    color: white;
    letter-spacing: 0.02em;
  }

  &__info {
    display: flex;
    flex-direction: column;
    gap: 3px;
    flex: 1;
    min-width: 0;
  }

  &__name {
    font-family: $dir-font-display;
    font-weight: 600;
    font-size: 0.88rem;
    color: $dir-black;
    letter-spacing: -0.01em;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  &__badge {
    display: inline-flex;
    align-self: flex-start;
    padding: 2px 8px;
    font-family: $dir-font-display;
    font-size: 0.62rem;
    font-weight: 700;
    letter-spacing: 0.03em;
    text-transform: uppercase;
    color: $dir-black;
    background: $dir-warm;
    border-radius: 6px;
  }

  &__arrow {
    flex-shrink: 0;
    color: #d6d3d1;
    transition: color 0.15s, transform 0.15s;

    .members-dir__card:hover & {
      color: $dir-muted;
      transform: translateX(2px);
    }
  }
}

@keyframes dir-spin { to { transform: rotate(360deg); } }
@keyframes dir-fade-in {
  from { opacity: 0; transform: translateY(6px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
