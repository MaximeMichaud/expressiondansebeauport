<template>
  <div v-if="hasAnyLink" class="ql-card rounded-xl overflow-hidden">
    <!-- Footer-style inline links -->
    <div v-if="mainSiteUrl || phone || email" class="ql-strip" :class="{ 'ql-strip--bordered': profEntries.length }">
      <a v-if="phone" :href="phoneHref" class="ql-link">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M22 16.92v3a2 2 0 01-2.18 2 19.79 19.79 0 01-8.63-3.07 19.5 19.5 0 01-6-6 19.79 19.79 0 01-3.07-8.67A2 2 0 014.11 2h3a2 2 0 012 1.72 12.84 12.84 0 00.7 2.81 2 2 0 01-.45 2.11L8.09 9.91a16 16 0 006 6l1.27-1.27a2 2 0 012.11-.45 12.84 12.84 0 002.81.7A2 2 0 0122 16.92z"/>
        </svg>
        {{ phone }}
      </a>
      <a v-if="email" :href="'mailto:' + email" class="ql-link">
        <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/>
          <polyline points="22,6 12,13 2,6"/>
        </svg>
        {{ email }}
      </a>
      <a v-if="mainSiteUrl" :href="mainSiteUrl" target="_blank" rel="noopener noreferrer" class="ql-link">
        Site principal
        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="7" y1="17" x2="17" y2="7"/><polyline points="7 7 17 7 17 17"/></svg>
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
          <span class="mt-1 block text-[11px] text-gray-500 leading-tight">{{ entry.groupNames.join(', ') }}</span>
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
  return '/'
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

<style scoped lang="scss">
.ql-card {
  background: #d6e8f7;
  border: 1px solid #a3c4e9;
  color: var(--soc-text, #292524);
}
:global(.soc--dark) .ql-card {
  background: rgba(59, 130, 246, 0.12);
  border-color: rgba(59, 130, 246, 0.3);
}

.ql-strip {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 12px 16px;
  font-size: 0.78rem;
  color: var(--soc-bar-text, #78716c);
}
.ql-strip--bordered {
  border-bottom: 1px solid var(--soc-divider, #f0f0f0);
}
.ql-link {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  color: var(--soc-bar-text, #78716c);
  text-decoration: none;
  font-weight: 500;
  cursor: pointer;
  transition: color 0.15s;
  white-space: nowrap;
}
.ql-link:hover {
  color: var(--soc-bar-text-strong, #1a1a1a);
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
