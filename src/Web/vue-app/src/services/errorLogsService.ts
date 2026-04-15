import {AxiosError, AxiosResponse} from "axios"

import {ApiService} from "@/services/apiService"
import {IErrorLogsService} from "@/injection/interfaces"
import {ErrorLog} from "@/types/entities"

export class ErrorLogsService extends ApiService implements IErrorLogsService {
  public async getAll(): Promise<ErrorLog[]> {
    try {
      const response = await this
        ._httpClient
        .get<any, AxiosResponse<ErrorLog[]>>(`${import.meta.env.VITE_API_BASE_URL}/admin/error-logs`)
      return response.data as ErrorLog[]
    } catch (error) {
      const axiosError = error as AxiosError<ErrorLog[]>
      if (!axiosError.response) return []
      return (axiosError.response.data ?? []) as ErrorLog[]
    }
  }
}
