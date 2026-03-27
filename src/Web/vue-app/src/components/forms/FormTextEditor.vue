<template>
  <div class="form__field" :class="{'error':!status.valid}">
    <FormTextEditorLinkPopup
      v-if="showLinkPopup"
      @close-popup="() => showLinkPopup = false"
      :editor="editor!"
      :name="name"
    />

    <div class="tiptap-wrapper" v-if="editor">
      <div class="tiptap-toolbar">
        <button type="button" @click="editor!.chain().focus().unsetAllMarks().clearNodes().run()" :title="t('textEditor.clean')">
          <i class="tiptap-icon">&#x2715;</i>
        </button>

        <span class="tiptap-toolbar__separator"></span>

        <button type="button" @click="editor!.chain().focus().toggleBold().run()" :class="{ 'is-active': editor!.isActive('bold') }">
          <b>B</b>
        </button>
        <button type="button" @click="editor!.chain().focus().toggleItalic().run()" :class="{ 'is-active': editor!.isActive('italic') }">
          <i>I</i>
        </button>

        <span class="tiptap-toolbar__separator"></span>

        <button type="button" @click="editor!.chain().focus().toggleOrderedList().run()" :class="{ 'is-active': editor!.isActive('orderedList') }">
          <i class="tiptap-icon">&#x2630;</i>
        </button>
        <button type="button" @click="editor!.chain().focus().toggleBulletList().run()" :class="{ 'is-active': editor!.isActive('bulletList') }">
          <i class="tiptap-icon">&#x2022;</i>
        </button>

        <span class="tiptap-toolbar__separator"></span>

        <select class="tiptap-toolbar__select" @change="handleHeadingChange">
          <option value="0" :selected="!editor!.isActive('heading')">{{ t('textEditor.normal') }}</option>
          <option value="2" :selected="editor!.isActive('heading', { level: 2 })">H2</option>
          <option value="3" :selected="editor!.isActive('heading', { level: 3 })">H3</option>
          <option value="4" :selected="editor!.isActive('heading', { level: 4 })">H4</option>
        </select>

        <span class="tiptap-toolbar__separator"></span>

        <button type="button" @click="setColor(primaryColor)" :class="{ 'is-active': editor!.isActive('textStyle', { color: primaryColor }) }">
          <span class="tiptap-color-swatch" :style="{ background: primaryColor }"></span>
        </button>
        <button type="button" @click="setColor('#000000')" :class="{ 'is-active': editor!.isActive('textStyle', { color: '#000000' }) }">
          <span class="tiptap-color-swatch" style="background:#000000"></span>
        </button>

        <span class="tiptap-toolbar__separator"></span>

        <button type="button" @click="editor!.chain().focus().setTextAlign('left').run()" :class="{ 'is-active': editor!.isActive({ textAlign: 'left' }) }">
          <i class="tiptap-icon">&#x2261;</i>
        </button>
        <button type="button" @click="editor!.chain().focus().setTextAlign('center').run()" :class="{ 'is-active': editor!.isActive({ textAlign: 'center' }) }">
          <i class="tiptap-icon">&#x2550;</i>
        </button>
        <button type="button" @click="editor!.chain().focus().setTextAlign('right').run()" :class="{ 'is-active': editor!.isActive({ textAlign: 'right' }) }">
          <i class="tiptap-icon">&#x2262;</i>
        </button>

        <span class="tiptap-toolbar__separator"></span>

        <button type="button" @click="showLinkPopup = true">
          <i class="tiptap-icon">&#x1F517;</i>
        </button>
        <button type="button" @click="addImage">
          <i class="tiptap-icon">&#x1F5BC;</i>
        </button>

        <span class="tiptap-toolbar__separator"></span>

        <button type="button" @click="toggleHtmlMode" :class="{ 'is-active': htmlMode }">
          <i class="tiptap-icon">&lt;/&gt;</i>
        </button>
        <button v-if="hasCssSupport" type="button" @click="toggleCssMode" :class="{ 'is-active': cssMode }">
          <i class="tiptap-icon">{ }</i>
        </button>
      </div>

      <textarea
        v-if="htmlMode"
        class="tiptap-html-editor"
        :id="name"
        :name="name"
        :value="htmlSource"
        @input="onHtmlInput"
        @blur="validateInput"
        :aria-invalid="!status.valid"
        :aria-describedby="!status.valid ? `error__${name}` : undefined"
      ></textarea>
      <textarea
        v-else-if="cssMode"
        class="tiptap-html-editor"
        :value="cssSource"
        @input="onCssInput"
        placeholder=".public-page__title { color: red; }"
      ></textarea>
      <EditorContent v-else :editor="editor" />
    </div>

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
import { ref, watch, onBeforeUnmount, computed } from "vue";
import { useEditor, EditorContent } from "@tiptap/vue-3";
import StarterKit from "@tiptap/starter-kit";
import Link from "@tiptap/extension-link";
import Image from "@tiptap/extension-image";
import TextAlign from "@tiptap/extension-text-align";
import { TextStyle } from "@tiptap/extension-text-style";
import Color from "@tiptap/extension-color";
import FormTextEditorLinkPopup from "@/components/forms/FormTextEditorLinkPopup.vue";
import { useI18n } from "vue-i18n";

