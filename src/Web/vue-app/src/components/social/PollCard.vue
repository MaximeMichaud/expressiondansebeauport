<template>
  <div
    class="mb-3 rounded-lg p-3 border"
    :style="{
      background: 'var(--soc-input-bg)',
      borderColor: 'var(--soc-border)'
    }"
  >
    <h3 class="mb-3 text-sm font-semibold" :style="{ color: 'var(--soc-text)' }">
      {{ poll.question }}
    </h3>

    <div class="space-y-2">
      <button
        v-for="option in poll.options"
        :key="option.id"
        type="button"
        :disabled="voting"
        class="poll-option relative block w-full rounded-lg text-left transition disabled:cursor-default disabled:opacity-70"
        :class="{ 'is-voted': option.hasVoted }"
        :style="{ '--poll-percent': option.percentage + '%' } as any"
        @click="vote(option)"
      >
        <div class="relative flex items-center justify-between px-3 py-2 text-xs">
          <span class="flex items-center gap-2 font-medium" :style="{ color: 'var(--soc-text)' }">
            <svg
              v-if="option.hasVoted"
              width="14"
              height="14"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="3"
              stroke-linecap="round"
              stroke-linejoin="round"
            >
              <polyline points="20 6 9 17 4 12"/>
            </svg>
            {{ option.text }}
          </span>
          <span :style="{ color: 'var(--soc-text-muted)' }">
            {{ option.voteCount }} · {{ Math.round(option.percentage) }}%
          </span>
        </div>
      </button>
    </div>

    <div class="mt-2 flex items-center justify-between text-[11px]" :style="{ color: 'var(--soc-text-muted)' }">
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

async function vote(option: PollOption) {
  if (voting.value) return
  voting.value = true
  try {
    const result = await socialService.votePoll(props.postId, option.id)
    if (result.succeeded) emit('voted')
  } finally {
    voting.value = false
  }
}
</script>

<style>
@property --poll-percent {
  syntax: '<percentage>';
  inherits: false;
  initial-value: 0%;
}
@property --poll-fill {
  syntax: '<color>';
  inherits: false;
  initial-value: rgba(0, 0, 0, 0);
}
</style>

<style scoped>
.poll-option {
  --poll-fill: rgba(124, 58, 237, 0.22);
  --poll-content-bg: var(--soc-content-bg);
  cursor: pointer;
  background: linear-gradient(
    to right,
    var(--poll-fill) 0%,
    var(--poll-fill) var(--poll-percent, 0%),
    var(--poll-content-bg) var(--poll-percent, 0%),
    var(--poll-content-bg) 100%
  );
  box-shadow: inset 0 0 0 1px var(--soc-border);
  transition:
    --poll-percent 0.6s cubic-bezier(0.22, 1, 0.36, 1),
    --poll-fill 0.3s ease,
    box-shadow 0.15s;
}
.poll-option:hover:not(:disabled) {
  box-shadow: inset 0 0 0 1px var(--soc-text);
}
.poll-option.is-voted {
  --poll-fill: rgba(124, 58, 237, 0.38);
  box-shadow: inset 0 0 0 1px var(--soc-text);
}

:global(body.soc--dark) .poll-option {
  --poll-fill: rgba(167, 139, 250, 0.25);
}
:global(body.soc--dark) .poll-option.is-voted {
  --poll-fill: rgba(167, 139, 250, 0.42);
}
</style>
