# Page Editor Non-Dev Improvements Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make the admin page editor usable by non-developers and make the home page editable from the CMS.

**Architecture:** Two files only — `AdminPageEditor.vue` gets a WYSIWYG editor (existing `FormTextEditor` component), auto-slug, URL preview, char counter, and a tooltip; `Home.vue` becomes dynamic by fetching the `accueil` Page entity from the API and rendering via `v-html` with scoped `:deep()` CSS replacing the static markup.

**Tech Stack:** Vue 3, Tiptap (via existing `FormTextEditor` component), Axios (already used in `PublicPage.vue`), `vue3-i18n`

---

## Task 1: Upgrade AdminPageEditor.vue

**Files:**
- Modify: `src/Web/vue-app/src/views/admin/pages/AdminPageEditor.vue`

No tests exist in this project. Verify manually by running `npm run dev` in `src/Web/vue-app` and opening the admin page editor.

### Step 1: Replace the content textarea with FormTextEditor

In `AdminPageEditor.vue`, make these changes to the `<template>`:

**Remove:**
```html
<div class="form-group">
  <label>{{ t('pages.pages.content') }}</label>
  <textarea v-model="page.content" rows="10" class="form-input form-textarea" placeholder="Rédigez le contenu de votre page ici..."></textarea>
</div>
```

**Replace with:**
```html
<div class="form-group">
  <FormTextEditor
    name="content"
    :label="t('pages.pages.content')"
    v-model="page.content"
    :rules="[]"
  />
</div>
```

Add the import in `<script setup>`:
```ts
import FormTextEditor from "@/components/forms/FormTextEditor.vue"
```

### Step 2: Add auto-slug generation from title

Add these reactive refs and logic in `<script setup>` (after the existing `ref` declarations):

```ts
const slugManuallyEdited = ref(false)

function slugify(text: string): string {
  return text
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9\s-]/g, '')
    .trim()
    .replace(/\s+/g, '-')
    .replace(/-+/g, '-')
}

watch(() => page.value.title, (newTitle) => {
  if (!slugManuallyEdited.value && !isEditing.value) {
    page.value.slug = slugify(newTitle ?? '')
  }
})
```

In `onMounted`, set `slugManuallyEdited` to `true` when editing (slug already exists):
```ts
onMounted(async () => {
  const id = route.params.id as string
  if (id) {
    isEditing.value = true
    slugManuallyEdited.value = true   // <-- add this line
    isLoading.value = true
    page.value = await pageService.get(id)
    isLoading.value = false
  } else {
    page.value.status = "Draft"
    page.value.sortOrder = 0
  }
})
```

### Step 3: Add URL preview, slug lock for accueil, and manual-edit flag

Replace the slug `<div class="form-group">` block:

**Remove:**
```html
<div class="form-group">
  <label>{{ t('pages.pages.slug') }}</label>
  <input type="text" v-model="page.slug" class="form-input" placeholder="a-propos-de-nous" />
</div>
```

**Replace with:**
```html
<div class="form-group">
  <label>{{ t('pages.pages.slug') }}</label>
  <input
    type="text"
    v-model="page.slug"
    class="form-input"
    :class="{ 'form-input--readonly': page.slug === 'accueil' }"
    :readonly="page.slug === 'accueil'"
    placeholder="a-propos-de-nous"
    @input="slugManuallyEdited = true"
  />
  <span v-if="page.slug === 'accueil'" class="slug-badge">Page d'accueil</span>
  <span v-else-if="page.slug" class="slug-preview">/{{ page.slug }}</span>
</div>
```

### Step 4: Add meta description character counter

Replace the meta description `<div class="form-group">` block:

**Remove:**
```html
<div class="form-group">
  <label>{{ t('pages.pages.metaDescription') }}</label>
  <textarea v-model="page.metaDescription" rows="3" class="form-input form-textarea" placeholder="Brève description pour les moteurs de recherche (160 caractères max)"></textarea>
</div>
```

**Replace with:**
```html
<div class="form-group">
  <label>{{ t('pages.pages.metaDescription') }}</label>
  <textarea v-model="page.metaDescription" rows="3" class="form-input form-textarea" placeholder="Brève description pour les moteurs de recherche (160 caractères max)"></textarea>
  <span class="char-counter" :class="{ 'char-counter--over': (page.metaDescription?.length ?? 0) > 160 }">
    {{ page.metaDescription?.length ?? 0 }} / 160
  </span>
</div>
```

### Step 5: Add sort order tooltip

Replace the sort order label:

**Remove:**
```html
<label>{{ t('pages.pages.sortOrder') }}</label>
```

**Replace with:**
```html
<label :title="t('pages.pages.sortOrderTooltip')">{{ t('pages.pages.sortOrder') }}</label>
```

Add the i18n key to `src/Web/vue-app/src/locales/fr.json` inside `pages.pages`:
```json
"sortOrderTooltip": "Détermine l'ordre d'affichage dans le menu (0 = premier)"
```

### Step 6: Add scoped styles for new UI elements

Append to the existing `<style scoped>` block:

