<template>
  <div class="user-avatar">
    <div class="user-avatar__img-container">
      <IconFaceMan class="icon icon--green"/>
    </div>
    <p class="user-avatar__name">{{ personStore.person.fullName }}</p>
  </div>
</template>

<script lang="ts" setup>
import IconFaceMan from 'vue-material-design-icons/FaceMan.vue';
import {usePersonStore} from "@/stores/personStore";

const personStore = usePersonStore();

</script>