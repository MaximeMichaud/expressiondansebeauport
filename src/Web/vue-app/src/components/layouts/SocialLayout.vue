<template>
  <div class="flex min-h-screen flex-col bg-white">
    <!-- Header -->
    <header class="border-b border-gray-200 bg-white">
      <div class="mx-auto flex max-w-2xl items-center justify-between px-4 py-3">
        <div class="flex items-center gap-3">
          <div class="flex h-10 w-10 items-center justify-center rounded-full bg-[#1a1a1a]">
            <span class="text-xs font-black text-[#be1e2c]">EDB</span>
          </div>
          <h1 class="text-lg font-bold text-gray-900">Expression Danse</h1>
        </div>
        <router-link
          v-if="isAuthenticated"
          :to="{ name: 'socialMessages' }"
          class="relative rounded-full p-2 text-gray-600 transition hover:bg-gray-100"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
          </svg>
          <span
            v-if="unreadCount > 0"
            class="absolute -right-0.5 -top-0.5 flex h-5 w-5 items-center justify-center rounded-full bg-[#be1e2c] text-[10px] font-bold text-white"
          >
            {{ unreadCount > 9 ? '9+' : unreadCount }}
          </span>
        </router-link>
      </div>

      <!-- Tabs -->
      <nav v-if="isAuthenticated && !isGroupPage && !isMessagesPage && !isAccountPage" class="mx-auto flex max-w-2xl border-t border-gray-100 px-4">
        <router-link
          :to="{ name: 'socialHome' }"
          :class="[
            'flex-1 py-3 text-center text-sm font-medium transition',
            isActiveTab('socialHome') ? 'border-b-2 border-[#be1e2c] text-[#be1e2c]' : 'text-gray-500 hover:text-gray-700'
          ]"
        >
          Menu Principal
        </router-link>
        <router-link
          :to="{ name: 'socialImportant' }"
          :class="[
            'flex-1 py-3 text-center text-sm font-medium transition',
            isActiveTab('socialImportant') ? 'border-b-2 border-[#be1e2c] text-[#be1e2c]' : 'text-gray-500 hover:text-gray-700'
          ]"
        >
          Important
        </router-link>
        <router-link
          :to="{ name: 'socialPortal' }"
          :class="[
            'flex-1 py-3 text-center text-sm font-medium transition',
            isActiveTab('socialPortal') ? 'border-b-2 border-[#be1e2c] text-[#be1e2c]' : 'text-gray-500 hover:text-gray-700'
          ]"
        >
          Portail EDB
        </router-link>
      </nav>
    </header>

    <!-- Content -->
    <main class="mx-auto w-full max-w-2xl flex-1">
      <RouterView v-slot="{ Component }">
        <template v-if="Component">
          <Suspense>
            <component :is="Component" />
            <template #fallback>
              <div class="flex items-center justify-center py-20">
                <div class="h-8 w-8 animate-spin rounded-full border-2 border-[#be1e2c] border-t-transparent"></div>
              </div>
            </template>
          </Suspense>
        </template>
      </RouterView>
    </main>

    <!-- Footer -->
    <footer v-if="isAuthenticated && !isGroupPage && !isMessagesPage" class="border-t border-gray-200 bg-gray-50 py-6 text-center text-sm text-gray-500">
      <p>Expression Danse de Beauport</p>
      <p class="mt-1">788 Av. de l'Éducation, Québec</p>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/userStore'

const router = useRouter()
const userStore = useUserStore()

const unreadCount = computed(() => 0) // TODO: wire to memberStore

const isAuthenticated = computed(() => !!userStore.user.email)

const isActiveTab = (name: string) => {
  return router.currentRoute.value.name === name
}

const isGroupPage = computed(() => {
  return router.currentRoute.value.name?.toString().startsWith('socialGroup') ?? false
})

const isMessagesPage = computed(() => {
  const name = router.currentRoute.value.name?.toString() ?? ''
  return name === 'socialMessages' || name === 'socialConversation'
})

const isAccountPage = computed(() => {
  return router.currentRoute.value.name === 'socialAccount'
})
</script>
