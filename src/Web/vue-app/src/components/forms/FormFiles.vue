<template>
  <div class="form__fields-container" :class="{ error: !status.valid }">
    <div class="form__field form-files">
      <input
        ref="filesInput"
        type="file"
        :name="name"
        :id="name"
        :accept="possiblesTypes?.toString()"
        :multiple="isMultiple"
        @change="handleChange"
        @blur="validateInput"
        :aria-invalid="!status.valid"
        :aria-describedby="`error__${name}`"
      />

      <label :for="name">
        {{ label ? label : name }}
        <span class="form__indicator" v-if="minNbFiles > 0">*</span>
        <span class="form__tooltip" v-if="tooltip">{{ tooltip }}</span>
        
        <span class="btn" v-if="isMultiple || files?.length != 1">
          {{ t(btnLabelKey) }}
          <span class="plus-sign" :class="{ 'plus-sign--red': !status.valid }"></span>
        </span>
      </label>

      <ul v-if="files != null && files.length > 0" class="form-files__files">
        <li v-for="file in files" v-bind:key="file.name" class="form-files__file">
          <p class="form-files__name">{{ file.name }}</p>
          <button
            @click="removeFile(file)"
            class="form-files__remove"
            type="button"
            :aria-label="t('global.removeFile')"
          >
            <span class="plus-sign plus-sign--plus plus-sign--minus"></span>
          </button>
        </li>
      </ul>
      <ul v-else-if="!hasChanged && (defaultSelectedFiles && defaultSelectedFiles.filter(x => x.toString().length).length > 0)" class="form-files__files">
        <li v-for="(file, index) in defaultSelectedFiles" v-bind:key="file.toString()" class="form-files__file">
          <p class="form-files__name">{{ file.toString().extractFileName() }}</p>
          <button
            v-if="canDelete"
            @click="clearSavedFile(index)"
            class="form-files__remove"
            type="button"
            :aria-label="t('global.removeFile')"
          >
            <span class="plus-sign plus-sign--plus plus-sign--minus"></span>
          </button>
        </li>
      </ul>
    </div>
    
    <span class="form__error-message" :id="`error__${name}`" v-if="!status.valid">
      {{ status.message }}
    </span>
    
  </div>
</template>

<script setup lang="ts">
import { Status } from "@/validation";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue3-i18n";

const { t } = useI18n();

// eslint-disable-next-line
const props = defineProps<{
  name: string;
  label?: string;
  modelValue: string | number | File | File[] | null | undefined;
  tooltip?: string;
  min?: number;
  max?: number;
  isMultiple?: boolean;
  possiblesTypes?: Array<string>;
  defaultSelectedFiles?: Array<File | string>;
  canDelete?:boolean
}>();

// eslint-disable-next-line
defineExpose({
  //to call validation in parent.
   validateInput
});

// eslint-disable-next-line
const emit = defineEmits<{
  // states that the event has to be called 'update:modelValue'
  (event: "update:modelValue", value: File | Array<File> | undefined): void;
  (event: "validated", name: string, validationStatus: Status): void;
  (event: "on-default-selected-files-change", value: string): void;
}>();

let minNbFiles = computed(() => props.min != null ? props.min : 0);
let maxNbFiles = computed(() => props.max != null ? props.max : -1);

const defaultSelectedFiles = ref(props.defaultSelectedFiles);
watch(defaultSelectedFiles, (newValue) => emit("on-default-selected-files-change", newValue?.length ? newValue[0].toString() : "") , { deep: true, immediate: true });
watch(props, (newProps) => {
  defaultSelectedFiles.value = newProps.defaultSelectedFiles;
});

const status = ref<Status>({ valid: true });
const files = ref<Array<File>>();
const filesInput = ref<HTMLInputElement>();

let btnLabelKey = computed(() => {
  let isReplacing = (files.value && files.value.length > 0) || (defaultSelectedFiles.value && defaultSelectedFiles.value.length > 0);
  let isPlural = maxNbFiles.value > 1 || props.isMultiple || (files.value && files.value.length > 1);
  
  return `global.${isReplacing ? 'replace' : 'add'}File${isPlural ? 's' : ''}`
})

function removeFile(file: File) {
  if (files.value) {
    files.value = files.value.filter((f) => f != file);
    emit("update:modelValue", files.value);
  }

  validateInput()
}

function clearSavedFile(index: number) {
  defaultSelectedFiles.value?.splice(index, 1);
}

const hasChanged = ref(false)
function handleChange(e: Event) {
  const inputEl = e.target as HTMLInputElement;

  hasChanged.value = true;

  if (inputEl.files != null && inputEl.files.length > 0) {
    files.value = Array.from(inputEl.files);
  }

  validateInput();

  emit("update:modelValue", files.value);
}

async function validateInput() {
  await updateStatus();
  emit("validated", props.name, status.value);
}

async function updateStatus() {
  status.value = {
    valid: true,
    message: ""
  }

  if (minNbFiles.value > 0 && (files.value == null && props.defaultSelectedFiles == null || (files.value != null && files.value.length < minNbFiles.value))) {
    status.value = {
      valid: false,
      message: t("validation.fileMin", { min: minNbFiles.value, s: minNbFiles.value > 1 ? "s" : "" })
    }

    return
  }

  if (maxNbFiles.value > 0 && files.value != null && files.value.length > maxNbFiles.value) {
    status.value = {
      valid: false,
      message: t("validation.fileMax", { max: maxNbFiles.value, s: maxNbFiles.value > 1 ? "s" : "" })
    }

    return
  }

  if (props.possiblesTypes != null && props.possiblesTypes.length > 0 && files.value != null) {
    files.value.forEach(file => {
      if (!props.possiblesTypes?.includes(file.type)) {
        files.value = [];

        status.value = {
          valid: false,
          message: t("validation.fileType", {type: props.possiblesTypes?.toString().replaceAll("image/", " .")})
        }
      }
    })
  }
}
</script>
