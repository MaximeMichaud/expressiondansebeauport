import i18n from "@/i18n";
import {Role} from "@/types/enums";
import {createRouter, createWebHistory} from "vue-router";

import Home from "@/views/public/Home.vue";
import PublicContact from "@/views/public/PublicContact.vue";
import PublicPage from "@/views/public/PublicPage.vue";
import NotFound from "@/views/public/NotFound.vue";
import InternalError from "@/views/public/InternalError.vue";
import Login from "@/views/Login.vue";
import TwoFactor from "@/views/TwoFactor.vue";
import ForgotPassword from "@/views/ForgotPassword.vue";
import ResetPassword from "@/views/ResetPassword.vue";
import Account from "@/views/shared/Account.vue";

import Admin from "../views/admin/Admin.vue";
import AdminMediaLibrary from "@/views/admin/media/AdminMediaLibrary.vue";
import AdminPageIndex from "@/views/admin/pages/AdminPageIndex.vue";
import AdminPageEditor from "@/views/admin/pages/AdminPageEditor.vue";
import AdminMenuIndex from "@/views/admin/menus/AdminMenuIndex.vue";
import AdminCustomizer from "@/views/admin/customizer/AdminCustomizer.vue";
import AdminSiteHealth from "@/views/admin/health/AdminSiteHealth.vue";
import AdminImportExport from "@/views/admin/importexport/AdminImportExport.vue";
import AdminBackup from "@/views/admin/backup/AdminBackup.vue";

import {useUserStore} from "@/stores/userStore";

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
    component: Login,
    meta: {
      title: "routes.login.name"
    }
  },
  {
    // Redirect old /connexion to /admin/connexion
    path: i18n.t("routes.login.path"),
    redirect: "/admin/connexion"
  },
  {
    path: i18n.t("routes.twoFactor.path"),
    name: "twoFactor",
    component: TwoFactor,
    meta: {
      title: "routes.twoFactor.name"
    }
  },
  {
    path: i18n.t("routes.forgotPassword.path"),
    name: "forgotPassword",
    component: ForgotPassword,
    meta: {
      title: "routes.forgotPassword.name"
    }
  },
  {
    path: i18n.t("routes.resetPassword.path"),
    name: "resetPassword",
    component: ResetPassword,
    props: (route: any) => ({userId: route.query.userId, token: route.query.token}),
    meta: {
      title: "routes.resetPassword.name"
    }
  },
  {
    path: i18n.t("routes.account.path"),
    name: "account",
    component: Account,
    meta: {
      title: "routes.account.name"
    }
  },
  {
    path: i18n.t("routes.admin.path"),
    name: "admin",
    component: Admin,
    meta: {
      requiredRole: Role.Admin,
      noLinkInBreadcrumbs: true,
      title: "routes.admin.name",
    },
    redirect: {name: 'admin.children.pages.index'},
    children: [
      {
        path: i18n.t("routes.admin.children.pages.path"),
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
            name: "admin.children.pages.add",
            component: AdminPageEditor,
          },
          {
            path: i18n.t("routes.admin.children.pages.edit.path"),
            name: "admin.children.pages.edit",
            component: AdminPageEditor,
            props: true,
          },
        ],
      },
      {
        path: i18n.t("routes.admin.children.menus.path"),
        name: "admin.children.menus",
        component: AdminMenuIndex,
      },
      {
        path: i18n.t("routes.admin.children.media.path"),
        name: "admin.children.media",
        component: AdminMediaLibrary,
      },
      {
        path: i18n.t("routes.admin.children.customizer.path"),
        name: "admin.children.customizer",
        component: AdminCustomizer,
      },
      {
        path: i18n.t("routes.admin.children.siteHealth.path"),
        name: "admin.children.siteHealth",
        component: AdminSiteHealth,
      },
      {
        path: i18n.t("routes.admin.children.importExport.path"),
        name: "admin.children.importExport",
        component: AdminImportExport,
      },
      {
        path: i18n.t("routes.admin.children.backup.path"),
        name: "admin.children.backup",
        component: AdminBackup,
      },
      {
        path: 'groupes',
        name: 'admin.children.groups',
        component: () => import('@/views/admin/groups/AdminGroupIndex.vue'),
      },
      {
        path: 'membres',
        name: 'admin.children.members',
        component: () => import('@/views/admin/members/AdminMemberIndex.vue'),
      },
      {
        path: 'sessions',
        name: 'admin.children.sessions',
        component: () => import('@/views/admin/sessions/AdminSessionIndex.vue'),
      },
    ]
  },
  {
    path: "/erreur",
    name: "internalError",
    component: InternalError,
    meta: { public: true, title: "routes.internalError.name" }
  },
  {
    path: "/nous-joindre",
    name: "publicContact",
    component: PublicContact,
    meta: {
      title: "routes.home.name",
      public: true
    }
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

// eslint-disable-next-line
router.beforeEach(async (to, from) => {
  const userStore = useUserStore()

  const requiredRole = to.matched.find(r => r.meta.requiredRole)?.meta.requiredRole;
  if (!requiredRole)
    return;

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
  const titleKey = [...to.matched].reverse().find(r => r.meta.title)?.meta.title as string | undefined;
  if (!titleKey) {
    document.title = social ? 'EDB Social' : 'EDB';
    return;
  }
  const title = social ? titleKey : i18n.t(titleKey);
  document.title = title ? `${title} - EDB Social` : 'EDB Social';
});

export const Router = router;
