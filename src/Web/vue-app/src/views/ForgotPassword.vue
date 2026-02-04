<template>
  <BackLink :path="{name: 'login'}"/>

  <Card :title="t('routes.forgotPassword.name')" 
        class="form" 
        :is-authentication="true">
    <Loader v-if="preventMultipleSubmit" />
    <FormTooltip>
      <p v-html="t('pages.forgotPassword.tooltip')"></p>
    </FormTooltip>
    <FormInput :ref="addFormInputRef"
               v-model="username"
               :label="t('global.username')"
               :rules="[required]"
               name="code"
               type="text"
               @validated="handleValidation"/>
    <button class="btn btn--full btn--purple btn--big" @click="sendForgotPasswordRequest" :disabled="preventMultipleSubmit">
      {{ t('global.submit') }}
    </button>
  </Card>
</template>
<script lang="ts" setup>
import {ref} from "vue"
import {useI18n} from "vue3-i18n"
import {required} from "@/validation/rules"
import {useAuthenticationService} from "@/inversify.config";
import {notifyError, notifySuccess} from "@/notify";
import {Status} from "@/validation";
import {IForgotPasswordRequest} from "@/types/requests";
import Card from "@/components/layouts/items/Card.vue";
import FormInput from "@/components/forms/FormInput.vue";
import FormTooltip from "@/components/layouts/items/Tooltip.vue";
import BackLink from "@/components/layouts/items/BackLink.vue";
import Loader from "@/components/layouts/items/Loader.vue";

const {t} = useI18n()
const authenticationService = useAuthenticationService()

const username = ref<string>('')

const formInputs = ref<(typeof FormInput)[]>([])
const inputValidationStatuses: any = {}

const preventMultipleSubmit = ref<boolean>(false);

function addFormInputRef(ref: typeof FormInput) {
  if (!formInputs.value.includes(ref))
    formInputs.value.push(ref)
}

async function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid
}

async function sendForgotPasswordRequest() {
  if(preventMultipleSubmit.value) return;

  preventMultipleSubmit.value = true;
  
  formInputs.value.forEach((x: typeof FormInput) => x.validateInput())
  if (Object.values(inputValidationStatuses).some(x => x === false)) {
    notifyError(t('validation.errorsInForm'))
    preventMultipleSubmit.value = false;
    return
  }

  let request = {
    username: username.value,
    resetPasswordRelativeUrl: t('routes.resetPassword.path')
  } as IForgotPasswordRequest
  let forgotPasswordResponse = await authenticationService.forgotPassword(request)
  if (!forgotPasswordResponse.succeeded) {
    notifyError(t('pages.forgotPassword.validation.errorOccured'))
    preventMultipleSubmit.value = false;
    return;
  }
  notifySuccess(t('pages.forgotPassword.validation.success'))
  preventMultipleSubmit.value = false;
}
</script>