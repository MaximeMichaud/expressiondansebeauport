import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import "@/extensions/date.extensions"
import {ApiService} from "@/services/apiService"
import {IPageService} from "@/injection/interfaces"
import {SucceededOrNotResponse} from "@/types/responses";
import {Page} from "@/types/entities";

@injectable()
export class PageService extends ApiService implements IPageService {
  public async getAll(): Promise<Page[]> {
    const response = await this
        ._httpClient
        .get<any, AxiosResponse<Page[]>>(`${import.meta.env.VITE_API_BASE_URL}/admin/pages`)
        .catch(function (error: AxiosError): AxiosResponse<Page[]> {
          return error.response as AxiosResponse<Page[]>
        })
    return response.data as Page[]
  }

  public async getPage(id: string): Promise<Page> {
    const response = await this
        ._httpClient
        .get<any, AxiosResponse<Page>>(`${import.meta.env.VITE_API_BASE_URL}/admin/pages/${id}`)
        .catch(function (error: AxiosError): AxiosResponse<Page> {
          return error.response as AxiosResponse<Page>
        })
    return response.data as Page
  }

  public async getPageBySlug(slug: string): Promise<Page> {
    const response = await this
        ._httpClient
        .get<any, AxiosResponse<Page>>(`${import.meta.env.VITE_API_BASE_URL}/pages/${slug}`)
        .catch(function (error: AxiosError): AxiosResponse<Page> {
          return error.response as AxiosResponse<Page>
        })
    return response.data as Page
  }

  public async updatePage(page: Page): Promise<SucceededOrNotResponse> {
    const response = await this
        ._httpClient
        .put<any, AxiosResponse<SucceededOrNotResponse>>(
            `${import.meta.env.VITE_API_BASE_URL}/admin/pages/${page.id}`,
            page,
            this.headersWithJsonContentType())
        .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
          return error.response as AxiosResponse<SucceededOrNotResponse>
        })
    const succeededOrNotResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(succeededOrNotResponse.succeeded, succeededOrNotResponse.errors)
  }
}
