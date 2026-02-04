<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>{{ t(`routes.admin.children.members.edit.name`) }}</h1>
    </div>

    <BackLink/>

    <Card>
      <Loader v-if="preventMultipleSubmit" />
      <MemberForm :member="member" @formSubmit="handleSubmit"/>
    </Card>
  </div>
</template>

<script lang="ts" setup>
import {ref} from "vue";
import {useI18n} from "vue3-i18n";
import {useRouter} from "vue-router";
import {useMemberService} from "@/inversify.config";
import {notifyError, notifySuccess} from "@/notify";
import {Member} from "@/types/entities";
import MemberForm from "@/components/members/MemberForm.vue";
import Card from "@/components/layouts/items/Card.vue";
import BackLink from "@/components/layouts/items/BackLink.vue";
import Loader from "@/components/layouts/items/Loader.vue";

// eslint-disable-next-line no-undef
const props = defineProps<{
  id: string
}>();

const {t} = useI18n()
const router = useRouter();
const memberService = useMemberService();

const member = ref<Member>(await memberService.getMember(props.id))

const preventMultipleSubmit = ref<boolean>(false);

async function handleSubmit(member: Member) {
  if(preventMultipleSubmit.value) return;

  preventMultipleSubmit.value = true;

  let succeededOrNotResponse = await memberService.updateMember(member)
  if (succeededOrNotResponse.succeeded) {
    preventMultipleSubmit.value = false;
    notifySuccess(t('pages.members.update.validation.successMessage'))
    setTimeout(() => {
      router.back();
    }, 1500);
    return;
  }

  let errorMessages = succeededOrNotResponse.getErrorMessages('pages.members.update.validation');
  if (errorMessages.length == 0)
    notifyError(t('pages.members.update.validation.failedMessage'))
  else
    notifyError(errorMessages[0])

  preventMultipleSubmit.value = false;
}
</script>
