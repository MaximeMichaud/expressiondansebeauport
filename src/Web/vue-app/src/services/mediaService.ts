import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {IMediaService} from "@/injection/interfaces"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {MediaFile} from "@/types/entities"

@injectable()
export class MediaService extends ApiService implements IMediaService {
  public async getAll(pageIndex: number, pageSize: number): Promise<PaginatedResponse<MediaFile>> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<PaginatedResponse<MediaFile>>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/media?pageIndex=${pageIndex}&pageSize=${pageSize}`)
      .catch(function (error: AxiosError): AxiosResponse<PaginatedResponse<MediaFile>> {
        return error.response as AxiosResponse<PaginatedResponse<MediaFile>>
      })
    return response.data as PaginatedResponse<MediaFile>
  }

  public async get(id: string): Promise<MediaFile> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<MediaFile>>(`${import.meta.env.VITE_API_BASE_URL}/admin/media/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<MediaFile> {
        return error.response as AxiosResponse<MediaFile>
      })
    return response.data as MediaFile
  }

  public async upload(file: File): Promise<MediaFile | null> {
    const formData = new FormData()
    formData.append("file", file)
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<MediaFile>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/media`,
        formData,
        this.headersWithFormDataContentType())
      .catch(function (error: AxiosError): AxiosResponse<MediaFile> {
        return error.response as AxiosResponse<MediaFile>
      })
    return response.data as MediaFile
  }

  public async update(id: string, altText: string): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .patch<any, AxiosResponse<MediaFile>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/media/${id}`,
        {altText},
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<MediaFile> {
        return error.response as AxiosResponse<MediaFile>
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async delete(id: string): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .delete<any, AxiosResponse<SucceededOrNotResponse>>(`${import.meta.env.VITE_API_BASE_URL}/admin/media/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
        return error.response as AxiosResponse<SucceededOrNotResponse>
      })
    return new SucceededOrNotResponse(response.status === 204)
  }
}
