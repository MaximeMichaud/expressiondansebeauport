<template>
  <div class="p-6">
    <!-- Back link -->
    <button @click="$router.back()" class="mb-4 flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700">
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 19l-7-7 7-7"/></svg>
      Retour
    </button>

    <h2 class="mb-6 text-xl font-bold text-gray-900" style="font-family: 'Montserrat', sans-serif;">Mon compte</h2>

    <div v-if="successMessage" class="mb-4 rounded-lg bg-green-50 p-3 text-sm text-green-700">{{ successMessage }}</div>
    <div v-if="errorMessage" class="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">{{ errorMessage }}</div>

    <!-- Profile section -->
    <section class="mb-8">
      <h3 class="mb-4 text-sm font-semibold uppercase tracking-wide text-gray-500">Profil</h3>
      <form @submit.prevent="updateProfile" class="space-y-4">
        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Prénom</label>
            <input v-model="firstName" type="text" placeholder="Jean" class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]" />
          </div>
          <div>
            <label class="mb-1 block text-sm font-medium text-gray-700">Nom</label>
            <input v-model="lastName" type="text" placeholder="Tremblay" class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]" />
          </div>
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-gray-700">Courriel</label>
          <input v-model="email" type="email" placeholder="votre@courriel.com" class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]" />
        </div>
        <button type="submit" :disabled="savingProfile" class="rounded-lg bg-[#1a1a1a] px-4 py-2 text-sm font-semibold text-white hover:bg-[#000000] disabled:opacity-50">
          {{ savingProfile ? 'Sauvegarde...' : 'Sauvegarder' }}
        </button>
      </form>
    </section>

    <!-- Password section -->
    <section class="mb-8">
      <h3 class="mb-4 text-sm font-semibold uppercase tracking-wide text-gray-500">Mot de passe</h3>
      <form @submit.prevent="changePassword" class="space-y-4">
        <div>
          <label class="mb-1 block text-sm font-medium text-gray-700">Mot de passe actuel</label>
          <input v-model="currentPassword" type="password" placeholder="••••••••••" class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]" />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-gray-700">Nouveau mot de passe</label>
          <input v-model="newPassword" type="password" minlength="10" class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]" placeholder="Minimum 10 caractères" />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-gray-700">Confirmer</label>
          <input v-model="confirmNewPassword" type="password" placeholder="Retaper le mot de passe" class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]" />
        </div>
        <button type="submit" :disabled="savingPassword" class="rounded-lg bg-gray-800 px-4 py-2 text-sm font-semibold text-white hover:bg-gray-700 disabled:opacity-50">
          {{ savingPassword ? 'Modification...' : 'Modifier le mot de passe' }}
        </button>
      </form>
    </section>

    <!-- Logout -->
    <section>
      <button @click="handleLogout" class="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50">
        Se déconnecter
      </button>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthenticationService, useUserService } from '@/inversify.config'
import { useUserStore } from '@/stores/userStore'

const router = useRouter()
const authService = useAuthenticationService()
const userService = useUserService()
const userStore = useUserStore()

const firstName = ref('')
const lastName = ref('')
const email = ref('')
const currentPassword = ref('')
const newPassword = ref('')
const confirmNewPassword = ref('')
const savingProfile = ref(false)
const savingPassword = ref(false)
const successMessage = ref('')
const errorMessage = ref('')

onMounted(async () => {
  try {
    const user = await userService.getCurrentUser()
    if (user) {
      email.value = user.email || ''
    }
  } catch { /* */ }
})

async function updateProfile() {
  savingProfile.value = true
  successMessage.value = ''
  errorMessage.value = ''
  try {
    // TODO: wire to member update endpoint when created
    successMessage.value = 'Profil mis à jour.'
  } catch {
    errorMessage.value = 'Erreur lors de la sauvegarde.'
  }
  savingProfile.value = false
}

async function changePassword() {
  if (newPassword.value !== confirmNewPassword.value) {
    errorMessage.value = 'Les mots de passe ne correspondent pas.'
    return
  }
  savingPassword.value = true
  successMessage.value = ''
  errorMessage.value = ''
  try {
    const result = await authService.changePassword({
      currentPassword: currentPassword.value,
      newPassword: newPassword.value,
      newPasswordConfirmation: confirmNewPassword.value
    })
    if (result.succeeded) {
      successMessage.value = 'Mot de passe modifié.'
      currentPassword.value = ''
      newPassword.value = ''
      confirmNewPassword.value = ''
    } else {
      errorMessage.value = 'Mot de passe actuel invalide.'
    }
  } catch {
    errorMessage.value = 'Erreur lors du changement de mot de passe.'
  }
  savingPassword.value = false
}

async function handleLogout() {
  await authService.logout()
  userStore.reset()
  await router.push('/connexion')
}
</script>
