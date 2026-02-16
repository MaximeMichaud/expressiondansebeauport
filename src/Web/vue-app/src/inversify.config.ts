import {Container} from "inversify";
import axios, {AxiosInstance} from 'axios';
import "reflect-metadata";

import {TYPES} from "@/injection/types";
import {
  IAdministratorService,
  IApiService,
  IAuthenticationService,
  IMemberService,
  IPageService,
  IUserService
} from "@/injection/interfaces";
import {
  ApiService,
  AuthenticationService,
  MemberService,
  UserService
} from "@/services";
import {AdministratorService} from "@/services/administratorService";
import {PageService} from "@/services/pageService";

const dependencyInjection = new Container();
dependencyInjection.bind<AxiosInstance>(TYPES.AxiosInstance).toConstantValue(axios.create())
dependencyInjection.bind<IApiService>(TYPES.IApiService).to(ApiService).inSingletonScope()
dependencyInjection.bind<IAdministratorService>(TYPES.IAdministratorService).to(AdministratorService).inSingletonScope()
dependencyInjection.bind<IAuthenticationService>(TYPES.IAuthenticationService).to(AuthenticationService).inSingletonScope()
dependencyInjection.bind<IMemberService>(TYPES.IMemberService).to(MemberService).inSingletonScope()
dependencyInjection.bind<IPageService>(TYPES.IPageService).to(PageService).inSingletonScope()
dependencyInjection.bind<IUserService>(TYPES.IUserService).to(UserService).inSingletonScope()

function useAdministratorService() {
  return dependencyInjection.get<IAdministratorService>(TYPES.IAdministratorService);
}

function useAuthenticationService() {
  return dependencyInjection.get<IAuthenticationService>(TYPES.IAuthenticationService);
}

function useMemberService() {
  return dependencyInjection.get<IMemberService>(TYPES.IMemberService);
}

function usePageService() {
  return dependencyInjection.get<IPageService>(TYPES.IPageService);
}

function useUserService() {
  return dependencyInjection.get<IUserService>(TYPES.IUserService);
}


export {
  dependencyInjection,
  useAdministratorService,
  useAuthenticationService,
  useMemberService,
  usePageService,
  useUserService
};