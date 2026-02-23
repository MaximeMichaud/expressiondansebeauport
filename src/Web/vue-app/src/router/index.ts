import i18n from "@/i18n";
import {Role} from "@/types/enums";
import {createRouter, createWebHistory} from "vue-router";

import Home from "@/views/public/Home.vue";
import PublicPage from "@/views/public/PublicPage.vue";
import Login from "@/views/Login.vue";
import TwoFactor from "@/views/TwoFactor.vue";
import ForgotPassword from "@/views/ForgotPassword.vue";
import ResetPassword from "@/views/ResetPassword.vue";
import Account from "@/views/shared/Account.vue";

import Admin from "../views/admin/Admin.vue";
import AdminMemberIndex from "@/views/admin/members/AdminMemberIndex.vue";
import AdminAddMemberForm from "@/views/admin/members/AdminAddMemberForm.vue";
import AdminEditMemberForm from "@/views/admin/members/AdminEditMemberForm.vue";
import AdminMediaLibrary from "@/views/admin/media/AdminMediaLibrary.vue";
import AdminPageIndex from "@/views/admin/pages/AdminPageIndex.vue";
import AdminPageEditor from "@/views/admin/pages/AdminPageEditor.vue";
import AdminMenuIndex from "@/views/admin/menus/AdminMenuIndex.vue";
import AdminCustomizer from "@/views/admin/customizer/AdminCustomizer.vue";
import AdminSiteHealth from "@/views/admin/health/AdminSiteHealth.vue";
import AdminImportExport from "@/views/admin/importexport/AdminImportExport.vue";

import {getLocalizedRoutes} from "@/locales/helpers";
import {useUserStore} from "@/stores/userStore";

const router = createRouter({
  // eslint-disable-next-line
  scrollBehavior(to, from, savedPosition) {
    // always scroll to top
    return {top: 0};
  },
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      name: "home",
      component: Home,
      meta: {
        title: "routes.home.name"
      }
    },
    {
      path: i18n.t("routes.login.path"),
      alias: getLocalizedRoutes("routes.login.path"),
      name: "login",
      component: Login,
      meta: {
        title: "routes.login.name"
      }
    },
    {
      path: i18n.t("routes.twoFactor.path"),
      alias: getLocalizedRoutes("routes.twoFactor.path"),
      name: "twoFactor",
      component: TwoFactor,
      meta: {
        title: "routes.twoFactor.name"
      }
    },
    {
      path: i18n.t("routes.forgotPassword.path"),
      alias: getLocalizedRoutes("routes.forgotPassword.path"),
      name: "forgotPassword",
      component: ForgotPassword,
      meta: {
        title: "routes.forgotPassword.name"
      }
    },
    {
      path: i18n.t("routes.resetPassword.path"),
      alias: getLocalizedRoutes("routes.resetPassword.path"),
      name: "resetPassword",
      component: ResetPassword,
      props: (route) => ({userId: route.query.userId, token: route.query.token}),
      meta: {
        title: "routes.resetPassword.name"
      }
    },
    {
      path: i18n.t("routes.account.path"),
      alias: getLocalizedRoutes("routes.account.path"),
      name: "account",
      component: Account,
      meta: {
        title: "routes.account.name"
      }
    },
    {
      path: i18n.t("routes.admin.path"),
      alias: getLocalizedRoutes("routes.admin.path"),
      name: "admin",
      component: Admin,
      meta: {
        requiredRole: Role.Admin,
        noLinkInBreadcrumbs: true,
      },
      redirect: {name: 'admin.children.members.index'},
      children: [
        {
          path: i18n.t("routes.admin.children.members.path"),
          alias: getLocalizedRoutes("routes.admin.children.members.path"),
          name: "admin.children.members",
          component: Admin,
          children: [
            {
              path: "",
              name: "admin.children.members.index",
              component: AdminMemberIndex,
            },
            {
              path: i18n.t("routes.admin.children.members.add.path"),
              alias: getLocalizedRoutes("routes.admin.children.members.add.path"),
              name: "admin.children.members.add",
              component: AdminAddMemberForm,
            },
            {
              path: i18n.t("routes.admin.children.members.edit.path"),
              alias: getLocalizedRoutes("routes.admin.children.members.edit.path"),
              name: "admin.children.members.edit",
              component: AdminEditMemberForm,
              props: true
            },
          ],
        },
        {
          path: i18n.t("routes.admin.children.media.path"),
          alias: getLocalizedRoutes("routes.admin.children.media.path"),
          name: "admin.children.media",
          component: AdminMediaLibrary,
        },
        {
          path: i18n.t("routes.admin.children.pages.path"),
          alias: getLocalizedRoutes("routes.admin.children.pages.path"),
          name: "admin.children.pages",
          component: Admin,
          children: [
            {
              path: "",
              name: "admin.children.pages.index",
              component: AdminPageIndex,
            },
            {
              path: i18n.t("routes.admin.children.pages.add.path"),
              alias: getLocalizedRoutes("routes.admin.children.pages.add.path"),
              name: "admin.children.pages.add",
              component: AdminPageEditor,
            },
            {
              path: i18n.t("routes.admin.children.pages.edit.path"),
              alias: getLocalizedRoutes("routes.admin.children.pages.edit.path"),
              name: "admin.children.pages.edit",
              component: AdminPageEditor,
              props: true
            },
          ],
        },
        {
          path: i18n.t("routes.admin.children.menus.path"),
          alias: getLocalizedRoutes("routes.admin.children.menus.path"),
          name: "admin.children.menus",
          component: AdminMenuIndex,
        },
        {
          path: i18n.t("routes.admin.children.customizer.path"),
          alias: getLocalizedRoutes("routes.admin.children.customizer.path"),
          name: "admin.children.customizer",
          component: AdminCustomizer,
        },
        {
          path: i18n.t("routes.admin.children.siteHealth.path"),
          alias: getLocalizedRoutes("routes.admin.children.siteHealth.path"),
          name: "admin.children.siteHealth",
          component: AdminSiteHealth,
        },
        {
          path: i18n.t("routes.admin.children.importExport.path"),
          alias: getLocalizedRoutes("routes.admin.children.importExport.path"),
          name: "admin.children.importExport",
          component: AdminImportExport,
        }
      ]
    },
    {
      path: "/:slug",
      name: "publicPage",
      component: PublicPage,
      meta: {
        title: "routes.home.name",
        public: true
      }
    },
  ]
});

// eslint-disable-next-line
router.beforeEach(async (to, from) => {
  const userStore = useUserStore()

  if (!to.meta.requiredRole)
    return;

  const isRoleArray = Array.isArray(to.meta.requiredRole)
  const doesNotHaveGivenRole = !isRoleArray && !userStore.hasRole(to.meta.requiredRole as Role);
  const hasNoRoleAmongRoleList = isRoleArray && !userStore.hasOneOfTheseRoles(to.meta.requiredRole as Role[]);
  if (doesNotHaveGivenRole || hasNoRoleAmongRoleList) {
    return {
      name: "account",
    };
  }
});

export const Router = router;