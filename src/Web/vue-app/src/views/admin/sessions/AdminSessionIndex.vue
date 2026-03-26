<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>Sessions</h1>
    </div>

    <Loader v-if="isLoading" />

    <div v-else-if="seasons.length === 0" class="sessions-empty">
      Aucune session trouvée.
    </div>

    <div v-else class="sessions-list">
      <div v-for="season in seasonsWithGroups" :key="season.name" class="session-card">
        <div class="session-card__info">
          <h3>{{ season.name }}</h3>
          <p class="session-card__count">
            {{ season.groupCount }} groupe{{ season.groupCount !== 1 ? 's' : '' }}
            <span v-if="season.archivedCount > 0" class="session-card__archived">
              ({{ season.archivedCount }} archivé{{ season.archivedCount !== 1 ? 's' : '' }})
            </span>
          </p>
        </div>
        <div class="session-card__actions">
          <button
            v-if="season.activeCount > 0"
            class="btn btn--small"
            :disabled="isArchiving"
            @click="archiveSeason(season.name)"
          >
            Archiver
          </button>
          <span v-else class="session-card__status">Tous archivés</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref, computed } from 'vue'
import axios from 'axios'
import Cookies from 'universal-cookie'
import Loader from '@/components/layouts/items/Loader.vue'

const API = import.meta.env.VITE_API_BASE_URL

function getHeaders() {
  const token = new Cookies().get('accessToken')
  return {
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  }
}

interface Group {
  id: string
  name: string
  season: string
  isArchived: boolean
}

const isLoading = ref(false)
const isArchiving = ref(false)
const seasons = ref<string[]>([])
const allGroups = ref<Group[]>([])

const seasonsWithGroups = computed(() => {
  return seasons.value.map(name => {
    const groupsInSeason = allGroups.value.filter(g => g.season === name)
    const archivedCount = groupsInSeason.filter(g => g.isArchived).length
    return {
      name,
      groupCount: groupsInSeason.length,
      archivedCount,
      activeCount: groupsInSeason.length - archivedCount,
    }
  })
})

onMounted(async () => {
  await loadData()
})

async function loadData() {
  isLoading.value = true
  try {
    const [seasonsRes, groupsRes] = await Promise.all([
      axios.get(`${API}/admin/groups/seasons`, getHeaders()),
      axios.get(`${API}/admin/groups`, getHeaders()),
    ])
    seasons.value = seasonsRes.data
    allGroups.value = groupsRes.data
  } catch (e) {
    console.error('Failed to load sessions data', e)
  }
  isLoading.value = false
}

async function archiveSeason(seasonName: string) {
  const confirmed = confirm(`Archiver tous les groupes de la session "${seasonName}" ?`)
  if (!confirmed) return

  isArchiving.value = true
  try {
    await axios.post(`${API}/admin/groups/archive`, { season: seasonName }, getHeaders())
    await loadData()
  } catch (e) {
    console.error('Failed to archive season', e)
    alert('Erreur lors de l\'archivage de la session.')
  }
  isArchiving.value = false
}
</script>

<style scoped>
.sessions-empty {
  color: #5c5c5c;
  font-size: 0.875rem;
  padding: 2rem 0;
}

.sessions-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-width: 600px;
}

.session-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 16px 20px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  background: #fff;
}

.session-card__info h3 {
  margin: 0 0 4px;
  font-size: 1rem;
}

.session-card__count {
  margin: 0;
  font-size: 0.875rem;
  color: #5c5c5c;
}

.session-card__archived {
  color: #9ca3af;
}

.session-card__actions {
  flex-shrink: 0;
}

.session-card__status {
  font-size: 0.8125rem;
  color: #9ca3af;
  font-style: italic;
}

.btn--small {
  font-size: 0.75rem;
  padding: 4px 10px;
}

@media (max-width: 767px) {
  .sessions-list {
    max-width: 100%;
  }

  .session-card {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
