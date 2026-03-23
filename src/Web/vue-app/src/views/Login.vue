<template>
  <Logo class="login__logo" />
  <Card class="form"
        :is-authentication="true"
        @keyup.enter="sendLoginRequest">
    <Loader v-if="preventMultipleSubmit" />
    <FormInput :ref="addFormInputRef"
               v-model="loginRequest.username"
               :label="t('global.username')"
               placeholder="exemple@courriel.com"
               :rules="[required]"
               name="username"
               type="email"
               @validated="handleValidation"/>
    <FormInput ref="passwordRef"
               v-model="loginRequest.password"
               :label="t('global.password')"
               :placeholder="t('pages.login.passwordPlaceholder')"
               :rules="[required]"
               name="password"
               type="password"
               @validated="handleValidation">
      <template v-slot:to-label-right>
        <TextLink :path="{path: t('routes.forgotPassword.path') }"
                  :text="t('pages.login.forgotPassword')"/>
      </template>
    </FormInput>
    <button class="btn btn--fullscreen" @click="sendLoginRequest" :disabled="preventMultipleSubmit">
      {{ t('pages.login.submit') }}
    </button>
  </Card>
</template>
<script lang="ts" setup>
import { ref, onMounted, type ComponentPublicInstance } from "vue"
import { useI18n } from "vue3-i18n"
import { required } from "@/validation/rules"
import { useRouter } from "vue-router";
import { useAuthenticationService, useUserService } from "@/inversify.config";
import { useUserStore } from "@/stores/userStore";
import { Role } from "@/types/enums";

import { Status } from "@/validation";
import { ILoginRequest } from "@/types/requests/loginRequest";

import Card from "@/components/layouts/items/Card.vue";
import Logo from "@/assets/icons/logo__edb.svg";
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

const formInputs = ref<ComponentPublicInstance[]>([])
const inputValidationStatuses: any = {}
const passwordRef = ref<any>(null)

const preventMultipleSubmit = ref<boolean>(false);

onMounted(() => {
  if (passwordRef.value) formInputs.value.push(passwordRef.value as ComponentPublicInstance)
})

function addFormInputRef(el: Element | ComponentPublicInstance | null) {
  if (!formInputs.value.includes(el as ComponentPublicInstance))
    formInputs.value.push(el as ComponentPublicInstance)
}

async function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid
}

async function sendLoginRequest() {
  if(preventMultipleSubmit.value) return;

  preventMultipleSubmit.value = true;

  formInputs.value.forEach((x: any) => x.validateInput())
  if (Object.values(inputValidationStatuses).some(x => x === false)) {
    preventMultipleSubmit.value = false;
    return
  }

  const succeededOrNotResponse = await authenticationService.login(loginRequest.value)
  if (succeededOrNotResponse.succeeded) {
    const user = await userService.getCurrentUser()
    userStore.setUser(user)
    userStore.setUsername(loginRequest.value.username)
    apiStore.setNeedToLogout(false)
    const destination = userStore.hasRole(Role.Admin)
      ? t("routes.admin.children.pages.fullPath")
      : t("routes.account.path")
    await router.push(destination)
    preventMultipleSubmit.value = false;
    return;
  }

  const twoFactorRequired = succeededOrNotResponse.errors.some(x => x.errorType == "TwoFactorRequired")
  if (twoFactorRequired) {
    userStore.setUsername(loginRequest.value.username)
    await router.push(t("routes.twoFactor.path"))
    preventMultipleSubmit.value = false;
    return;
  }

  const errorMessages = succeededOrNotResponse.getErrorMessages('pages.login.validation')
  const message = errorMessages.length > 0 ? errorMessages[0] : t('pages.login.validation.errorOccured')
  passwordRef.value?.setError(message)

  preventMultipleSubmit.value = false;
}
</script>

<style scoped>
.login__logo {
  display: block;
  margin: 0 auto 2rem;
  width: 180px;
  height: auto;
}
</style>