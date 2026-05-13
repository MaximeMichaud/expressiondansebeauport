<template>
  <button
    type="button"
    class="help-button"
    :aria-label="t('components.helpButton.label')"
    :title="t('components.helpButton.tooltip')"
    @click="onClick"
  >
    <HelpCircle :size="18" aria-hidden="true" />
  </button>
</template>

<script lang="ts" setup>
import {useI18n} from 'vue-i18n'
import {HelpCircle} from 'lucide-vue-next'

import {useHelpDrawerStore} from '@/stores/helpDrawerStore'

const {t} = useI18n()
const helpDrawer = useHelpDrawerStore()

function onClick() {
  helpDrawer.toggle()
}
</script>

<style scoped>
.help-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2.25rem;
  height: 2.25rem;
  border-radius: 9999px;
  border: 0;
  background: transparent;
  color: #4b5563;
  cursor: pointer;
  transition: background-color 0.15s, color 0.15s, transform 0.15s;
}

.help-button:hover {
  background: #f3f4f6;
  color: #111827;
}

.help-button:focus-visible {
  outline: 2px solid #b91c1c;
  outline-offset: 2px;
}

.help-button:active {
  transform: scale(0.95);
}
</style>
