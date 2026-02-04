<template>
  <div>
      <FormTextEditor
        :modelValue="valueFr"
        :ref="addFormInputRef"
        :name="nameFr" 
        :label="labelFr"
        :rules="rulesFr"
        @validated="handleValidation"
      />
      <FormTextEditor
        :modelValue="valueEn"
        :ref="addFormInputRef"
        :name="nameEn" 
        :label="labelEn"
        :rules="rulesEn"
        @validated="handleValidation"
      />
  </div>
</template>

<script setup lang="ts">
import FormTextEditor from "@/components/forms/FormTextEditor.vue";
import { Status } from '@/validation'
import { ref } from "vue";
import { Rule } from "@/validation/rules";

// eslint-disable-next-line
const props = defineProps<{
  valueFr?: string
  valueEn?: string
  nameFr: string
  nameEn: string
  labelFr?: string
  labelEn?: string
  rulesFr?: Rule[]
  rulesEn?: Rule[]
}>();

// eslint-disable-next-line
defineExpose({
    //to call validation in parent.
    validateInput
})

// eslint-disable-next-line
const emit = defineEmits<{
  // states that the event has to be called 'validated
  (event: "validated", name: string, validationStatus: Status): void;
}>();

const valueFr = ref<string>(props.valueFr ?? '')
const valueEn = ref<string>(props.valueEn ?? '')
const formInputs = ref<(typeof FormTextEditor)[]>([])

function addFormInputRef(ref: typeof FormTextEditor) {
    if (!formInputs.value.includes(ref))
        formInputs.value.push(ref)
}

function validateInput() {
    formInputs.value.forEach((x: typeof FormTextEditor) => x.validateTextEditor())
}

async function handleValidation(name: string, validationStatus: Status) {
  emit("validated", name, validationStatus);
}
</script>
