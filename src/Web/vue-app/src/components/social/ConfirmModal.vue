<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="confirm-modal__backdrop"
      @click.self="$emit('cancel')"
    >
      <div class="confirm-modal__card">
        <div v-if="danger" class="confirm-modal__icon">
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#dc2626" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="3 6 5 6 21 6"/>
            <path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/>
          </svg>
        </div>
        <h3 class="confirm-modal__title">{{ title }}</h3>
        <p class="confirm-modal__message">{{ message }}</p>
        <div class="confirm-modal__actions">
          <button
            type="button"
            class="confirm-modal__btn confirm-modal__btn--cancel"
            @click="$emit('cancel')"
          >
            {{ cancelLabel || 'Annuler' }}
          </button>
          <button
            type="button"
            :class="['confirm-modal__btn', danger ? 'confirm-modal__btn--danger' : 'confirm-modal__btn--confirm']"
            @click="$emit('confirm')"
          >
            {{ confirmLabel || 'Confirmer' }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script lang="ts" setup>
defineProps<{
  open: boolean
  title: string
  message: string
  confirmLabel?: string
  cancelLabel?: string
  danger?: boolean
}>()

defineEmits<{
  (e: 'confirm'): void
  (e: 'cancel'): void
}>()
</script>

<style>
.confirm-modal__backdrop {
  position: fixed;
  inset: 0;
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
  -webkit-backdrop-filter: blur(4px);
}

.confirm-modal__card {
  width: 100%;
  max-width: 380px;
  border-radius: 1rem;
  padding: 2rem 2rem 1.75rem;
  text-align: center;
  background: white;
  color: #1c1917;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.35);
}

.confirm-modal__icon {
  width: 3.5rem;
  height: 3.5rem;
  border-radius: 9999px;
  background: rgba(220, 38, 38, 0.08);
  border: 2px solid rgba(220, 38, 38, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1rem;
}

.confirm-modal__title {
  font-size: 1rem;
  font-weight: 700;
  margin: 0 0 0.25rem;
  color: #1c1917;
}

.confirm-modal__message {
  font-size: 0.875rem;
  margin: 0 0 1.25rem;
  color: #6b7280;
}

.confirm-modal__actions {
  display: flex;
  gap: 0.75rem;
}

.confirm-modal__btn {
  flex: 1;
  border: 0;
  border-radius: 0.5rem;
  padding: 0.625rem 1rem;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s, opacity 0.15s;
}

.confirm-modal__btn--cancel {
  background: #f3f4f6;
  color: #374151;
}
.confirm-modal__btn--cancel:hover {
  background: #e5e7eb;
}

.confirm-modal__btn--confirm {
  background: #1a1a1a;
  color: white;
}
.confirm-modal__btn--confirm:hover {
  background: #000;
}

.confirm-modal__btn--danger {
  background: #dc2626;
  color: white;
}
.confirm-modal__btn--danger:hover {
  background: #b91c1c;
}

/* Dark mode (driven by .soc--dark on body via SocialLayout) */
body.soc--dark .confirm-modal__card {
  background: #222120 !important;
  color: #e7e5e4 !important;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.7) !important;
}

body.soc--dark .confirm-modal__icon {
  background: rgba(220, 38, 38, 0.15) !important;
  border-color: rgba(220, 38, 38, 0.3) !important;
}

body.soc--dark .confirm-modal__title {
  color: #e7e5e4 !important;
}

body.soc--dark .confirm-modal__message {
  color: #a8a29e !important;
}

body.soc--dark .confirm-modal__btn--cancel {
  background: rgba(255, 255, 255, 0.08) !important;
  color: #e7e5e4 !important;
}
body.soc--dark .confirm-modal__btn--cancel:hover {
  background: rgba(255, 255, 255, 0.14) !important;
}

body.soc--dark .confirm-modal__btn--confirm {
  background: #e7e5e4 !important;
  color: #0a0a09 !important;
}
body.soc--dark .confirm-modal__btn--confirm:hover {
  background: #fff !important;
}
</style>
