<template>
  <div class="soc-account">
    <!-- Header -->
    <div class="soc-account__header">
      <button @click="$router.back()" class="soc-account__back">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 19l-7-7 7-7"/></svg>
      </button>
      <h2 class="soc-account__title">Mon compte</h2>
    </div>

    <!-- Name + Email summary -->
    <div class="soc-account__identity">
      <AvatarUploader
        :image-url="memberStore.member?.profileImageUrl"
        :fallback-initials="userInitials"
        :fallback-color="userAvatarColor"
        :size="80"
        :can-edit="false"
      />
      <div class="soc-account__identity-info">
        <p class="text-sm font-semibold" :style="{ color: 'var(--soc-text)' }">{{ firstName || 'Prénom' }} {{ lastName || 'Nom' }}</p>
        <p class="text-xs" :style="{ color: 'var(--soc-text-muted)' }">{{ email || 'courriel@exemple.com' }}</p>
      </div>
    </div>

    <!-- Avatar card -->
    <section class="soc-account__card">
      <div class="soc-account__card-header">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M23 19a2 2 0 01-2 2H3a2 2 0 01-2-2V8a2 2 0 012-2h4l2-3h6l2 3h4a2 2 0 012 2z"/><circle cx="12" cy="13" r="4"/></svg>
        <h3>Photo de profil</h3>
      </div>
      <div class="soc-account__card-body">
        <div class="soc-account__avatar-actions">
          <input
            ref="avatarInputRef"
            type="file"
            accept="image/*"
            hidden
            @change="onAvatarFileChange"
          />
          <button
            type="button"
            class="soc-account__avatar-btn"
            :disabled="uploadingAvatar"
            @click="avatarInputRef?.click()"
          >
            {{ uploadingAvatar ? 'Envoi...' : (memberStore.member?.profileImageUrl ? 'Changer la photo' : 'Ajouter une photo') }}
          </button>
          <button
            v-if="memberStore.member?.profileImageUrl && !uploadingAvatar"
            type="button"
            class="soc-account__avatar-btn soc-account__avatar-btn--danger"
            @click="confirmAvatarRemove = true"
          >
            Supprimer la photo
          </button>
        </div>
      </div>
    </section>

    <!-- Profile card -->
    <section class="soc-account__card">
      <div class="soc-account__card-header">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="4"/><path d="M20 21a8 8 0 00-16 0"/></svg>
        <h3>Informations personnelles</h3>
      </div>
      <form @submit.prevent="updateProfile" class="soc-account__card-body">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
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

    <!-- Danger zone -->
    <section class="soc-account__card">
      <div class="soc-account__card-header">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#dc2626" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
        <h3>Zone de danger</h3>
      </div>
      <div class="soc-account__card-body">
        <p class="text-xs" :style="{ color: 'var(--soc-text-muted)' }">
          La suppression de votre compte est définitive. Vous ne pourrez plus vous connecter avec ce courriel.
        </p>
        <button
          type="button"
          class="soc-account__avatar-btn soc-account__avatar-btn--danger inline-flex items-center justify-center gap-2"
          :disabled="deletingAccount"
          @click="confirmDeleteAccount = true"
        >
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/></svg>
          <span>{{ deletingAccount ? 'Suppression...' : 'Supprimer mon compte' }}</span>
        </button>
      </div>
    </section>

    <ConfirmModal
      :open="confirmAvatarRemove"
      title="Retirer la photo?"
      message="Votre photo de profil sera remplacée par vos initiales."
      confirm-label="Retirer"
      :danger="true"
      @confirm="handleAvatarRemove"
      @cancel="confirmAvatarRemove = false"
    />

    <ConfirmModal
      :open="confirmDeleteAccount"
      title="Supprimer votre compte?"
      message="Cette action est irréversible. Votre compte sera supprimé et vous serez déconnecté."
      confirm-label="Supprimer mon compte"
      :danger="true"
      @confirm="handleDeleteAccount"
      @cancel="confirmDeleteAccount = false"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthenticationService, useSocialService } from '@/inversify.config'
import { useMemberStore } from '@/stores/memberStore'
import { useUserStore } from '@/stores/userStore'
import { useAvatarRegistryStore } from '@/stores/avatarRegistryStore'
import { useSocialToast } from '@/composables/useSocialToast'
import AvatarUploader from '@/components/social/AvatarUploader.vue'
import ConfirmModal from '@/components/social/ConfirmModal.vue'

const router = useRouter()
const authService = useAuthenticationService()
const socialService = useSocialService()
const memberStore = useMemberStore()
const userStore = useUserStore()
const avatarRegistry = useAvatarRegistryStore()
const toast = useSocialToast()

const firstName = ref('')
const lastName = ref('')
const email = ref('')
const currentPassword = ref('')
const newPassword = ref('')
const confirmNewPassword = ref('')
const savingProfile = ref(false)
const savingPassword = ref(false)
const uploadingAvatar = ref(false)
const confirmAvatarRemove = ref(false)
const avatarInputRef = ref<HTMLInputElement | null>(null)
const confirmDeleteAccount = ref(false)
const deletingAccount = ref(false)

async function handleDeleteAccount() {
  if (deletingAccount.value) return
  deletingAccount.value = true
  try {
    const result = await socialService.deleteMyAccount()
    if (result.succeeded) {
      confirmDeleteAccount.value = false
      await authService.logout()
      userStore.reset()
      memberStore.reset()
      await router.push({ name: 'socialLogin' })
    } else {
      toast.error(result.errors?.[0]?.errorMessage || 'Erreur lors de la suppression.')
    }
  } catch {
    toast.error('Erreur de connexion.')
  }
  deletingAccount.value = false
}

