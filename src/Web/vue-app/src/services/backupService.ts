import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {IBackupService} from "@/injection/interfaces"
import {BackupRecord} from "@/types/entities"
import {SucceededOrNotResponse} from "@/types/responses"

@injectable()
export class BackupService extends ApiService implements IBackupService {
  public async getAll(): Promise<BackupRecord[]> {
    const response = await this
      ._httpClient
      .get<BackupRecord[]>(`${import.meta.env.VITE_API_BASE_URL}/admin/backups`)
      .catch(function (error: AxiosError): AxiosResponse<BackupRecord[]> {
        return error.response as AxiosResponse<BackupRecord[]>
      })
    return response.data ?? []
  }

  public async create(): Promise<BackupRecord | null> {
    const response = await this
      ._httpClient
      .post<BackupRecord>(`${import.meta.env.VITE_API_BASE_URL}/admin/backup`)
      .catch(function (error: AxiosError): AxiosResponse<BackupRecord> {
        return error.response as AxiosResponse<BackupRecord>
      })
    if (response.status >= 200 && response.status < 300) {
      return response.data
    }
    return null
  }

  public async download(fileName: string): Promise<Blob> {
    const response = await this
      ._httpClient
      .get(`${import.meta.env.VITE_API_BASE_URL}/admin/backup/${fileName}`, {
        responseType: 'blob'
      })
      .catch(function (error: AxiosError): AxiosResponse<Blob> {
        return error.response as AxiosResponse<Blob>
      })
    return response.data as Blob
  }

  public async deleteBackup(id: string): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .delete(`${import.meta.env.VITE_API_BASE_URL}/admin/backup/${id}`)
      .catch(function (error: AxiosError): AxiosResponse {
        return error.response as AxiosResponse
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async restore(fileName: string): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .post(`${import.meta.env.VITE_API_BASE_URL}/admin/backup/restore`,
        {fileName},
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse {
        return error.response as AxiosResponse
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async checkStatus(): Promise<boolean> {
    const response = await this
      ._httpClient
      .get<{available: boolean}>(`${import.meta.env.VITE_API_BASE_URL}/admin/backup/status`)
      .catch(() => null)
    return response?.data?.available ?? false
  }
}
