import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {IErrorLogsService} from "@/injection/interfaces"
import {ErrorLog} from "@/types/entities"

@injectable()
export class ErrorLogsService extends ApiService implements IErrorLogsService {
  public async getAll(): Promise<ErrorLog[]> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<ErrorLog[]>>(`${import.meta.env.VITE_API_BASE_URL}/admin/error-logs`)
      .catch(function (error: AxiosError): AxiosResponse<ErrorLog[]> {
        return error.response as AxiosResponse<ErrorLog[]>
      })
    return response.data as ErrorLog[]
  }
}
