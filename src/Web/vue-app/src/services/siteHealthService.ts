import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {ISiteHealthService} from "@/injection/interfaces"
import {SiteHealth} from "@/types/entities"

@injectable()
export class SiteHealthService extends ApiService implements ISiteHealthService {
  public async get(): Promise<SiteHealth> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<SiteHealth>>(`${import.meta.env.VITE_API_BASE_URL}/admin/site-health`)
      .catch(function (error: AxiosError): AxiosResponse<SiteHealth> {
        return error.response as AxiosResponse<SiteHealth>
      })
    return response.data as SiteHealth
  }
}
