<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="confirm-modal__backdrop"
      @click.self="$emit('cancel')"
    >
      <div
        class="confirm-modal__card"
        :style="{
          background: 'var(--soc-content-bg)',
          borderColor: 'var(--soc-card-border)',
          color: 'var(--soc-text)'
        }"
      >
        <h3 class="mb-1 text-base font-bold" :style="{ color: 'var(--soc-text)' }">
          {{ title }}
        </h3>
        <p class="mb-5 text-sm" :style="{ color: 'var(--soc-text-muted)' }">
          {{ message }}
        </p>
        <div class="flex gap-3">
          <button
            type="button"
            class="soc-btn-cancel flex-1 rounded-lg px-4 py-2.5 text-sm font-semibold cursor-pointer"
            @click="$emit('cancel')"
          >
            {{ cancelLabel || 'Annuler' }}
          </button>
          <button
            type="button"
            :class="['flex-1 rounded-lg px-4 py-2.5 text-sm font-semibold cursor-pointer', danger ? 'soc-btn-danger' : 'soc-btn-confirm']"
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

<style scoped>
.confirm-modal__backdrop {
  position: fixed;
  inset: 0;
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(4px);
  -webkit-backdrop-filter: blur(4px);
}

.confirm-modal__card {
  width: 100%;
  max-width: 380px;
  border-radius: 1rem;
  padding: 1.5rem;
  text-align: center;
  border-width: 1px;
  border-style: solid;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
}

.soc-btn-cancel {
  background: var(--soc-bar-hover);
  color: var(--soc-text);
  transition: background 0.15s;
}
.soc-btn-cancel:hover {
  background: var(--soc-bar-active);
}

.soc-btn-confirm {
  background: var(--soc-text);
  color: var(--soc-content-bg);
  transition: opacity 0.15s;
}
.soc-btn-confirm:hover {
  opacity: 0.9;
}

.soc-btn-danger {
  background: #dc2626;
  color: white;
  transition: background 0.15s;
}
.soc-btn-danger:hover {
  background: #b91c1c;
}
</style>
