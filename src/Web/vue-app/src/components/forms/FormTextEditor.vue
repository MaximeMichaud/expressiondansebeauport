<template>
  <div class="form__field" :class="{'error':!status.valid}">
    <FormTextEditorLinkPopup
      v-if="showQuillEditorLinkPopup"
      @close-popup="() => showQuillEditorLinkPopup = false"
      :quill-editor="quillEditor"
      :name="name"
    />
    <QuillEditor
      ref="quillEditor"
      :toolbar="toolbarOptions"
      :modules="modules"
      content-type="html"
      v-model:content="contentModel"
      :id="name"
      @update:content="handleChange"
      @blur="handleBlur"
      :aria-invalid="!status.valid"
      :aria-describedby="`error__${name}`"
    />

    <label :for="name">
      {{ label ? label : name }}
      <span class="form__indicator" v-if="isRequired">*</span>
      <span class="form__tooltip">
        {{ tooltip ? `${tooltip}<br/>` : ""}}
        {{ t("textEditorLinkPopup.tooltip") }}
      </span>
    </label>

    <span class="form__error-message" :id="`error__${name}`" v-if="!status.valid">
      {{ status.message }}
    </span>
  </div>
</template>

<script setup lang="ts">
import { requiredTextEditor, Rule } from '@/validation/rules'
import { Status, validate } from '@/validation'
import { ref, watch } from "vue";
import { QuillEditor } from "@vueup/vue-quill";
import BlotFormatter from "quill-blot-formatter";
import MagicUrl from 'quill-magic-url'
import FormTextEditorLinkPopup from "@/components/forms/FormTextEditorLinkPopup.vue";

//The doc tell us to load styles like this:
// import "@vueup/vue-quill/dist/vue-quill.snow.css";
//but they dont show up in prod. So I stocked them in our project directly :
import "@/assets/plugins/vue-quill/vue-quill.snow.min.css";
import { useI18n } from "vue3-i18n";

// eslint-disable-next-line
const props = defineProps<{
  name: string;
  label?: string;
  modelValue: string;
  tooltip?: string;
  rules?: Rule[];
}>();

// eslint-disable-next-line
defineExpose({
  //to call validation in parent.
  validateInput : validateTextEditor
})

const { t } = useI18n()

const toolbarOptions = [
  ["clean"], // remove formatting button

  ["bold", "italic"], // toggled buttons

  [{ list: "ordered" }, { list: "bullet" }],
  [{ indent: "-1" }, { indent: "+1" }], // outdent/indent

  [{ header: [2, 3, 4, false] }],

  [{ color: ["#528965", "#000000"] }], // dropdown with defaults from theme
  [{ align: [] }],

  [ 'link', 'image' ],
];

const modules = [
  {
    name: 'blotFormatter', //so images can be resized in editor
    module: BlotFormatter,
    options: {}
  },
  {
    name: 'magicUrl', //so urls are recognize when pasted.
    module: MagicUrl,
    options: {}
  }
]

// eslint-disable-next-line
const emit = defineEmits<{
  // states that the event has to be called 'update:modelValue'
  (event: "update:modelValue", value: string): void;
  (event: "validated", name: string, validationStatus: Status): void;
}>();

const contentModel = ref(props.modelValue);
watch(props, (newProps) => contentModel.value = newProps.modelValue);
const status = ref<Status>({ valid: true });
const isRequired = !(props.rules != null && props.rules.length == 0);

const showQuillEditorLinkPopup = ref<boolean>(false);
const quillEditor = ref<typeof QuillEditor | null>(null);
watch(quillEditor, (editor) => {
  //since Quill cant do it itself after 8 (!!!) years, I'm customizing the add link input.
  const quill = editor?.getQuill();
  const toolbar = quill.getModule("toolbar");

  if (toolbar) {
    const showLinkPopup = () => {
      showQuillEditorLinkPopup.value = true;
    }

    toolbar.addHandler("link", showLinkPopup)
  }
});

function handleChange() {
  const value = quillEditor.value?.getContents();
  validateTextEditor();

  contentModel.value = value
  emit("update:modelValue", value);
}

function handleBlur() {
  validateTextEditor();
}

function validateTextEditor() {
  const value = quillEditor.value?.getContents();
  let validationRules = props.rules ? props.rules : [requiredTextEditor]
  status.value = validate(value, validationRules)

  emit("validated", props.name, status.value);
}
</script>