```css
.slug-preview {
  display: block;
  margin-top: 0.25rem;
  font-size: 0.75rem;
  color: var(--color-gray-500, #6b7280);
  font-family: monospace;
}

.slug-badge {
  display: inline-block;
  margin-top: 0.25rem;
  font-size: 0.75rem;
  background: var(--color-gray-100, #f3f4f6);
  color: var(--color-gray-600, #4b5563);
  border: 1px solid var(--color-gray-300, #d1d5db);
  border-radius: 0.25rem;
  padding: 0.125rem 0.5rem;
}

.form-input--readonly {
  background: var(--color-gray-100, #f3f4f6);
  cursor: not-allowed;
  color: var(--color-gray-500, #6b7280);
}

.char-counter {
  display: block;
  margin-top: 0.25rem;
  font-size: 0.75rem;
  color: var(--color-gray-500, #6b7280);
  text-align: right;
}

.char-counter--over {
  color: #dc2626;
  font-weight: 600;
}
```

### Step 7: Verify in browser

Run `npm run dev` in `src/Web/vue-app`. Open admin → Pages → Add page:
- Type a title → slug should auto-fill
- Edit the slug manually → auto-fill should stop
- Check the WYSIWYG toolbar appears for the content field
- Check `/slug` URL preview appears below slug input
- Check char counter on meta description
- Hover "Ordre de tri" label → tooltip should appear
- Open existing page with slug `accueil` → slug field should be readonly with the "Page d'accueil" badge

### Step 8: Commit

```bash
git add src/Web/vue-app/src/views/admin/pages/AdminPageEditor.vue src/Web/vue-app/src/locales/fr.json
git commit -m "feat: improve admin page editor for non-dev users

- Replace content textarea with Tiptap WYSIWYG editor
- Auto-generate slug from title (stops on manual edit)
- Add URL preview and read-only lock for accueil page
- Add meta description character counter
- Add sort order tooltip"
```

---

## Task 2: Make Home.vue dynamic

**Files:**
- Modify: `src/Web/vue-app/src/views/public/Home.vue`

### Step 1: Rewrite Home.vue

Replace the entire content of `Home.vue` with:

```vue
<template>
  <section class="home-hero">
    <div class="home-hero__content">
      <div v-if="isLoading" class="home-hero__loader">
        <Loader />
      </div>
      <div v-else-if="content" v-html="content" class="home-hero__body"></div>
    </div>
  </section>
</template>

<script lang="ts" setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'
import Loader from '@/components/layouts/items/Loader.vue'

const isLoading = ref(true)
const content = ref<string>('')

onMounted(async () => {
  try {
    const { data } = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/pages/accueil`)
    content.value = data.content ?? ''
  } catch {
    // page not found or not published — show nothing (hero remains blank)
  } finally {
    isLoading.value = false
  }
})
</script>

<style scoped>
.home-hero__loader {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 200px;
}

.home-hero__body :deep(p:first-child) {
  font-family: var(--font-karla, 'Karla', sans-serif);
  font-size: 0.75rem;
  font-weight: 600;
  letter-spacing: 0.18em;
  text-transform: uppercase;
  color: #888;
  margin-bottom: 20px;
}

.home-hero__body :deep(h1) {
  font-family: var(--font-montserrat, 'Montserrat', sans-serif);
  font-weight: 700;
  font-size: 2.375rem;
  line-height: 1.15;
  color: #222;
  margin-bottom: 20px;
}

@media (min-width: 48em) {
  .home-hero__body :deep(h1) {
    font-size: 3.875rem;
  }
}

.home-hero__body :deep(p:not(:first-child)) {
  font-family: var(--font-karla, 'Karla', sans-serif);
  font-size: 1.125rem;
  line-height: 1.6;
  color: #444;
  margin-bottom: 44px;
}

@media (min-width: 48em) {
  .home-hero__body :deep(p:not(:first-child)) {
    font-size: 1.25rem;
  }
}

.home-hero__body :deep(a) {
  display: inline-flex;
  align-items: center;
  font-family: var(--font-montserrat, 'Montserrat', sans-serif);
  font-weight: 700;
  font-size: 0.9375rem;
  color: #fff;
  background-color: #be1e2d;
  padding: 15px 38px;
  border-radius: 100px;
  text-decoration: none;
  box-shadow: 0 4px 24px rgba(190, 30, 45, 0.3);
  transition: transform 0.25s ease, box-shadow 0.25s ease, background-color 0.25s ease;
}

.home-hero__body :deep(a:hover) {
  background-color: #96181e;
  transform: translateY(-2px);
  box-shadow: 0 8px 36px rgba(190, 30, 45, 0.4);
}
</style>
```

### Step 2: Create the accueil page in the admin

**One-time manual setup** (not automated):

1. Open the admin panel → Pages → Add page
2. Fill in:
   - **Titre:** Expression Danse de Beauport
   - **Slug:** accueil (type it manually — leave the title blank first, type slug, then fill title so auto-gen doesn't overwrite)
   - **Contenu** (in WYSIWYG):
     ```
     Paragraph: École de danse — Beauport, Québec
     H1: Expression Danse de Beauport
     Paragraph: La danse pour tous, à tout âge
     Link: S'inscrire maintenant → https://www.qidigo.com/u/Expression-Danse-de-Beauport
     ```
   - **Statut:** Publié
3. Save

The home page at `/` will now load this content from the DB.

### Step 3: Verify in browser

Open `/` — the hero should look identical to the current static version. Edit the page in admin → change the subtitle → refresh `/` — the new text should appear.

### Step 4: Commit

```bash
git add src/Web/vue-app/src/views/public/Home.vue
git commit -m "feat: make home page dynamically editable via accueil CMS page"
```

---

## Manual Setup Note

After deployment, the `accueil` page must be created once in the admin (Step 2 of Task 2). Until it's created, the home page hero will appear blank (the `.home-hero` wrapper still shows, but with no content inside). This is acceptable — the setup is a one-time action.
