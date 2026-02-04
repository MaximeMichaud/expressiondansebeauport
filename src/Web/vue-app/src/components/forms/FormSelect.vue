<template>
  <div class="form__field" :class="{ error: !status.valid }">
    <select
      :name="name"
      :id="name"
      v-model="inputValue"
      @change="handleChange"
      @blur="handleBlur"
      :aria-invalid="!status.valid"
      :aria-describedby="`error__${name}`"
      >
      <option v-for="option in options" :key="option.name" :value="option.name" :selected="modelValue === option.name">{{option.label}}</option>
    </select>
    <label :for="name" v-if="!noLabel"> 
      {{ label ? label : name }}
      <span class="form__indicator" v-if="isRequired">*</span>
    </label>

    <span class="form__error-message" :id="`error__${name}`" v-if="!status.valid">
      {{ t('validation.select') }}
    </span>
  </div>
</template>

<script setup lang="ts">
import { FormOption } from "@/types/formOption";
import {validate, Status} from '@/validation'
import {required, Rule} from '@/validation/rules'
import {ref, watch} from "vue";
import { useI18n } from "vue3-i18n";

// eslint-disable-next-line
const props = defineProps<{
  name: string;
  modelValue: string;
  label?: string;
  rules?: Rule[];
  options: FormOption[];
  noLabel?: boolean;
}>();

// eslint-disable-next-line
defineExpose({
  //to call validation in parent.
    validateInput
})

// eslint-disable-next-line
const emit = defineEmits<{
  // states that the event has to be called 'update:modelValue'
  (event: "update:modelValue", value: string): void;
  (event: "validated", name: string, validationStatus: Status): void;
}>();

const { t } = useI18n();
const status = ref<Status>( { valid: true });
const isRequired = !(props.rules != null && props.rules.length == 0);
const inputValue = ref<string>(props.modelValue as string);
watch(props, (newProps) => inputValue.value = newProps.modelValue)

function handleChange() {
  validateInput();
  emit("update:modelValue", inputValue.value);
}

function handleBlur() {
  validateInput()
}

function validateInput() {
  let validationRules = props.rules ? props.rules : [required]
  status.value = validate(inputValue.value, validationRules)

  emit("validated", props.name, status.value);
}
</script>
