<template>
  <div class="content-grid content-grid--subpage">
    <div class="content-grid__header">
      <h1>{{ page.title }}</h1>
    </div>

    <BackLink/>

    <Loader v-if="preventMultipleSubmit" />
    <form class="form page-editor" novalidate @submit.prevent="handleSubmit">
      <div class="page-editor__title-card">
        <FormInput
            :ref="addFormInputRef"
            v-model="page.title"
            :label="t('pages.pages.title')"
            :rules="[required]"
            name="pageTitle"
            type="text"
            @validated="handleValidation"
        />
      </div>

      <div class="page-editor__sections-header">
        <h2>{{ t('pages.pages.sections') }} ({{ page.sections?.length ?? 0 }})</h2>
        <button type="button" class="btn btn--small btn--secondary" @click="addSection">
          + {{ t('pages.pages.addSection') }}
        </button>
      </div>

      <div
          v-for="(section, index) in page.sections"
          :key="section.id ?? `new-${index}`"
          class="page-editor__section"
          :class="{ 'is-collapsed': collapsedSections[index] }"
      >
        <div class="page-editor__section-header" @click="toggleSection(index)">
          <div class="page-editor__section-info">
            <span class="page-editor__section-number">{{ index + 1 }}</span>
            <span class="page-editor__section-name">{{ section.title || t('pages.pages.section') }}</span>
          </div>
          <div class="page-editor__section-actions">
            <button
                v-if="page.sections!.length > 1"
                type="button"
                class="page-editor__section-remove"
                @click.stop="removeSection(index)"
            >
              {{ t('pages.pages.removeSection') }}
            </button>
            <span class="page-editor__section-chevron">{{ collapsedSections[index] ? '▸' : '▾' }}</span>
          </div>
        </div>

        <div v-show="!collapsedSections[index]" class="page-editor__section-body">
          <FormRow>
            <FormInput
                :ref="addFormInputRef"
                v-model="section.title"
                :label="t('pages.pages.sectionTitle')"
                :rules="[required]"
                :name="`sectionTitle-${index}`"
                type="text"
                @validated="handleValidation"
            />
          </FormRow>

          <div class="page-editor__editor-wrapper">
            <label class="page-editor__editor-label">{{ t('pages.pages.sectionContent') }}</label>
            <FormTextEditor
                :ref="addFormInputRef"
                v-model="section.htmlContent"
                :label="t('pages.pages.sectionContent')"
                :name="`sectionContent-${index}`"
                :rules="[]"
                @validated="handleValidation"
            />
          </div>

          <FormRow>
            <FormInput
                :ref="addFormInputRef"
                v-model="section.imageUrl"
                :label="t('pages.pages.sectionImageUrl')"
                :rules="[]"
                :name="`sectionImageUrl-${index}`"
                type="text"
                @validated="handleValidation"
            />
          </FormRow>
        </div>
      </div>

      <div class="page-editor__footer">
        <button type="button" class="btn btn--secondary" @click="addSection">
          + {{ t('pages.pages.addSection') }}
        </button>
        <button class="btn btn--fullscreen" type="submit">{{ t('global.save') }}</button>
      </div>
    </form>
  </div>
</template>

<script lang="ts" setup>
import {reactive, ref, type ComponentPublicInstance} from "vue";
import {useI18n} from "vue3-i18n";
import {useRouter} from "vue-router";
import {usePageService} from "@/inversify.config";
import {notifyError, notifySuccess} from "@/notify";
import {Page, PageSection} from "@/types/entities";
import {required} from "@/validation/rules";
import {Status} from "@/validation";
import BackLink from "@/components/layouts/items/BackLink.vue";
import Loader from "@/components/layouts/items/Loader.vue";
import FormRow from "@/components/forms/FormRow.vue";
import FormInput from "@/components/forms/FormInput.vue";
import FormTextEditor from "@/components/forms/FormTextEditor.vue";

const props = defineProps<{
  id: string
}>();

const {t} = useI18n()
const router = useRouter();
const pageService = usePageService();

const page = ref<Page>(await pageService.getPage(props.id))
const preventMultipleSubmit = ref<boolean>(false);
const formInputs = ref<any[]>([])
const inputValidationStatuses: any = {}
const collapsedSections = reactive<Record<number, boolean>>({})

function addFormInputRef(el: Element | ComponentPublicInstance | null) {
  if (!formInputs.value.includes(el as ComponentPublicInstance) && el)
    formInputs.value.push(el as ComponentPublicInstance)
}

function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid
}

function toggleSection(index: number) {
  collapsedSections[index] = !collapsedSections[index]
}

function addSection() {
  if (!page.value.sections) page.value.sections = [];
  const newIndex = page.value.sections.length;
  page.value.sections.push({
    title: '',
    htmlContent: '',
    imageUrl: undefined,
    sortOrder: newIndex
  } as PageSection);
  collapsedSections[newIndex] = false;
}

function removeSection(index: number) {
  page.value.sections!.splice(index, 1);
  page.value.sections!.forEach((s, i) => s.sortOrder = i);
}

async function handleSubmit() {
  if (preventMultipleSubmit.value) return;

  formInputs.value.forEach((x: any) => x.validateInput())
  if (Object.values(inputValidationStatuses).some(x => x === false)) {
    notifyError(t('validation.errorsInForm'))
    return
  }

  preventMultipleSubmit.value = true;

  page.value.sections!.forEach((s, i) => s.sortOrder = i);

  const succeededOrNotResponse = await pageService.updatePage(page.value)
  if (succeededOrNotResponse.succeeded) {
    preventMultipleSubmit.value = false;
    notifySuccess(t('pages.pages.update.validation.successMessage'))
    setTimeout(() => {
      router.back();
    }, 1500);
    return;
  }

  const errorMessages = succeededOrNotResponse.getErrorMessages('pages.pages.update.validation');
  if (errorMessages.length == 0)
    notifyError(t('pages.pages.update.validation.failedMessage'))
  else
    notifyError(errorMessages[0])

  preventMultipleSubmit.value = false;
}
</script>
