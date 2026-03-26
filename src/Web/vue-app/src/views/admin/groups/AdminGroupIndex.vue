<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>Groupes</h1>
      <button class="btn" @click="showCreateForm = !showCreateForm">
        {{ showCreateForm ? 'Annuler' : 'Nouveau groupe' }}
      </button>
    </div>

    <!-- Create group form -->
    <div v-if="showCreateForm" class="form card group-create-form">
      <h3>Créer un groupe</h3>
      <div class="form__field">
        <label>Nom</label>
        <input type="text" v-model="newGroup.name" placeholder="Nom du groupe" />
      </div>
      <div class="form__field">
        <label>Description</label>
        <input type="text" v-model="newGroup.description" placeholder="Description (optionnel)" />
      </div>
      <div class="form__field">
        <label>Session</label>
        <input type="text" v-model="newGroup.season" placeholder="ex: Hiver 2026" />
      </div>
      <div class="form__field">
        <label>Code d'invitation</label>
        <input type="text" v-model="newGroup.inviteCode" placeholder="Code (optionnel)" />
      </div>
      <button class="btn" :disabled="isCreating" @click="createGroup">Créer</button>
    </div>

    <!-- Archive season -->
    <div class="group-archive-bar">
      <label>Archiver une session :</label>
      <input type="text" v-model="archiveSeason" placeholder="ex: Hiver 2026" />
      <button class="btn" :disabled="isArchiving" @click="archiveSeasonAction">Archiver</button>
    </div>

    <Loader v-if="isLoading" />

    <table v-else class="admin-table">
      <thead>
        <tr>
          <th>Nom</th>
          <th>Session</th>
          <th>Code d'invitation</th>
          <th>Membres</th>
          <th>Archivé</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-if="groups.length === 0">
          <td colspan="6" class="admin-table__empty">Aucun groupe</td>
        </tr>
        <tr v-for="group in groups" :key="group.id">
          <td>{{ group.name }}</td>
          <td>{{ group.season }}</td>
          <td>{{ group.inviteCode || '—' }}</td>
          <td>{{ group.memberCount }}</td>
          <td>{{ group.isArchived ? 'Oui' : 'Non' }}</td>
          <td class="admin-table__actions">
            <button class="btn btn--small" @click="startEdit(group)">Modifier</button>
            <button class="btn btn--small" @click="openAssignProfessor(group)">Professeur</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Edit modal -->
    <div v-if="editingGroup" class="modal-overlay" @click.self="editingGroup = null">
      <div class="form modal-content">
        <h3>Modifier le groupe</h3>
        <div class="form__field">
          <label>Nom</label>
          <input type="text" v-model="editingGroup.name" />
        </div>
        <div class="form__field">
          <label>Description</label>
          <input type="text" v-model="editingGroup.description" />
        </div>
        <div class="form__field">
          <label>Session</label>
          <input type="text" v-model="editingGroup.season" />
        </div>
        <div class="modal-actions">
          <button class="btn" :disabled="isSaving" @click="saveEdit">Sauvegarder</button>
          <button class="modal-cancel" @click="editingGroup = null">Annuler</button>
        </div>
      </div>
    </div>

    <!-- Assign professor modal -->
    <div v-if="assigningGroup" class="modal-overlay" @click.self="assigningGroup = null">
      <div class="form modal-content">
        <h3>Assigner un professeur — {{ assigningGroup.name }}</h3>
        <div class="form__field">
          <label>Rechercher un membre</label>
          <input type="text" v-model="professorSearch" placeholder="Nom ou courriel" @input="searchProfessors" />
        </div>
        <ul v-if="professorResults.length" class="professor-results">
          <li v-for="member in professorResults" :key="member.id" class="professor-result-item">
            <span>{{ member.fullName }} ({{ member.email }})</span>
            <button class="btn btn--small" @click="assignProfessor(member.id)">Assigner</button>
          </li>
        </ul>
        <div class="modal-actions">
          <button class="modal-cancel" @click="assigningGroup = null">Fermer</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref } from 'vue'
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
  description: string | null
  imageUrl: string | null
  inviteCode: string | null
  season: string
  isArchived: boolean
  memberCount: number
}

interface MemberResult {
  id: string
  fullName: string
  email: string
}

const isLoading = ref(false)
const isCreating = ref(false)
const isSaving = ref(false)
const isArchiving = ref(false)
const groups = ref<Group[]>([])
const showCreateForm = ref(false)
const archiveSeason = ref('')

const newGroup = ref({
  name: '',
  description: '',
  season: '',
  inviteCode: '',
})

const editingGroup = ref<Group | null>(null)
const assigningGroup = ref<Group | null>(null)
const professorSearch = ref('')
const professorResults = ref<MemberResult[]>([])

onMounted(async () => {
  await loadGroups()
})

