<template>
  <div class="mb-3 rounded-lg bg-gray-50 p-3">
    <h3 class="mb-3 text-sm font-semibold text-gray-900">{{ poll.question }}</h3>

    <div class="space-y-2">
      <button
        v-for="option in poll.options"
        :key="option.id"
        type="button"
        :disabled="!isClickable(option) || voting"
        class="relative block w-full overflow-hidden rounded-lg border border-gray-200 bg-white text-left transition disabled:cursor-default"
        :class="{ 'hover:border-[#1a1a1a] cursor-pointer': isClickable(option) }"
        @click="vote(option)"
      >
        <div
          class="absolute inset-0 bg-gray-100 transition-all"
          :style="{ width: `${option.percentage}%` }"
        ></div>
        <div class="relative flex items-center justify-between px-3 py-2 text-xs">
          <span class="flex items-center gap-2 font-medium text-gray-800">
            <svg
              v-if="option.hasVoted"
              width="14"
              height="14"
              viewBox="0 0 24 24"
              fill="none"
              stroke="#1a1a1a"
              stroke-width="3"
              stroke-linecap="round"
              stroke-linejoin="round"
            >
              <polyline points="20 6 9 17 4 12"/>
            </svg>
            {{ option.text }}
          </span>
          <span class="text-gray-500">{{ option.voteCount }} · {{ Math.round(option.percentage) }}%</span>
        </div>
      </button>
    </div>

    <div class="mt-2 flex items-center justify-between text-[11px] text-gray-400">
      <span>{{ totalVotes }} {{ totalVotes === 1 ? 'vote' : 'votes' }}</span>
      <span v-if="poll.allowMultipleAnswers">Plusieurs réponses possibles</span>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue'
import { useSocialService } from '@/inversify.config'
import type { Poll, PollOption } from '@/types/entities'

const props = defineProps<{ postId: string; poll: Poll }>()
const emit = defineEmits<{ voted: [] }>()

const socialService = useSocialService()
const voting = ref(false)

const totalVotes = computed(() =>
  props.poll.options.reduce((sum, o) => sum + o.voteCount, 0)
)

function isClickable(option: PollOption): boolean {
  if (option.hasVoted) return false
  if (props.poll.hasVoted && !props.poll.allowMultipleAnswers) return false
  return true
}

async function vote(option: PollOption) {
  if (!isClickable(option) || voting.value) return
  voting.value = true
  try {
    const result = await socialService.votePoll(props.postId, option.id)
    if (result.succeeded) emit('voted')
  } finally {
    voting.value = false
  }
}
</script>
