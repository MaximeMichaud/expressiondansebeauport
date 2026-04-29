import i18n from "@/i18n";
import {Role} from "@/types/enums";
import {createRouter, createWebHistory} from "vue-router";
import type {Component} from "vue";
import {Library, FileText, LayoutList, Palette, Activity, ArrowLeftRight, HardDriveDownload, AlertTriangle, UserCircle} from "lucide-vue-next";

declare module "vue-router" {
  interface RouteMeta {
    navIcon?: Component;
  }
}

import Home from "@/views/public/Home.vue";
import PublicPage from "@/views/public/PublicPage.vue";
import NotFound from "@/views/public/NotFound.vue";

import {useUserStore} from "@/stores/userStore";
import {useApiStore} from "@/stores/apiStore";
import {useUserService} from "@/serviceRegistry";
import axios from "axios";

const socialRoutes = [
  {
    path: '/social/connexion',
    name: 'socialLogin',
    component: () => import('@/views/social/SocialLogin.vue'),
    meta: { title: 'Connexion', public: true, socialAuth: true, social: true }
  },
  {
    path: '/social/inscription',
    name: 'socialRegister',
    component: () => import('@/views/social/SocialRegister.vue'),
    meta: { title: 'Inscription', public: true, socialAuth: true, social: true }
  },
  {
    path: '/social/confirmation',
    name: 'socialConfirm',
    component: () => import('@/views/social/SocialConfirm.vue'),
    meta: { title: 'Confirmation', public: true, socialAuth: true, social: true }
  },
  {
    path: '/social',
    redirect: '/social/annonces'
  },
  {
    path: '/social/annonces',
    name: 'socialImportant',
    component: () => import('@/views/social/SocialImportant.vue'),
    meta: { title: 'Annonces', requiredRole: [Role.Member, Role.Professor, Role.Admin], social: true }
  },
  {
    path: '/social/annonces/:id',
    name: 'socialAnnouncement',
    component: () => import('@/views/social/SocialAnnouncement.vue'),
    meta: { title: 'Annonce', requiredRole: [Role.Member, Role.Professor, Role.Admin], social: true },
    props: true
  },
  {
    path: '/social/groupes',
    name: 'socialPortal',
    component: () => import('@/views/social/SocialPortal.vue'),
    meta: { title: 'Groupes', requiredRole: [Role.Member, Role.Professor, Role.Admin], social: true }
  },
  {
    path: '/social/groupes/:id',
    name: 'socialGroup',
    component: () => import('@/views/social/SocialGroup.vue'),
    meta: { title: 'Groupe', requiredRole: [Role.Member, Role.Professor, Role.Admin], social: true },
    props: true
  },
  {
    path: '/social/membres',
    name: 'socialMembers',
    component: () => import('@/views/social/SocialMembers.vue'),
    meta: { title: 'Membres', requiredRole: [Role.Member, Role.Professor, Role.Admin], social: true }
  },
  {
    path: '/social/membres/:id',
    name: 'socialMemberProfile',
    component: () => import('@/views/social/SocialMemberProfile.vue'),
    meta: { title: 'Profil', requiredRole: [Role.Member, Role.Professor, Role.Admin], social: true },
    props: true
  },
  {
    path: '/social/messages',
    name: 'socialMessages',
    component: () => import('@/views/social/SocialMessages.vue'),
    meta: { title: 'Messages', requiredRole: [Role.Member, Role.Professor, Role.Admin], social: true }
  },
  {
    path: '/social/messages/admin/:conversationId',
    name: 'socialAdminConversation',
    component: () => import('@/views/social/SocialConversation.vue'),
    meta: { title: 'Conversation (Admin)', requiredRole: [Role.Admin], social: true },
    props: true
  },
  {
    path: '/social/messages/:conversationId',
    name: 'socialConversation',
    component: () => import('@/views/social/SocialConversation.vue'),
    meta: { title: 'Conversation', requiredRole: [Role.Member, Role.Professor, Role.Admin], social: true },
    props: true
  },
  {
    path: '/social/compte',
    name: 'socialAccount',
    component: () => import('@/views/social/SocialAccount.vue'),
    meta: { title: 'Mon compte', requiredRole: [Role.Member, Role.Professor, Role.Admin], social: true }
  },
]

