<template>
  <div>
    <h2 class="mb-6 text-center text-2xl font-bold text-gray-900">Inscription</h2>


    <form @submit.prevent="handleRegister" class="space-y-4">
      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="mb-1 block text-sm font-medium text-gray-700">Prénom</label>
          <input
            v-model="firstName"
            type="text"
            required
            class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
            placeholder="Prénom"
          />
        </div>
        <div>
          <label class="mb-1 block text-sm font-medium text-gray-700">Nom</label>
          <input
            v-model="lastName"
            type="text"
            required
            class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
            placeholder="Nom de famille"
          />
        </div>
      </div>

      <div>
        <label class="mb-1 block text-sm font-medium text-gray-700">Courriel</label>
        <input
          v-model="email"
          type="email"
          required
          class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
          placeholder="votre@courriel.com"
        />
      </div>

      <div>
        <label class="mb-1 block text-sm font-medium text-gray-700">Mot de passe</label>
        <input
          v-model="password"
          type="password"
          required
          class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
          placeholder="Minimum 10 caractères"
        />
        <!-- Password rules -->
        <div v-if="password" class="mt-2 space-y-1">
          <div v-for="rule in passwordRules" :key="rule.label" class="flex items-center gap-2">
            <div
              :class="[
                'flex h-4 w-4 flex-shrink-0 items-center justify-center rounded-full transition-all duration-200',
                rule.valid ? 'bg-emerald-600' : 'border border-gray-300'
              ]"
            >
              <svg v-if="rule.valid" class="h-2.5 w-2.5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3">
                <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <span :class="['text-xs transition-colors duration-200', rule.valid ? 'text-emerald-700' : 'text-gray-400']">
              {{ rule.label }}
            </span>
          </div>
        </div>
      </div>

      <div>
        <label class="mb-1 block text-sm font-medium text-gray-700">Confirmer le mot de passe</label>
        <input
          v-model="confirmPassword"
          type="password"
          required
          class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
          placeholder="Retaper le mot de passe"
        />
        <p v-if="confirmPassword && password !== confirmPassword" class="mt-1 text-xs text-red-500">
          Les mots de passe ne correspondent pas.
        </p>
      </div>

      <button
        type="submit"
        :disabled="loading || !isPasswordValid || password !== confirmPassword"
        class="w-full rounded-lg bg-[#1a1a1a] px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-[#000000] disabled:opacity-50"
      >
        {{ loading ? 'Inscription...' : "S'inscrire" }}
      </button>
    </form>

    <p class="mt-6 text-center text-sm text-gray-500">
      Vous avez déjà un compte?
      <router-link :to="{ name: 'socialLogin' }" class="font-medium text-[#1a1a1a] hover:underline">Se connecter</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useSocialToast } from '@/composables/useSocialToast'

const router = useRouter()
const socialService = useSocialService()
const toast = useSocialToast()

const firstName = ref('')
const lastName = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const loading = ref(false)

const passwordRules = computed(() => [
  { label: '10 caractères minimum', valid: password.value.length >= 10 },
  { label: 'Une lettre majuscule', valid: /[A-Z]/.test(password.value) },
  { label: 'Une lettre minuscule', valid: /[a-z]/.test(password.value) },
  { label: 'Un chiffre', valid: /\d/.test(password.value) },
  { label: 'Un caractère spécial (!@#$...)', valid: /[^a-zA-Z0-9]/.test(password.value) },
])

const isPasswordValid = computed(() => passwordRules.value.every(r => r.valid))

async function handleRegister() {
  if (!isPasswordValid.value) return
  if (password.value !== confirmPassword.value) {
    toast.error('Les mots de passe ne correspondent pas.')
    return
  }

  loading.value = true

  try {
    const result = await socialService.register(firstName.value, lastName.value, email.value, password.value)
    if (result.succeeded) {
      await router.push({ name: 'socialConfirm', query: { email: email.value } })
    } else {
      toast.error(result.errors?.[0]?.errorMessage || "Une erreur est survenue lors de l'inscription.")
    }
  } catch {
    toast.error("Une erreur est survenue. Veuillez réessayer.")
  } finally {
    loading.value = false
  }
}
</script>
