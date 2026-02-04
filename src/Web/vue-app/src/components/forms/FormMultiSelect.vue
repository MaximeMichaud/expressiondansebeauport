<template>
  <div :class="{ error: !status.valid }" class="form__field form-select-multiple">
    <tippy
        :arrow="false"
        :popper-options="popperOptions"
        interactive
        max-width="100%"
        offset="0"
        theme="custom-form-select"
        trigger="click focus"
    >
      <template #default>
        <p class="form-select-multiple__selection">
          <span v-for="option in selectedOptions"
                :key="`fake__${option.name}`"
                class="form-select-multiple__selection-option"
                @click="toggleOption(option)"
          >
            {{ option.label }}
          </span>
        </p>
      </template>
      <template #content>
        <ul v-if="options" class="form-select-multiple__options">
          <li v-for="option in options"
              :key="option.name"
              :class="{ 'selected': selectedOptions && selectedOptions.find(o => o.name == option.name) }"
              class="form-select-multiple__option"
              @click="toggleOption(option)"
          >
            {{ option.label }}
          </li>
        </ul>
      </template>
    </tippy>

    <label v-if="!noLabel" :for="name">
      {{ label ? label : name }}
      <span v-if="isRequired" class="form__indicator">*</span>
    </label>

    <span v-if="!status.valid" :id="`error__${name}`" class="form__error-message">
      {{ t('validation.select') }}
    </span>

  </div>
</template>

<script lang="ts" setup>
import {FormOption} from "@/types/formOption";
import {Status, validateArray} from '@/validation'
import {requiredArray, RuleArray} from '@/validation/rules'
import {ref, watch} from "vue";
import {useI18n} from "vue3-i18n";

// eslint-disable-next-line
const props = defineProps<{
  name: string;
  modelValue: FormOption[];
  label?: string;
  rules?: RuleArray[];
  options: FormOption[];
  noLabel?: boolean;
  maxItems?: number;
}>();

// eslint-disable-next-line
defineExpose({
  //to call validation in parent.
  validateInput
})

// eslint-disable-next-line
const emit = defineEmits<{
  // states that the event has to be called 'update:modelValue'
  (event: "onOptionChange", value: FormOption[]): void;
  (event: "validated", name: string, validationStatus: Status): void;
}>();

const {t} = useI18n();
const status = ref<Status>({valid: true});
const isRequired = !(props.rules != null && props.rules.length == 0);
const selectedOptions = ref<FormOption[]>(props.modelValue as FormOption[]);

watch(props, (newProps) => selectedOptions.value = newProps.modelValue)

const popperOptions: Partial<import("@popperjs/core").Options> = {
  modifiers: [
    {
      name: "sameWidth",
      enabled: true,
      fn: ({state}) => {
        state.styles.popper.width = `${state.rects.reference.width}px`;
      },
      phase: "beforeWrite",
      requires: ["computeStyles"],
      effect: ({state}) => {
        state.elements.popper.style.width = `${state.elements.reference.clientWidth}px`;
      },
    },
  ],
}

function toggleOption(option: FormOption) {
  let options = selectedOptions.value && selectedOptions.value.find(o => o.name == option.name)
    ? selectedOptions.value.filter(o => o.name != option.name)
    : [...selectedOptions.value, option];
  if (props.maxItems && options.length > props.maxItems)
    return;

  selectedOptions.value = options as FormOption[];
  emit("onOptionChange", options);

  validateInput();
}

function validateInput() {
  let validationRules = props.rules ? props.rules : [requiredArray]
  status.value = validateArray(selectedOptions.value, validationRules)

  emit("validated", props.name, status.value);
}
</script>
