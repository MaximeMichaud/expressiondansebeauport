<template>
  <div class="content-grid content-grid--subpage content-grid--subpage-table">
    <div class="content-grid__header">
      <h1 class="back-link">{{ t(`routes.admin.children.members.name`) }}</h1>
      <div class="content-grid__filters">
        <SearchInput v-model="searchValue"/>
      </div>
    </div>
    <div class="content-grid__actions">
      <BtnLink
          :name="t('routes.admin.children.members.add.name')"
          :path="{ path: t('routes.admin.children.members.add.fullPath') }"
      />
    </div>
    <Loader v-if="preventMultipleSubmit" />
    <DataTable
        :headers="memberHeader"
        :is-loading="membersAreLoading"
        :items="tableMembers"
        :total-items="paginatedResponse.totalItems"
        :search-value="searchValue"
        @accept="accept"
        @decline="decline"
        @delete="onDelete"
        @reload="loadPageClients"
    />
  </div>
</template>

<script lang="ts" setup>
import {useI18n} from "vue3-i18n";
import {computed, onMounted, ref, watch} from "vue";
import {useMemberService} from "@/inversify.config";
import {notifyError, notifySuccess} from "@/notify";
import {Member} from "@/types/entities";
import {PaginatedResponse} from "@/types/responses";
import SearchInput from "@/components/layouts/items/SearchInput.vue";
import DataTable from "@/components/layouts/items/DataTable.vue";
import BtnLink from "@/components/layouts/items/BtnLink.vue";
import Loader from "@/components/layouts/items/Loader.vue";
import {Tables} from "@/types/enums";

const {t} = useI18n()

const memberService = useMemberService()

const preventMultipleSubmit = ref<boolean>(false);

const searchValue = ref("");
const membersAreLoading = ref(false);
const pageMembers = ref<Member[]>([]);
const paginatedResponse = ref<PaginatedResponse<Member>>({totalItems: 0});

const tableMembers = computed(() => {
  return pageMembers.value.map((x: Member) => {
    return {
      id: x.id,
      firstName: x.firstName,
      lastName: x.lastName,
      email: x.email,
      actions: {
        edit: {name: `admin.children.members.edit`, params: {id: x.id}},
        delete: true
      }
    }
  })
})

watch(searchValue, () => {
  loadPageClients(1, Tables.DefaultRowsPerPage)
});

onMounted(async () => {
  await loadPageClients(1, Tables.DefaultRowsPerPage);
});

async function loadPageClients(pageIndex: number, pageSize: number) {
  membersAreLoading.value = true;
  let response = await memberService.search(pageIndex, pageSize, searchValue.value);
  if (response) {
    paginatedResponse.value = response;
    if (response.items)
      pageMembers.value = response.items;
  }
  membersAreLoading.value = false;
}

async function onDelete(item: any) {
  if(preventMultipleSubmit.value) return;

  preventMultipleSubmit.value = true;

  let confirmApprobation = confirm(t("pages.members.delete.confirmation"));
  if (!confirmApprobation) {
    preventMultipleSubmit.value = false;
    return;
  }

  let succeededOrNotResponse = await memberService.deleteMember(item.id)
  if (succeededOrNotResponse && succeededOrNotResponse.succeeded) {
    let memberIndex = pageMembers.value.indexOf(pageMembers.value.filter(x => x.id == item.id)[0])
    pageMembers.value.splice(memberIndex, 1)
    notifySuccess(t('pages.members.delete.validation.successMessage'))
    preventMultipleSubmit.value = false;
  } else {
    let errorMessages = succeededOrNotResponse.getErrorMessages('pages.members.delete.validation');
    if (errorMessages.length == 0)
      notifyError(t('validation.errorOccured'))
    else
      notifyError(errorMessages[0])
    preventMultipleSubmit.value = false;
  }
}

const memberHeader = computed(() => [
  {
    text: t("global.firstName"),
    value: 'firstName',
    width: 150,
  },
  {
    text: t("global.lastName"),
    value: "lastName",
    width: 150,
  },
  {
    text: t("global.email"),
    value: "email",
    width: 125,
  },
  {
    text: t("global.table.actions"),
    value: "actions",
    width: 150
  },
])

</script>
