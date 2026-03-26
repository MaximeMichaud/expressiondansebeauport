<template>
  <div>
    <h2 class="mb-2 text-center text-2xl font-bold text-gray-900">Confirmation</h2>
    <p class="mb-6 text-center text-sm text-gray-500">
      Entrez le code à 6 chiffres envoyé à <strong>{{ emailAddress }}</strong>
    </p>

    <form @submit.prevent="handleConfirm" class="space-y-4">
      <div class="flex justify-center gap-2">
        <input
          v-for="(_, i) in 6"
          :key="i"
          :ref="(el) => { if (el) digitRefs[i] = el as HTMLInputElement }"
          v-model="digits[i]"
          type="text"
          inputmode="numeric"
          maxlength="1"
          class="h-12 w-12 rounded-lg border border-gray-300 text-center text-lg font-bold focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
          @input="onDigitInput(i)"
          @keydown.backspace="onBackspace(i)"
        />
      </div>

      <button
        type="submit"
        :disabled="loading || code.length !== 6"
        class="w-full rounded-lg bg-[#1a1a1a] px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-[#000000] disabled:opacity-50"
      >
        {{ loading ? 'Vérification...' : 'Confirmer' }}
      </button>
    </form>

    <button
      @click="handleResend"
      :disabled="resendLoading || resendCooldown > 0"
      class="mt-4 w-full text-center text-sm text-[#1a1a1a] hover:underline disabled:text-gray-400"
    >
      {{ resendCooldown > 0 ? `Renvoyer le code (${resendCooldown}s)` : 'Renvoyer le code' }}
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useSocialToast } from '@/composables/useSocialToast'

const route = useRoute()
const router = useRouter()
const socialService = useSocialService()
const toast = useSocialToast()

const emailAddress = computed(() => (route.query.email as string) || '')
const digits = ref<string[]>(['', '', '', '', '', ''])
const digitRefs = ref<HTMLInputElement[]>([])
const loading = ref(false)
const resendLoading = ref(false)
const resendCooldown = ref(0)

const code = computed(() => digits.value.join(''))

function onDigitInput(index: number) {
  const val = digits.value[index]
  if (val && index < 5) {
    digitRefs.value[index + 1]?.focus()
  }
}

function onBackspace(index: number) {
  if (!digits.value[index] && index > 0) {
    digitRefs.value[index - 1]?.focus()
  }
}

async function handleConfirm() {
  loading.value = true

  try {
    const result = await socialService.confirmEmail(emailAddress.value, code.value)
    if (result.succeeded) {
      toast.success('Compte confirmé! Redirection...')
      setTimeout(() => router.push({ name: 'socialLogin' }), 1500)
    } else {
      toast.error('Code invalide ou expiré.')
    }
  } catch {
    toast.error('Une erreur est survenue.')
  } finally {
    loading.value = false
  }
}

async function handleResend() {
  resendLoading.value = true

  try {
    const result: any = await socialService.resendCode(emailAddress.value)
    const code = result.confirmationCode || result.ConfirmationCode
    if (code) toast.success(`Code: ${code}`, 15000)
    else toast.success('Un nouveau code a été envoyé.')
    resendCooldown.value = 60
    const interval = setInterval(() => {
      resendCooldown.value--
      if (resendCooldown.value <= 0) clearInterval(interval)
    }, 1000)
  } catch {
    toast.error('Impossible de renvoyer le code.')
  } finally {
    resendLoading.value = false
  }
}

onMounted(() => {
  digitRefs.value[0]?.focus()
})
</script>
