import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import "@/extensions/date.extensions"
import {ApiService} from "@/services/apiService"
import {IMemberService} from "@/injection/interfaces"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses";
import {Member} from "@/types/entities";
import {Guid} from "@/types";

@injectable()
export class MemberService extends ApiService implements IMemberService {
  public async getAuthenticated(): Promise<Member | undefined> {
    try {
      const response = await this
        ._httpClient
        .get<Member, AxiosResponse<Member>>(`${import.meta.env.VITE_API_BASE_URL}/members/me`)
      return response.data
    } catch (error) {
      return Promise.reject(error)
    }
  }

  public async search(pageIndex: number, pageSize: number, searchValue: string): Promise<PaginatedResponse<Member>> {
    const response = await this
        ._httpClient
        .get<any, AxiosResponse<PaginatedResponse<Member>>>(
            `${import.meta.env.VITE_API_BASE_URL}/members?pageIndex=${pageIndex}&pageSize=${pageSize}&searchValue=${searchValue}`)
        .catch(function (error: AxiosError): AxiosResponse<PaginatedResponse<Member>> {
          return error.response as AxiosResponse<PaginatedResponse<Member>>
        })
    return response.data as PaginatedResponse<Member>
  }

  public async getMember(id: string): Promise<Member> {
    const response = await this
        ._httpClient
        .get<any, AxiosResponse<Member>>(`${import.meta.env.VITE_API_BASE_URL}/members/${id}`)
        .catch(function (error: AxiosError): AxiosResponse<Member> {
          return error.response as AxiosResponse<Member>
        })
    return response.data as Member
  }

  public async createMember(member: Member): Promise<SucceededOrNotResponse> {
    const response = await this
        ._httpClient
        .post<any, AxiosResponse<SucceededOrNotResponse>>(
            `${import.meta.env.VITE_API_BASE_URL}/members`,
            member,
            this.headersWithJsonContentType())
        .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
          return error.response as AxiosResponse<SucceededOrNotResponse>
        })
    const succeededOrNotResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(succeededOrNotResponse.succeeded, succeededOrNotResponse.errors)
  }

  public async updateMember(member: Member): Promise<SucceededOrNotResponse> {
    const response = await this
        ._httpClient
        .put<any, AxiosResponse<SucceededOrNotResponse>>(
            `${import.meta.env.VITE_API_BASE_URL}/members/${member.id}`,
            member,
            this.headersWithJsonContentType())
        .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
          return error.response as AxiosResponse<SucceededOrNotResponse>
        })
    const succeededOrNotResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(succeededOrNotResponse.succeeded, succeededOrNotResponse.errors)
  }

  public async deleteMember(id: Guid): Promise<SucceededOrNotResponse> {
    const response = await this
        ._httpClient
        .delete<any, AxiosResponse<SucceededOrNotResponse>>(`${import.meta.env.VITE_API_BASE_URL}/members/${id}`)
        .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
          return error.response as AxiosResponse<SucceededOrNotResponse>
        })
    return new SucceededOrNotResponse(response.status === 204)
  }
}