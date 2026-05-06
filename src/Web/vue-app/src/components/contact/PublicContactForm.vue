<template>
  <section class="contact-form-block">
    <div class="contact-form-block__header">
      <h2 class="contact-form-block__title">{{ resolvedTitle }}</h2>
      <p v-if="resolvedIntroText" class="contact-form-block__intro">{{ resolvedIntroText }}</p>
      <p v-if="!isEnabled" class="contact-form-block__disabled">{{ t('pages.blocks.contactForm.public.disabled') }}</p>
    </div>

    <form v-if="isEnabled" class="contact-form-block__form" @submit.prevent="submitForm" novalidate>
      <div class="contact-form-block__grid">
        <div class="contact-form-block__field">
          <label for="contact-name">{{ t('pages.blocks.contactForm.public.name') }}</label>
          <input
            id="contact-name"
            v-model.trim="form.name"
            type="text"
            maxlength="100"
            :placeholder="t('pages.blocks.contactForm.public.namePlaceholder')"
            :disabled="isSubmitting"
          />
          <span v-if="errors.name" class="contact-form-block__error">{{ errors.name }}</span>
        </div>

        <div class="contact-form-block__field">
          <label for="contact-email">{{ t('pages.blocks.contactForm.public.email') }}</label>
          <input
            id="contact-email"
            v-model.trim="form.email"
            type="email"
            maxlength="320"
            :placeholder="t('pages.blocks.contactForm.public.emailPlaceholder')"
            :disabled="isSubmitting"
          />
          <span v-if="errors.email" class="contact-form-block__error">{{ errors.email }}</span>
        </div>
      </div>

      <div class="contact-form-block__field">
        <label for="contact-message">{{ t('pages.blocks.contactForm.public.message') }}</label>
        <textarea
          id="contact-message"
          v-model.trim="form.message"
          rows="6"
          maxlength="2000"
          :placeholder="t('pages.blocks.contactForm.public.messagePlaceholder')"
          :disabled="isSubmitting"
        />
        <span v-if="errors.message" class="contact-form-block__error">{{ errors.message }}</span>
      </div>

      <div class="contact-form-block__honeypot" aria-hidden="true">
        <label for="contact-website">Website</label>
        <input id="contact-website" v-model="form.honeypot" type="text" tabindex="-1" autocomplete="off" />
      </div>

      <div class="contact-form-block__actions">
        <button class="btn" type="submit" :disabled="isSubmitting">
          {{ isSubmitting ? t('pages.blocks.contactForm.public.sending') : resolvedSubmitLabel }}
        </button>
      </div>

      <p v-if="successMessageVisible" class="contact-form-block__success">{{ resolvedSuccessMessage }}</p>
      <p v-if="errorMessage" class="contact-form-block__error contact-form-block__error--submit">{{ errorMessage }}</p>
    </form>
  </section>
</template>

<script lang="ts" setup>
import {computed, reactive, ref} from "vue"
import {useI18n} from "vue-i18n"
import {useContactService} from "@/serviceRegistry"

const props = defineProps<{
  blockId?: string
  pageSlug?: string
  title?: string
  introText?: string
  submitLabel?: string
  successMessage?: string
  enabled?: boolean
}>()

const {t} = useI18n()
const contactService = useContactService()

const isSubmitting = ref(false)
const successMessageVisible = ref(false)
const errorMessage = ref('')
const form = reactive({
  name: '',
  email: '',
  message: '',
  honeypot: '',
})

const errors = reactive({
  name: '',
  email: '',
  message: '',
})

const resolvedTitle = computed(() => props.title || t('pages.blocks.contactForm.defaultTitle'))
const resolvedIntroText = computed(() => props.introText || '')
const resolvedSubmitLabel = computed(() => props.submitLabel || t('pages.blocks.contactForm.defaultSubmitLabel'))
const resolvedSuccessMessage = computed(() => props.successMessage || t('pages.blocks.contactForm.defaultSuccessMessage'))
const isEnabled = computed(() => props.enabled !== false)

async function submitForm() {
  if (isSubmitting.value) return

  resetMessages()
  if (!validateForm()) return

  isSubmitting.value = true

  try {
    const response = await contactService.submit({
      name: form.name,
      email: form.email,
      message: form.message,
      honeypot: form.honeypot,
      blockId: props.blockId,
      pageSlug: props.pageSlug,
    })

    if (!response.succeeded) {
      const errorMessages = response.getErrorMessages('pages.blocks.contactForm.public.validation', 'pages.blocks.contactForm.public.error')
      errorMessage.value = errorMessages[0] || t('pages.blocks.contactForm.public.error')
      return
    }

    successMessageVisible.value = true
    resetForm()
  } catch {
    errorMessage.value = t('pages.blocks.contactForm.public.error')
  } finally {
    isSubmitting.value = false
  }
}

function validateForm(): boolean {
  errors.name = form.name ? '' : t('validation.empty')
  errors.email = !form.email
    ? t('validation.empty')
    : isValidEmail(form.email) ? '' : t('validation.email')
  errors.message = form.message ? '' : t('validation.empty')

  return !errors.name && !errors.email && !errors.message
}

function isValidEmail(value: string): boolean {
  return /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(value)
}

function resetForm() {
  form.name = ''
  form.email = ''
  form.message = ''
  form.honeypot = ''
}

function resetMessages() {
  successMessageVisible.value = false
  errorMessage.value = ''
  errors.name = ''
  errors.email = ''
  errors.message = ''
}
</script>

<style scoped>
.contact-form-block {
  background: #fffaf5;
  border: 1px solid #f1dfcf;
  border-radius: 1rem;
  padding: 1.5rem;
  margin: 2rem 0;
}

.contact-form-block__header {
  margin-bottom: 1.25rem;
}

.contact-form-block__title {
  margin: 0 0 0.5rem;
  color: var(--color-primary, #be1e2c);
}

.contact-form-block__intro,
.contact-form-block__disabled {
  margin: 0;
  color: #5b5b5b;
  line-height: 1.6;
}

.contact-form-block__form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.contact-form-block__grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.contact-form-block__field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.contact-form-block__field label {
  font-weight: 600;
  font-size: 0.95rem;
}

.contact-form-block__field input,
.contact-form-block__field textarea {
  width: 100%;
  padding: 0.8rem 0.9rem;
  border: 1px solid #d7c6b6;
  border-radius: 0.75rem;
  background: #fff;
  font: inherit;
}

.contact-form-block__field textarea {
  resize: vertical;
  min-height: 10rem;
}

.contact-form-block__field input:disabled,
.contact-form-block__field textarea:disabled {
  background: #f8f5f1;
}

.contact-form-block__honeypot {
  position: absolute;
  left: -9999px;
  width: 1px;
  height: 1px;
  overflow: hidden;
}

.contact-form-block__actions {
  display: flex;
  justify-content: flex-start;
}

.contact-form-block__success {
  margin: 0;
  color: #166534;
  font-weight: 600;
}

.contact-form-block__error {
  color: #b42318;
  font-size: 0.875rem;
}

.contact-form-block__error--submit {
  margin: 0;
}

@media (max-width: 640px) {
  .contact-form-block__grid {
    grid-template-columns: 1fr;
  }
}
</style>
