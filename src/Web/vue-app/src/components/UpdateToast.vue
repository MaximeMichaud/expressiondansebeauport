<template>
  <Transition name="update-toast">
    <div v-if="updateAvailable && !dismissed" class="update-toast" role="status" aria-live="polite">
      <span class="update-toast__message">{{ t('updateToast.message') }}</span>
      <div class="update-toast__actions">
        <button type="button" class="update-toast__btn update-toast__btn--primary" @click="reload">
          {{ t('updateToast.reload') }}
        </button>
        <button type="button" class="update-toast__btn" @click="dismissed = true">
          {{ t('updateToast.dismiss') }}
        </button>
      </div>
    </div>
  </Transition>
</template>

<script lang="ts" setup>
import { ref } from "vue"
import { useI18n } from "vue-i18n"
import { useVersionCheck } from "@/composables/useVersionCheck"

const { t } = useI18n()
const { updateAvailable, reload } = useVersionCheck()
const dismissed = ref(false)
</script>

<style lang="scss" scoped>
.update-toast {
  position: fixed;
  bottom: 1.5rem;
  right: 1.5rem;
  z-index: 10000;
  max-width: 22rem;
  padding: 1rem 1.25rem;
  background: var(--color-surface, #ffffff);
  color: var(--color-text, #111827);
  border: 1px solid var(--color-border, #e5e7eb);
  border-radius: 0.5rem;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.12);
  display: flex;
  flex-direction: column;
  gap: 0.75rem;

  &__message {
    font-size: 0.95rem;
    line-height: 1.4;
  }

  &__actions {
    display: flex;
    gap: 0.5rem;
    justify-content: flex-end;
  }

  &__btn {
    padding: 0.4rem 0.9rem;
    font-size: 0.875rem;
    border-radius: 0.375rem;
    border: 1px solid var(--color-border, #e5e7eb);
    background: transparent;
    color: inherit;
    cursor: pointer;
    transition: background 0.15s ease;

    &:hover {
      background: var(--color-surface-hover, #f3f4f6);
    }

    &--primary {
      background: var(--color-primary, #2563eb);
      color: #ffffff;
      border-color: transparent;

      &:hover {
        background: var(--color-primary-hover, #1d4ed8);
      }
    }
  }
}

.update-toast-enter-active,
.update-toast-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.update-toast-enter-from,
.update-toast-leave-to {
  opacity: 0;
  transform: translateY(0.5rem);
}
</style>
