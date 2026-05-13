import {AxiosError, AxiosResponse} from "axios"

import {ApiService} from "@/services/apiService"
import {IHelpArticleService} from "@/injection/interfaces"
import {SucceededOrNotResponse} from "@/types/responses"
import {HelpArticle} from "@/types/entities"

function silenceNotFound(error: AxiosError): AxiosResponse<HelpArticle> | null {
  if (error.response?.status === 404) return null
  return (error.response ?? null) as AxiosResponse<HelpArticle> | null
}

export class HelpArticleService extends ApiService implements IHelpArticleService {
  public async getAll(category?: string, isPublished?: boolean): Promise<HelpArticle[]> {
    const params = new URLSearchParams()
    if (category) params.append('category', category)
    if (isPublished !== undefined) params.append('isPublished', String(isPublished))
    const query = params.toString()
    const url = `${import.meta.env.VITE_API_BASE_URL}/admin/help-articles${query ? `?${query}` : ''}`
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<HelpArticle[]>>(url)
      .catch(function (error: AxiosError): AxiosResponse<HelpArticle[]> {
        return error.response as AxiosResponse<HelpArticle[]>
      })
    return response.data as HelpArticle[] ?? []
  }

  public async getById(id: string): Promise<HelpArticle | null> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<HelpArticle>>(`${import.meta.env.VITE_API_BASE_URL}/admin/help-articles/${id}`)
      .catch(silenceNotFound)
    if (response?.status !== undefined && response.status >= 200 && response.status < 300) {
      return response.data as HelpArticle
    }
    return null
  }

  public async getBySlug(slug: string): Promise<HelpArticle | null> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<HelpArticle>>(`${import.meta.env.VITE_API_BASE_URL}/admin/help-articles/by-slug/${encodeURIComponent(slug)}`)
      .catch(silenceNotFound)
    if (response?.status !== undefined && response.status >= 200 && response.status < 300) {
      return response.data as HelpArticle
    }
    return null
  }

  public async getByRoute(routeName: string): Promise<HelpArticle | null> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<HelpArticle>>(`${import.meta.env.VITE_API_BASE_URL}/admin/help-articles/by-route?routeName=${encodeURIComponent(routeName)}`)
      .catch(silenceNotFound)
    if (response?.status !== undefined && response.status >= 200 && response.status < 300) {
      return response.data as HelpArticle
    }
    return null
  }

  public async create(article: HelpArticle): Promise<{ succeeded: boolean; article?: HelpArticle }> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<HelpArticle>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/help-articles`,
        article,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<HelpArticle> {
        return error.response as AxiosResponse<HelpArticle>
      })
    const succeeded = response.status >= 200 && response.status < 300
    return { succeeded, article: succeeded ? response.data as HelpArticle : undefined }
  }

  public async update(article: HelpArticle): Promise<{ succeeded: boolean; article?: HelpArticle }> {
    const response = await this
      ._httpClient
      .put<any, AxiosResponse<HelpArticle>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/help-articles/${article.id}`,
        article,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<HelpArticle> {
        return error.response as AxiosResponse<HelpArticle>
      })
    const succeeded = response.status >= 200 && response.status < 300
    return { succeeded, article: succeeded ? response.data as HelpArticle : undefined }
  }

  public async delete(id: string): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .delete<any, AxiosResponse<SucceededOrNotResponse>>(`${import.meta.env.VITE_API_BASE_URL}/admin/help-articles/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
        return error.response as AxiosResponse<SucceededOrNotResponse>
      })
    return new SucceededOrNotResponse(response.status === 204)
  }

  public async getPermissions(): Promise<{ canEdit: boolean }> {
    try {
      const response = await this
        ._httpClient
        .get<any, AxiosResponse<{ canEdit: boolean }>>(`${import.meta.env.VITE_API_BASE_URL}/admin/help-articles/permissions`)
      return response.data ?? { canEdit: false }
    } catch {
      return { canEdit: false }
    }
  }
}
