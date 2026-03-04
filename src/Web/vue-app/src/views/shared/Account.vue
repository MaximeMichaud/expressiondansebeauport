<template>
  <AdminAccount v-if="userStore.hasRole(Role.Admin)" />
</template>

<script lang="ts" setup>
import AdminAccount from "@/views/admin/AdminAccount.vue";
import { useUserStore } from "@/stores/userStore";
import { Role } from "@/types/enums";

const userStore = useUserStore()
</script>