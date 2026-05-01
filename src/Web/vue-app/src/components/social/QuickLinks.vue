<template>
  <div v-if="hasAnyLink" class="mb-4 rounded-xl border border-gray-200 overflow-hidden bg-white">
    <div class="px-4 py-3 border-b border-gray-100">
      <h3 class="text-xs font-bold uppercase tracking-wide text-gray-500">Liens rapides</h3>
    </div>
    <div class="divide-y divide-gray-100">
      <a
        v-if="mainSiteUrl"
        :href="mainSiteUrl"
        target="_blank"
        rel="noopener noreferrer"
        class="quick-link"
      >
        <span class="quick-link__icon">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"/>
            <line x1="2" y1="12" x2="22" y2="12"/>
            <path d="M12 2a15.3 15.3 0 014 10 15.3 15.3 0 01-4 10 15.3 15.3 0 01-4-10 15.3 15.3 0 014-10z"/>
          </svg>
        </span>
        <span class="quick-link__label">Site principal</span>
        <svg class="quick-link__chevron" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 18l6-6-6-6"/></svg>
      </a>

      <a
        v-if="phone"
        :href="phoneHref"
        class="quick-link"
      >
        <span class="quick-link__icon">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07 19.5 19.5 0 01-6-6 19.79 19.79 0 01-3.07-8.67A2 2 0 014.11 2h3a2 2 0 012 1.72 12.84 12.84 0 00.7 2.81 2 2 0 01-.45 2.11L8.09 9.91a16 16 0 006 6l1.27-1.27a2 2 0 012.11-.45 12.84 12.84 0 002.81.7A2 2 0 0122 16.92z"/>
          </svg>
        </span>
        <span class="quick-link__label">{{ phone }}</span>
        <svg class="quick-link__chevron" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 18l6-6-6-6"/></svg>
      </a>

      <a
        v-if="email"
        :href="'mailto:' + email"
        class="quick-link"
      >
        <span class="quick-link__icon">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/>
            <polyline points="22,6 12,13 2,6"/>
          </svg>
        </span>
        <span class="quick-link__label">{{ email }}</span>
        <svg class="quick-link__chevron" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 18l6-6-6-6"/></svg>
      </a>

      <button
        v-for="entry in profEntries"
        :key="entry.groupId + ':' + entry.profMemberId"
        @click="openProfDm(entry.profMemberId)"
        :disabled="opening === entry.profMemberId"
        class="quick-link quick-link--button"
      >
        <span class="quick-link__icon">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/>
          </svg>
        </span>
        <span class="quick-link__label">
          <span class="block font-semibold">{{ entry.profName }}</span>
          <span class="block text-[11px] text-gray-500">{{ entry.groupName }}</span>
        </span>
        <svg class="quick-link__chevron" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 18l6-6-6-6"/></svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useSocialService } from '@/serviceRegistry'
import { useSiteSettingsService } from '@/serviceRegistry'
import { useMemberStore } from '@/stores/memberStore'
import { useSocialToast } from '@/composables/useSocialToast'

interface ProfEntry {
  groupId: string
  groupName: string
  profMemberId: string
  profName: string
}

const router = useRouter()
const socialService = useSocialService()
const siteSettingsService = useSiteSettingsService()
const memberStore = useMemberStore()
const toast = useSocialToast()

const phone = ref<string | undefined>(undefined)
const email = ref<string | undefined>(undefined)
const profEntries = ref<ProfEntry[]>([])
const opening = ref<string | null>(null)

const mainSiteUrl = computed(() => {
  const host = window.location.host
  if (host.startsWith('social.')) {
    return `${window.location.protocol}//${host.replace(/^social\./, '')}`
  }
  return null
})

const hasAnyLink = computed(() =>
  !!mainSiteUrl.value || !!phone.value || !!email.value || profEntries.value.length > 0
)

const phoneHref = computed(() =>
  phone.value ? 'tel:' + phone.value.replace(/[^0-9+]/g, '') : ''
)

async function openProfDm(profMemberId: string) {
  if (opening.value) return
  opening.value = profMemberId
  try {
    const conversation = await socialService.startConversation(profMemberId)
    if (conversation && conversation.id) {
      router.push({ name: 'socialConversation', params: { conversationId: conversation.id } })
    } else {
      toast.error("Impossible d'ouvrir la conversation.")
    }
  } catch {
    toast.error("Impossible d'ouvrir la conversation.")
  }
  opening.value = null
}

onMounted(async () => {
  // Site settings (phone, email)
  try {
    const settings = await siteSettingsService.getPublic()
    phone.value = settings?.footerPhone || undefined
    email.value = settings?.footerEmail || undefined
  } catch { /* */ }

  // Profs in user's groups
  try {
    const groups = await socialService.getMyGroups()
    const myId = memberStore.member?.id
    const entries: ProfEntry[] = []
    for (const group of groups) {
      try {
        const members = await socialService.getGroupMembers(group.id, 1)
        for (const m of members) {
          if (m.role === 'Professor' && m.memberId !== myId) {
            entries.push({
              groupId: group.id,
              groupName: group.name,
              profMemberId: m.memberId,
              profName: m.fullName,
            })
          }
        }
      } catch { /* */ }
    }
    profEntries.value = entries
  } catch { /* */ }
})
</script>

<style scoped>
.quick-link {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  padding: 12px 16px;
  font-size: 0.875rem;
  color: var(--soc-bar-text-strong, #1a1a1a);
  background: transparent;
  border: 0;
  text-align: left;
  cursor: pointer;
  transition: background 0.15s;
}
.quick-link:hover {
  background: var(--soc-bar-hover, #f9fafb);
}
.quick-link:disabled {
  opacity: 0.5;
  cursor: default;
}
.quick-link__icon {
  flex-shrink: 0;
  width: 36px;
  height: 36px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
  background: var(--soc-bar-hover, #f5f3f0);
  color: var(--soc-bar-text-strong, #1a1a1a);
}
.quick-link__label {
  flex: 1;
  min-width: 0;
}
.quick-link__chevron {
  flex-shrink: 0;
  color: var(--soc-text-muted, #d6d3d1);
}
</style>
