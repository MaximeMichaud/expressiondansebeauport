<template>
  <div class="form__field" :class="{'error':!status.valid}">
    <input
      type="checkbox"
      :name="name"
      :id="name"
      v-model="checked"
      @change="handleChange"
      @blur="handleBlur"
      :aria-invalid="!status.valid"
      :aria-describedby="'error__' + name"
    />
    <label :for="name"> 
      {{ label ? label : name }}
      <span class="form__indicator" v-if="required">*</span>
    </label>

    <span class="form__error-message" :id="'error' + name" v-if="!status.valid">
      {{ status.message }}
    </span>
  </div>
</template>

<script setup lang="ts">
import { Status, validateBoolean } from '@/validation'
import { requiredBoolean } from '@/validation/rules';
import {ref} from "vue";

// eslint-disable-next-line
const props = defineProps<{
  name: string
  modelValue: boolean,
  label?: string,
  required?: boolean
}>();

// eslint-disable-next-line
defineExpose({
  //to call validation in parent.
  validateInput: validateCheckbox
})

// eslint-disable-next-line
const emit = defineEmits<{
  // states that the event has to be called 'update:modelValue'
  (event: "update:modelValue", value: boolean): void;
  (event: "validated", name: string, validationStatus: Status): void;
}>();

const status = ref<Status>( { valid: true });
const checked = ref<boolean>(props.modelValue);

function handleChange(e: Event) {
  const checked = (e.target as HTMLInputElement).checked;

  validateCheckbox(checked);

  emit("update:modelValue", checked);
}

function handleBlur(e: Event) {
  const checked = (e.target as HTMLInputElement).checked;
  validateCheckbox(checked)
}

function validateCheckbox(checked:boolean) {
  status.value = validateBoolean(checked, props.required ? [requiredBoolean] : []);
  emit("validated", props.name, status.value);
}
</script>
