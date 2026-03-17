<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>Membres</h1>
    </div>

    <div class="member-search-bar">
      <input
        type="text"
        v-model="search"
        placeholder="Rechercher par nom ou courriel..."
        @input="onSearchInput"
      />
    </div>

    <Loader v-if="isLoading" />

    <table v-else class="admin-table">
      <thead>
        <tr>
          <th>Nom</th>
          <th>Courriel</th>
          <th>Rôles</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-if="members.length === 0">
          <td colspan="4" class="admin-table__empty">Aucun membre trouvé</td>
        </tr>
        <tr v-for="member in members" :key="member.id">
          <td>{{ member.fullName }}</td>
          <td>{{ member.email }}</td>
          <td>
            <span
              v-for="role in member.roles"
              :key="role"
              class="role-badge"
              :class="{ 'role-badge--professor': role === 'Professor' }"
            >
              {{ roleLabels[role] || role }}
            </span>
          </td>
          <td class="admin-table__actions">
            <button
              v-if="!isProfessor(member)"
              class="btn btn--small"
              :disabled="isPromoting"
              @click="promote(member)"
            >
              Promouvoir
            </button>
            <button
              v-else
              class="btn btn--small"
              :disabled="isDemoting"
              @click="demote(member)"
            >
              Rétrograder
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Pagination -->
    <div v-if="totalPages > 1" class="pagination">
      <button
        class="pagination__btn"
        :disabled="currentPage <= 1"
        @click="goToPage(currentPage - 1)"
      >
        &laquo; Précédent
      </button>
      <span class="pagination__info">
        Page {{ currentPage }} / {{ totalPages }}
      </span>
      <button
        class="pagination__btn"
        :disabled="currentPage >= totalPages"
        @click="goToPage(currentPage + 1)"
      >
        Suivant &raquo;
      </button>
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

interface Member {
  id: string
  firstName: string
  lastName: string
  fullName: string
  email: string
  profileImageUrl: string | null
  userId: string
  roles: string[]
}

const roleLabels: Record<string, string> = {
  Member: 'Membre',
  Professor: 'Professeur',
  Admin: 'Administrateur',
}

const isLoading = ref(false)
const isPromoting = ref(false)
const isDemoting = ref(false)
const members = ref<Member[]>([])
const search = ref('')
const currentPage = ref(1)
const pageSize = 20
const totalItems = ref(0)

const totalPages = computed(() => Math.max(1, Math.ceil(totalItems.value / pageSize)))

onMounted(async () => {
  await loadMembers()
})

async function loadMembers() {
  isLoading.value = true
  try {
    const res = await axios.get(`${API}/admin/members`, {
      ...getHeaders(),
      params: {
        Search: search.value || undefined,
        Page: currentPage.value,
        PageSize: pageSize,
      },
    })
    members.value = res.data.items
    totalItems.value = res.data.total
  } catch (e) {
    console.error('Failed to load members', e)
  }
  isLoading.value = false
}

let searchTimeout: ReturnType<typeof setTimeout> | null = null

function onSearchInput() {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    currentPage.value = 1
    loadMembers()
  }, 300)
}

function goToPage(page: number) {
  currentPage.value = page
  loadMembers()
}

function isProfessor(member: Member): boolean {
  return member.roles.includes('Professor')
}

async function promote(member: Member) {
  const confirmed = confirm(`Promouvoir ${member.fullName} au rôle de Professeur ?`)
  if (!confirmed) return

  isPromoting.value = true
  try {
    const res = await axios.post(`${API}/admin/members/${member.id}/promote`, {}, getHeaders())
    if (res.data.succeeded) {
      await loadMembers()
    } else {
      alert('Erreur lors de la promotion.')
    }
  } catch (e) {
    console.error('Failed to promote member', e)
    alert('Erreur lors de la promotion.')
  }
  isPromoting.value = false
}

async function demote(member: Member) {
  const confirmed = confirm(`Rétrograder ${member.fullName} du rôle de Professeur ?`)
  if (!confirmed) return

  isDemoting.value = true
  try {
    const res = await axios.post(`${API}/admin/members/${member.id}/demote`, {}, getHeaders())
    if (res.data.succeeded) {
      await loadMembers()
    } else {
      alert('Erreur lors de la rétrogradation.')
    }
  } catch (e) {
    console.error('Failed to demote member', e)
    alert('Erreur lors de la rétrogradation.')
  }
  isDemoting.value = false
}
</script>

<style scoped>
.member-search-bar {
  margin-bottom: 1.5rem;
  max-width: 400px;
}

.member-search-bar input {
  width: 100%;
}

/* Shared table styles */
.admin-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.admin-table th,
.admin-table td {
  padding: 10px 12px;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}

.admin-table th {
  font-weight: 600;
  background: #f8f8f8;
  white-space: nowrap;
}

.admin-table__empty {
  text-align: center;
  color: #5c5c5c;
  padding: 2rem !important;
}

.admin-table__actions {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.btn--small {
  font-size: 0.75rem;
  padding: 4px 10px;
}

/* Role badges */
.role-badge {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 600;
  background: #e5e7eb;
  color: #374151;
  margin-right: 4px;
}

.role-badge--professor {
  background: #dbeafe;
  color: #1d4ed8;
}

/* Pagination */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  margin-top: 1.5rem;
  padding: 1rem 0;
}

.pagination__btn {
  background: none;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  padding: 6px 14px;
  font-size: 0.875rem;
  cursor: pointer;
  color: #374151;
  transition: background-color 0.15s;
}

.pagination__btn:hover:not(:disabled) {
  background: #f3f4f6;
}

.pagination__btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.pagination__info {
  font-size: 0.875rem;
  color: #5c5c5c;
}

@media (max-width: 767px) {
  .member-search-bar {
    max-width: 100%;
  }

  .admin-table {
    display: block;
    overflow-x: auto;
  }
}
</style>
