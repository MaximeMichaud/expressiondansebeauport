import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {IPageService} from "@/injection/interfaces"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {Page} from "@/types/entities"

@injectable()
export class PageService extends ApiService implements IPageService {
  public async getAll(pageIndex: number, pageSize: number, status?: string): Promise<PaginatedResponse<Page>> {
    let url = `${import.meta.env.VITE_API_BASE_URL}/admin/pages?pageIndex=${pageIndex}&pageSize=${pageSize}`
    if (status) url += `&status=${status}`
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<PaginatedResponse<Page>>>(url)
      .catch(function (error: AxiosError): AxiosResponse<PaginatedResponse<Page>> {
        return error.response as AxiosResponse<PaginatedResponse<Page>>
      })
    return response.data as PaginatedResponse<Page>
  }

  public async get(id: string): Promise<Page> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<Page>>(`${import.meta.env.VITE_API_BASE_URL}/admin/pages/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<Page> {
        return error.response as AxiosResponse<Page>
      })
    return response.data as Page
  }

  public async create(page: Page): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<Page>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/pages`,
        page,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<Page> {
        return error.response as AxiosResponse<Page>
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async update(page: Page): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .put<any, AxiosResponse<Page>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/pages/${page.id}`,
        page,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<Page> {
        return error.response as AxiosResponse<Page>
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async delete(id: string): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .delete<any, AxiosResponse<SucceededOrNotResponse>>(`${import.meta.env.VITE_API_BASE_URL}/admin/pages/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
        return error.response as AxiosResponse<SucceededOrNotResponse>
      })
    return new SucceededOrNotResponse(response.status === 204)
  }
}
