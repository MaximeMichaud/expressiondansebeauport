<template>
  <form
    class="form"
    novalidate
    @submit.prevent="handleSubmit"
    enctype="multipart/form-data"
  >
    <ProductBaseForm
      :product="book"
      :title="t('book.info')"
      name-fr-label="book.titleFr"
      name-en-label="book.titleEn"
      description-fr-label="book.summaryFr"
      description-en-label="book.summaryEn"
      @handle-validation="handleValidation"
      @register-form-input-ref="addFormInputRef"
    >
      <template v-slot:before-prices>
        <FormRow>
          <FormInput
            v-model="book.author"
            type="text"
            name="author"
            :ref="addFormInputRef"
            :label="t('book.author')"
            @validated="handleValidation" />
          <FormInput
            v-model="book.editor"
            type="text"
            name="editor"
            :ref="addFormInputRef"
            :label="t('book.editor')"
            @validated="handleValidation" />
        </FormRow>

        <FormRow>
          <FormInput
            v-model="book.isbn"
            type="text"
            name="isbn"
            :ref="addFormInputRef"
            :label="t('book.isbn')"
            @validated="handleValidation" />
          <FormInput
            v-model="book.yearOfPublication"
            type="number"
            name="yearOfPublication"
            :ref="addFormInputRef"
            :label="t('book.yearOfPublication')"
            @validated="handleValidation" />
        </FormRow>
        <FormRow>
          <FormInput
            v-model="book.numberOfPages"
            type="number"
            name="numberOfPages"
            :ref="addFormInputRef"
            :label="t('book.numberOfPages')"
            @validated="handleValidation" />
        </FormRow>
      </template>
    </ProductBaseForm>

    <button v-if="!props.book" class="form__submit btn btn--fullscreen" type="submit">
        {{ t("global.add") }}
    </button>
    <button v-else class="form__submit btn btn--fullscreen" type="submit">
        {{ t("global.save") }}
    </button>
  </form>
</template>

<script lang="ts" setup>
import FormRow from "@/components/forms/FormRow.vue"
import FormInput from "@/components/forms/FormInput.vue"
import ProductBaseForm from "@/components/products/ProductBaseForm.vue"
import {ref} from "vue"
import {useI18n} from "vue3-i18n"
import {notifyError} from "@/notify"
import {Status} from "@/validation"
import {Book} from "@/types/entities"

// eslint-disable-next-line no-undef
const props = defineProps<{
    book?: Book
}>()

// eslint-disable-next-line
const emit = defineEmits<{
    (event: "formSubmit", book: Book): void
}>()

const { t } = useI18n()

const book = ref<Book>(props.book ?? {})

const formInputs = ref<any[]>([])
const inputValidationStatuses:any = {}

function addFormInputRef(ref: typeof FormInput) {
    if (!formInputs.value.includes(ref) && ref)
        formInputs.value.push(ref)
}

async function handleValidation(name: string, validationStatus: Status) {
    inputValidationStatuses[name] = validationStatus.valid
}

async function handleSubmit() {
    formInputs.value.forEach((x: typeof FormInput) => x.validateInput())
    if (Object.values(inputValidationStatuses).some(x => x === false)) {
        notifyError(t('global.formErrorNotification'))
        return
    }
    emit("formSubmit", book.value)
}
</script>