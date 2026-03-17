<template>
  <div>
    <h2 class="mb-6 text-center text-2xl font-bold text-gray-900">Connexion</h2>

    <div v-if="errorMessage" class="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">
      {{ errorMessage }}
    </div>

    <form @submit.prevent="handleLogin" class="space-y-4">
      <div>
        <label class="mb-1 block text-sm font-medium text-gray-700">Courriel</label>
        <input
          v-model="email"
          type="email"
          required
          class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#be1e2c] focus:outline-none focus:ring-1 focus:ring-[#be1e2c]"
          placeholder="votre@courriel.com"
        />
      </div>

      <div>
        <label class="mb-1 block text-sm font-medium text-gray-700">Mot de passe</label>
        <input
          v-model="password"
          type="password"
          required
          class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#be1e2c] focus:outline-none focus:ring-1 focus:ring-[#be1e2c]"
          placeholder="••••••••••"
        />
      </div>

      <button
        type="submit"
        :disabled="loading"
        class="w-full rounded-lg bg-[#be1e2c] px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-[#a01825] disabled:opacity-50"
      >
        {{ loading ? 'Connexion...' : 'Se connecter' }}
      </button>
    </form>

    <p class="mt-6 text-center text-sm text-gray-500">
      Pas encore de compte?
      <router-link to="/inscription" class="font-medium text-[#be1e2c] hover:underline">S'inscrire</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthenticationService } from '@/inversify.config'
import { useUserStore } from '@/stores/userStore'
import { useUserService } from '@/inversify.config'

const router = useRouter()
const authService = useAuthenticationService()
const userService = useUserService()
const userStore = useUserStore()

const email = ref('')
const password = ref('')
const loading = ref(false)
const errorMessage = ref('')

async function handleLogin() {
  loading.value = true
  errorMessage.value = ''

  try {
    const result = await authService.login({ username: email.value, password: password.value })
    if (result.succeeded) {
      const user = await userService.getCurrentUser()
      userStore.setUser(user)
      await router.push('/')
    } else {
      errorMessage.value = 'Courriel ou mot de passe invalide.'
    }
  } catch (e) {
    errorMessage.value = 'Une erreur est survenue. Veuillez réessayer.'
  } finally {
    loading.value = false
  }
}
</script>
