<template>
  <div class="content-grid">
    <div class="content-grid__header">
      <h1>{{ t('routes.account.name') }}</h1>
    </div>

    <Card>
      <Loader v-if="isLoadingProfile" />
      <AdminForm v-if="admin" :admin="admin" @formSubmit="handleUpdateAdmin" />
    </Card>

    <Card>
      <Loader v-if="isLoadingPassword" />
      <form class="form" novalidate @submit.prevent="submitChangePassword">
        <FormRow :withThreeColumns="true">
          <FormInput :ref="addPasswordInputRef"
                     v-model="changePasswordRequest.currentPassword"
                     :label="t('pages.account.currentPassword')"
                     :rules="[required]"
                     name="currentPassword"
                     type="password"
                     placeholder="••••••••"
                     @validated="handlePasswordValidation" />
          <FormInput :ref="addPasswordInputRef"
                     v-model="changePasswordRequest.newPassword"
                     :label="t('pages.account.newPassword')"
                     :rules="[required]"
                     name="newPassword"
                     type="password"
                     placeholder="••••••••"
                     @validated="handlePasswordValidation" />
          <FormInput :ref="addPasswordInputRef"
                     v-model="changePasswordRequest.newPasswordConfirmation"
                     :label="t('pages.account.newPasswordConfirmation')"
                     :rules="[required]"
                     name="newPasswordConfirmation"
                     type="password"
                     placeholder="••••••••"
                     @validated="handlePasswordValidation" />
        </FormRow>
        <button class="form__submit btn" type="submit" :disabled="isLoadingPassword">
          {{ t('global.save') }}
        </button>
      </form>
    </Card>
    <div class="text-xs text-muted-foreground">
      <p class="font-medium mb-1.5">{{ t('pages.account.passwordRequirementsTitle') }}</p>
      <ul class="space-y-1">
        <li v-for="key in ['minLength','upper','lower','digit','nonAlphanumeric']"
            :key="key"
            class="flex items-center gap-2">
          <span class="size-1 rounded-full bg-muted-foreground/50 shrink-0"></span>
          {{ t(`pages.account.passwordRequirements.${key}`) }}
        </li>
      </ul>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, type ComponentPublicInstance } from "vue";
import { useI18n } from "vue-i18n";
import { required } from "@/validation/rules";
import { Status } from "@/validation";
import { IChangePasswordRequest } from "@/types/requests";
import { Administrator } from "@/types/entities";
import { useAuthenticationService, useAdministratorService } from "@/inversify.config";
import { usePersonStore } from "@/stores/personStore";
import { notifyError, notifySuccess } from "@/notify";
import Card from "@/components/layouts/items/Card.vue";
import FormInput from "@/components/forms/FormInput.vue";
import FormRow from "@/components/forms/FormRow.vue";
import Loader from "@/components/layouts/items/Loader.vue";
import AdminForm from "@/components/admins/AdminForm.vue";

const { t } = useI18n();
const authenticationService = useAuthenticationService();
const administratorService = useAdministratorService();
const personStore = usePersonStore();

const admin = ref<Administrator | undefined>(undefined);
const isLoadingProfile = ref(false);
const isLoadingPassword = ref(false);

const passwordInputs = ref<ComponentPublicInstance[]>([]);
const passwordValidationStatuses: any = {};

const changePasswordRequest = ref<IChangePasswordRequest>({
  currentPassword: '',
  newPassword: '',
  newPasswordConfirmation: '',
});

onMounted(async () => {
  isLoadingProfile.value = true
  try {
    admin.value = await administratorService.getAuthenticated();
  } catch {
    notifyError(t('validation.errorOccured'));
  } finally {
    isLoadingProfile.value = false
  }
});

function addPasswordInputRef(el: Element | ComponentPublicInstance | null) {
  if (!passwordInputs.value.includes(el as ComponentPublicInstance))
    passwordInputs.value.push(el as ComponentPublicInstance);
}

function handlePasswordValidation(name: string, validationStatus: Status) {
  passwordValidationStatuses[name] = validationStatus.valid;
}

async function handleUpdateAdmin(updatedAdmin: Administrator) {
  if (isLoadingProfile.value) return;
  isLoadingProfile.value = true;

  const response = await administratorService.updateMe(updatedAdmin);
  if (response.succeeded) {
    updatedAdmin.fullName = `${updatedAdmin.firstName} ${updatedAdmin.lastName}`;
    admin.value = updatedAdmin;
    personStore.setPerson(updatedAdmin);
    notifySuccess(t('pages.account.personalInfoSuccess'));
    isLoadingProfile.value = false;
    return;
  }

  notifyError(t('pages.account.personalInfoError'));
  isLoadingProfile.value = false;
}

async function submitChangePassword() {
  if (isLoadingPassword.value) return;

  passwordInputs.value.forEach((x: any) => x.validateInput());
  if (Object.values(passwordValidationStatuses).some(x => x === false)) return;

  isLoadingPassword.value = true;

  const response = await authenticationService.changePassword(changePasswordRequest.value);
  if (response.succeeded) {
    notifySuccess(t('pages.account.validation.success'));
    changePasswordRequest.value = { currentPassword: '', newPassword: '', newPasswordConfirmation: '' };
    isLoadingPassword.value = false;
    return;
  }

  const errorMessages = response.getErrorMessages('pages.account.validation');
  notifyError(errorMessages.length > 0 ? errorMessages[0] : t('pages.account.validation.errorOccured'));
  isLoadingPassword.value = false;
}
</script>
