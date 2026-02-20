import {AxiosError, AxiosResponse} from "axios"
import {injectable} from "inversify"

import {ApiService} from "@/services/apiService"
import {IMenuService} from "@/injection/interfaces"
import {SucceededOrNotResponse} from "@/types/responses"
import {NavigationMenu, NavigationMenuItem} from "@/types/entities"

@injectable()
export class MenuService extends ApiService implements IMenuService {
  public async getAll(): Promise<NavigationMenu[]> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<NavigationMenu[]>>(`${import.meta.env.VITE_API_BASE_URL}/admin/menus`)
      .catch(function (error: AxiosError): AxiosResponse<NavigationMenu[]> {
        return error.response as AxiosResponse<NavigationMenu[]>
      })
    return response.data as NavigationMenu[]
  }

  public async get(id: string): Promise<NavigationMenu> {
    const response = await this
      ._httpClient
      .get<any, AxiosResponse<NavigationMenu>>(`${import.meta.env.VITE_API_BASE_URL}/admin/menus/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<NavigationMenu> {
        return error.response as AxiosResponse<NavigationMenu>
      })
    return response.data as NavigationMenu
  }

  public async create(menu: NavigationMenu): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<NavigationMenu>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/menus`,
        menu,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<NavigationMenu> {
        return error.response as AxiosResponse<NavigationMenu>
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async update(menu: NavigationMenu): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .patch<any, AxiosResponse<NavigationMenu>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/menus/${menu.id}`,
        menu,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<NavigationMenu> {
        return error.response as AxiosResponse<NavigationMenu>
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async delete(id: string): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .delete<any, AxiosResponse<any>>(`${import.meta.env.VITE_API_BASE_URL}/admin/menus/${id}`)
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async addMenuItem(menuId: string, item: NavigationMenuItem): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<NavigationMenuItem>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/menus/${menuId}/items`,
        item,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<NavigationMenuItem> {
        return error.response as AxiosResponse<NavigationMenuItem>
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async updateMenuItem(menuId: string, item: NavigationMenuItem): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .patch<any, AxiosResponse<NavigationMenuItem>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/menus/${menuId}/items/${item.id}`,
        item,
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<NavigationMenuItem> {
        return error.response as AxiosResponse<NavigationMenuItem>
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async deleteMenuItem(menuId: string, itemId: string): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .delete<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/menus/${menuId}/items/${itemId}`)
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }

  public async reorderMenuItems(menuId: string, itemIds: string[]): Promise<SucceededOrNotResponse> {
    const response = await this
      ._httpClient
      .post<any, AxiosResponse<any>>(
        `${import.meta.env.VITE_API_BASE_URL}/admin/menus/${menuId}/items/reorder`,
        {itemIds},
        this.headersWithJsonContentType())
      .catch(function (error: AxiosError): AxiosResponse<any> {
        return error.response as AxiosResponse<any>
      })
    return new SucceededOrNotResponse(response.status >= 200 && response.status < 300)
  }
}
