import axios from 'axios'

import {
  AuthenticationService,
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
import {BackupService} from "@/services/backupService"

const axiosInstance = axios.create()

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
const errorLogsService = new ErrorLogsService(axiosInstance)
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

function useErrorLogsService() {
  return errorLogsService
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
  useImportExportService,
  useSocialService,
  useBackupService
};
