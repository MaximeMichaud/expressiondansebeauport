<template>
  <Teleport to="body">
    <TransitionGroup name="soc-toast" tag="div" class="soc-toast-container">
      <div
        v-for="toast in toasts"
        :key="toast.id"
        :class="['soc-toast', `soc-toast--${toast.type}`]"
        @click="dismiss(toast.id)"
      >
        <svg v-if="toast.type === 'success'" class="soc-toast__icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6L9 17l-5-5"/></svg>
        <svg v-else class="soc-toast__icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>
        <span>{{ toast.message }}</span>
      </div>
    </TransitionGroup>
  </Teleport>
</template>

<script setup lang="ts">
import { useSocialToast } from '@/composables/useSocialToast'

const { toasts, dismiss } = useSocialToast()
</script>

<style lang="scss">
// Mounted via <Teleport to="body">, so styles must be unscoped and global.
// Imported by every layout that needs toasts (SocialLayout, SocialAuthLayout)
// so the CSS chunk loads regardless of which layout the user lands on first.
.soc-toast-container {
  position: fixed;
  bottom: 20px;
  right: 20px;
  z-index: 9999;
  display: flex;
  flex-direction: column-reverse;
  gap: 8px;
  pointer-events: none;
}

.soc-toast {
  pointer-events: auto;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 18px;
  border-radius: 12px;
  font-family: 'Karla', sans-serif;
  font-size: 0.82rem;
  font-weight: 500;
  cursor: pointer;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.12), 0 0 0 1px rgba(0, 0, 0, 0.04);
  backdrop-filter: blur(8px);
  max-width: 360px;

  &--success {
    background: rgba(21, 128, 61, 0.06);
    color: #15803d;
    border: 1px solid rgba(21, 128, 61, 0.15);
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08), 0 0 0 1px rgba(21, 128, 61, 0.1);
  }

  &--error {
    background: rgba(220, 38, 38, 0.06);
    color: #dc2626;
    border: 1px solid rgba(220, 38, 38, 0.15);
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08), 0 0 0 1px rgba(220, 38, 38, 0.1);
  }

  &__icon { flex-shrink: 0; }
}

.soc-toast-enter-active {
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
.soc-toast-leave-active {
  transition: all 0.2s ease-in;
}
.soc-toast-enter-from {
  opacity: 0;
  transform: translateX(30px) scale(0.95);
}
.soc-toast-leave-to {
  opacity: 0;
  transform: translateX(10px) scale(0.98);
}

@media (max-width: 47.99em) {
  .soc-toast-container {
    left: 16px;
    right: 16px;
    bottom: 16px;
  }
  .soc-toast { max-width: 100%; }
}

// Dark mode (the `.soc--dark` class is toggled on <body> by both layouts)
.soc--dark .soc-toast {
  &--success {
    background: rgba(21, 128, 61, 0.15);
    color: #86efac;
    border-color: rgba(21, 128, 61, 0.3);
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3), 0 0 0 1px rgba(21, 128, 61, 0.2);
  }
  &--error {
    background: rgba(220, 38, 38, 0.15);
    color: #fca5a5;
    border-color: rgba(220, 38, 38, 0.3);
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3), 0 0 0 1px rgba(220, 38, 38, 0.2);
  }
}
</style>
