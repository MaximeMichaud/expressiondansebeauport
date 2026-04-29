import { createApp } from "vue";
import App from "./App.vue";
import { pinia } from "@/stores/pinia";
import { Router } from "./router";
import axios from "axios";

// Reload automatique quand un chunk lazy n'existe plus (après un redéploiement).
// L'event est émis nativement par Vite lors d'un échec d'import dynamique.
// Le flag sessionStorage évite la boucle infinie si le reload ne résout pas le problème.
window.addEventListener("vite:preloadError", (event) => {
  if (sessionStorage.getItem("vite-preload-reloaded") === "1") {
    return;
  }
  sessionStorage.setItem("vite-preload-reloaded", "1");
  event.preventDefault();
  window.location.reload();
});
window.addEventListener("load", () => {
  sessionStorage.removeItem("vite-preload-reloaded");
});

// Redirige les visiteurs vers la page d'erreur 500 en cas d'erreur serveur,
// sauf dans les pages admin (qui gèrent leurs propres erreurs).
axios.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 500) {
      const isAdminRoute = !!Router.currentRoute.value.meta.requiredRole;
      if (!isAdminRoute) {
        Router.push({ name: "internalError" });
      }
    }
    return Promise.reject(error);
  }
);
import i18n from "@/i18n";
import { createHead } from "@unhead/vue/client";
import Notifications from "@kyvg/vue3-notification";
import "@/assets/css/globals.css";

const head = createHead();

const app = createApp(App)
  .use(head)
  .use(i18n)
  .use(Router)
  .use(pinia) // pinia store should be loaded after router to access  (https://pinia.vuejs.org/core-concepts/outside-component-usage.html#single-page-applications)
  .use(Notifications);

Router.isReady().then(() => app.mount("#app"));
