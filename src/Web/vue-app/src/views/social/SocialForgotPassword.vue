<template>
  <div>
    <h2 class="mb-2 text-center text-2xl font-bold text-gray-900">Mot de passe oublié</h2>
    <p class="mb-6 text-center text-sm text-gray-500">
      Entrez votre courriel et nous vous enverrons un lien pour réinitialiser votre mot de passe.
    </p>

    <form v-if="!sent" @submit.prevent="handleSubmit" class="space-y-4" novalidate>
      <div>
        <label class="mb-1 block text-sm font-medium text-gray-700">Courriel</label>
        <input
          v-model="email"
          type="email"
          :class="[
            'w-full rounded-lg border px-3 py-2 text-sm focus:outline-none focus:ring-1',
            emailError
              ? 'border-red-500 focus:border-red-500 focus:ring-red-500'
              : 'border-gray-300 focus:border-[#1a1a1a] focus:ring-[#1a1a1a]'
          ]"
          placeholder="votre@courriel.com"
        />
        <p v-if="emailError" class="mt-1 text-xs text-red-500">{{ emailError }}</p>
      </div>

      <button
        type="submit"
        :disabled="loading"
        class="w-full rounded-lg bg-[#1a1a1a] px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-[#000000] disabled:opacity-50"
      >
        Envoyer le lien
      </button>
    </form>

    <div v-else class="rounded-lg bg-emerald-50 p-4 text-center text-sm text-emerald-600">
      C'est envoyé! Vérifiez votre courriel.
    </div>

    <p class="mt-6 text-center text-sm text-gray-500">
      <router-link :to="{ name: 'socialLogin' }" class="font-medium text-[#1a1a1a] hover:underline">Retour à la connexion</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useAuthenticationService } from '@/serviceRegistry'
import { useSocialToast } from '@/composables/useSocialToast'

const authService = useAuthenticationService()
const toast = useSocialToast()

const email = ref('')
const emailError = ref('')
const loading = ref(false)
const sent = ref(false)

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

function validate() {
  emailError.value = ''
  if (!email.value.trim()) emailError.value = 'Veuillez entrer votre courriel.'
  else if (!emailRegex.test(email.value.trim())) emailError.value = 'Courriel invalide.'
  return !emailError.value
}

async function handleSubmit() {
  if (!validate()) return
  loading.value = true

  try {
    const result = await authService.forgotPassword({
      username: email.value.trim(),
      resetPasswordRelativeUrl: '/social/reinitialiser-mot-de-passe'
    })
    if (result.succeeded) {
      sent.value = true
    } else if (result.errors?.some((e: any) => e.errorType === 'UserNotFound')) {
      emailError.value = 'Aucun compte n\'existe avec ce courriel.'
    } else {
      toast.error('Une erreur est survenue. Veuillez réessayer.')
    }
  } catch {
    toast.error('Une erreur est survenue. Veuillez réessayer.')
  } finally {
    loading.value = false
  }
}
</script>
