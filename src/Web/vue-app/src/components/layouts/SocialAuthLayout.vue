<template>
  <div class="flex min-h-screen flex-col items-center justify-center bg-gray-50 px-4">
    <div class="mb-8 flex flex-col items-center">
      <div class="flex h-16 w-16 items-center justify-center rounded-full bg-[#1a1a1a]">
        <span class="text-sm font-black text-[#be1e2c]">EDB</span>
      </div>
      <h1 class="mt-3 text-xl font-bold text-gray-900">Expression Danse</h1>
    </div>
    <div class="w-full max-w-md rounded-xl bg-white p-6 shadow-sm">
      <RouterView v-slot="{ Component }">
        <template v-if="Component">
          <Suspense>
            <component :is="Component" />
            <template #fallback>
              <div class="flex items-center justify-center py-10">
                <div class="h-8 w-8 animate-spin rounded-full border-2 border-[#be1e2c] border-t-transparent"></div>
              </div>
            </template>
          </Suspense>
        </template>
      </RouterView>
    </div>
  </div>
</template>
