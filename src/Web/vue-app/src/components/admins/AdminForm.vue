<template>
  <form class="form" novalidate @submit.prevent="handleSubmit">
    <FormRow :withThreeColumns="true">
      <FormInput :ref="addFormInputRef"
                 v-model="admin.firstName"
                 :label="t('global.firstName')"
                 :rules="[required]"
                 name="firstName"
                 type="text"
                 placeholder="Jean"
                 @validated="handleValidation"/>
      <FormInput :ref="addFormInputRef"
                 v-model="admin.lastName"
                 :label="t('global.lastName')"
                 :rules="[required]"
                 name="lastName"
                 type="text"
                 placeholder="Tremblay"
                 @validated="handleValidation"/>
      <FormInput :ref="addFormInputRef"
                 v-model="admin.email"
                 :label="t('global.email')"
                 :rules="[required]"
                 name="email"
                 type="text"
                 placeholder="jean.tremblay@exemple.com"
                 @validated="handleValidation"/>
    </FormRow>
    <button class="form__submit btn">{{ t('global.save') }}</button>
  </form>
</template>

<script lang="ts" setup>
import { ref, type ComponentPublicInstance } from "vue"
import { useI18n } from "vue-i18n"
import { Status } from "@/validation"
import { Administrator } from "@/types/entities"
import { required } from "@/validation/rules"
import FormRow from "@/components/forms/FormRow.vue"
import FormInput from "@/components/forms/FormInput.vue"

const props = defineProps<{
  admin?: Administrator
}>()

const emit = defineEmits<{
  (event: "formSubmit", admin: Administrator): void
}>()

const { t } = useI18n()

const admin = ref<Administrator>(props.admin ?? {})

const formInputs = ref<any[]>([])
const inputValidationStatuses: any = {}

function addFormInputRef(el: Element | ComponentPublicInstance | null) {
  if (!formInputs.value.includes(el as ComponentPublicInstance) && el)
    formInputs.value.push(el as ComponentPublicInstance)
}

function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid
}

function handleSubmit() {
  formInputs.value.forEach((x: any) => x.validateInput())
  if (Object.values(inputValidationStatuses).some(x => x === false)) return
  emit("formSubmit", admin.value)
}
</script>
