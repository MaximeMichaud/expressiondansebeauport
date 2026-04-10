<template>
  <div v-if="open" class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4" @click.self="handleCancel">
    <div class="w-full max-w-lg rounded-xl bg-white p-6 shadow-xl">
      <h2 class="mb-4 text-lg font-bold text-gray-900">Créer un sondage</h2>

      <div class="mb-4">
        <label class="mb-1 block text-sm font-medium text-gray-700">Question</label>
        <textarea
          v-model="question"
          rows="2"
          maxlength="500"
          placeholder="Posez votre question..."
          class="w-full resize-none rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:bg-white focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
        ></textarea>
        <div class="mt-1 text-right text-xs text-gray-400">{{ question.length }} / 500</div>
      </div>

      <div class="mb-4">
        <label class="mb-1 block text-sm font-medium text-gray-700">Options</label>
        <div v-for="(_, i) in options" :key="i" class="mb-2 flex items-center gap-2">
          <input
            v-model="options[i]"
            maxlength="200"
            :placeholder="`Option ${i + 1}`"
            class="flex-1 rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-sm focus:border-[#1a1a1a] focus:bg-white focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
          />
          <button
            v-if="options.length > 2"
            type="button"
            class="flex h-8 w-8 items-center justify-center rounded-lg text-gray-400 hover:bg-gray-100 hover:text-red-600"
            @click="removeOption(i)"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        <button
          v-if="options.length < 10"
          type="button"
          class="mt-2 text-sm font-medium text-[#1a1a1a] hover:underline"
          @click="addOption"
        >
          + Ajouter une option
        </button>
      </div>

      <label class="mb-4 flex items-center gap-2 text-sm text-gray-700">
        <input v-model="allowMultipleAnswers" type="checkbox" class="rounded border-gray-300" />
        Permettre plusieurs réponses
      </label>

      <div class="flex justify-end gap-2">
        <button
          type="button"
          class="rounded-lg px-4 py-2 text-sm font-medium text-gray-600 hover:bg-gray-100"
          @click="handleCancel"
        >
          Annuler
        </button>
        <button
          type="button"
          :disabled="!canSubmit || submitting"
          class="rounded-lg bg-[#1a1a1a] px-4 py-2 text-sm font-semibold text-white hover:bg-black disabled:opacity-50"
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
import { useSocialService } from '@/inversify.config'

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
