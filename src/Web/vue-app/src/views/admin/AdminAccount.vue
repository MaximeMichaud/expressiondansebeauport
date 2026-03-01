<template>
  <div class="content-grid">
    <div class="content-grid__header">
      <h1>{{ t('routes.account.name') }}</h1>
    </div>

    <div class="account__card">
    <Card>
      <Loader v-if="isLoading" />
      <form class="form" novalidate @submit.prevent="submitChangePassword">
        <FormRow :withThreeColumns="true">
          <FormInput :ref="addFormInputRef"
                     v-model="changePasswordRequest.currentPassword"
                     :label="t('pages.account.currentPassword')"
                     :rules="[required]"
                     name="currentPassword"
                     type="password"
                     placeholder="••••••••"
                     @validated="handleValidation" />
          <FormInput :ref="addFormInputRef"
                     v-model="changePasswordRequest.newPassword"
                     :label="t('pages.account.newPassword')"
                     :rules="[required]"
                     name="newPassword"
                     type="password"
                     placeholder="••••••••"
                     @validated="handleValidation" />
          <FormInput :ref="addFormInputRef"
                     v-model="changePasswordRequest.newPasswordConfirmation"
                     :label="t('pages.account.newPasswordConfirmation')"
                     :rules="[required]"
                     name="newPasswordConfirmation"
                     type="password"
                     placeholder="••••••••"
                     @validated="handleValidation" />
        </FormRow>
        <button class="form__submit btn" type="submit" :disabled="isLoading">
          {{ t('global.submit') }}
        </button>
      </form>
    </Card>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, type ComponentPublicInstance } from "vue";
import { useI18n } from "vue3-i18n";
import { required } from "@/validation/rules";
import { Status } from "@/validation";
import { IChangePasswordRequest } from "@/types/requests";
import { useAuthenticationService } from "@/inversify.config";
import Card from "@/components/layouts/items/Card.vue";
import FormInput from "@/components/forms/FormInput.vue";
import FormRow from "@/components/forms/FormRow.vue";
import Loader from "@/components/layouts/items/Loader.vue";

const { t } = useI18n();
const authenticationService = useAuthenticationService();

const isLoading = ref(false);
const formInputs = ref<ComponentPublicInstance[]>([]);
const inputValidationStatuses: any = {};

const changePasswordRequest = ref<IChangePasswordRequest>({
  currentPassword: '',
  newPassword: '',
  newPasswordConfirmation: '',
});

function addFormInputRef(el: Element | ComponentPublicInstance | null) {
  if (!formInputs.value.includes(el as ComponentPublicInstance))
    formInputs.value.push(el as ComponentPublicInstance);
}

function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid;
}

async function submitChangePassword() {
  if (isLoading.value) return;

  formInputs.value.forEach((x: any) => x.validateInput());
  if (Object.values(inputValidationStatuses).some(x => x === false)) return;

  isLoading.value = true;

  const response = await authenticationService.changePassword(changePasswordRequest.value);
  if (response.succeeded) {
    changePasswordRequest.value = { currentPassword: '', newPassword: '', newPasswordConfirmation: '' };
    isLoading.value = false;
    return;
  }

  isLoading.value = false;
}
</script>

<style scoped>
.account__card {
  width: 75%;
}

@media (max-width: 767px) {
  .account__card {
    width: 100%;
  }
}
</style>
