<template>
  <Card :title="t('routes.twoFactor.name')" 
        class="form"
        :is-authentication="true"
        @keyup.enter="sendTwoFactorAuthenticationRequest">
    <Loader v-if="preventMultipleSubmit" />
    <FormTooltip>
      <p v-html="t('pages.twoFactor.tooltip')"></p>
    </FormTooltip>
    <FormInput :ref="addFormInputRef"
               v-model="code"
               :label="t('pages.twoFactor.code')"
               :rules="[required]"
               name="code"
               type="text"
               @validated="handleValidation"/>
    <button class="btn btn--full btn--purple btn--big" @click="sendTwoFactorAuthenticationRequest" :disabled="preventMultipleSubmit">
      {{ t('pages.twoFactor.submit') }}
    </button>
  </Card>
</template>
<script lang="ts" setup>
import {ref} from "vue"
import {useI18n} from "vue3-i18n"
import {required} from "@/validation/rules"
import {useRouter} from "vue-router";
import {useAuthenticationService, useUserService} from "@/inversify.config";
import {notifyError} from "@/notify";
import {useUserStore} from "@/stores/userStore";

import {Status} from "@/validation";
import {ITwoFactorRequest} from "@/types/requests/twoFactorRequest";

import Card from "@/components/layouts/items/Card.vue";
import FormInput from "@/components/forms/FormInput.vue";
import FormTooltip from "@/components/layouts/items/Tooltip.vue";
import {useApiStore} from "@/stores/apiStore";
import Loader from "@/components/layouts/items/Loader.vue";

const {t} = useI18n()
const router = useRouter();
const apiStore = useApiStore();
const userStore = useUserStore();
const userService = useUserService();
const authenticationService = useAuthenticationService()

const code = ref<string>('')

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

async function sendTwoFactorAuthenticationRequest() {
  if(preventMultipleSubmit.value) return;

  preventMultipleSubmit.value = true;
  
  formInputs.value.forEach((x: typeof FormInput) => x.validateInput())
  if (Object.values(inputValidationStatuses).some(x => x === false)) {
    notifyError(t('validation.errorsInForm'))
    preventMultipleSubmit.value = false;
    return
  }

  let request = {username: userStore.username, code: code.value} as ITwoFactorRequest
  let twoFactorResponse = await authenticationService.twoFactor(request)
  if (!twoFactorResponse.succeeded) {
    let errorMessages = twoFactorResponse.getErrorMessages('pages.twoFactor.validation');
    if (errorMessages.length == 0)
      notifyError(t('pages.twoFactor.validation.errorOccured'))
    else
      notifyError(errorMessages[0])

    preventMultipleSubmit.value = false;

    return;
  }

  let user = await userService.getCurrentUser()
  userStore.setUser(user)
  apiStore.setNeedToLogout(false)
  await router.push(t("routes.account.path"))
  preventMultipleSubmit.value = false;
}
</script>