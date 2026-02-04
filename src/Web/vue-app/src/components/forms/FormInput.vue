<template>
  <div :class="{'error':!status.valid}" class="form__field">
    <slot name="before-input"></slot>

    <input
        :id="name"
        v-model="inputValue"
        :aria-describedby="`error__${name}`"
        :aria-invalid="!status.valid"
        :list="list"
        :min="type == 'number' ? '0' : ''"
        :name="name"
        :placeholder="placeholder"
        :type="type"
        @blur="handleBlur"
        @input="handleInput"
    />

    <label :for="name">
      {{ label ? label : name }}
      <span v-if="isRequired" class="form__indicator">*</span>
      <span v-if="tooltip" class="form__tooltip">{{ tooltip }}</span>
    </label>

    <span v-if="!status.valid" :id="`error__${name}`" class="form__error-message">
      {{ status.message }}
    </span>
  </div>
</template>

<script lang="ts" setup>
import IconHelpCircle from "vue-material-design-icons/HelpCircle.vue"
import {Rule} from '@/validation/rules'
import {Status, validate} from '@/validation'
import {ref, watch} from "vue";

// eslint-disable-next-line
const props = defineProps<{
  name: string
  label?: string
  modelValue: string | number | undefined
  placeholder?: string
  type: string
  rules?: Rule[]
  tooltip?: string,
  list?: string
}>();

// eslint-disable-next-line
defineExpose({
  //to call validation in parent.
  validateInput
})

const inputValue = ref<string | number | undefined>(props.modelValue === '' ? undefined : props.modelValue);
watch(props, () => inputValue.value = props.modelValue === '' ? undefined : props.modelValue)

// eslint-disable-next-line
const emit = defineEmits<{
  // states that the event has to be called 'update:modelValue'
  (event: "update:modelValue", value: string | number | undefined): void;
  (event: "validated", name: string, validationStatus: Status): void;
}>();

const status = ref<Status>({valid: true});
const isRequired = !(props.rules != undefined && props.rules.length == 0);

function handleInput() {
  validateInput();
  inputValue.value = inputValue.value === '' ? undefined : inputValue.value
  emit("update:modelValue", inputValue.value);
}

function handleBlur() {
  validateInput();
}

function validateInput() {
  let validationRules = props.rules ? props.rules : []
  status.value = validate(inputValue.value as string, validationRules)
  emit("validated", props.name, status.value);
}
</script>
