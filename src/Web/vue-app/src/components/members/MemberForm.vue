<template>
  <form class="form" novalidate @submit.prevent="handleSubmit">
    <FormRow :withThreeColumns="true">
      <FormInput :ref="addFormInputRef"
                 v-model="member.firstName"
                 :label="t('global.firstName')"
                 :rules="[required]"
                 name="firstName"
                 type="text"
                 @validated="handleValidation"/>
      <FormInput :ref="addFormInputRef"
                 v-model="member.lastName"
                 :label="t('global.lastName')"
                 :rules="[required]"
                 name="lastName"
                 type="text"
                 @validated="handleValidation"/>
      <FormInput :ref="addFormInputRef"
                 v-model="member.email"
                 :label="t('global.email')"
                 :rules="[required]"
                 name="email"
                 type="text"
                 @validated="handleValidation"/>
    </FormRow>
    <FormRow :withThreeColumns="true">
      <div class="form__2fr-1fr-fields">
        <FormInput :ref="addFormInputRef"
                   v-model="member.phoneNumber"
                   :label="t('global.phoneNumber')"
                   :placeholder="t('global.phoneNumberFormat')"
                   :rules="[required, mustMatchPhoneNumberFormat]"
                   name="phoneNumber"
                   type="tel"
                   @validated="handleValidation"/>
        <FormInput :ref="addFormInputRef"
                   v-model="member.phoneExtension"
                   :label="t('global.phoneExtension')"
                   name="phoneExtension"
                   type="number"
                   @validated="handleValidation"/>
      </div>
      <FormInput :ref="addFormInputRef"
                 v-model="member.apartment"
                 :label="t('global.apartment')"
                 name="apartment"
                 type="number"
                 @validated="handleValidation"/>
      <FormInput :ref="addFormInputRef"
                 v-model="member.street"
                 :label="t('global.street')"
                 :rules="[required]"
                 name="street"
                 type="text"
                 @validated="handleValidation"/>
    </FormRow>
    <FormRow :withThreeColumns="true">
      <FormInput :ref="addFormInputRef"
                 v-model="member.city"
                 :label="t('global.city')"
                 :rules="[required]"
                 name="city"
                 type="text"
                 @validated="handleValidation"/>
      <FormInput :ref="addFormInputRef"
                 v-model="member.zipCode"
                 :label="t('global.zipCode')"
                 :rules="[required, mustMatchZipCodeFormat]"
                 name="zipCode"
                 type="zip"
                 @validated="handleValidation"/>
    </FormRow>
    <button class="form__submit btn btn--fullscreen">{{ t('global.save') }}</button>
  </form>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n"
import {notifyError} from "@/notify"
import {Status} from "@/validation"
import {Member} from "@/types/entities"
import {ref, type ComponentPublicInstance} from "vue"
import {
  mustMatchPhoneNumberFormat,
  mustMatchZipCodeFormat,
  required
} from "@/validation/rules";
import FormRow from "@/components/forms/FormRow.vue"
import FormInput from "@/components/forms/FormInput.vue"

 
const props = defineProps<{
  member?: Member
}>()

 
const emit = defineEmits<{
  (event: "formSubmit", member: Member): void
}>()

const {t} = useI18n()

const member = ref<Member>(props.member ?? {})

const formInputs = ref<any[]>([])
const inputValidationStatuses: any = {}

function addFormInputRef(el: Element | ComponentPublicInstance | null) {
  if (!formInputs.value.includes(el as ComponentPublicInstance) && el)
    formInputs.value.push(el as ComponentPublicInstance)
}

async function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid
}

async function handleSubmit() {
  formInputs.value.forEach((x: any) => x.validateInput())
  if (Object.values(inputValidationStatuses).some(x => x === false)) {
    notifyError(t('validation.errorsInForm'))
    return
  }
  emit("formSubmit", member.value)
}
</script>