<template>
  <div class="form__fields-container" :class="{ error: !status.valid }">
    <p class="form__fields-title" v-if="title">{{ title }}</p>
    <ul class="form__field form__radios">
      <li class="form__radio" v-for="option in options" :key="option.name">
        <input
          type="radio"
          :name="name"
          :id="`${name}-${option.name}`"
          :value="option.name"
          :aria-invalid="!status.valid"
          :aria-describedby="`error__${name}`"
          :checked="modelValue === option.name"
          @change="handleChange"
          @blur="handleBlur"
        />
        <label :for="`${name}-${option.name}`">
          {{option.label}}
          <span class="form__indicator" v-if="required">*</span>
        </label>
      </li>
    </ul>
    <span
      class="form__error-message"
      :id="`error__${name}`"
      v-if="!status.valid"
    >
      {{ t('validation.select') }}
    </span>
  </div>
</template>

<script setup lang="ts">
import { FormOption } from "@/types/formOption";
import { Status} from '@/validation'
import { ref } from "vue";
import { useI18n } from "vue3-i18n";

// eslint-disable-next-line
const props = defineProps<{
  title?: string;
  name: string;
  modelValue: string;
  options: FormOption[];
  required?: boolean;
}>();

// eslint-disable-next-line
defineExpose({
  //to call validation in parent.
  validateInput: validateRadios
})

// eslint-disable-next-line
const emit = defineEmits<{
  // states that the event has to be called 'update:modelValue'
  (event: "update:modelValue", value: string): void;
  (event: "validated", name: string, validationStatus: Status): void;
}>();

const { t } = useI18n();
const status = ref<Status>({ valid: true });

function handleChange(e: Event) {
  const groupName = (e.target as HTMLInputElement).name;
  validateRadios(groupName);

  const value = (e.target as HTMLInputElement).value;
  emit("update:modelValue", value);
}

function handleBlur(e: Event) {
  const groupName = (e.target as HTMLInputElement).name;
  validateRadios(groupName)
}

function validateRadios(groupName:string) {
  const isChecked = document.querySelector(`[name=${groupName}]:checked`) != null;

  status.value = { valid: props.required ? isChecked : true };

  emit("validated", props.name, status.value);
}
</script>
