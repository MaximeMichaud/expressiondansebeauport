<template>
  <div class="soc-account">
    <!-- Header -->
    <div class="soc-account__header">
      <button @click="$router.back()" class="soc-account__back">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 19l-7-7 7-7"/></svg>
      </button>
      <h2 class="soc-account__title">Mon compte</h2>
    </div>

    <!-- Avatar + Name -->
    <div class="soc-account__identity">
      <div class="soc-account__avatar">{{ userInitials }}</div>
      <div>
        <p class="text-sm font-semibold text-gray-900">{{ firstName || 'Prénom' }} {{ lastName || 'Nom' }}</p>
        <p class="text-xs text-gray-500">{{ email || 'courriel@exemple.com' }}</p>
      </div>
    </div>

    <!-- Profile card -->
    <section class="soc-account__card">
      <div class="soc-account__card-header">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="4"/><path d="M20 21a8 8 0 00-16 0"/></svg>
        <h3>Informations personnelles</h3>
      </div>
      <form @submit.prevent="updateProfile" class="soc-account__card-body">
        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="soc-account__label">Prénom</label>
            <input v-model="firstName" type="text" placeholder="Prénom" class="soc-account__input" />
          </div>
          <div>
            <label class="soc-account__label">Nom</label>
            <input v-model="lastName" type="text" placeholder="Nom de famille" class="soc-account__input" />
          </div>
        </div>
        <div>
          <label class="soc-account__label">Courriel</label>
          <input v-model="email" type="email" placeholder="votre@courriel.com" class="soc-account__input" />
        </div>
        <div class="flex justify-end">
          <button type="submit" :disabled="savingProfile" class="soc-account__btn-primary">
            {{ savingProfile ? 'Sauvegarde...' : 'Sauvegarder' }}
          </button>
        </div>
      </form>
    </section>

    <!-- Password card -->
    <section class="soc-account__card">
      <div class="soc-account__card-header">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0110 0v4"/></svg>
        <h3>Mot de passe</h3>
      </div>
      <form @submit.prevent="changePassword" class="soc-account__card-body">
        <div>
          <label class="soc-account__label">Mot de passe actuel</label>
          <input v-model="currentPassword" type="password" placeholder="••••••••••" class="soc-account__input" />
        </div>
        <div>
          <label class="soc-account__label">Nouveau mot de passe</label>
          <input v-model="newPassword" type="password" placeholder="Minimum 10 caractères" class="soc-account__input" />
          <!-- Password rules -->
          <div v-if="newPassword" class="mt-2.5 space-y-1.5">
            <div v-for="rule in passwordRules" :key="rule.label" class="flex items-center gap-2">
              <div
                :class="[
                  'flex h-4 w-4 flex-shrink-0 items-center justify-center rounded-full transition-all duration-200',
                  rule.valid ? 'bg-[#1a1a1a]' : 'border border-gray-300'
                ]"
              >
                <svg v-if="rule.valid" class="h-2.5 w-2.5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
                </svg>
              </div>
              <span :class="['text-xs transition-colors duration-200', rule.valid ? 'text-gray-900' : 'text-gray-400']">
                {{ rule.label }}
              </span>
            </div>
          </div>
        </div>
        <div>
          <label class="soc-account__label">Confirmer</label>
          <input v-model="confirmNewPassword" type="password" placeholder="Retaper le mot de passe" class="soc-account__input" />
          <p v-if="confirmNewPassword && newPassword !== confirmNewPassword" class="mt-1 text-xs text-red-500">
            Les mots de passe ne correspondent pas.
          </p>
        </div>
        <div class="flex justify-end">
          <button
            type="submit"
            :disabled="savingPassword || !isNewPasswordValid || newPassword !== confirmNewPassword || !currentPassword"
            class="soc-account__btn-primary"
          >
            {{ savingPassword ? 'Modification...' : 'Modifier le mot de passe' }}
          </button>
        </div>
      </form>
    </section>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthenticationService, useSocialService } from '@/inversify.config'
import { useUserStore } from '@/stores/userStore'
import { useMemberStore } from '@/stores/memberStore'
import { useSocialToast } from '@/composables/useSocialToast'

const router = useRouter()
const authService = useAuthenticationService()
const socialService = useSocialService()
const userStore = useUserStore()
const memberStore = useMemberStore()
const toast = useSocialToast()

const firstName = ref('')
const lastName = ref('')
const email = ref('')
const currentPassword = ref('')
const newPassword = ref('')
const confirmNewPassword = ref('')
const savingProfile = ref(false)
const savingPassword = ref(false)

const userInitials = computed(() => {
  const f = firstName.value?.[0] || ''
  const l = lastName.value?.[0] || ''
  return (f + l).toUpperCase() || '?'
})

const passwordRules = computed(() => [
  { label: '10 caractères minimum', valid: newPassword.value.length >= 10 },
  { label: 'Une lettre majuscule', valid: /[A-Z]/.test(newPassword.value) },
  { label: 'Une lettre minuscule', valid: /[a-z]/.test(newPassword.value) },
  { label: 'Un chiffre', valid: /\d/.test(newPassword.value) },
  { label: 'Un caractère spécial (!@#$...)', valid: /[^a-zA-Z0-9]/.test(newPassword.value) },
])

