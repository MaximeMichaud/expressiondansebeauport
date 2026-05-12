import {AxiosError, AxiosResponse} from "axios"

import {ApiService} from "@/services/apiService"
import {IAppVersionService} from "@/injection/interfaces"
import {AppVersion} from "@/types/entities"

export class AppVersionService extends ApiService implements IAppVersionService {
  public async get(): Promise<AppVersion> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<AppVersion>>(`${import.meta.env.VITE_API_BASE_URL}/admin/version`)
      .catch(function (error: AxiosError): AxiosResponse<AppVersion> {
        return error.response as AxiosResponse<AppVersion>
      })
    return response.data as AppVersion
  }
}
