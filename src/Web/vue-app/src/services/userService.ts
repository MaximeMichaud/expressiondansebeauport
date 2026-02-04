import {AxiosError, AxiosResponse} from "axios";
import {injectable} from "inversify";

import "@/extensions/date.extensions";
import {ApiService} from "@/services/apiService";
import {IUserService} from "@/injection/interfaces";
import {User} from "@/types";

@injectable()
export class UserService extends ApiService implements IUserService {
  public async getCurrentUser(): Promise<User> {
    const response = await this
    ._httpClient
    .get<any, AxiosResponse<User>>(`${import.meta.env.VITE_API_BASE_URL}/users/me`)
    .catch(function (error: AxiosError): AxiosResponse<User> {
      return error.response as AxiosResponse<User>
    })
    return response.data as User
  }
}