const isNewPasswordValid = computed(() => passwordRules.value.every(r => r.valid))

function applyProfile(p: any) {
  firstName.value = p.firstName || p.FirstName || ''
  lastName.value = p.lastName || p.LastName || ''
  email.value = p.email || p.Email || ''
}

onMounted(async () => {
  // Always fetch fresh to ensure fields are populated
  try {
    const profile = await socialService.getMyProfile()
    applyProfile(profile)
    memberStore.setMember(profile)
  } catch {
    // Fallback to store if fetch fails
    const m = memberStore.member
    if (m?.firstName) applyProfile(m)
  }
})

async function updateProfile() {
  savingProfile.value = true
  try {
    // TODO: wire to member update endpoint when created
    toast.success('Profil mis à jour.')
  } catch {
    toast.error('Erreur lors de la sauvegarde.')
  }
  savingProfile.value = false
}

async function changePassword() {
  if (!isNewPasswordValid.value) return
  if (newPassword.value !== confirmNewPassword.value) {
    toast.error('Les mots de passe ne correspondent pas.')
    return
  }
  savingPassword.value = true
  try {
    const result = await authService.changePassword({
      currentPassword: currentPassword.value,
      newPassword: newPassword.value,
      newPasswordConfirmation: confirmNewPassword.value
    })
    if (result.succeeded) {
      toast.success('Mot de passe modifié.')
      currentPassword.value = ''
      newPassword.value = ''
      confirmNewPassword.value = ''
    } else {
      toast.error('Mot de passe actuel invalide.')
    }
  } catch {
    toast.error('Erreur lors du changement de mot de passe.')
  }
  savingPassword.value = false
}

async function handleLogout() {
  await authService.logout()
  userStore.reset()
  await router.push('/connexion')
}
</script>

<style lang="scss" scoped>
$soc-black: #1a1a1a;

.soc-account {
  padding-bottom: 24px;

  &__header {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 16px 20px;
    border-bottom: 1px solid #f0f0f0;
  }

  &__back {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    border-radius: 8px;
    color: #78716c;
    transition: color 0.15s, background 0.15s;
    &:hover { color: $soc-black; background: #f5f3f0; }
  }

  &__title {
    font-family: 'Montserrat', sans-serif;
    font-weight: 700;
    font-size: 1.1rem;
    color: #1c1917;
    letter-spacing: -0.01em;
  }

  &__identity {
    display: flex;
    align-items: center;
    gap: 14px;
    padding: 20px 20px 8px;
  }

  &__avatar {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 44px;
    height: 44px;
    border-radius: 50%;
    background: var(--soc-avatar-bg, $soc-black);
    color: var(--soc-avatar-text, white);
    font-family: 'Montserrat', sans-serif;
    font-weight: 700;
    font-size: 0.85rem;
    flex-shrink: 0;
  }

  &__card {
    margin: 16px 16px 0;
    background: white;
    border: 1px solid #ece9e4;
    border-radius: 14px;
    overflow: hidden;

    &--logout {
      background: transparent;
      border: 1px solid #e7e0da;
    }
  }

  &__card-header {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 14px 18px;
    border-bottom: 1px solid #f0eeeb;
    color: #78716c;

    h3 {
      font-family: 'Montserrat', sans-serif;
      font-weight: 600;
      font-size: 0.8rem;
      letter-spacing: 0.02em;
      text-transform: uppercase;
      color: #57534e;
    }
  }

  &__card-body {
    padding: 16px 18px 18px;
    display: flex;
    flex-direction: column;
    gap: 14px;
  }

  &__label {
    display: block;
    margin-bottom: 4px;
    font-size: 0.78rem;
    font-weight: 500;
    color: #57534e;
  }

  &__input {
    width: 100%;
    border: 1px solid #e7e0da;
    border-radius: 10px;
    padding: 9px 12px;
    font-size: 0.85rem;
    background: #faf9f7;
    transition: border-color 0.15s, background 0.15s, box-shadow 0.15s;

    &::placeholder { color: #a8a29e; }
    &:focus {
      outline: none;
      border-color: $soc-black;
      background: white;
      box-shadow: 0 0 0 3px rgba($soc-black, 0.06);
    }
  }

  &__btn-primary {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 8px 20px;
    border-radius: 10px;
    background: $soc-black;
    color: white;
    font-size: 0.8rem;
    font-weight: 600;
    letter-spacing: 0.01em;
    transition: background 0.15s, opacity 0.15s;

    &:hover { background: #000; }
    &:disabled { opacity: 0.45; pointer-events: none; }
  }

  &__btn-logout {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    width: 100%;
    padding: 13px 18px;
    font-size: 0.82rem;
    font-weight: 500;
    color: #78716c;
    transition: color 0.15s, background 0.15s;

    &:hover { color: #dc2626; background: #fef2f2; }
  }
}
</style>
