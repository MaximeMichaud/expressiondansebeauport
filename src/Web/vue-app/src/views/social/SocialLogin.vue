<template>
  <div>
    <h2 class="mb-6 text-center text-2xl font-bold text-gray-900">Connexion</h2>

    <form @submit.prevent="handleLogin" class="space-y-4" novalidate>
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

      <div>
        <div class="mb-1 flex items-center justify-between">
          <label class="block text-sm font-medium text-gray-700">Mot de passe</label>
          <router-link :to="{ name: 'socialForgotPassword' }" class="text-xs text-[#1a1a1a] hover:underline">
            Mot de passe oublié?
          </router-link>
        </div>
        <input
          v-model="password"
          type="password"
          :class="[
            'w-full rounded-lg border px-3 py-2 text-sm focus:outline-none focus:ring-1',
            passwordError
              ? 'border-red-500 focus:border-red-500 focus:ring-red-500'
              : 'border-gray-300 focus:border-[#1a1a1a] focus:ring-[#1a1a1a]'
          ]"
          placeholder="••••••••••"
        />
        <p v-if="passwordError" class="mt-1 text-xs text-red-500">{{ passwordError }}</p>
      </div>

      <button
        type="submit"
        :disabled="loading"
        class="w-full rounded-lg bg-[#1a1a1a] px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-[#000000] disabled:opacity-50"
      >
        {{ loading ? 'Connexion...' : 'Se connecter' }}
      </button>
    </form>

    <p class="mt-6 text-center text-sm text-gray-500">
      Pas encore de compte?
      <router-link :to="{ name: 'socialRegister' }" class="font-medium text-[#1a1a1a] hover:underline">S'inscrire</router-link>
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthenticationService } from '@/serviceRegistry'
import { useUserStore } from '@/stores/userStore'
import { useUserService } from '@/serviceRegistry'
import { useSocialToast } from '@/composables/useSocialToast'

const router = useRouter()
const authService = useAuthenticationService()
const userService = useUserService()
const userStore = useUserStore()
const toast = useSocialToast()

const email = ref('')
const password = ref('')
const emailError = ref('')
const passwordError = ref('')
const loading = ref(false)

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

function validate() {
  emailError.value = ''
  passwordError.value = ''
  if (!email.value.trim()) emailError.value = 'Veuillez entrer votre courriel.'
  else if (!emailRegex.test(email.value.trim())) emailError.value = 'Courriel invalide.'
  if (!password.value) passwordError.value = 'Veuillez entrer votre mot de passe.'
  return !emailError.value && !passwordError.value
}

async function handleLogin() {
  if (!validate()) return
  loading.value = true

  try {
    const result = await authService.login({ username: email.value, password: password.value })
    if (result.succeeded) {
      const user = await userService.getCurrentUser()
      if (!user) {
        toast.error('Une erreur est survenue. Veuillez réessayer.')
        return
      }
      userStore.setUser(user)
      await router.push({ name: 'socialImportant' })
    } else {
      const emailNotConfirmed = result.errors?.some((e: any) => e.errorType === 'EmailNotConfirmed')
      if (emailNotConfirmed) {
        await router.push({ name: 'socialConfirm', query: { email: email.value } })
        return
      }
      toast.error('Courriel ou mot de passe invalide.')
    }
  } catch {
    toast.error('Une erreur est survenue. Veuillez réessayer.')
  } finally {
    loading.value = false
  }
}
</script>
