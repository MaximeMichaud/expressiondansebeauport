import {AxiosError, AxiosResponse} from "axios"

import "@/extensions/date.extensions"
import {ApiService} from "@/services/apiService"
import {IAdministratorService} from "@/injection/interfaces"
import {Administrator} from "@/types/entities";
import {SucceededOrNotResponse} from "@/types/responses";

export class AdministratorService extends ApiService implements IAdministratorService {
  public async getAuthenticated(): Promise<Administrator | undefined> {
    try {
      const response = await this
        ._httpClient
        .get<Administrator, AxiosResponse<Administrator>>(`${import.meta.env.VITE_API_BASE_URL}/admins/me`)
      return response.data
    } catch (error) {
      return Promise.reject(error)
    }
  }

  public async updateMe(admin: Administrator): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .put<Administrator, AxiosResponse<SucceededOrNotResponse>>(
        `${import.meta.env.VITE_API_BASE_URL}/admins/me`,
        admin,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
        return error.response as AxiosResponse<SucceededOrNotResponse>
      })
    const succeededOrNotResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(succeededOrNotResponse.succeeded, succeededOrNotResponse.errors)
  }
}
