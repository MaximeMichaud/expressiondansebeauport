<template>
  <div>
    <h2 class="mb-6 text-center text-2xl font-bold text-gray-900">Connexion</h2>

    <div v-if="showConfirmationLink" class="mb-4 text-center">
      <router-link
        :to="{ path: '/confirmation', query: { email: email } }"
        class="text-xs font-medium text-[#1a1a1a] hover:underline"
      >
        Vous n'avez pas recu votre code de confirmation?
      </router-link>
    </div>

    <form @submit.prevent="handleLogin" class="space-y-4">
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
          placeholder="••••••••••"
        />
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
import { useAuthenticationService } from '@/inversify.config'
import { useUserStore } from '@/stores/userStore'
import { useUserService } from '@/inversify.config'
import { useSocialToast } from '@/composables/useSocialToast'

const router = useRouter()
const authService = useAuthenticationService()
const userService = useUserService()
const userStore = useUserStore()
const toast = useSocialToast()

const email = ref('')
const password = ref('')
const loading = ref(false)
const showConfirmationLink = ref(false)

async function handleLogin() {
  loading.value = true
  showConfirmationLink.value = false

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
      showConfirmationLink.value = true
    }
  } catch {
    toast.error('Une erreur est survenue. Veuillez réessayer.')
  } finally {
    loading.value = false
  }
}
</script>
