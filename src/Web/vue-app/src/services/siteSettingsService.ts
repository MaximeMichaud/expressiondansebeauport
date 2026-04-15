import {AxiosError, AxiosResponse} from "axios"

import {ApiService} from "@/services/apiService"
import {ISiteSettingsService} from "@/injection/interfaces"
import {SucceededOrNotResponse} from "@/types/responses"
import {FooterPartner, SiteSettings, SocialLink} from "@/types/entities"

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

  public async addSocialLink(link: SocialLink): Promise<SocialLink | null> {
    const response = await this._httpClient
      .post<any, AxiosResponse<SocialLink>>(`${import.meta.env.VITE_API_BASE_URL}/admin/site-settings/social-links`, link, this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<SocialLink> { return error.response as AxiosResponse<SocialLink> })
    return response.status >= 200 && response.status < 300 ? response.data : null
  }

  public async updateSocialLink(link: SocialLink): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient
      .patch<any, AxiosResponse<SocialLink>>(`${import.meta.env.VITE_API_BASE_URL}/admin/site-settings/social-links/${link.id}`, link, this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<SocialLink> { return error.response as AxiosResponse<SocialLink> })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async deleteSocialLink(id: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient
      .delete<any, AxiosResponse<any>>(`${import.meta.env.VITE_API_BASE_URL}/admin/site-settings/social-links/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<any> { return error.response as AxiosResponse<any> })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async addFooterPartner(partner: FooterPartner): Promise<FooterPartner | null> {
    const response = await this._httpClient
      .post<any, AxiosResponse<FooterPartner>>(`${import.meta.env.VITE_API_BASE_URL}/admin/site-settings/partners`, partner, this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<FooterPartner> { return error.response as AxiosResponse<FooterPartner> })
    return response.status >= 200 && response.status < 300 ? response.data : null
  }

  public async updateFooterPartner(partner: FooterPartner): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient
      .patch<any, AxiosResponse<FooterPartner>>(`${import.meta.env.VITE_API_BASE_URL}/admin/site-settings/partners/${partner.id}`, partner, this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<FooterPartner> { return error.response as AxiosResponse<FooterPartner> })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async deleteFooterPartner(id: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient
      .delete<any, AxiosResponse<any>>(`${import.meta.env.VITE_API_BASE_URL}/admin/site-settings/partners/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<any> { return error.response as AxiosResponse<any> })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }
}
