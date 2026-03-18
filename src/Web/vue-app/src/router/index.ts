import i18n from "@/i18n";
import {Role} from "@/types/enums";
import {createRouter, createWebHistory} from "vue-router";

import Home from "@/views/public/Home.vue";
import PublicPage from "@/views/public/PublicPage.vue";
import NotFound from "@/views/public/NotFound.vue";
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

import {useUserStore} from "@/stores/userStore";

const isSocialSubdomain = (): boolean => {
  const hostname = window.location.hostname
  return hostname.startsWith('social.') || import.meta.env.VITE_FORCE_SOCIAL === 'true'
}

export const isSocial = isSocialSubdomain()

const socialRoutes = [
  {
    path: '/connexion',
    name: 'socialLogin',
    component: () => import('@/views/social/SocialLogin.vue'),
    meta: { title: 'Connexion', public: true, socialAuth: true }
  },
  {
    path: '/inscription',
    name: 'socialRegister',
    component: () => import('@/views/social/SocialRegister.vue'),
    meta: { title: 'Inscription', public: true, socialAuth: true }
  },
  {
    path: '/confirmation',
    name: 'socialConfirm',
    component: () => import('@/views/social/SocialConfirm.vue'),
    meta: { title: 'Confirmation', public: true, socialAuth: true }
  },
  {
    path: '/',
    name: 'socialHome',
    component: () => import('@/views/social/SocialHome.vue'),
    meta: { title: 'Menu Principal', requiredRole: [Role.Member, Role.Professor, Role.Admin] }
  },
  {
    path: '/important',
    name: 'socialImportant',
    component: () => import('@/views/social/SocialImportant.vue'),
    meta: { title: 'Important', requiredRole: [Role.Member, Role.Professor, Role.Admin] }
  },
  {
    path: '/portail',
    name: 'socialPortal',
    component: () => import('@/views/social/SocialPortal.vue'),
    meta: { title: 'Portail EDB', requiredRole: [Role.Member, Role.Professor, Role.Admin] }
  },
  {
    path: '/groupes/:id',
    name: 'socialGroup',
    component: () => import('@/views/social/SocialGroup.vue'),
    meta: { title: 'Groupe', requiredRole: [Role.Member, Role.Professor, Role.Admin] },
    props: true
  },
  {
    path: '/membres',
    name: 'socialMembers',
    component: () => import('@/views/social/SocialMembers.vue'),
    meta: { title: 'Membres', requiredRole: [Role.Member, Role.Professor, Role.Admin] }
  },
  {
    path: '/membres/:id',
    name: 'socialMemberProfile',
    component: () => import('@/views/social/SocialMemberProfile.vue'),
    meta: { title: 'Profil', requiredRole: [Role.Member, Role.Professor, Role.Admin] },
    props: true
  },
  {
    path: '/messages',
    name: 'socialMessages',
    component: () => import('@/views/social/SocialMessages.vue'),
    meta: { title: 'Messages', requiredRole: [Role.Member, Role.Professor, Role.Admin] }
  },
  {
    path: '/messages/:conversationId',
    name: 'socialConversation',
    component: () => import('@/views/social/SocialConversation.vue'),
    meta: { title: 'Conversation', requiredRole: [Role.Member, Role.Professor, Role.Admin] },
    props: true
  },
  {
    path: '/compte',
    name: 'socialAccount',
    component: () => import('@/views/social/SocialAccount.vue'),
    meta: { title: 'Mon compte', requiredRole: [Role.Member, Role.Professor, Role.Admin] }
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'notFound',
    component: NotFound,
    meta: { public: true }
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
  routes: isSocial ? socialRoutes : mainRoutes
});

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
    if (isSocial) {
      return { name: "socialLogin" };
    }
    return {
      name: "login",
    };
  }
});

router.afterEach((to) => {
  const titleKey = [...to.matched].reverse().find(r => r.meta.title)?.meta.title as string | undefined;
  if (!titleKey) {
    document.title = isSocial ? 'EDB Social' : 'EDB';
    return;
  }
  // Social routes use plain strings, main routes use i18n keys
  const title = isSocial ? titleKey : i18n.t(titleKey);
  document.title = title ? `${title} | EDB Social` : 'EDB Social';
});

export const Router = router;
