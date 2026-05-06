import axios from 'axios'

import {
  AuthenticationService,
  AuditLogService,
  ContactService,
  ErrorLogsService,
  ImportExportService,
  MediaService,
  MenuService,
  PageService,
  SiteHealthService,
  SiteSettingsService,
  UserService
} from "@/services"
import {AdministratorService} from "@/services/administratorService"
import {SocialService} from "@/services/socialService"
import {PushService} from "@/services/pushService"
import {BackupService} from "@/services/backupService"
import {setupInterceptors} from "@/services/apiService"

const axiosInstance = axios.create({ withCredentials: true })
setupInterceptors(axiosInstance)

const administratorService = new AdministratorService(axiosInstance)
const authenticationService = new AuthenticationService(axiosInstance)
const userService = new UserService(axiosInstance)
const mediaService = new MediaService(axiosInstance)
const pageService = new PageService(axiosInstance)
const menuService = new MenuService(axiosInstance)
const siteSettingsService = new SiteSettingsService(axiosInstance)
const siteHealthService = new SiteHealthService(axiosInstance)
const importExportService = new ImportExportService(axiosInstance)
const socialService = new SocialService(axiosInstance)
const pushService = new PushService(axiosInstance)
const errorLogsService = new ErrorLogsService(axiosInstance)
const auditLogService = new AuditLogService(axiosInstance)
const contactService = new ContactService(axiosInstance)
const backupService = new BackupService(axiosInstance)

function useAdministratorService() {
  return administratorService
}

function useAuthenticationService() {
  return authenticationService
}

function useUserService() {
  return userService
}

function useMediaService() {
  return mediaService
}

function usePageService() {
  return pageService
}

function useMenuService() {
  return menuService
}

function useSiteSettingsService() {
  return siteSettingsService
}

function useSiteHealthService() {
  return siteHealthService
}

function useImportExportService() {
  return importExportService
}

function useSocialService() {
  return socialService
}

function usePushService() {
  return pushService
}

function useErrorLogsService() {
  return errorLogsService
}

function useAuditLogService() {
  return auditLogService
}

function useContactService() {
  return contactService
}

function useBackupService() {
  return backupService
}

export {
  useAdministratorService,
  useAuthenticationService,
  useUserService,
  useMediaService,
  usePageService,
  useMenuService,
  useSiteSettingsService,
  useSiteHealthService,
  useErrorLogsService,
  useAuditLogService,
  useContactService,
  useImportExportService,
  useSocialService,
  usePushService,
  useBackupService
};
