<template>
  <div class="flex h-[calc(100vh-60px)] flex-col">
    <!-- Header -->
    <div class="flex items-center gap-3 border-b border-gray-200 px-4 py-3">
      <button @click="$router.push({ name: 'socialMessages' })" class="text-gray-600">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
        </svg>
      </button>
      <h2 class="text-base font-semibold text-gray-900">{{ otherMemberName }}</h2>
    </div>

    <!-- Messages -->
    <div ref="messagesContainer" class="flex flex-1 flex-col-reverse gap-1 overflow-y-auto px-4 py-3">
      <div v-if="loading" class="flex justify-center py-10">
        <div class="h-6 w-6 animate-spin rounded-full border-2 border-[#1a1a1a] border-t-transparent"></div>
      </div>
      <div
        v-for="msg in messages"
        :key="msg.id"
        :class="['flex', msg.senderMemberId === currentMemberId ? 'justify-end' : 'justify-start']"
      >
        <div
          :class="[
            'max-w-[75%] rounded-2xl px-3.5 py-2 text-sm',
            msg.senderMemberId === currentMemberId
              ? 'bg-[#1a1a1a] text-white'
              : 'bg-gray-100 text-gray-900'
          ]"
        >
          {{ msg.content }}
          <p :class="['mt-0.5 text-[10px]', msg.senderMemberId === currentMemberId ? 'text-gray-300' : 'text-gray-400']">
            {{ formatTime(msg.created) }}
          </p>
        </div>
      </div>
    </div>

    <!-- Input -->
    <div class="border-t border-gray-200 px-4 py-3">
      <div class="flex gap-2">
        <input
          v-model="newMessage"
          type="text"
          class="flex-1 rounded-full border border-gray-300 px-4 py-2 text-sm focus:border-[#1a1a1a] focus:outline-none focus:ring-1 focus:ring-[#1a1a1a]"
          placeholder="Écrire un message..."
          @keyup.enter="sendMessage"
        />
        <button
          @click="sendMessage"
          :disabled="!newMessage.trim() || sending"
          class="flex h-10 w-10 items-center justify-center rounded-full bg-[#1a1a1a] text-white transition hover:bg-[#000000] disabled:opacity-50"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
          </svg>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useSocialService } from '@/inversify.config'
import { useUserStore } from '@/stores/userStore'
import type { Message } from '@/types/entities'

const route = useRoute()
const socialService = useSocialService()
const userStore = useUserStore()

const conversationId = computed(() => route.params.conversationId as string)
const messages = ref<Message[]>([])
const loading = ref(true)
const newMessage = ref('')
const sending = ref(false)
const otherMemberName = ref('Conversation')
const currentMemberId = ref('')

function formatTime(dateStr: string) {
  return new Date(dateStr).toLocaleTimeString('fr-CA', { hour: '2-digit', minute: '2-digit' })
}

async function loadMessages() {
  loading.value = true
  try {
    messages.value = await socialService.getMessages(conversationId.value)
    await socialService.markAsRead(conversationId.value)
  } catch (e) { /* */ }
  loading.value = false
}

async function sendMessage() {
  if (!newMessage.value.trim()) return
  sending.value = true
  try {
    await socialService.sendMessage(conversationId.value, newMessage.value)
    newMessage.value = ''
    await loadMessages()
  } catch (e) { /* */ }
  sending.value = false
}

onMounted(loadMessages)
</script>
