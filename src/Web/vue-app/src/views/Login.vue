<template>
  <Card :title="t('routes.login.name')"
        class="form"
        :is-authentication="true"
        @keyup.enter="sendLoginRequest">
    <Loader v-if="preventMultipleSubmit" />
    <FormInput :ref="addFormInputRef"
               v-model="loginRequest.username"
               :label="t('global.username')"
               :rules="[required]"
               name="username"
               type="email"
               @validated="handleValidation"/>
    <FormInput :ref="addFormInputRef"
               v-model="loginRequest.password"
               :label="t('global.password')"
               :rules="[required]"
               name="password"
               type="password"
               @validated="handleValidation">
      <template v-slot:to-label-right>
        <TextLink :path="{path: t('routes.forgotPassword.path') }"
                  :text="t('pages.login.forgotPassword')"/>
      </template>
    </FormInput>
    <button class="btn btn--full btn--purple btn--big" @click="sendLoginRequest" :disabled="preventMultipleSubmit">
      {{ t('pages.login.submit') }}
    </button>
  </Card>
</template>
<script lang="ts" setup>
import { ref } from "vue"
import { useI18n } from "vue3-i18n"
import { required } from "@/validation/rules"
import { useRouter } from "vue-router";
import { useAuthenticationService, useUserService } from "@/inversify.config";
import { useUserStore } from "@/stores/userStore";
import { notifyError } from "@/notify";

import { Status } from "@/validation";
import { ILoginRequest } from "@/types/requests/loginRequest";

import Card from "@/components/layouts/items/Card.vue";
import FormInput from "@/components/forms/FormInput.vue";
import TextLink from "@/components/layouts/items/TextLink.vue";
import { useApiStore } from "@/stores/apiStore";
import Loader from "@/components/layouts/items/Loader.vue";

const { t } = useI18n()
const router = useRouter();
const apiStore = useApiStore();
const userStore = useUserStore();
const userService = useUserService();
const authenticationService = useAuthenticationService()

const loginRequest = ref<ILoginRequest>({ username: '', password: '' })

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

async function sendLoginRequest() {
  if(preventMultipleSubmit.value) return;

  preventMultipleSubmit.value = true;

  formInputs.value.forEach((x: typeof FormInput) => x.validateInput())
  if (Object.values(inputValidationStatuses).some(x => x === false)) {
    notifyError(t('validation.errorsInForm'))
    preventMultipleSubmit.value = false;
    return
  }

  let succeededOrNotResponse = await authenticationService.login(loginRequest.value)
  if (succeededOrNotResponse.succeeded) {
    let user = await userService.getCurrentUser()
    userStore.setUser(user)
    userStore.setUsername(loginRequest.value.username)
    apiStore.setNeedToLogout(false)
    await router.push(t("routes.account.path"))
    preventMultipleSubmit.value = false;
    return;
  }

  let twoFactorRequired = succeededOrNotResponse.errors.some(x => x.errorType == "TwoFactorRequired")
  if (twoFactorRequired) {
    userStore.setUsername(loginRequest.value.username)
    await router.push(t("routes.twoFactor.path"))
    preventMultipleSubmit.value = false;
    return;
  }

  let errorMessages = succeededOrNotResponse.getErrorMessages('pages.login.validation');
  if (errorMessages.length == 0)
    notifyError(t('pages.login.validation.errorOccured'))
  else
    notifyError(errorMessages[0])

  preventMultipleSubmit.value = false;
}
</script>