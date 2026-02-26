# Page Editor Non-Dev Improvements — Design

**Date:** 2026-02-25
**Status:** Approved

## Problem

The admin page editor is not usable by non-developers:
- Content field is a plain textarea requiring raw HTML
- Slug concept is unclear with no URL context
- Sort Order field has no explanation
- Meta description has no character feedback
- The home page (Page d'accueil) is hardcoded static HTML, completely outside the CMS

## Solution: 5 Improvements

### 1. WYSIWYG Content Editor

Replace the `<textarea>` in `AdminPageEditor.vue` with the existing `<FormTextEditor>` component (Tiptap-based, already installed and used elsewhere in the project). This gives non-devs a formatting toolbar: bold, italic, headings, lists, links, images, color, alignment.

**Files:** `AdminPageEditor.vue`

### 2. Auto-Slug from Title

Add a `watch` on `page.title` that converts the title to a URL-safe slug (lowercase, accents stripped, spaces → hyphens) and writes to `page.slug` — **only if the user has not manually edited the slug field**.

- A `slugManuallyEdited` boolean flag tracks this
- On create: flag starts `false` (auto-generation active)
- On edit: flag starts `true` (slug already set, don't overwrite)
- On any manual keypress in the slug input: flag becomes `true`, auto-gen stops

**Files:** `AdminPageEditor.vue`

### 3. URL Preview + Slug Lock for Reserved Pages

Show `/a-propos-de-nous` as a small grey label below the slug input, updated reactively.

For the reserved home page (slug = `accueil`), the slug field is **read-only** to prevent breaking the URL binding. A small badge labels it "Page d'accueil".

**Files:** `AdminPageEditor.vue`

### 4. Meta Description Character Counter + Sort Order Tooltip

- Character counter below meta description: `47 / 160` (160 is the SEO practical target)
- `title` attribute on Sort Order label: "Détermine l'ordre d'affichage dans le menu (0 = premier)"

**Files:** `AdminPageEditor.vue`

### 5. Home Page as Editable CMS Page

Make the `/` route dynamic by fetching the Page with slug `accueil` from `/public/pages/accueil`.

- `Home.vue` gets the same fetch/render pattern as `PublicPage.vue`
- The existing `.home-hero` wrapper CSS is preserved — content rendered via `v-html` inside it
- In the admin, the `accueil` page must be created manually (once) with the current static content as the initial value
- The slug field is locked for `accueil` (covered by improvement #3)

**Files:** `Home.vue`

## Non-Goals

- No backend changes required — all existing API endpoints are sufficient
- No new DB migrations — the home page is created via the existing admin UI (one-time setup)
- No special delete protection for the `accueil` page (trust admin)
- No image picker integration in the page editor (out of scope)

## File Change Summary

| File | Change |
|------|--------|
| `src/Web/vue-app/src/views/admin/pages/AdminPageEditor.vue` | FormTextEditor, auto-slug, URL preview, slug lock, char counter, tooltip |
| `src/Web/vue-app/src/views/public/Home.vue` | Fetch from `/public/pages/accueil`, render dynamically |