const mainRoutes = [
  {
    path: "/",
    name: "home",
    component: Home,
    meta: {
      title: "routes.home.name"
    }
  },
  {
    path: "/admin/connexion",
    name: "login",
    component: () => import("@/views/Login.vue"),
    meta: {
      title: "routes.login.name"
    }
  },
  {
    // Redirect old /connexion to /admin/connexion
    path: i18n.global.t("routes.login.path"),
    redirect: "/admin/connexion"
  },
  {
    path: i18n.global.t("routes.twoFactor.path"),
    name: "twoFactor",
    component: () => import("@/views/TwoFactor.vue"),
    meta: {
      title: "routes.twoFactor.name"
    }
  },
  {
    path: i18n.global.t("routes.forgotPassword.path"),
    name: "forgotPassword",
    component: () => import("@/views/ForgotPassword.vue"),
    meta: {
      title: "routes.forgotPassword.name"
    }
  },
  {
    path: i18n.global.t("routes.resetPassword.path"),
    name: "resetPassword",
    component: () => import("@/views/ResetPassword.vue"),
    props: (route: any) => ({userId: route.query.userId, token: route.query.token}),
    meta: {
      title: "routes.resetPassword.name"
    }
  },
  {
    path: i18n.global.t("routes.account.path"),
    name: "account",
    component: () => import("@/views/shared/Account.vue"),
    meta: {
      title: "routes.account.name",
      navIcon: UserCircle,
    }
  },
  {
    path: i18n.global.t("routes.admin.path"),
    name: "admin",
    component: () => import("@/views/admin/Admin.vue"),
    meta: {
      requiredRole: Role.Admin,
      noLinkInBreadcrumbs: true,
      title: "routes.admin.name",
    },
    redirect: {name: 'admin.children.pages.index'},
    children: [
      {
        path: i18n.global.t("routes.admin.children.pages.path"),
        name: "admin.children.pages",
        component: () => import("@/views/admin/Admin.vue"),
        meta: { navIcon: FileText },
        children: [
          {
            path: "",
            name: "admin.children.pages.index",
            component: () => import("@/views/admin/pages/AdminPageIndex.vue"),
          },
          {
            path: i18n.global.t("routes.admin.children.pages.add.path"),
            name: "admin.children.pages.add",
            component: () => import("@/views/admin/pages/AdminPageEditor.vue"),
          },
          {
            path: i18n.global.t("routes.admin.children.pages.edit.path"),
            name: "admin.children.pages.edit",
            component: () => import("@/views/admin/pages/AdminPageEditor.vue"),
            props: true,
          },
        ],
      },
      {
        path: i18n.global.t("routes.admin.children.menus.path"),
        name: "admin.children.menus",
        component: () => import("@/views/admin/menus/AdminMenuIndex.vue"),
        meta: { navIcon: LayoutList },
      },
      {
        path: i18n.global.t("routes.admin.children.media.path"),
        name: "admin.children.media",
        component: () => import("@/views/admin/media/AdminMediaLibrary.vue"),
        meta: { navIcon: Library },
      },
      {
        path: i18n.global.t("routes.admin.children.customizer.path"),
        name: "admin.children.customizer",
        component: () => import("@/views/admin/customizer/AdminCustomizer.vue"),
        meta: { navIcon: Palette },
      },
      {
        path: i18n.global.t("routes.admin.children.siteHealth.path"),
        name: "admin.children.siteHealth",
        component: () => import("@/views/admin/health/AdminSiteHealth.vue"),
        meta: { navIcon: Activity },
      },
      {
        path: i18n.global.t("routes.admin.children.importExport.path"),
        name: "admin.children.importExport",
        component: () => import("@/views/admin/importexport/AdminImportExport.vue"),
        meta: { navIcon: ArrowLeftRight },
      },
      {
        path: i18n.global.t("routes.admin.children.backup.path"),
        name: "admin.children.backup",
        component: () => import("@/views/admin/backup/AdminBackup.vue"),
        meta: { navIcon: HardDriveDownload },
      },
      {
        path: i18n.global.t("routes.admin.children.errorLogs.path"),
        name: "admin.children.errorLogs",
        component: () => import("@/views/admin/errorlogs/AdminErrorLogs.vue"),
        meta: { navIcon: AlertTriangle },
      },
    ]
  },
  {
    path: "/erreur",
    name: "internalError",
    component: () => import("@/views/public/InternalError.vue"),
    meta: { public: true, title: "routes.internalError.name" }
  },
  {
    path: "/preview/:slug",
    name: "previewPage",
    component: () => import("@/views/public/PreviewPage.vue"),
    meta: {
      title: "Aperçu",
      public: true
    }
  },
  {
    path: "/maintenance",
    name: "maintenance",
    component: () => import("@/views/public/Maintenance.vue"),
    meta: { public: true, title: "routes.maintenance.name" }
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
  {
    path: "/:pathMatch(.*)*",
    name: "notFound",
    component: NotFound,
    meta: { public: true }
  },
]

const router = createRouter({
  // eslint-disable-next-line
  scrollBehavior(to, from, savedPosition) {
    return {top: 0};
  },
  history: createWebHistory(),
  routes: [...mainRoutes, ...socialRoutes]
});

export function isSocialRoute(route: { meta?: Record<string, any> }): boolean {
  return route.meta?.social === true || route.meta?.socialAuth === true
}

// Ensures we only hit /users/me once per page load. Subsequent navigations
// rely on the populated store (or the first attempt's failure).
let rehydrationAttempted = false;

function hasAuthCookie(): boolean {
  return document.cookie.split(';').some(c => c.trim().startsWith('accessToken='))
}

async function rehydrateWithRetry() {
  const maxAttempts = 6
  const delayMs = 2000
  for (let i = 0; i < maxAttempts; i++) {
    const user = await useUserService().getCurrentUser()
    if (user) return user
    // L'intercepteur pose ce flag quand le refresh-token échoue (403) —
    // l'auth est définitivement invalide, inutile de réessayer.
    if (useApiStore().needToLogout) return null
    if (i < maxAttempts - 1 && hasAuthCookie()) {
      await new Promise(r => setTimeout(r, delayMs))
    }
  }
  return null
}

// eslint-disable-next-line
router.beforeEach(async (to, from) => {
  const userStore = useUserStore()

  const isAdminRoute = !!to.matched.find(r => r.meta.requiredRole)?.meta.requiredRole;
  const isMaintenancePage = to.name === "maintenance";
  const isLoginRoute = to.name === "login" || to.name === "socialLogin";

  if (!isAdminRoute && !isMaintenancePage && !isLoginRoute) {
    try {
      const response = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/maintenance-status`)
      if (response.data?.isMaintenanceMode) {
        return { name: "maintenance" }
      }
    } catch {
      // Si l'API est indisponible, on laisse la navigation continuer normalement.
    }
  }

  const requiredRole = to.matched.find(r => r.meta.requiredRole)?.meta.requiredRole;
  if (!requiredRole)
    return;

  // On a hard reload the Pinia store is empty, but the auth cookie may still
  // be valid server-side. Try to rehydrate from /users/me before deciding to
  // bounce the user to login. Retries when an auth cookie exists but the
  // backend hasn't started yet (dev server restart race condition).
  if (!rehydrationAttempted && !userStore.user.email) {
    rehydrationAttempted = true;
    const user = await rehydrateWithRetry();
    if (user) {
      userStore.setUser(user);
    }
  }

  const isRoleArray = Array.isArray(requiredRole)
  const doesNotHaveGivenRole = !isRoleArray && !userStore.hasRole(requiredRole as Role);
  const hasNoRoleAmongRoleList = isRoleArray && !userStore.hasOneOfTheseRoles(requiredRole as Role[]);
  if (doesNotHaveGivenRole || hasNoRoleAmongRoleList) {
    if (isSocialRoute(to)) {
      return { name: "socialLogin" };
    }
    return {
      name: "login",
    };
  }
});

router.afterEach((to) => {
  const social = isSocialRoute(to)
  if (social) {
    const titleKey = [...to.matched].reverse().find(r => r.meta.title)?.meta.title as string | undefined;
    document.title = titleKey ? `EDB Social - ${titleKey}` : 'EDB Social';
  }
});

export const Router = router;
