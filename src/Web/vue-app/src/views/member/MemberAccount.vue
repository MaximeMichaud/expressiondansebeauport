<template>
  <div class="content-grid content-grid--double">
    <div class="content-grid__header">
      <h1>{{ t('routes.account.name') }}</h1>
    </div>

    <div v-if="member" class="flex items-center gap-5 p-6 bg-card rounded-lg border border-border sm:col-span-2">
      <div class="size-16 rounded-full bg-primary flex items-center justify-center shrink-0">
        <span class="text-xl font-bold text-primary-foreground uppercase">{{ initials }}</span>
      </div>
      <div>
        <p class="text-lg font-semibold text-foreground">{{ member.fullName }}</p>
        <p class="text-sm text-muted-foreground">{{ member.email }}</p>
      </div>
    </div>

    <Card>
      <Loader v-if="isLoadingProfile" />
      <MemberForm v-if="member" :member="member" @formSubmit="handleUpdateMember" />
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
          {{ t('global.submit') }}
        </button>
      </form>
    </Card>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted, type ComponentPublicInstance } from "vue";
import { useI18n } from "vue3-i18n";
import { required } from "@/validation/rules";
import { Status } from "@/validation";
import { IChangePasswordRequest } from "@/types/requests";
import { Member } from "@/types/entities";
import { useAuthenticationService, useMemberService } from "@/inversify.config";
import { notifyError, notifySuccess } from "@/notify";
import Card from "@/components/layouts/items/Card.vue";
import FormInput from "@/components/forms/FormInput.vue";
import FormRow from "@/components/forms/FormRow.vue";
import Loader from "@/components/layouts/items/Loader.vue";
import MemberForm from "@/components/members/MemberForm.vue";

const { t } = useI18n();
const authenticationService = useAuthenticationService();
const memberService = useMemberService();

const member = ref<Member | undefined>(undefined);
const isLoadingProfile = ref(false);
const isLoadingPassword = ref(false);

const passwordInputs = ref<ComponentPublicInstance[]>([]);
const passwordValidationStatuses: any = {};

const changePasswordRequest = ref<IChangePasswordRequest>({
  currentPassword: '',
  newPassword: '',
  newPasswordConfirmation: '',
});

const initials = computed(() => {
  const name = member.value?.fullName?.trim();
  if (!name) return '?';
  const parts = name.split(/\s+/);
  if (parts.length >= 2) return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  return parts[0][0].toUpperCase();
});

onMounted(async () => {
  try {
    member.value = await memberService.getAuthenticated();
  } catch {
    // pas de profil membre
  }
});

function addPasswordInputRef(el: Element | ComponentPublicInstance | null) {
  if (!passwordInputs.value.includes(el as ComponentPublicInstance))
    passwordInputs.value.push(el as ComponentPublicInstance);
}

function handlePasswordValidation(name: string, validationStatus: Status) {
  passwordValidationStatuses[name] = validationStatus.valid;
}

async function handleUpdateMember(updatedMember: Member) {
  if (isLoadingProfile.value) return;
  isLoadingProfile.value = true;

  const response = await memberService.updateMember(updatedMember);
  if (response.succeeded) {
    member.value = updatedMember;
    notifySuccess(t('pages.account.personalInfoSuccess'));
    isLoadingProfile.value = false;
    return;
  }

  const errorMessages = response.getErrorMessages('pages.members.update.validation');
  notifyError(errorMessages.length > 0 ? errorMessages[0] : t('pages.account.personalInfoError'));
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
