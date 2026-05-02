<template>
  <section class="soc-account__card">
    <div class="soc-account__card-header">
      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
        <path d="M18 8a6 6 0 00-12 0c0 7-3 9-3 9h18s-3-2-3-9"/>
        <path d="M13.73 21a2 2 0 01-3.46 0"/>
      </svg>
      <h3>Notifications</h3>
    </div>

    <div class="soc-account__card-body">
      <div v-if="!isSupported" class="text-sm" :style="{ color: 'var(--soc-text-muted)' }">
        Votre navigateur ne supporte pas les notifications push.
      </div>

      <div v-else-if="!isPwaInstalled">
        <p class="text-sm mb-3" :style="{ color: 'var(--soc-text)' }">
          Pour recevoir des notifications, vous devez d'abord ajouter l'application à votre écran d'accueil.
        </p>
        <div v-if="isIOS" class="text-sm space-y-2" :style="{ color: 'var(--soc-text-muted)' }">
          <p><strong>Sur iPhone :</strong></p>
          <ol class="list-decimal pl-5 space-y-1">
            <li>Touchez le bouton <strong>Partager</strong> en bas de Safari</li>
            <li>Sélectionnez <strong>« Sur l'écran d'accueil »</strong></li>
            <li>Ouvrez l'application depuis l'icône de votre écran d'accueil</li>
          </ol>
        </div>
        <div v-else-if="isAndroid" class="text-sm space-y-2" :style="{ color: 'var(--soc-text-muted)' }">
          <p><strong>Sur Android :</strong></p>
          <ol class="list-decimal pl-5 space-y-1">
            <li>Touchez le menu <strong>⋮</strong> de Chrome</li>
            <li>Sélectionnez <strong>« Installer l'application »</strong></li>
            <li>Ouvrez l'application depuis l'icône de votre écran d'accueil</li>
          </ol>
        </div>
        <div v-else class="text-sm space-y-2" :style="{ color: 'var(--soc-text-muted)' }">
          <p><strong>Sur ordinateur :</strong> Cliquez sur l'icône d'installation dans la barre d'adresse de votre navigateur.</p>
        </div>
      </div>

      <div v-else-if="!isSubscribed">
        <p class="text-sm mb-3" :style="{ color: 'var(--soc-text)' }">
          Activez les notifications sur cet appareil pour recevoir messages, publications et annonces.
        </p>
        <button
          type="button"
          class="soc-account__btn-primary inline-flex items-center gap-2"
          :disabled="isSubscribing"
          @click="onActivate"
        >
          <span>{{ isSubscribing ? 'Activation...' : 'Activer sur cet appareil' }}</span>
        </button>
        <p v-if="permission === 'denied'" class="text-sm mt-3 text-red-600">
          Les notifications ont été bloquées. Pour les réactiver, allez dans les paramètres de votre navigateur.
        </p>
      </div>

      <div v-else class="space-y-3">
        <ToggleRow v-model="prefs.directMessage" label="Messages privés" @update:modelValue="savePrefs" />
        <ToggleRow v-model="prefs.announcement" label="Annonces" @update:modelValue="savePrefs" />

        <div>
          <ToggleRow v-model="prefs.groupPost" label="Publications dans mes groupes" @update:modelValue="savePrefs" />

          <div v-if="prefs.groupPost && groups.length > 0" class="mt-3 ml-4 pl-3 border-l-2 space-y-2" :style="{ borderColor: 'var(--soc-border)' }">
            <ToggleRow
              v-for="g in groups"
              :key="g.id"
              :model-value="!mutedGroupIds.has(g.id)"
              :label="g.name"
              size="sm"
              @update:model-value="(v: boolean) => onGroupToggle(g.id, v)"
            />
          </div>
        </div>

        <div class="pt-2">
          <button
            type="button"
            class="text-xs text-red-600 hover:underline"
            @click="onDeactivate"
          >
            Désactiver les notifications sur cet appareil
          </button>
        </div>
      </div>
    </div>
  </section>
</template>

<script lang="ts" setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { usePushSubscription } from '@/composables/usePushSubscription'
import { usePushService, useSocialService } from '@/serviceRegistry'
import ToggleRow from './ToggleRow.vue'

const pushService = usePushService()
const socialService = useSocialService()

const { permission, isSupported, isPwaInstalled, isSubscribed, isSubscribing, subscribe, unsubscribe, refreshState } = usePushSubscription()

const prefs = reactive<{ directMessage: boolean; announcement: boolean; groupPost: boolean }>({
  directMessage: true,
  announcement: true,
  groupPost: true
})

const mutedGroupIds = ref(new Set<string>())
const groups = ref<{ id: string; name: string }[]>([])

const isIOS = computed(() => /iPad|iPhone|iPod/.test(navigator.userAgent) && !(window as unknown as { MSStream?: unknown }).MSStream)
const isAndroid = computed(() => /Android/.test(navigator.userAgent))

async function loadPrefs() {
  if (!isSubscribed.value) return
  const data = await pushService.getPreferences()
  prefs.directMessage = data.directMessage
  prefs.announcement = data.announcement
  prefs.groupPost = data.groupPost
  mutedGroupIds.value = new Set(data.mutedGroups.map(m => m.groupId))
}

async function loadGroups() {
  const list = await socialService.getMyGroups()
  groups.value = list.map((g: { id: string; name: string }) => ({ id: g.id, name: g.name }))
}

async function savePrefs() {
  await pushService.updatePreferences({
    directMessage: prefs.directMessage,
    announcement: prefs.announcement,
    groupPost: prefs.groupPost
  })
}

async function onGroupToggle(groupId: string, enabled: boolean) {
  await pushService.updateGroupPreference(groupId, enabled)
  if (enabled) mutedGroupIds.value.delete(groupId)
  else mutedGroupIds.value.add(groupId)
}

async function onActivate() {
  const ok = await subscribe()
  if (ok) {
    await loadPrefs()
    await loadGroups()
  }
}

async function onDeactivate() {
  await unsubscribe()
}

onMounted(async () => {
  await refreshState()
  if (isSubscribed.value) {
    await loadPrefs()
    await loadGroups()
  }
})
</script>
