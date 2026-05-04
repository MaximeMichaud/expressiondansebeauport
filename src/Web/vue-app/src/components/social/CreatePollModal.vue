<template>
  <div
    v-if="open"
    class="fixed inset-0 z-[9999] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4"
    @click.self="handleCancel"
  >
    <div
      class="w-full max-w-lg rounded-2xl p-6 shadow-2xl border"
      :style="{
        background: 'var(--soc-content-bg)',
        borderColor: 'var(--soc-card-border)',
        color: 'var(--soc-text)'
      }"
    >
      <h2 class="mb-4 text-lg font-bold" :style="{ color: 'var(--soc-text)' }">
        Créer un sondage
      </h2>

      <div class="mb-4">
        <label class="mb-1 block text-sm font-medium" :style="{ color: 'var(--soc-text)' }">
          Question
        </label>
        <textarea
          v-model="question"
          rows="2"
          maxlength="500"
          placeholder="Posez votre question..."
          class="soc-input w-full resize-none rounded-lg px-3 py-2 text-sm focus:outline-none"
        ></textarea>
        <div class="mt-1 text-right text-xs" :style="{ color: 'var(--soc-text-muted)' }">
          {{ question.length }} / 500
        </div>
      </div>

      <div class="mb-4">
        <label class="mb-2 block text-sm font-medium" :style="{ color: 'var(--soc-text)' }">
          Options
        </label>
        <div v-for="(_, i) in options" :key="i" class="mb-2 flex items-center gap-2">
          <input
            v-model="options[i]"
            maxlength="200"
            :placeholder="`Option ${i + 1}`"
            class="soc-input flex-1 rounded-lg px-3 py-2 text-sm focus:outline-none"
          />
          <button
            v-if="options.length > 2"
            type="button"
            class="soc-icon-btn flex h-8 w-8 items-center justify-center rounded-lg"
            @click="removeOption(i)"
            aria-label="Retirer l'option"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        <button
          v-if="options.length < 10"
          type="button"
          class="soc-link-btn mt-2 text-sm font-medium"
          @click="addOption"
        >
          + Ajouter une option
        </button>
      </div>

      <label
        class="mb-5 flex cursor-pointer items-center gap-2 text-sm"
        :style="{ color: 'var(--soc-text)' }"
      >
        <input v-model="allowMultipleAnswers" type="checkbox" class="soc-checkbox" />
        Permettre plusieurs réponses
      </label>

      <div class="flex justify-end gap-2">
        <button
          type="button"
          class="soc-btn-secondary rounded-lg px-4 py-2 text-sm font-semibold cursor-pointer"
          @click="handleCancel"
        >
          Annuler
        </button>
        <button
          type="button"
          :disabled="!canSubmit || submitting"
          class="soc-btn-primary rounded-lg px-4 py-2 text-sm font-semibold cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed"
          @click="handleSubmit"
        >
          Publier le sondage
        </button>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, watch } from 'vue'
import { useSocialService } from '@/serviceRegistry'

const props = defineProps<{ groupId: string; open: boolean }>()
const emit = defineEmits<{ close: []; created: [] }>()

const socialService = useSocialService()

const question = ref('')
const options = ref<string[]>(['', ''])
const allowMultipleAnswers = ref(false)
const submitting = ref(false)

const trimmedOptions = computed(() =>
  options.value.map(o => o.trim()).filter(o => o.length > 0)
)

const canSubmit = computed(() => {
  if (!question.value.trim()) return false
  if (trimmedOptions.value.length < 2) return false
  const unique = new Set(trimmedOptions.value.map(o => o.toLowerCase()))
  if (unique.size !== trimmedOptions.value.length) return false
  return true
})

function addOption() {
  if (options.value.length < 10) options.value.push('')
}

function removeOption(i: number) {
  if (options.value.length > 2) options.value.splice(i, 1)
}

async function handleSubmit() {
  if (!canSubmit.value || submitting.value) return
  submitting.value = true
  try {
    const result = await socialService.createPoll(
      props.groupId,
      question.value.trim(),
      trimmedOptions.value,
      allowMultipleAnswers.value
    )
    if (result.succeeded) {
      emit('created')
      reset()
      emit('close')
    }
  } finally {
    submitting.value = false
  }
}

function handleCancel() {
  reset()
  emit('close')
}

function reset() {
  question.value = ''
  options.value = ['', '']
  allowMultipleAnswers.value = false
}

watch(() => props.open, (val) => { if (val) reset() })
</script>

<style scoped>
.soc-input {
  background: var(--soc-input-bg);
  border: 1px solid var(--soc-input-border);
  color: var(--soc-text);
  transition: border-color 0.15s, box-shadow 0.15s, background 0.15s;
}
.soc-input::placeholder {
  color: var(--soc-text-muted);
  opacity: 0.7;
}
.soc-input:focus {
  border-color: var(--soc-text);
  box-shadow: 0 0 0 1px var(--soc-text);
}

.soc-icon-btn {
  color: var(--soc-text-muted);
  transition: color 0.15s, background 0.15s;
}
.soc-icon-btn:hover {
  color: #ef4444;
  background: var(--soc-bar-hover);
}

.soc-link-btn {
  color: var(--soc-text);
  transition: opacity 0.15s;
}
.soc-link-btn:hover {
  text-decoration: underline;
  opacity: 0.85;
}

.soc-checkbox {
  width: 1rem;
  height: 1rem;
  border-radius: 0.25rem;
  border: 1px solid var(--soc-input-border);
  background: var(--soc-input-bg);
  cursor: pointer;
  accent-color: var(--soc-text);
}

.soc-btn-secondary {
  background: var(--soc-bar-hover);
  color: var(--soc-text);
  transition: background 0.15s, opacity 0.15s;
}
.soc-btn-secondary:hover {
  background: var(--soc-bar-active);
}

.soc-btn-primary {
  background: var(--soc-text);
  color: var(--soc-content-bg);
  transition: opacity 0.15s;
}
.soc-btn-primary:hover:not(:disabled) {
  opacity: 0.9;
}
</style>
