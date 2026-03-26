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
    <div v-else-if="members.length === 0" class="flex flex-col items-center justify-center gap-3 py-20 text-gray-400">
      <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 00-3-3.87"/><path d="M16 3.13a4 4 0 010 7.75"/></svg>
      <span v-if="searchQuery" class="text-sm">Aucun membre trouvé pour « {{ searchQuery }} »</span>
      <span v-else class="text-sm">Aucun membre pour le moment.</span>
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
        <div class="members-dir__avatar" :style="{ background: member.avatarColor || '#1a1a1a' }">
          <img v-if="member.profileImageUrl" :src="member.profileImageUrl" :alt="member.fullName" class="members-dir__avatar-img" />
          <span v-else class="members-dir__avatar-initials">{{ getInitials(member.fullName) }}</span>
        </div>

        <!-- Info -->
        <div class="members-dir__info">
          <span class="members-dir__name">{{ member.fullName }}</span>
          <span v-if="isAdminRole(member)" class="members-dir__badge members-dir__badge--admin">Admin</span>
          <span v-else-if="isProfessor(member)" class="members-dir__badge members-dir__badge--professor">Prof</span>
          <span v-else class="members-dir__badge members-dir__badge--member">Membre</span>
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
  if (!name || !name.trim()) return '??'
  return name.split(' ').filter(n => n.length > 0).map(n => n[0]).join('').toUpperCase().slice(0, 2)
}

function isProfessor(member: any) {
  return member.roles?.includes('professor')
}

function isAdminRole(member: any) {
  return member.roles?.includes('admin')
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
$dir-font-display: 'Montserrat', sans-serif;
$dir-font-body: 'Karla', sans-serif;

.members-dir {
  padding: 0;

  &__search {
    position: sticky;
    top: 0;
    z-index: 10;
    background: var(--soc-content-bg, white);
    padding: 20px 20px 12px;
    border-bottom: 1px solid var(--soc-border, #e7e0da);
    transition: background 0.3s, border-color 0.3s;
  }

  &__search-inner {
    position: relative;
    display: flex;
    align-items: center;
  }

  &__search-icon {
    position: absolute;
    left: 14px;
    color: var(--soc-text-muted, #78716c);
    pointer-events: none;
  }

  &__search-input {
    width: 100%;
    padding: 12px 40px 12px 44px;
    font-family: $dir-font-body;
    font-size: 0.9rem;
    color: var(--soc-text, #292524);
    background: var(--soc-input-bg, #f5f3f0);
    border: 1px solid transparent;
    border-radius: 12px;
    outline: none;
    transition: border-color 0.15s, background 0.15s, color 0.3s;

    &::placeholder { color: var(--soc-text-muted, #a8a29e); }
    &:focus {
      background: var(--soc-content-bg, white);
      border-color: var(--soc-bar-text-strong, #1a1a1a);
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
    color: var(--soc-text-muted, #78716c);
    cursor: pointer;
    transition: background 0.15s;
    &:hover { background: var(--soc-bar-hover, #f5f3f0); color: var(--soc-bar-text-strong, #1a1a1a); }
  }

  &__count {
    margin-top: 8px;
    font-family: $dir-font-display;
    font-size: 0.7rem;
    font-weight: 600;
    letter-spacing: 0.04em;
    text-transform: uppercase;
    color: var(--soc-text-muted, #78716c);
  }

  &__loading {
    display: flex;
    justify-content: center;
    padding: 60px 0;
  }

  &__spinner {
    width: 24px;
    height: 24px;
    border: 2.5px solid var(--soc-border, #e7e0da);
    border-top-color: var(--soc-bar-text-strong, #1a1a1a);
    border-radius: 50%;
    animation: dir-spin 0.7s linear infinite;
  }

  &__empty {
    padding: 60px 20px;
    text-align: center;
    font-size: 0.9rem;
    color: var(--soc-text-muted, #78716c);
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
    color: var(--soc-text, #292524);
    transition: background 0.15s, color 0.3s;
    animation: dir-fade-in 0.3s ease both;

    &:hover {
      background: var(--soc-bar-hover, #f5f3f0);
    }
  }

  &__avatar {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 44px;
    height: 44px;
    border-radius: 50%;
    flex-shrink: 0;
    background: var(--soc-avatar-bg, #1a1a1a);
    color: var(--soc-avatar-text, white);
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
    color: var(--soc-bar-text-strong, #1a1a1a);
    letter-spacing: -0.01em;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    transition: color 0.3s;
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
    color: var(--soc-text-muted, #78716c);
    background: var(--soc-bar-hover, #f5f3f0);
    border-radius: 6px;
    transition: background 0.3s, color 0.3s;

    &--professor { color: #15803d; background: rgba(21, 128, 61, 0.1); }
    &--admin { color: #1d4ed8; background: rgba(29, 78, 216, 0.1); }
    &--member { color: #b45309; background: rgba(180, 83, 9, 0.1); }
  }

  &__arrow {
    flex-shrink: 0;
    color: var(--soc-border, #d6d3d1);
    transition: color 0.15s, transform 0.15s;

    .members-dir__card:hover & {
      color: var(--soc-text-muted, #78716c);
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
