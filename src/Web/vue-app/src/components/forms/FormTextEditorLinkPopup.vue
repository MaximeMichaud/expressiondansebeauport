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
import { computed, onMounted, ref } from "vue"
import { useI18n } from "vue3-i18n"
import { notifyError } from "@/notify"
import { Status } from "@/validation"
import FormInput from "@/components/forms/FormInput.vue"
import { Link } from "@/types";
import { Quill, QuillEditor } from "@vueup/vue-quill";
import FormTooltip from "@/components/forms/FormTooltip.vue";

// eslint-disable-next-line no-undef
const props = defineProps<{
  name: string;
  quillEditor: typeof QuillEditor
}>()

// eslint-disable-next-line
const emit = defineEmits<{
  (event: "closePopup"): void
}>()

const { t } = useI18n()

const selection = ref<any>(null)
const selectionHTML = ref<any>(null)
const selectionContainsImage = computed(() => selectionHTML.value?.includes("<img"))
const selectedText = ref<any>(null)
const link = ref<Link>({
  label: "",
  url: "",
})

//Functions ======================================
onMounted( () => {
  //keep quill editor in memory 
  const quill = props.quillEditor.getQuill();
  
  if(quill) {
    //if the popup is open, init label as selection if applicable.
    selection.value = quill.getSelection(true);
    selectionHTML.value = getSelectionHtml(quill.getContents(selection.value.index, selection.value.length));
    selectedText.value = quill.getText(selection.value.index, selection.value.length);

    if (selectedText.value) {
      link.value.label = selectedText.value;
    }
  }
});

function getSelectionHtml(selectedContent:any) {
  let tempContainer = document.createElement('div')
  let tempQuill = new Quill(tempContainer);
  
  tempQuill.setContents(selectedContent);
  return tempContainer.querySelector('.ql-editor')?.innerHTML;
}

function handleSubmit() {
  formInputs.value.forEach((x: typeof FormInput) => x?.validateInput())
  
  if (Object.values(inputValidationStatuses).some(x => x === false)) {
    notifyError(t('global.formErrorNotification'))
    return
  }

  addLinkToQuill();
}

function addLinkToQuill() {
  const quill = props.quillEditor.getQuill();

  if(quill){
    const url = !/^https?:\/\//i.test(link.value.url) ? "https://" + link.value.url.replace("http://", "") : link.value.url;
    const content = selectionContainsImage.value ? selectionHTML.value : link.value.label;
    
    const linkTag = `<a href="${url}">${content}</a>`
    
    selection.value = quill.getSelection(true);
    
    //if selected, delete the old text
    if (selectionContainsImage.value) quill.deleteText(selection.value.index, 1);
    if (selectedText.value) quill.deleteText(selection.value.index, selectedText.value.length);
    
    //then, add the new tag
    quill.clipboard.dangerouslyPasteHTML(selection.value.index, linkTag, 'user');

    if (selectionContainsImage.value) quill.setSelection(selection.value.index, 1);
    if (link.value.label) quill.setSelection(selection.value.index, link.value.label.length);

    //then, close popup
    closePopup();
  }
}

function closePopup() {
  emit("closePopup");
}

//Validation =================================
const formInputs = ref<(typeof FormInput)[]>([])
const inputValidationStatuses: any = {}

function addFormInputRef(ref: typeof FormInput) {
  if (!formInputs.value.includes(ref))
    formInputs.value.push(ref)
}

async function handleValidation(name: string, validationStatus: Status) {
  inputValidationStatuses[name] = validationStatus.valid
}
</script>
