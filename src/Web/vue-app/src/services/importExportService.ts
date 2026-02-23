import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {IImportExportService} from "@/injection/interfaces"
import {SucceededOrNotResponse} from "@/types/responses"

@injectable()
export class ImportExportService extends ApiService implements IImportExportService {
  public async exportData(): Promise<Blob> {
    const response = await this
      ._httpClient
      .get(`${import.meta.env.VITE_API_BASE_URL}/admin/export`, {
        responseType: 'blob'
      })
      .catch(function (error: AxiosError): AxiosResponse<Blob> {
        return error.response as AxiosResponse<Blob>
      })
    return response.data as Blob
  }

  public async importData(file: File): Promise<SucceededOrNotResponse> {
    const formData = new FormData()
    formData.append("file", file)
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/import`,
        formData,
        this.headersWithFormDataContentType())
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }
}
