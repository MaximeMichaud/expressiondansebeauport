<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-[9999] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4"
      @click.self="$emit('cancel')"
    >
      <div
        class="w-full max-w-[380px] rounded-2xl p-6 text-center shadow-2xl border"
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
