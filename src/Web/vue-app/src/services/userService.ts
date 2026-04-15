import {AxiosResponse} from "axios";

import "@/extensions/date.extensions";
import {ApiService} from "@/services/apiService";
import {IUserService} from "@/injection/interfaces";
import {User} from "@/types";

export class UserService extends ApiService implements IUserService {
  public async getCurrentUser(): Promise<User | null> {
    try {
      const response = await this
      ._httpClient
      .get<any, AxiosResponse<User>>(`${import.meta.env.VITE_API_BASE_URL}/users/me`)
      return response.data ?? null
    } catch {
      return null
    }
  }
}