const props = defineProps<{
  name: string;
  label?: string;
  modelValue?: string;
  cssModelValue?: string;
  tooltip?: string;
  rules?: Rule[];
}>();

const hasCssSupport = computed(() => props.cssModelValue !== undefined);

defineExpose({
  validateInput
})

const { t } = useI18n()

const primaryColor = computed(() =>
  getComputedStyle(document.documentElement).getPropertyValue('--primary').trim() || '#be1e2c'
)

const emit = defineEmits<{
  (event: "update:modelValue", value: string): void;
  (event: "update:cssModelValue", value: string): void;
  (event: "validated", name: string, validationStatus: Status): void;
}>();

const status = ref<Status>({ valid: true });
const isRequired = !(props.rules != null && props.rules.length == 0);
const showLinkPopup = ref(false);
const htmlMode = ref(false);
const cssMode = ref(false);
const htmlSource = ref('');
const cssSource = ref(props.cssModelValue || '');

const editor = useEditor({
  content: props.modelValue || '',
  extensions: [
    StarterKit,
    Link.configure({
      openOnClick: false,
      HTMLAttributes: { target: '_blank' },
    }),
    Image.configure({
      inline: true,
      allowBase64: true,
    }),
    TextAlign.configure({
      types: ['heading', 'paragraph'],
    }),
    TextStyle,
    Color,
  ],
  onUpdate({ editor }) {
    const html = editor.getHTML();
    emit("update:modelValue", html);
    validateInput();
  },
  onBlur() {
    validateInput();
  },
});

watch(() => props.modelValue, (newValue) => {
  if (htmlMode.value) {
    htmlSource.value = newValue || '';
    return;
  }
  if (editor.value && newValue !== editor.value.getHTML()) {
    editor.value.commands.setContent(newValue || '', { emitUpdate: false });
  }
});

watch(() => props.cssModelValue, (newValue) => {
  if (cssMode.value) {
    cssSource.value = newValue || '';
  }
});

onBeforeUnmount(() => {
  editor.value?.destroy();
});

function handleHeadingChange(e: Event) {
  const level = parseInt((e.target as HTMLSelectElement).value);
  if (level === 0) {
    editor.value!.chain().focus().setParagraph().run();
  } else {
    editor.value!.chain().focus().toggleHeading({ level: level as 2 | 3 | 4 }).run();
  }
}

function setColor(color: string) {
  editor.value!.chain().focus().setColor(color).run();
}

function addImage() {
  const url = window.prompt(t('textEditor.imageUrl'));
  if (url) {
    editor.value!.chain().focus().setImage({ src: url }).run();
  }
}

function toggleHtmlMode() {
  if (htmlMode.value) {
    emit("update:modelValue", htmlSource.value);
    editor.value!.commands.setContent(htmlSource.value, { emitUpdate: false });
    htmlMode.value = false;
  } else {
    cssMode.value = false;
    htmlSource.value = editor.value!.getHTML();
    htmlMode.value = true;
  }
}

function toggleCssMode() {
  if (cssMode.value) {
    cssMode.value = false;
  } else {
    if (htmlMode.value) {
      emit("update:modelValue", htmlSource.value);
      editor.value!.commands.setContent(htmlSource.value, { emitUpdate: false });
      htmlMode.value = false;
    }
    cssSource.value = props.cssModelValue || '';
    cssMode.value = true;
  }
}

function onCssInput(e: Event) {
  cssSource.value = (e.target as HTMLTextAreaElement).value;
  emit("update:cssModelValue", cssSource.value);
}

function onHtmlInput(e: Event) {
  htmlSource.value = (e.target as HTMLTextAreaElement).value;
  emit("update:modelValue", htmlSource.value);
}

function validateInput() {
  const html = htmlMode.value ? htmlSource.value : (editor.value?.getHTML() ?? '');
  const validationRules = props.rules ? props.rules : [requiredTextEditor]
  status.value = validate(html, validationRules)
  emit("validated", props.name, status.value);
}
</script>
