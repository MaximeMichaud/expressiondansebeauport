import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {ISiteSettingsService} from "@/injection/interfaces"
import {SucceededOrNotResponse} from "@/types/responses"
import {SiteSettings} from "@/types/entities"

@injectable()
export class SiteSettingsService extends ApiService implements ISiteSettingsService {
  public async get(): Promise<SiteSettings> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<SiteSettings>>(`${import.meta.env.VITE_API_BASE_URL}/admin/site-settings`)
      .catch(function (error: AxiosError): AxiosResponse<SiteSettings> {
        return error.response as AxiosResponse<SiteSettings>
      })
    return response.data as SiteSettings
  }

  public async update(settings: SiteSettings): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .put<any, AxiosResponse<SiteSettings>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/site-settings`,
        settings,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<SiteSettings> {
        return error.response as AxiosResponse<SiteSettings>
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async getPublic(): Promise<SiteSettings> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<SiteSettings>>(`${import.meta.env.VITE_API_BASE_URL}/public/site-settings`)
      .catch(function (error: AxiosError): AxiosResponse<SiteSettings> {
        return error.response as AxiosResponse<SiteSettings>
      })
    return response.data as SiteSettings
  }
}
