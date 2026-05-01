<template>
  <div>
    <h2 class="mb-6 text-center text-2xl font-bold text-gray-900">Nouveau mot de passe</h2>

    <div v-if="!hasValidLink" class="rounded-lg bg-red-50 p-4 text-sm text-red-800">
      Lien invalide ou expiré.
      <router-link :to="{ name: 'socialForgotPassword' }" class="font-medium underline">Demander un nouveau lien</router-link>
    </div>

    <form v-else-if="!success" @submit.prevent="handleSubmit" class="space-y-4" novalidate>
      <div>
        <label class="mb-1 block text-sm font-medium text-gray-700">Nouveau mot de passe</label>
        <input
          v-model="password"
          type="password"
          class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
          placeholder="Minimum 10 caractères"
        />
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
        {{ loading ? 'Réinitialisation...' : 'Réinitialiser' }}
      </button>
    </form>

    <div v-else class="rounded-lg bg-emerald-50 p-4 text-sm text-emerald-800">
      Mot de passe réinitialisé! Redirection...
    </div>

    <p v-if="hasValidLink && !success" class="mt-6 text-center text-sm text-gray-500">
      <router-link :to="{ name: 'socialLogin' }" class="font-medium text-[#1a1a1a] hover:underline">Retour à la connexion</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthenticationService } from '@/serviceRegistry'
import { useSocialToast } from '@/composables/useSocialToast'
import { Guid } from '@/types/guid'

const route = useRoute()
const router = useRouter()
const authService = useAuthenticationService()
const toast = useSocialToast()

const userId = computed(() => (route.query.userId as string) || '')
const token = computed(() => (route.query.token as string) || '')
const hasValidLink = computed(() => !!userId.value && !!token.value)

const password = ref('')
const confirmPassword = ref('')
const loading = ref(false)
const success = ref(false)

const passwordRules = computed(() => [
  { label: '10 caractères minimum', valid: password.value.length >= 10 },
  { label: 'Une lettre majuscule', valid: /[A-Z]/.test(password.value) },
  { label: 'Une lettre minuscule', valid: /[a-z]/.test(password.value) },
  { label: 'Un chiffre', valid: /\d/.test(password.value) },
  { label: 'Un caractère spécial (!@#$...)', valid: /[^a-zA-Z0-9]/.test(password.value) },
])

const isPasswordValid = computed(() => passwordRules.value.every(r => r.valid))

async function handleSubmit() {
  if (!isPasswordValid.value || password.value !== confirmPassword.value) return
  loading.value = true

  try {
    const result = await authService.resetPassword({
      userId: new Guid(userId.value),
      token: token.value,
      password: password.value,
      passwordConfirmation: confirmPassword.value
    })
    if (result.succeeded) {
      success.value = true
      setTimeout(() => router.push({ name: 'socialLogin' }), 1500)
    } else {
      toast.error('Lien invalide ou expiré. Demandez un nouveau lien.')
    }
  } catch {
    toast.error('Une erreur est survenue. Veuillez réessayer.')
  } finally {
    loading.value = false
  }
}
</script>