function onAvatarFileChange(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) handleAvatarUpload(file)
  if (input) input.value = ''
}

const userInitials = computed(() => {
  const f = firstName.value?.[0] || ''
  const l = lastName.value?.[0] || ''
  return (f + l).toUpperCase() || '?'
})

const avatarColors = ['#e53e3e', '#dd6b20', '#d69e2e', '#38a169', '#319795', '#3182ce', '#5a67d8', '#805ad5', '#d53f8c', '#e53e3e']
function getAvatarColor(name: string) {
  let hash = 0
  for (let i = 0; i < (name?.length || 0); i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return avatarColors[Math.abs(hash) % avatarColors.length]
}
const userAvatarColor = computed(() => {
  const m = memberStore.member
  if (m?.avatarColor) return m.avatarColor
  return getAvatarColor(`${firstName.value} ${lastName.value}`)
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
    const result = await socialService.updateMyProfile(firstName.value, lastName.value, email.value)
    if (result.succeeded) {
      // Update memberStore so the name changes immediately in the navbar
      const updated = { ...memberStore.member, firstName: firstName.value, lastName: lastName.value, fullName: `${firstName.value} ${lastName.value}`, email: email.value }
      memberStore.setMember(updated)
      toast.success('Profil mis à jour.')
    } else {
      const errorMsg = (result as any).errors?.[0]?.errorMessage || (result as any).errors?.[0]?.ErrorMessage || 'Erreur lors de la sauvegarde.'
      toast.error(errorMsg)
    }
  } catch {
    toast.error('Erreur lors de la sauvegarde.')
  }
  savingProfile.value = false
}

async function handleAvatarUpload(file: File) {
  if (uploadingAvatar.value) return
  uploadingAvatar.value = true
  try {
    const uploaded = await socialService.uploadFile(file)
    if (!uploaded.succeeded || !uploaded.displayUrl) {
      toast.error("Échec du téléversement de l'image.")
      return
    }
    const result = await socialService.setMyProfileImage(uploaded.displayUrl)
    if (result.succeeded) {
      const profile = await socialService.getMyProfile()
      memberStore.setMember(profile)
      if (profile?.id) avatarRegistry.setAvatar(profile.id, uploaded.displayUrl)
      toast.success('Photo de profil mise à jour.')
    } else {
      toast.error("Impossible d'enregistrer la photo.")
    }
  } catch {
    toast.error('Erreur lors du téléversement.')
  } finally {
    uploadingAvatar.value = false
  }
}

async function handleAvatarRemove() {
  confirmAvatarRemove.value = false
  try {
    const result = await socialService.removeMyProfileImage()
    if (result.succeeded) {
      const profile = await socialService.getMyProfile()
      memberStore.setMember(profile)
      if (profile?.id) avatarRegistry.clearAvatar(profile.id)
      toast.success('Photo de profil retirée.')
    } else {
      toast.error('Impossible de retirer la photo.')
    }
  } catch {
    toast.error('Erreur lors du retrait.')
  }
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

</script>

<style lang="scss" scoped>
$soc-black: #1a1a1a;

.soc-account {
  padding-bottom: 24px;

  &__header {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 16px 20px;
    border-bottom: 1px solid #f0f0f0;
  }

  &__back {
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    width: 32px;
    height: 32px;
    padding: 0;
    border-radius: 8px;
    color: var(--soc-text-muted, #78716c);
    cursor: pointer;
    transition: color 0.15s, background 0.15s;
    &:hover { color: var(--soc-bar-text-strong, #1a1a1a); background: var(--soc-bar-hover, #f5f3f0); }
  }

  &__title {
    font-family: 'Montserrat', sans-serif;
    font-weight: 700;
    font-size: 1.1rem;
    line-height: 1;
    color: #1c1917;
    letter-spacing: -0.01em;
  }

  &__identity {
    display: flex;
    align-items: center;
    gap: 14px;
    padding: 20px 20px 8px;
  }

  &__identity-info {
    display: flex;
    flex-direction: column;
    gap: 2px;
    min-width: 0;
  }

  &__avatar-actions {
    display: flex;
    flex-direction: column;
    gap: 8px;
    width: 100%;

    @media (min-width: 48em) {
      flex-direction: row;
    }
  }

  &__avatar-btn {
    flex: 1 1 0;
    width: 100%;
    padding: 9px 14px;
    font-family: 'Montserrat', sans-serif;
    font-size: 0.8rem;
    font-weight: 600;
    border-radius: 8px;
    color: white;
    background: #1a1a1a;
    border: 1px solid #1a1a1a;
    cursor: pointer;
    transition: opacity 0.15s;
    &:hover:not(:disabled) { opacity: 0.85; }
    &:disabled { opacity: 0.5; cursor: default; }

    .soc--dark & {
      color: #1a1a1a;
      background: white;
      border-color: white;
    }

    &--danger {
      color: #dc2626;
      border-color: rgba(220, 38, 38, 0.2);
      background: rgba(220, 38, 38, 0.06);
      &:hover:not(:disabled) { background: rgba(220, 38, 38, 0.12); opacity: 1; }

      .soc--dark & {
        color: #fca5a5;
        background: rgba(220, 38, 38, 0.12);
        border-color: rgba(220, 38, 38, 0.3);
        &:hover:not(:disabled) { background: rgba(220, 38, 38, 0.2); }
      }
    }
  }

  &__avatar {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 44px;
    height: 44px;
    border-radius: 50%;
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
