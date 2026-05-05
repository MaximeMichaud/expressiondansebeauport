import {AxiosError, AxiosResponse} from "axios"
import {IContactService} from "@/injection/interfaces"
import {ApiService} from "@/services/apiService"
import {SucceededOrNotResponse} from "@/types/responses"

export class ContactService extends ApiService implements IContactService {
  public async submit(payload: {
    name: string
    email: string
    message: string
    honeypot?: string
    recipientEmail?: string
    blockId?: string
    pageSlug?: string
  }): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<SucceededOrNotResponse>>(
        `${import.meta.env.VITE_API_BASE_URL}/contact`,
        payload,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
        return error.response as AxiosResponse<SucceededOrNotResponse>
      })

    const data = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(data?.succeeded ?? false, data?.errors ?? [])
  }
}
