<template>
  <div class="jr-card" :class="statusClass">
    <template v-if="status === 'Pending' && !isMine">
      <p class="jr-card__text">
        <strong>{{ requesterName }}</strong> souhaite rejoindre <strong>{{ groupName }}</strong>
      </p>
      <div class="jr-card__actions">
        <button
          class="jr-card__btn jr-card__btn--accept"
          :disabled="loading"
          @click="handleAccept"
        >
          {{ loading ? '...' : 'Accepter' }}
        </button>
        <button
          class="jr-card__btn jr-card__btn--reject"
          :disabled="loading"
          @click="handleReject"
        >
          Refuser
        </button>
      </div>
    </template>
    <template v-else-if="status === 'Pending' && isMine">
      <p class="jr-card__text">
        Vous avez demandé à rejoindre <strong>{{ groupName }}</strong>
      </p>
      <p class="jr-card__status">En attente...</p>
    </template>
    <template v-else-if="status === 'Accepted'">
      <p class="jr-card__text">
        <strong>{{ requesterName }}</strong> souhaite rejoindre <strong>{{ groupName }}</strong>
      </p>
      <p class="jr-card__result jr-card__result--accepted">Demande acceptée</p>
    </template>
    <template v-else-if="status === 'Rejected'">
      <p class="jr-card__text">
        <strong>{{ requesterName }}</strong> souhaite rejoindre <strong>{{ groupName }}</strong>
      </p>
      <p class="jr-card__result jr-card__result--rejected">Demande refusée</p>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useSocialService } from '@/inversify.config'
import { useSocialToast } from '@/composables/useSocialToast'

const props = defineProps<{
  joinRequestId: string
  groupName: string
  requesterName: string
  status: 'Pending' | 'Accepted' | 'Rejected'
  resolvedByName?: string
  isMine: boolean
}>()

const emit = defineEmits<{
  resolved: []
}>()

const socialService = useSocialService()
const toast = useSocialToast()
const loading = ref(false)

const statusClass = {
  'jr-card--pending': props.status === 'Pending',
  'jr-card--accepted': props.status === 'Accepted',
  'jr-card--rejected': props.status === 'Rejected',
}

async function handleAccept() {
  loading.value = true
  try {
    const result = await socialService.acceptJoinRequest(props.joinRequestId)
    if (result.succeeded) {
      toast.success('Membre ajouté au groupe!')
      emit('resolved')
    } else {
      toast.error(result.errors?.[0]?.errorMessage || 'Erreur')
    }
  } catch {
    toast.error('Erreur de connexion')
  }
  loading.value = false
}

async function handleReject() {
  loading.value = true
  try {
    const result = await socialService.rejectJoinRequest(props.joinRequestId)
    if (result.succeeded) {
      toast.success('Demande refusée.')
      emit('resolved')
    } else {
      toast.error(result.errors?.[0]?.errorMessage || 'Erreur')
    }
  } catch {
    toast.error('Erreur de connexion')
  }
  loading.value = false
}
</script>

<style lang="scss">
.jr-card {
  padding: 12px 16px;
  border-radius: 12px;
  background: var(--soc-card-bg, #f8f7f5);
  border: 1px solid var(--soc-border, #e5e3df);
  max-width: 300px;

  &__text {
    font-size: 0.85rem;
    line-height: 1.4;
    color: var(--soc-text, #1a1a1a);
    margin: 0 0 8px;

    strong {
      font-weight: 600;
    }

  }

  &__result {
    font-size: 0.82rem;
    font-weight: 600;
    margin: 0;
    padding: 6px 12px;
    border-radius: 8px;
    text-align: center;

    &--accepted {
      background: rgba(34, 197, 94, 0.15);
      color: #22c55e;
    }

    &--rejected {
      background: rgba(239, 68, 68, 0.15);
      color: #ef4444;
    }
  }

  &__status {
    font-size: 0.78rem;
    color: var(--soc-text-muted, #78716c);
    margin: 0;
  }

  &__actions {
    display: flex;
    gap: 8px;
  }

  &__btn {
    flex: 1;
    padding: 6px 12px;
    border-radius: 8px;
    border: none;
    font-size: 0.82rem;
    font-weight: 600;
    cursor: pointer;
    transition: opacity 0.15s;

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    &--accept {
      background: #22c55e;
      color: white;

      &:hover:not(:disabled) {
        opacity: 0.9;
      }
    }

    &--reject {
      background: var(--soc-input-bg, #f0eeeb);
      color: var(--soc-text, #1a1a1a);

      &:hover:not(:disabled) {
        background: var(--soc-bar-hover, #e5e3df);
      }
    }
  }
}
</style>
