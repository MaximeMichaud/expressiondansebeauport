import {AxiosError, AxiosResponse} from "axios"

import {ApiService} from "@/services/apiService"
import {IPageService} from "@/injection/interfaces"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {Page, PageRevision, PageRevisionListItem} from "@/types/entities"

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
      .post<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/pages`,
        page,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    const succeeded = response.status >= 200 && response.status < 300
    return new SucceededOrNotResponse(succeeded, succeeded ? [] : response.data?.errors ?? [])
  }

  public async update(page: Page): Promise<{ succeeded: boolean; page?: Page }> {
    const response = await this
      ._httpClient
      .put<any, AxiosResponse<Page>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/pages/${page.id}`,
        page,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<Page> {
        return error.response as AxiosResponse<Page>
      })
    const succeeded = response.status >= 200 && response.status < 300
    return { succeeded, page: succeeded ? response.data as Page : undefined }
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

  public async duplicate(id: string): Promise<Page | null> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<Page>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/pages/${id}/duplicate`,
        {},
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<Page> {
        return error.response as AxiosResponse<Page>
      })
    if (response.status >= 200 && response.status < 300) {
      return response.data as Page
    }
    return null
  }

  public async getRevisions(pageId: string): Promise<PageRevisionListItem[]> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<PageRevisionListItem[]>>(`${import.meta.env.VITE_API_BASE_URL}/admin/pages/${pageId}/revisions`)
      .catch(function (error: AxiosError): AxiosResponse<PageRevisionListItem[]> {
        return error.response as AxiosResponse<PageRevisionListItem[]>
      })
    return response.data as PageRevisionListItem[] ?? []
  }

  public async getRevision(pageId: string, revisionId: string): Promise<PageRevision> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<PageRevision>>(`${import.meta.env.VITE_API_BASE_URL}/admin/pages/${pageId}/revisions/${revisionId}`)
      .catch(function (error: AxiosError): AxiosResponse<PageRevision> {
        return error.response as AxiosResponse<PageRevision>
      })
    return response.data as PageRevision
  }

  public async restoreRevision(pageId: string, revisionId: string): Promise<Page | null> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<Page>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/pages/${pageId}/revisions/${revisionId}/restore`,
        {},
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<Page> {
        return error.response as AxiosResponse<Page>
      })
    if (response.status >= 200 && response.status < 300) {
      return response.data as Page
    }
    return null
  }

  public async autosave(pageId: string, data: Partial<Page>): Promise<{ savedAt: string } | null> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<{ savedAt: string }>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/pages/${pageId}/autosave`,
        data,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<{ savedAt: string }> {
        return error.response as AxiosResponse<{ savedAt: string }>
      })
    if (response.status >= 200 && response.status < 300) {
      return response.data
    }
    return null
  }

  public async createPreview(pageId: string): Promise<{ token: string; previewUrl: string } | null> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<{ token: string; previewUrl: string }>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/pages/${pageId}/preview`,
        {},
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<{ token: string; previewUrl: string }> {
        return error.response as AxiosResponse<{ token: string; previewUrl: string }>
      })
    if (response.status >= 200 && response.status < 300) {
      return response.data
    }
    return null
  }
}