async function loadGroups() {
  isLoading.value = true
  try {
    const res = await axios.get(`${API}/admin/groups`, getHeaders())
    groups.value = res.data
  } catch (e) {
    console.error('Failed to load groups', e)
  }
  isLoading.value = false
}

async function createGroup() {
  if (!newGroup.value.name || !newGroup.value.season) {
    alert('Le nom et la session sont requis.')
    return
  }
  isCreating.value = true
  try {
    await axios.post(`${API}/admin/groups`, newGroup.value, getHeaders())
    newGroup.value = { name: '', description: '', season: '', inviteCode: '' }
    showCreateForm.value = false
    await loadGroups()
  } catch (e) {
    console.error('Failed to create group', e)
    alert('Erreur lors de la création du groupe.')
  }
  isCreating.value = false
}

async function archiveSeasonAction() {
  if (!archiveSeason.value) {
    alert('Veuillez entrer le nom de la session à archiver.')
    return
  }
  const confirmed = confirm(`Archiver tous les groupes de la session "${archiveSeason.value}" ?`)
  if (!confirmed) return

  isArchiving.value = true
  try {
    await axios.post(`${API}/admin/groups/archive`, { season: archiveSeason.value }, getHeaders())
    archiveSeason.value = ''
    await loadGroups()
  } catch (e) {
    console.error('Failed to archive season', e)
    alert('Erreur lors de l\'archivage.')
  }
  isArchiving.value = false
}

function startEdit(group: Group) {
  editingGroup.value = { ...group }
}

async function saveEdit() {
  if (!editingGroup.value) return
  isSaving.value = true
  try {
    await axios.put(`${API}/admin/groups/${editingGroup.value.id}`, {
      id: editingGroup.value.id,
      name: editingGroup.value.name,
      description: editingGroup.value.description,
      season: editingGroup.value.season,
      imageUrl: editingGroup.value.imageUrl,
    }, getHeaders())
    editingGroup.value = null
    await loadGroups()
  } catch (e) {
    console.error('Failed to update group', e)
    alert('Erreur lors de la modification.')
  }
  isSaving.value = false
}

function openAssignProfessor(group: Group) {
  assigningGroup.value = group
  professorSearch.value = ''
  professorResults.value = []
}

let searchTimeout: ReturnType<typeof setTimeout> | null = null

function searchProfessors() {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(async () => {
    if (!professorSearch.value.trim()) {
      professorResults.value = []
      return
    }
    try {
      const res = await axios.get(`${API}/admin/members`, {
        ...getHeaders(),
        params: { Search: professorSearch.value, Page: 1, PageSize: 10 },
      })
      professorResults.value = res.data.items
    } catch (e) {
      console.error('Failed to search members', e)
    }
  }, 300)
}

async function assignProfessor(memberId: string) {
  if (!assigningGroup.value) return
  try {
    await axios.post(`${API}/admin/groups/${assigningGroup.value.id}/professors`, {
      groupId: assigningGroup.value.id,
      memberId,
    }, getHeaders())
    alert('Professeur assigné avec succès.')
    assigningGroup.value = null
  } catch (e) {
    console.error('Failed to assign professor', e)
    alert('Erreur lors de l\'assignation.')
  }
}
</script>

<style scoped>
.group-create-form {
  margin-bottom: 1.5rem;
  padding: 1.5rem;
  max-width: 500px;
}

.group-create-form h3 {
  margin-bottom: 1rem;
}

.group-archive-bar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
}

.group-archive-bar label {
  font-weight: 600;
  font-size: 0.875rem;
  white-space: nowrap;
}

.group-archive-bar input {
  max-width: 200px;
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

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: #fff;
  padding: 28px;
  border-radius: 8px;
  width: 100%;
  max-width: 480px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
}

.modal-content h3 {
  margin-bottom: 1.25rem;
}

.modal-actions {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-top: 1.5rem;
}

.modal-cancel {
  background: none;
  border: none;
  color: #5c5c5c;
  font-size: 0.875rem;
  cursor: pointer;
  padding: 0;
}

.modal-cancel:hover {
  color: #232323;
}

/* Professor search results */
.professor-results {
  list-style: none;
  padding: 0;
  margin: 0.75rem 0 0;
  max-height: 200px;
  overflow-y: auto;
}

.professor-result-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  padding: 8px 0;
  border-bottom: 1px solid #e5e7eb;
  font-size: 0.875rem;
}

.professor-result-item span {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

@media (max-width: 767px) {
  .group-archive-bar {
    flex-direction: column;
    align-items: flex-start;
  }

  .group-archive-bar input {
    max-width: 100%;
  }

  .admin-table {
    display: block;
    overflow-x: auto;
  }

  .modal-content {
    margin: 1rem;
    max-width: calc(100% - 2rem);
    padding: 20px;
  }
}
</style>
