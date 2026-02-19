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
import { ref, type ComponentPublicInstance } from "vue";
import { Rule } from "@/validation/rules";

 
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

 
defineExpose({
    //to call validation in parent.
    validateInput
})

 
const emit = defineEmits<{
  // states that the event has to be called 'validated
  (event: "validated", name: string, validationStatus: Status): void;
}>();

const valueFr = ref<string>(props.valueFr ?? '')
const valueEn = ref<string>(props.valueEn ?? '')
const formInputs = ref<ComponentPublicInstance[]>([])

function addFormInputRef(el: Element | ComponentPublicInstance | null) {
    if (!formInputs.value.includes(el as ComponentPublicInstance))
        formInputs.value.push(el as ComponentPublicInstance)
}

function validateInput() {
    formInputs.value.forEach((x: any) => x.validateInput())
}

async function handleValidation(name: string, validationStatus: Status) {
  emit("validated", name, validationStatus);
}
</script>
