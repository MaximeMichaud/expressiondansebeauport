import "reflect-metadata";
import { createApp } from "vue";
import App from "./App.vue";
import { pinia } from "@/stores/pinia";
import { Router } from "./router";
import i18n from "@/i18n";
import { VueWindowSizePlugin } from 'vue-window-size/plugin';
import Notifications from "@kyvg/vue3-notification";
import Vue3EasyDataTable from "vue3-easy-data-table";
import "vue3-easy-data-table/dist/style.css";
import VueTippy from 'vue-tippy'

createApp(App)
  .use(i18n)
  .use(VueWindowSizePlugin)
  .use(Router)
  .use(pinia) // pinia store should be loaded after router to access  (https://pinia.vuejs.org/core-concepts/outside-component-usage.html#single-page-applications)
  .use(Notifications)
  .component('EasyDataTable', Vue3EasyDataTable)
  .use(VueTippy, {
    defaultProps: {
        offset: [0, 12],
        zIndex: 30000,
        placement: "bottom",
        theme: "custom-garneau-template-app",
        interactive: true
    },
  })
  .mount("#app");
