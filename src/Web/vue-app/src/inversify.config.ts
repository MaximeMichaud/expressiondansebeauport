import {Container} from "inversify";
import axios, {AxiosInstance} from 'axios';
import "reflect-metadata";

import {TYPES} from "@/injection/types";
import {
  IAdministratorService,
  IApiService,
  IAuthenticationService,
  IImportExportService,
  IMediaService,
  IMemberService,
  IMenuService,
  IPageService,
  ISiteHealthService,
  ISiteSettingsService,
  IUserService
} from "@/injection/interfaces";
import {
  ApiService,
  AuthenticationService,
  ImportExportService,
  MediaService,
  MemberService,
  MenuService,
  PageService,
  SiteHealthService,
  SiteSettingsService,
  UserService
} from "@/services";
import {AdministratorService} from "@/services/administratorService";

const dependencyInjection = new Container();
dependencyInjection.bind<AxiosInstance>(TYPES.AxiosInstance).toConstantValue(axios.create())
dependencyInjection.bind<IApiService>(TYPES.IApiService).to(ApiService).inSingletonScope()
dependencyInjection.bind<IAdministratorService>(TYPES.IAdministratorService).to(AdministratorService).inSingletonScope()
dependencyInjection.bind<IAuthenticationService>(TYPES.IAuthenticationService).to(AuthenticationService).inSingletonScope()
dependencyInjection.bind<IMemberService>(TYPES.IMemberService).to(MemberService).inSingletonScope()
dependencyInjection.bind<IUserService>(TYPES.IUserService).to(UserService).inSingletonScope()
dependencyInjection.bind<IMediaService>(TYPES.IMediaService).to(MediaService).inSingletonScope()
dependencyInjection.bind<IPageService>(TYPES.IPageService).to(PageService).inSingletonScope()
dependencyInjection.bind<IMenuService>(TYPES.IMenuService).to(MenuService).inSingletonScope()
dependencyInjection.bind<ISiteSettingsService>(TYPES.ISiteSettingsService).to(SiteSettingsService).inSingletonScope()
dependencyInjection.bind<ISiteHealthService>(TYPES.ISiteHealthService).to(SiteHealthService).inSingletonScope()
dependencyInjection.bind<IImportExportService>(TYPES.IImportExportService).to(ImportExportService).inSingletonScope()

function useAdministratorService() {
  return dependencyInjection.get<IAdministratorService>(TYPES.IAdministratorService);
}

function useAuthenticationService() {
  return dependencyInjection.get<IAuthenticationService>(TYPES.IAuthenticationService);
}

function useMemberService() {
  return dependencyInjection.get<IMemberService>(TYPES.IMemberService);
}

function useUserService() {
  return dependencyInjection.get<IUserService>(TYPES.IUserService);
}

function useMediaService() {
  return dependencyInjection.get<IMediaService>(TYPES.IMediaService);
}

function usePageService() {
  return dependencyInjection.get<IPageService>(TYPES.IPageService);
}

function useMenuService() {
  return dependencyInjection.get<IMenuService>(TYPES.IMenuService);
}

function useSiteSettingsService() {
  return dependencyInjection.get<ISiteSettingsService>(TYPES.ISiteSettingsService);
}

function useSiteHealthService() {
  return dependencyInjection.get<ISiteHealthService>(TYPES.ISiteHealthService);
}

function useImportExportService() {
  return dependencyInjection.get<IImportExportService>(TYPES.IImportExportService);
}

export {
  dependencyInjection,
  useAdministratorService,
  useAuthenticationService,
  useMemberService,
  useUserService,
  useMediaService,
  usePageService,
  useMenuService,
  useSiteSettingsService,
  useSiteHealthService,
  useImportExportService
};