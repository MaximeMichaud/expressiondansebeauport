# Garneau Template - App Vue

## Installation

```bash
npm install
```

## Scripts

```bash
npm run dev          # Start dev server (http://localhost:8080/vue/)
npm run build        # Build for production
npm run build:watch  # Build and watch for changes
npm run build:check  # Type-check and build
```

## Technos

[Vue 3](https://v3.vuejs.org/guide/)

[Typescript](https://vuejs.org/guide/typescript/overview.html)

[Vite](https://vite.dev/guide/)

[Vue 3 i18n](https://github.com/webkong/vue3-i18n) pour la gestion des langues.

[vite-svg-loader](https://github.com/jpkleemans/vite-svg-loader) pour ajouter des icônes .svg dans nos components.
*Si vous devez utiliser des svgs dans les .scss, transformé les en .png. -> Le loader les targets & les rends impossible à afficher.*

[Vue WindowSize Plugin](https://github.com/mya-ake/vue-window-size/tree/master) pour avoir la grandeur de la fenêtre partout et faire des affichages conditionnels aux mediaqueries.

[@vueup/vue-quill@latest](https://vueup.github.io/vue-quill/guide) VueQuill is a Component for building rich text editors, powered by Vue 3 and Quill.

[Vue 3 Notifications](https://github.com/kyvg/vue3-notification) pour les notifications.

[Vue 3 Easy Data Table](https://github.com/HC200ok/vue3-easy-data-table/) pour les tableaux. 

[jsPDF](https://github.com/parallax/jsPDF) A library to generate PDFs in JavaScript. with [Autotable](https://github.com/simonbengtsson/jsPDF-AutoTable).

[Vue Tippy](https://vue-tippy.netlify.app/basic-usage) version vue 3 du plugin Tippy utilisé dans le site principal pour les tooltips.

### Some definitions
#### `ref` vs `reactive` vs `computed` vs `watch`
- `ref` : ref is short for reactive reference and allows us to make a primitive reactive (update the DOM)
- `reactive` : useful to make complex objects like an object, a Map or a Set reactive (update the DOM)
- `computed` : useful for derived/calculated values : if any reactivity API (ref or reactive) is used inside a computed property, it will automatically recompute this value and update the DOM for us
- `watch` : useful to be notified when a value or the property of an object has changed

#### Other
`composable` : functions to separate out logic by shared concerns are known as composables.
