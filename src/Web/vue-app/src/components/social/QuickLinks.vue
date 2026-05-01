<template>
  <div v-if="hasAnyLink" class="mb-4 rounded-xl border border-gray-200 overflow-hidden bg-white">
    <!-- Compact icon row -->
    <div v-if="mainSiteUrl || phone || email" class="flex items-center gap-2 p-3" :class="{ 'border-b border-gray-100': profEntries.length }">
      <a v-if="mainSiteUrl" :href="mainSiteUrl" target="_blank" rel="noopener noreferrer" class="quick-icon" title="Site principal">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="10"/>
          <line x1="2" y1="12" x2="22" y2="12"/>
          <path d="M12 2a15.3 15.3 0 014 10 15.3 15.3 0 01-4 10 15.3 15.3 0 01-4-10 15.3 15.3 0 014-10z"/>
        </svg>
      </a>
      <a v-if="phone" :href="phoneHref" class="quick-icon" :title="phone">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07 19.5 19.5 0 01-6-6 19.79 19.79 0 01-3.07-8.67A2 2 0 014.11 2h3a2 2 0 012 1.72 12.84 12.84 0 00.7 2.81 2 2 0 01-.45 2.11L8.09 9.91a16 16 0 006 6l1.27-1.27a2 2 0 012.11-.45 12.84 12.84 0 002.81.7A2 2 0 0122 16.92z"/>
        </svg>
      </a>
      <a v-if="email" :href="'mailto:' + email" class="quick-icon" :title="email">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/>
          <polyline points="22,6 12,13 2,6"/>
        </svg>
      </a>
    </div>

    <!-- DM profs (compact rows) -->
    <div v-if="profEntries.length" class="divide-y divide-gray-100">
      <button
        v-for="entry in profEntries"
        :key="entry.profMemberId"
        @click="openProfDm(entry.profMemberId)"
        :disabled="opening === entry.profMemberId"
        class="quick-row"
      >
        <span class="quick-row__icon">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/>
          </svg>
        </span>
        <span class="quick-row__label">
          <span class="block text-[13px] font-semibold leading-tight">{{ entry.profName }}</span>
          <span
            v-for="name in entry.groupNames"
            :key="name"
            class="block text-[11px] text-gray-500 leading-tight"
          >{{ name }}</span>
        </span>
        <svg class="quick-row__chevron" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 18l6-6-6-6"/></svg>
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
  profMemberId: string
  profName: string
  groupNames: string[]
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
  try {
    const settings = await siteSettingsService.getPublic()
    phone.value = settings?.footerPhone || undefined
    email.value = settings?.footerEmail || undefined
  } catch { /* */ }

  try {
    const groups = await socialService.getMyGroups()
    const myId = memberStore.member?.id
    const byProf = new Map<string, ProfEntry>()
    for (const group of groups) {
      try {
        const members = await socialService.getGroupMembers(group.id, 1)
        for (const m of members as any[]) {
          const isProf = (m.roles || []).includes('professor') || m.role === 'Professor'
          if (isProf && m.memberId !== myId) {
            const existing = byProf.get(m.memberId)
            if (existing) {
              if (!existing.groupNames.includes(group.name)) existing.groupNames.push(group.name)
            } else {
              byProf.set(m.memberId, {
                profMemberId: m.memberId,
                profName: m.fullName,
                groupNames: [group.name],
              })
            }
          }
        }
      } catch { /* */ }
    }
    profEntries.value = Array.from(byProf.values())
  } catch { /* */ }
})
</script>

<style scoped>
.quick-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: var(--soc-bar-hover, #f5f3f0);
  color: var(--soc-bar-text-strong, #1a1a1a);
  transition: background 0.15s, color 0.15s;
  cursor: pointer;
}
.quick-icon:hover {
  background: var(--soc-bar-active, #eae8e4);
}

.quick-row {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 8px 12px;
  font-size: 0.85rem;
  color: var(--soc-bar-text-strong, #1a1a1a);
  background: transparent;
  border: 0;
  text-align: left;
  cursor: pointer;
  transition: background 0.15s;
}
.quick-row:hover {
  background: var(--soc-bar-hover, #f9fafb);
}
.quick-row:disabled {
  opacity: 0.5;
  cursor: default;
}
.quick-row__icon {
  flex-shrink: 0;
  width: 28px;
  height: 28px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  background: var(--soc-bar-hover, #f5f3f0);
  color: var(--soc-bar-text-strong, #1a1a1a);
}
.quick-row__label {
  flex: 1;
  min-width: 0;
}
.quick-row__chevron {
  flex-shrink: 0;
  color: var(--soc-text-muted, #d6d3d1);
}
</style>
