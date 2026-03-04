<template>
  <AdminAccount v-if="userStore" />
</template>

<script lang="ts" setup>
import AdminAccount from "@/views/admin/AdminAccount.vue";
import { useUserStore } from "@/stores/userStore";

const userStore = useUserStore()
</script>