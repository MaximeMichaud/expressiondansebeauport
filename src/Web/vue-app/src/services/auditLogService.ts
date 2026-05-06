import {AxiosError, AxiosResponse} from "axios"

import {ApiService} from "@/services/apiService"
import {IAuditLogService} from "@/injection/interfaces"
import {AuditLog} from "@/types/entities"
import {PaginatedResponse} from "@/types/responses"

export class AuditLogService extends ApiService implements IAuditLogService {
  public async getAll(
    pageIndex: number,
    pageSize: number,
    filters?: { user?: string; actionType?: string; fromDate?: string; toDate?: string }
  ): Promise<PaginatedResponse<AuditLog>> {
    const params = new URLSearchParams({
      pageIndex: pageIndex.toString(),
      pageSize: pageSize.toString(),
    })

    if (filters?.user?.trim()) params.set('user', filters.user.trim())
    if (filters?.actionType?.trim()) params.set('actionType', filters.actionType.trim())
    if (filters?.fromDate?.trim()) params.set('fromDate', filters.fromDate.trim())
    if (filters?.toDate?.trim()) params.set('toDate', filters.toDate.trim())

    const response = await this
      ._httpClient
      .get<any, AxiosResponse<PaginatedResponse<AuditLog>>>(`${import.meta.env.VITE_API_BASE_URL}/admin/audit-logs?${params.toString()}`)
      .catch(function (error: AxiosError): AxiosResponse<PaginatedResponse<AuditLog>> {
        return error.response as AxiosResponse<PaginatedResponse<AuditLog>>
      })

    return response.data as PaginatedResponse<AuditLog>
  }
}
