<template>
  <div class="form__field">
    <input
      type="text"
      readonly
      :name="name"
      :id="name"
      :value="modelValue ? modelValue : t('global.undefined')"
    />
    <label :for="name"> {{ label ? label : name }}</label>
  </div>
</template>

<script setup lang="ts">
import {useI18n} from "vue3-i18n";

// eslint-disable-next-line no-undef
defineProps<{
  name: string
  label?:string
  modelValue: string
}>();

const {t} = useI18n()
</script>
