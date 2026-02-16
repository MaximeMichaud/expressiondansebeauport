<template>
  <Card :title="t('routes.resetPassword.name')" 
        class="form" 
        :is-authentication="true">
    <Loader v-if="preventMultipleSubmit" />
    <FormInput :ref="addFormInputRef"
               v-model="resetPasswordRequest.password"
               :label="t('global.password')"
               :rules="[required]"
               name="password"
               type="password"
               @validated="handleValidation"/>
    <FormInput :ref="addFormInputRef"
               v-model="resetPasswordRequest.passwordConfirmation"
               :label="t('global.passwordConfirmation')"
               :rules="[required]"
               name="passwordConfirmation"
               type="password"
               @validated="handleValidation"/>
    <button class="btn btn--full btn--purple btn--big" @click="sendResetPasswordRequest" :disabled="preventMultipleSubmit">
      {{ t('global.submit') }}
    </button>
  </Card>
</template>
<script lang="ts" setup>
import {ref, type ComponentPublicInstance} from "vue"
import {useI18n} from "vue3-i18n"
import {required} from "@/validation/rules"
import {useAuthenticationService} from "@/inversify.config";
import {notifyError, notifySuccess} from "@/notify";
import {Status} from "@/validation";
import {IResetPasswordRequest} from "@/types/requests";
import Card from "@/components/layouts/items/Card.vue";
import FormInput from "@/components/forms/FormInput.vue";
import {Guid} from "@/types";
import {useRouter} from "vue-router";
import Loader from "@/components/layouts/items/Loader.vue";

 
const props = defineProps<{
  userId: Guid
  token: string
}>()

const {t} = useI18n()
const router = useRouter()
const authenticationService = useAuthenticationService()

const resetPasswordRequest = ref<IResetPasswordRequest>({
  userId: props.userId,
  token: props.token,
  password: '',
  passwordConfirmation: ''
})

const formInputs = ref<ComponentPublicInstance[]>([])
const inputValidationStatuses: any = {}

const preventMultipleSubmit = ref<boolean>(false);

function addFormInputRef(el: Element | ComponentPublicInstance | null) {
  if (!formInputs.value.includes(el as ComponentPublicInstance))
    formInputs.value.push(el as ComponentPublicInstance)
}

async function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid
}

async function sendResetPasswordRequest() {
  if(preventMultipleSubmit.value) return;

  preventMultipleSubmit.value = true;
  
  formInputs.value.forEach((x: any) => x.validateInput())
  if (Object.values(inputValidationStatuses).some(x => x === false)) {
    notifyError(t('validation.errorsInForm'))
    preventMultipleSubmit.value = false;
    return;
  }

  const resetPasswordResponse = await authenticationService.resetPassword(resetPasswordRequest.value)
  if (resetPasswordResponse.succeeded) {
    preventMultipleSubmit.value = false;
    notifySuccess(t('pages.resetPassword.validation.success'))
    setTimeout(() => {
      router.push(t("routes.login.path"))
    }, 1500);
    return;
  }

  const errorMessages = resetPasswordResponse.getErrorMessages('pages.resetPassword.validation');
  if (errorMessages.length == 0)
    notifyError(t('pages.resetPassword.validation.errorOccured'))
  else
    notifyError(errorMessages[0])

  preventMultipleSubmit.value = false;
}
</script>