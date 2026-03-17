<template>
  <div>
    <h2 class="mb-6 text-center text-2xl font-bold text-gray-900">Inscription</h2>

    <div v-if="errorMessage" class="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">
      {{ errorMessage }}
    </div>

    <form @submit.prevent="handleRegister" class="space-y-4">
      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="mb-1 block text-sm font-medium text-gray-700">Prénom</label>
          <input
            v-model="firstName"
            type="text"
            required
            class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#be1e2c] focus:outline-none focus:ring-1 focus:ring-[#be1e2c]"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-gray-700">Nom</label>
          <input
            v-model="lastName"
            type="text"
            required
            class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#be1e2c] focus:outline-none focus:ring-1 focus:ring-[#be1e2c]"
          />
        </div>
      </div>

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
          minlength="10"
          class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#be1e2c] focus:outline-none focus:ring-1 focus:ring-[#be1e2c]"
          placeholder="Minimum 10 caractères"
        />
      </div>

      <div>
        <label class="mb-1 block text-sm font-medium text-gray-700">Confirmer le mot de passe</label>
        <input
          v-model="confirmPassword"
          type="password"
          required
          class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#be1e2c] focus:outline-none focus:ring-1 focus:ring-[#be1e2c]"
        />
      </div>

      <button
        type="submit"
        :disabled="loading"
        class="w-full rounded-lg bg-[#be1e2c] px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-[#a01825] disabled:opacity-50"
      >
        {{ loading ? 'Inscription...' : "S'inscrire" }}
      </button>
    </form>

    <p class="mt-6 text-center text-sm text-gray-500">
      Vous avez déjà un compte?
      <router-link to="/connexion" class="font-medium text-[#be1e2c] hover:underline">Se connecter</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useSocialService } from '@/inversify.config'

const router = useRouter()
const socialService = useSocialService()

const firstName = ref('')
const lastName = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const loading = ref(false)
const errorMessage = ref('')

async function handleRegister() {
  if (password.value !== confirmPassword.value) {
    errorMessage.value = 'Les mots de passe ne correspondent pas.'
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    const result = await socialService.register(firstName.value, lastName.value, email.value, password.value)
    if (result.succeeded) {
      await router.push({ path: '/confirmation', query: { email: email.value } })
    } else {
      errorMessage.value = result.errors?.[0]?.errorMessage || "Une erreur est survenue lors de l'inscription."
    }
  } catch (e) {
    errorMessage.value = "Une erreur est survenue. Veuillez réessayer."
  } finally {
    loading.value = false
  }
}
</script>
