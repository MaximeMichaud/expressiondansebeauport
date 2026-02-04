<template>
  <div>
    <Fieldset :title="props.title">
      <FormRow>
        <FormFiles
          v-model="product.cardImage"
          name="cardImage"
          :ref="addFormInputRef"
          :label="t('products.form.image-card')"
          :tooltip="t(`products.form.image-card-tooltip`)"
          :possiblesTypes="['image/png', 'image/jpg', 'image/jpeg', 'image/webp']"
          :default-selected-files="product.savedCardImage != null ? [product.savedCardImage] : []"
          can-delete
          @on-default-selected-files-change="onSavedImageChange($event, 'card')"
          @change="onImageChange($event, 'card')"
          @validated="handleValidation"
        />
        <slot name="image"></slot>
      </FormRow>

      <slot name="after-images"></slot>

      <FormRow>
        <FormInput
          v-model="product.nameFr"
          type="text"
          name="name-fr"
          :ref="addFormInputRef"
          :label="t(props.nameFrLabel ?? 'products.form.name-fr')"
          @validated="handleValidation" />
        <FormInput
          v-model="product.nameEn"
          type="text"
          name="name-en"
          :ref="addFormInputRef"
          :label="t(props.nameEnLabel ?? 'products.form.name-en')"
          @validated="handleValidation" />
      </FormRow>

      <FormTextEditor
        v-model="product.descriptionFr"
        :ref="addFormInputRef"
        name="description-fr"
        :label="t(props.descriptionFrLabel ?? 'products.form.description-fr')"
        @validated="handleValidation"
      />
      <FormTextEditor
        v-model="product.descriptionEn"
        :ref="addFormInputRef"
        name="description-en"
        :label="t(props.descriptionEnLabel ?? 'products.form.description-en')"
        @validated="handleValidation"
      />

      <slot name="before-prices"></slot>

    </Fieldset>

    <Fieldset :title="t('products.form.price')">
      <FormRow>
        <FormInput
          type="number"
          name="member-price"
          v-model.number="product.price"
          :ref="addFormInputRef"
          :label="t('products.form.price')"
          :rules="[required, min(0)]"
          :tooltip="t('products.form.no-taxes')"
          @validated="handleValidation"
        />
      </FormRow>
    </Fieldset>
  </div>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import { Status } from "@/validation";
import { required, min } from "@/validation/rules";
import { useI18n } from "vue3-i18n";
import FormInput from "@/components/forms/FormInput.vue";
import Fieldset from "@/components/forms/Fieldset.vue";
import FormRow from "@/components/forms/FormRow.vue";
import FormFiles from "@/components/forms/FormFiles.vue";
import {Product} from "@/types/entities/product";
import FormTextEditor from "@/components/forms/FormTextEditor.vue";

const { t } = useI18n();

// eslint-disable-next-line no-undef
const props = defineProps<{
    product: Product,
    title: string
    nameFrLabel?: string
    nameEnLabel?: string
    descriptionFrLabel?: string
    descriptionEnLabel?: string
}>()

// eslint-disable-next-line
const emit = defineEmits<{
    (event: "handleValidation", name: string, validationStatus: Status): void
    (event: "registerFormInputRef", ref: typeof FormInput): void
}>()

const product = ref<Product>(props.product)

function addFormInputRef(ref: typeof FormInput) {
    emit("registerFormInputRef", ref);
}

async function onImageChange(event: any, type: string) {
  const file = event?.target?.files[0];
  if (file && type === "card") product.value.cardImage = file as File
}

async function onSavedImageChange(url: string, type: string) {
  if (type === "card") product.value.savedCardImage = url
}

async function handleValidation(name: string, validationStatus: Status) {
    emit("handleValidation", name, validationStatus);
}
</script>
