<template>
  <form class="popup popup--visible" novalidate @submit.prevent="handleSubmit">
    <div class="popup__bg" @click="closePopup"></div>
    <div class="popup__container">
      <div class="popup__content">
        <div class="popup__header">
          <p class="h2-like">{{ t("textEditorLinkPopup.addLink") }}</p>
        </div>
        <div class="popup__block">
          <FormInput v-if="!selectionContainsImage"
                     v-model="link.label"
                     :ref="addFormInputRef"
                     :label="t('textEditorLinkPopup.label')"
                     :name="`link-label-${name}`"
                     type="text"
                     @validated="handleValidation"/>
          <FormTooltip v-else>
            <p>{{ t("textEditorLinkPopup.image") }}</p>
          </FormTooltip>
          <FormInput v-model="link.url"
                     :ref="addFormInputRef"
                     :label="t('textEditorLinkPopup.url')"
                     :name="`link-url-${name}`"
                     type="url"
                     @validated="handleValidation"/>
          <div class="form__submit">
            <button class="btn btn--fullscreen">{{ t('global.add') }}</button>
            <button class="btn btn--fullscreen btn--red" type="button" @click="closePopup">{{ t('global.cancel') }}</button>
          </div>
        </div>
      </div>
    </div>
  </form>
</template>

<script lang="ts" setup>
import { computed, onMounted, ref, type ComponentPublicInstance } from "vue"
import { useI18n } from "vue3-i18n"
import { notifyError } from "@/notify"
import { Status } from "@/validation"
import FormInput from "@/components/forms/FormInput.vue"
import { Link } from "@/types";
import type { Editor } from "@tiptap/vue-3";
import { DOMSerializer } from "@tiptap/pm/model";
import FormTooltip from "@/components/forms/FormTooltip.vue";

const props = defineProps<{
  name: string;
  editor: Editor
}>()

const emit = defineEmits<{
  (event: "closePopup"): void
}>()

const { t } = useI18n()

const selectionHTML = ref("")
const selectionContainsImage = computed(() => selectionHTML.value?.includes("<img"))
const selectedText = ref("")
const link = ref<Link>({
  label: "",
  url: "",
})

onMounted(() => {
  const { from, to } = props.editor.state.selection;

  if (from !== to) {
    selectedText.value = props.editor.state.doc.textBetween(from, to, ' ');

    const fragment = props.editor.state.doc.slice(from, to).content;
    const tempDiv = document.createElement('div');
    const serializer = DOMSerializer.fromSchema(props.editor.schema);
    const dom = serializer.serializeFragment(fragment);
    tempDiv.appendChild(dom);
    selectionHTML.value = tempDiv.innerHTML;

    if (selectedText.value) {
      link.value.label = selectedText.value;
    }
  }
});

function handleSubmit() {
  formInputs.value.forEach((x: any) => x?.validateInput())

  if (Object.values(inputValidationStatuses).some(x => x === false)) {
    notifyError(t('global.formErrorNotification'))
    return
  }

  addLinkToEditor();
}

function addLinkToEditor() {
  const url = !/^https?:\/\//i.test(link.value.url) ? "https://" + link.value.url.replace("http://", "") : link.value.url;

  const { from, to } = props.editor.state.selection;

  if (selectionContainsImage.value) {
    props.editor
      .chain()
      .focus()
      .deleteRange({ from, to })
      .insertContentAt(from, selectionHTML.value.replace(/<img/, `<a href="${url}"><img`).replace(/>$/, `></a>`))
      .run();
  } else {
    const content = link.value.label;
    const linkHtml = `<a href="${url}">${content}</a>`;

    props.editor
      .chain()
      .focus()
      .deleteRange({ from, to })
      .insertContentAt(from, linkHtml)
      .run();
  }

  closePopup();
}

function closePopup() {
  emit("closePopup");
}

//Validation =================================
const formInputs = ref<ComponentPublicInstance[]>([])
const inputValidationStatuses: any = {}

function addFormInputRef(el: Element | ComponentPublicInstance | null) {
  if (!formInputs.value.includes(el as ComponentPublicInstance))
    formInputs.value.push(el as ComponentPublicInstance)
}

async function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid
}
</script>
